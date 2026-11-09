from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[5]
failures: list[str] = []


def read(path: str) -> str:
    candidate = ROOT / path
    if not candidate.is_file():
        failures.append(f"missing required file: {path}")
        return ""
    return candidate.read_text(encoding="utf-8-sig")


def require_file(path: str) -> None:
    if not (ROOT / path).is_file():
        failures.append(f"missing required file: {path}")


def require_contains(path: str, needle: str, reason: str) -> None:
    if needle not in read(path):
        failures.append(f"{path}: missing {reason}: {needle}")


def require_not_contains(path: str, needle: str, reason: str) -> None:
    if needle in read(path):
        failures.append(f"{path}: contains {reason}: {needle}")


required_files = [
    "Deployment/Common/release.yml",
    "Deployment/Common/Scripts/ci-docker-smoke.sh",
    "Deployment/Common/Scripts/Tests/test-ci-docker-smoke.sh",
    "Deployment/Common/Scripts/audit-deployment.sh",
    "Deployment/Common/Scripts/with-runner-build-admission.sh",
    "Deployment/Common/Scripts/with-node-browser-lock.sh",
    "Deployment/Common/Scripts/Tests/test-with-runner-build-admission.sh",
    "Deployment/Common/Scripts/Tests/test-with-node-browser-lock.sh",
    "Deployment/Common/Scripts/validate_release_manifest.py",
    "Deployment/Common/Scripts/validate-common-release.sh",
    "Deployment/LocalSingleNode/compose/docker-compose.yml",
    "Deployment/LocalSingleNode/inventory/group_vars/all.yml",
    "Deployment/LocalSingleNode/ansible/playbooks/PrepareSingleNode.yml",
    "Deployment/LocalSingleNode/ansible/playbooks/site.yml",
    "Deployment/LocalSingleNode/ansible/roles/single_node_stack/tasks/main.yml",
    "Deployment/LocalSingleNode/ansible/roles/single_node_backup/tasks/main.yml",
    "Deployment/LocalSingleNode/Scripts/acceptance-check.sh",
    "Deployment/LocalSingleNode/Scripts/backup-db.sh",
    "Deployment/LocalSingleNode/Scripts/verify-backup.sh",
    "Deployment/LocalSingleNode/Scripts/validate-rendered-templates.sh",
    "Deployment/LocalSingleNode/Scripts/verify-release-identity.sh",
    "Deployment/LocalSingleNode/Scripts/Tests/test-verify-release-identity.sh",
    "Deployment/LocalSingleNode/Scripts/Tests/test-target-gate.sh",
    "Deployment/LocalSingleNode/Scripts/run-maintenance.sh",
    "Deployment/LocalSingleNode/Scripts/lib/maintenance.py",
    "Deployment/LocalSingleNode/Scripts/lib/backup.py",
    "Scripts/CI/validate-frontend-profiles.sh",
    "Scripts/CI/check-runner-capacity.sh",
    "Scripts/CI/require_ci_results.py",
    "Scripts/Test-DeployedSite.ps1",
    ".github/workflows/ci.yml",
    ".github/workflows/cd-localsinglenode.yml",
    ".github/workflows/localsinglenode-maintenance.yml",
    ".github/workflows/registry-retention.yml",
    ".github/workflows/release-status.yml",
]
for item in required_files:
    require_file(item)


removed_paths = [
    "Deployment/Cloud",
    "Deployment/LocalCluster",
    ".github/workflows/cd-cloud.yml",
    ".github/workflows/cd-localcluster.yml",
    ".github/workflows/localcluster-docker-maintenance.yml",
    ".github/workflows/localcluster-prepare-vault.yml",
]
for item in removed_paths:
    candidate = ROOT / item
    if candidate.is_dir():
        leftovers = [
            path for path in candidate.rglob("*")
            if path.is_symlink() or path.is_file()
            if not (path.is_file() and not path.is_symlink() and "__pycache__" in path.parts and path.suffix == ".pyc")
        ]
        if leftovers:
            failures.append(f"unsupported deployment target must be removed from RitzauDemo: {item}")
    elif candidate.exists():
        failures.append(f"unsupported deployment target must be removed from RitzauDemo: {item}")


