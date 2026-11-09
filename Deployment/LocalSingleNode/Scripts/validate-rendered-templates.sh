#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec python3 - "$SCRIPT_DIR/.." <<'PY'
import json
from pathlib import Path
import re
import subprocess
import sys
import tempfile
from jinja2 import Environment, StrictUndefined


DOCKERFILE = (Path(sys.argv[1]).resolve().parents[1] / 'RitzauDemo/Dockerfile').read_text(encoding='utf-8')
ASPNET_IMAGE_MATCH = re.search(r'^FROM (mcr\.microsoft\.com/dotnet/aspnet:[^\s]+) AS base$', DOCKERFILE, re.M)
if ASPNET_IMAGE_MATCH is None:
    raise SystemExit('rendered template validation failed: ASP.NET runtime image is missing from the product Dockerfile')
ASPNET_IMAGE = ASPNET_IMAGE_MATCH.group(1)


def assert_runtime_environment(folder, expected):
    verifier = folder / 'verify-env.yml'
    start_marker, end_marker = '__RITZAU_ENV_START__', '__RITZAU_ENV_END__'
    shell_values = '"$$' + '{Accounts__AdminSeed__Password}" "$$' + '{Accounts__UserSeed__Password}"'
    shell_script = (
        f"printf '{start_marker}'; printf '%s\\000' " + shell_values + f"; printf '{end_marker}'"
    )
    verifier.write_text(json.dumps({'services': {'verify': {
        'image': ASPNET_IMAGE,
        'network_mode': 'none',
        'env_file': [{'path': './.env', 'format': 'raw'}],
        'command': ['sh', '-c', shell_script],
    }}}, indent=2))
    empty_interpolation = folder / 'empty-interpolation.env'
    empty_interpolation.write_text('')
    result = subprocess.check_output([
        'docker', 'compose', '--project-directory', str(folder), '--env-file', str(empty_interpolation),
        '-f', str(verifier), 'run', '--rm', '--no-deps', '-T', 'verify'
    ], stderr=subprocess.STDOUT)
    start = result.find(start_marker.encode('ascii'))
    end = result.find(end_marker.encode('ascii'), start + len(start_marker)) if start >= 0 else -1
    if start < 0 or end < 0:
        raise AssertionError('synthetic account environment verifier did not return its markers')
    output = result[start + len(start_marker):end]
    actual = output[:-1].split(b'\x00') if output.endswith(b'\x00') else []
    assert actual == [expected['admin_seed_password'].encode('utf-8'), expected['user_seed_password'].encode('utf-8')], 'synthetic seed account environment did not round-trip'


