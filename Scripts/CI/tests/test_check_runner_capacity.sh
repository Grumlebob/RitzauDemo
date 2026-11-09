#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCRIPT="$SCRIPT_DIR/../check-runner-capacity.sh"
TMP_ROOT="$(mktemp -d)"
trap 'rm -rf -- "$TMP_ROOT"' EXIT
mkdir -p "$TMP_ROOT/bin" "$TMP_ROOT/work" "$TMP_ROOT/temp" "$TMP_ROOT/docker"

cat > "$TMP_ROOT/bin/docker" <<'SH'
#!/usr/bin/env bash
[[ "${1:-}" == info && "${2:-}" == --format && "${3:-}" == '{{.DockerRootDir}}' ]] || exit 90
printf '%s\n' "$FAKE_DOCKER_ROOT"
SH

cat > "$TMP_ROOT/bin/df" <<'SH'
#!/usr/bin/env bash
case "${1:-}" in
  -Pm)
    printf 'Filesystem 1024-blocks Used Available Capacity Mounted on\n'
    printf '/dev/fake 100000 10000 %s 10%% /fake\n' "${FAKE_FREE_MIB:-30000}"
    ;;
  -Pi)
    printf 'Filesystem Inodes IUsed IFree IUse%% Mounted on\n'
    printf '/dev/fake 100000 10000 %s 10%% /fake\n' "${FAKE_FREE_INODES:-90000}"
    ;;
  *) exit 91 ;;
esac
SH
chmod +x "$TMP_ROOT/bin/docker" "$TMP_ROOT/bin/df"

export PATH="$TMP_ROOT/bin:$PATH"
export GITHUB_WORKSPACE="$TMP_ROOT/work"
export RUNNER_TEMP="$TMP_ROOT/temp"
export FAKE_DOCKER_ROOT="$TMP_ROOT/docker"

bash "$SCRIPT" >/dev/null
if FAKE_FREE_MIB=1000 bash "$SCRIPT" >/dev/null 2>&1; then
  echo "low free-space fixture unexpectedly passed" >&2
  exit 1
fi
if FAKE_FREE_INODES=1000 bash "$SCRIPT" >/dev/null 2>&1; then
  echo "low free-inode fixture unexpectedly passed" >&2
  exit 1
fi

echo "runner capacity fail-closed fixtures passed"