try:
    tracked_result = subprocess.run(
        ["git", "ls-files"],
        cwd=ROOT,
        check=True,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    tracked = {line.strip().replace("\\", "/") for line in tracked_result.stdout.splitlines() if line.strip()}
except (OSError, subprocess.CalledProcessError) as exc:
    tracked = set()
    failures.append(f"unable to inspect tracked files: {exc}")

for secret_path in (
    ".env",
    "secrets.env",
    "Deployment/LocalSingleNode/machine.yml",
    "Deployment/LocalSingleNode/inventory/hosts.yml",
    "Deployment/LocalSingleNode/inventory/group_vars/vault.yml",
):
    if secret_path in tracked:
        failures.append(f"private machine or secret file must not be tracked: {secret_path}")

for path in sorted(tracked):
    if path.startswith(("Deployment/Cloud/", "Deployment/LocalCluster/")):
        failures.append(f"removed deployment target is still tracked: {path}")
    if re.search(r"(^|/)(__pycache__|\.pytest_cache)(/|$)", path) or re.search(r"\.(pyc|pyo|pyd|pfx|tmp|log)$", path):
        failures.append(f"generated, cache, or secret-like file must not be tracked: {path}")


settings_path = "Deployment/LocalSingleNode/inventory/group_vars/all.yml"
for key, value in (
    ("app_name", "ritzaudemo"),
    ("observability_enabled", "false"),
    ("accounts_registration_enabled", "false"),
    ("accounts_admin_seed_enabled", "false"),
):
    if not re.search(rf"(?m)^{re.escape(key)}:\s*{re.escape(value)}\s*$", read(settings_path)):
        failures.append(f"{settings_path}: {key} must default to {value}")

release = read("Deployment/Common/release.yml")
for key, value in (
    ("app_image", "ghcr.io/grumlebob/ritzaudemo"),
    ("migration_bundle_name", "ritzaudemo-migrate"),
    ("migration_runtime", "linux-x64"),
):
    if not re.search(rf"(?m)^{re.escape(key)}:\s*{re.escape(value)}\s*$", release):
        failures.append(f"Deployment/Common/release.yml: {key} must be {value}")


prepare = read("Deployment/LocalSingleNode/ansible/playbooks/PrepareSingleNode.yml")
for needle, why in (
    ("mint_base_upgrade_packages: false", "no broad package upgrades on an existing host"),
    ("mint_base_reboot_after_upgrade: false", "no implicit reboot on an existing host"),
    ("name: avahi-daemon", "preserved native name-resolution service"),
    ("state: started", "native name resolution remains available"),
):
    if needle not in prepare:
        failures.append(f"PrepareSingleNode.yml: missing {why}")

compose = read("Deployment/LocalSingleNode/compose/docker-compose.yml")
if "127.0.0.1" not in compose:
    failures.append("LocalSingleNode Compose must bind private app/data ports to loopback")
for service in ("web:", "postgres:", "redis:"):
    if service not in compose:
        failures.append(f"LocalSingleNode Compose is missing service {service}")

site = read("Deployment/LocalSingleNode/ansible/roles/single_node_caddy_site/templates/app.caddy.j2")
for needle, why in (
    ("reverse_proxy 127.0.0.1", "LAN proxy targets the app's loopback port"),
    ("/health/ready", "Caddy checks application readiness"),
):
    if needle not in site:
        failures.append(f"LocalSingleNode Caddy site template: missing {why}")


ci = read(".github/workflows/ci.yml")
for needle, why in (
    ("ci-result-gate:", "required result gate job"),
    ("'build-test-push'", "stable required result name"),
    ("needs: [validate, publish-main]", "validation and publication dependencies"),
    ("run: python3 Scripts/CI/require_ci_results.py", "event-specific result validation"),
    ("packages: write", "main image publication permission"),
    ("Deployment/Common/Scripts/with-runner-build-admission.sh", "shared native build admission"),
    ("Deployment/Common/Scripts/ci-docker-smoke.sh", "single-node packaged smoke"),
    ("--cleanup-only", "run-owned smoke cleanup after interruption"),
    ("test-with-runner-build-admission.sh", "admission behavior checks"),
):
    if needle not in ci:
        failures.append(f".github/workflows/ci.yml: missing {why}")
result_gate = read("Scripts/CI/require_ci_results.py")
for needle, why in (
    ('event_name == "pull_request"', "PR publication skip behavior"),
    ('ref != "refs/heads/main"', "main-only push behavior"),
    ('validation != "success"', "successful validation requirement"),
    ('publication != "success"', "successful main publication requirement"),
):
    if needle not in result_gate:
        failures.append(f"Scripts/CI/require_ci_results.py: missing {why}")
for stale in (
    "Deployment/Cloud/",
    "Deployment/LocalCluster/",
    "validate-observability.sh",
    "grafana",
):
    if stale in ci.lower():
        failures.append(f".github/workflows/ci.yml: obsolete product workflow reference: {stale}")

deploy = read(".github/workflows/cd-localsinglenode.yml")
for needle, why in (
    ("refs/heads/main", "deployment is restricted to main"),
    ("merge-base --is-ancestor", "deployment target must be on main"),
    ("Verify running release identity", "running image identity check"),
    ("Run HTTP acceptance", "independent HTTP acceptance"),
    ("migrations", "explicit migration control"),
):
    if needle not in deploy:
        failures.append(f".github/workflows/cd-localsinglenode.yml: missing {why}")
for stale in ("Deployment/Cloud/", "Deployment/LocalCluster/"):
    if stale in deploy:
        failures.append(f".github/workflows/cd-localsinglenode.yml: obsolete target reference: {stale}")

for path, content in (
    ("Deployment/Common/Scripts/ci-docker-smoke.sh", read("Deployment/Common/Scripts/ci-docker-smoke.sh")),
    ("Deployment/Common/Scripts/Tests/test-ci-docker-smoke.sh", read("Deployment/Common/Scripts/Tests/test-ci-docker-smoke.sh")),
):
    for label in ("localcluster.ci.repository", "localcluster.ci.owner", "localcluster.ci.run_id", "localcluster.ci.run_attempt"):
        if label not in content:
            failures.append(f"{path}: missing shared CI resource ownership label {label}")
    if "docker volume prune" in content or "system prune" in content:
        failures.append(f"{path}: CI smoke must use owned cleanup and never prune the shared host")

smoke = read("Deployment/Common/Scripts/ci-docker-smoke.sh")
if "RenderModeE2ETests" not in smoke:
    failures.append("packaged smoke must run only the opt-in Blazor hydration test")
if 'case "${RUN_E2E:-0}" in' not in smoke or "Skipping optional packaged Blazor render/hydration smoke" not in smoke:
    failures.append("packaged Blazor hydration smoke must remain opt-in")
if "React" in smoke or "SMTP" in smoke:
    failures.append("packaged demo smoke must not initialize React or SMTP fixtures")

backup = read("Deployment/LocalSingleNode/Scripts/lib/backup.py")
for needle, why in (
    ("Fresh protected backup:", "fresh protected backup evidence"),
    ("completed < started", "stale completion markers are rejected"),
):
    if needle not in backup:
        failures.append(f"LocalSingleNode backup helper is missing {why}")

maintenance = read("Deployment/LocalSingleNode/Scripts/lib/maintenance.py")
for needle, why in (("protected_application_image_references", "current and recovery images are protected"),):
    if needle not in maintenance:
        failures.append(f"LocalSingleNode maintenance helper is missing {why}")

for path in (
    "Deployment/LocalSingleNode/Scripts/backup-db.sh",
    "Deployment/LocalSingleNode/Scripts/verify-backup.sh",
    "Deployment/LocalSingleNode/Scripts/acceptance-check.sh",
    "Deployment/LocalSingleNode/Scripts/verify-release-identity.sh",
):
    if not (ROOT / path).is_file():
        failures.append(f"required single-node operations script is missing: {path}")

if failures:
    print("Deployment audit failed:")
    for failure in failures:
        print(f" - {failure}")
    sys.exit(1)

print("Deployment audit passed.")
