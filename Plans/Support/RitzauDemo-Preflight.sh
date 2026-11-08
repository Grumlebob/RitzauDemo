#!/usr/bin/env bash
# Read-only source preflight. Test fixtures use their own temporary directories.
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo 'Usage: bash RitzauDemo-Preflight.sh /absolute/path/to/GrumPlates' >&2
  exit 2
fi
source_root="$(realpath -- "$1")"
test -f "$source_root/GrumPlates.sln"
test -f "$source_root/Deployment/LocalSingleNode/Scripts/lib/setup_status.py"
export PYTHONDONTWRITEBYTECODE=1
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$PATH"
command -v python3 >/dev/null
command -v dotnet >/dev/null
echo "Source commit: $(git -C "$source_root" rev-parse HEAD)"
echo 'This preflight runs fixture tests only; it does not bootstrap or deploy.'
dotnet --version
support_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
python3 -B - "$source_root" "$support_root/RitzauDemo-ExecutionPacket.md" <<'PYTHON'
from pathlib import Path
import sys
import tempfile

sys.path.insert(0, str(Path(sys.argv[1]) / 'Deployment/LocalSingleNode/Scripts/lib'))
from ls_settings import settings

packet = Path(sys.argv[2]).read_text(encoding='utf-8')
candidate = packet.split('```yaml\n', 1)[1].split('```', 1)[0]
with tempfile.TemporaryDirectory(prefix='ritzaudemo-settings-preflight-') as directory:
    path = Path(directory) / 'settings.yml'
    path.write_text(candidate, encoding='utf-8')
    values = settings(path)
    assert values['app_name'] == 'ritzaudemo'
    assert values['accounts_registration_enabled'] is False
    assert values['accounts_admin_seed_enabled'] is False
print('Candidate settings schema valid; live collisions are not checked here')
PYTHON

bash "$source_root/Deployment/Common/Scripts/Tests/test-with-node-browser-lock.sh"

python3 -B -m unittest discover \
  -s "$source_root/Deployment/LocalSingleNode/Scripts/Tests" \
  -p test_setup_cli.py -v
python3 -B -m unittest discover \
  -s "$source_root/Deployment/LocalSingleNode/Scripts/Tests" \
  -p test_single_node.py -v
python3 -B -m unittest discover \
  -s "$source_root/Deployment/LocalSingleNode/Scripts/Tests" \
  -p test_settings_fixture.py -v
python3 -B -m unittest discover \
  -s "$source_root/Scripts/CI/tests" \
  -p test_frontend_profile.py -v
echo 'Fixture preflight passed. The full local gate and native acceptance remain required.'