target = Path(sys.argv[1]).resolve()
root = target.parents[1]
context = dict(app_name='books', inventory_hostname='node-rehearsal', node_ip='192.0.2.10', lan_http_port=80, app_port=8080, postgres_port=5432, redis_port=6379, lan_hostnames=['books.example.invalid'], docker_subnet='172.30.10.0/24', app_image='ghcr.io/fixture-owner/books', release_image_digest='sha256:' + 'a' * 64, app_version='b' * 40, vault_postgres_password='FixturePassword12345678901234567890', vault_redis_password='FixtureRedis12345678901234567890123', vault_data_protection_certificate_password='d' * 64, data_protection_certificate_gid='1001')
context.update(accounts_admin_seed_enabled=False, accounts_admin_seed_email='', vault_accounts_admin_seed_password='', accounts_user_seed_enabled=False, accounts_user_seed_email='', vault_accounts_user_seed_password='')
jinja = Environment(undefined=StrictUndefined, trim_blocks=True, lstrip_blocks=True)
with tempfile.TemporaryDirectory(prefix='single-node-render-') as directory:
    folder = Path(directory)
    template = jinja.from_string((target / 'ansible/templates/app.env.j2').read_text())
    compose_template = jinja.from_string((target / 'ansible/templates/compose.env.j2').read_text())
    single_fixture = folder / 'single-node-compose'
    single_fixture.mkdir()
    single_compose = single_fixture / 'docker-compose.yml'
    single_compose.write_text((target / 'compose/docker-compose.yml').read_text())
    environment = template.render(**context)
    env = single_fixture / '.env'
    env.write_text(environment)
    env.chmod(0o600)
    interpolation_env = single_fixture / 'compose-input.env'
    interpolation_env.write_text(compose_template.render(**context))
    assert 'Accounts__' not in interpolation_env.read_text(), 'Compose interpolation allowlist must exclude account settings'
    output = subprocess.check_output(['docker', 'compose', '--project-directory', str(single_fixture), '--env-file', str(interpolation_env), '-f', str(single_compose), 'config', '--format', 'json'], text=True)
    config = json.loads(output)
    resource_limits = {
        'web': {'cpus': 1.5, 'mem_limit': '536870912', 'pids_limit': 256},
        'postgres': {'cpus': 1.0, 'mem_limit': '805306368', 'pids_limit': 256},
        'redis': {'cpus': 0.25, 'mem_limit': '134217728', 'pids_limit': 128},
    }
    for service_name, expected in resource_limits.items():
        service = config['services'][service_name]
        for key, value in expected.items():
            assert service.get(key) == value, f'{service_name} must set {key}={value}'
    redis_command = config['services']['redis']['command']
    assert redis_command[redis_command.index('--maxmemory') + 1] == '64mb'
    assert redis_command[redis_command.index('--maxmemory-policy') + 1] == 'noeviction'
    for service in config['services'].values():
        for port in service.get('ports', []):
            assert port['host_ip'] == '127.0.0.1', 'Compose published a non-loopback port'
    assert config['services']['web']['image'] == context['app_image'] + '@' + context['release_image_digest']
    assert 'Accounts__Enabled' not in config['services']['web']['environment']
    assert config['services']['web']['environment']['Accounts__UserSeed__Enabled'] == 'false'
    assert config['services']['web']['environment']['DataProtection__KeyEncryptionCertificatePath'] == '/run/secrets/data-protection.pem'
    assert config['services']['web']['environment']['DataProtection__KeyEncryptionCertificatePassword'] == 'd' * 64
    assert config['services']['web']['group_add'] == ['1001']
    assert any(volume.get('target') == '/run/secrets/data-protection.pem' and volume.get('read_only') for volume in config['services']['web']['volumes'])
    assert 'Accounts__UserSeed__Password' not in (config['services']['postgres'].get('environment') or {})
    assert 'Accounts__AdminSeed__Password' not in (config['services']['postgres'].get('environment') or {})
    assert config['networks']['default']['ipam']['config'][0]['subnet'] == context['docker_subnet']

    admin_password = "Admin $one #two = \"three\" 'four' spaced"
    user_password = "User $one #two = \"three\" 'four' spaced"
    enabled_context = {
        **context,
        'accounts_admin_seed_enabled': True,
        'accounts_admin_seed_email': 'admin@example.invalid',
        'vault_accounts_admin_seed_password': admin_password,
        'accounts_user_seed_enabled': True,
        'accounts_user_seed_email': 'user@example.invalid',
        'vault_accounts_user_seed_password': user_password,
    }
    enabled_env = env
    enabled_environment = template.render(**enabled_context)
    enabled_env.write_text(enabled_environment)
    enabled_env.chmod(0o600)
    interpolation_env.write_text(compose_template.render(**enabled_context))
    assert 'Accounts__' not in interpolation_env.read_text(), 'enabled account settings must stay out of Compose interpolation'
    enabled_output = subprocess.check_output(['docker', 'compose', '--project-directory', str(single_fixture), '--env-file', str(interpolation_env), '-f', str(single_compose), 'config', '--format', 'json'], text=True)
    enabled_config = json.loads(enabled_output)
    web_environment = enabled_config['services']['web']['environment']
    assert web_environment['Accounts__AdminSeed__Enabled'] == 'true'
    assert web_environment['Accounts__AdminSeed__Email'] == 'admin@example.invalid'
    assert web_environment['Accounts__UserSeed__Enabled'] == 'true'
    assert web_environment['Accounts__UserSeed__Email'] == 'user@example.invalid'
    assert_runtime_environment(single_fixture, {
        'admin_seed_password': admin_password,
        'user_seed_password': enabled_context['vault_accounts_user_seed_password'],
    })
    for service_name in ('postgres', 'redis'):
        assert 'Accounts__AdminSeed__Password' not in (enabled_config['services'][service_name].get('environment') or {})
        assert 'Accounts__UserSeed__Password' not in (enabled_config['services'][service_name].get('environment') or {})

    root_fixture = folder / 'root-compose'
    root_fixture.mkdir()
    root_env = (root / '.env.example').read_text()
    root_env = root_env.replace('Accounts__AdminSeed__Enabled=false', 'Accounts__AdminSeed__Enabled=true')
    root_env = root_env.replace('Accounts__AdminSeed__Email=\n', 'Accounts__AdminSeed__Email=admin@localhost.test\n')
    root_env = root_env.replace('Accounts__AdminSeed__Password=\n', f"Accounts__AdminSeed__Password={admin_password}\n")
    root_env = root_env.replace('Accounts__UserSeed__Enabled=false', 'Accounts__UserSeed__Enabled=true')
    root_env = root_env.replace('Accounts__UserSeed__Email=\n', 'Accounts__UserSeed__Email=user@localhost.test\n')
    root_env = root_env.replace('Accounts__UserSeed__Password=\n', f"Accounts__UserSeed__Password={user_password}\n")
    (root_fixture / '.env').write_text(root_env)
    root_interpolation = root_fixture / 'compose-input.env'
    root_interpolation.write_text('\n'.join(line for line in root_env.splitlines() if not line.startswith('Accounts__')) + '\n')
    root_compose = root / 'docker-compose.yml'
    (root_fixture / 'docker-compose.yml').write_text(root_compose.read_text())
    root_output = subprocess.check_output([
        'docker', 'compose', '--project-directory', str(root_fixture), '--env-file', str(root_interpolation),
        '-f', str(root_fixture / 'docker-compose.yml'), 'config', '--format', 'json'
    ], text=True, cwd=root)
    root_config = json.loads(root_output)
    assert root_config['services']['web']['environment']['Accounts__AdminSeed__Enabled'] == 'true'
    assert root_config['services']['web']['environment']['Accounts__AdminSeed__Email'] == 'admin@localhost.test'
    assert root_config['services']['web']['environment']['Accounts__AdminSeed__Password'] == admin_password.replace('$', '$$')
    assert root_config['services']['web']['environment']['Accounts__UserSeed__Enabled'] == 'true'
    assert root_config['services']['web']['environment']['Accounts__UserSeed__Email'] == 'user@localhost.test'
    assert root_config['services']['web']['environment']['Accounts__UserSeed__Password'] == user_password.replace('$', '$$')
    for service_name in ('postgres', 'redis', 'redisinsight'):
        assert 'Accounts__AdminSeed__Password' not in (root_config['services'][service_name].get('environment') or {})
        assert 'Accounts__UserSeed__Password' not in (root_config['services'][service_name].get('environment') or {})
    assert root_config['services']['postgres']['environment']['POSTGRES_PASSWORD'] == 'postgres'
    assert_runtime_environment(root_fixture, {
        'admin_seed_password': admin_password,
        'user_seed_password': user_password,
    })
    site = jinja.from_string((target / 'ansible/roles/single_node_caddy_site/templates/app.caddy.j2').read_text()).render(**context)
    assert 'http://192.0.2.10:80' in site and 'http://node-rehearsal.local:80' in site
    assert 'reverse_proxy 127.0.0.1:8080' in site and 'health_uri /health/ready' in site
    assert '{{' not in site and '{%' not in site
print('LocalSingleNode and root seed account settings, secret round-trip, Compose and Caddy checks passed')
PY
