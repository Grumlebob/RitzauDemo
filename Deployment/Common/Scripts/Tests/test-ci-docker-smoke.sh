#!/usr/bin/env bash
# Prove packaged smoke resources are run-owned and cleaned on success and failure.
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCRIPT="$SCRIPT_DIR/../ci-docker-smoke.sh"
TMP_ROOT="$(mktemp -d)"
trap 'rm -rf "$TMP_ROOT"' EXIT
mkdir -p "$TMP_ROOT/bin" "$TMP_ROOT/state" "$TMP_ROOT/runner-temp"
export SMOKE_STATE="$TMP_ROOT/state"
export SMOKE_CALLS="$TMP_ROOT/calls.jsonl"
export PATH="$TMP_ROOT/bin:$PATH"
export APP_IMAGE=example/app APP_VERSION=test APP_NAME=sample GITHUB_REPOSITORY=example/repo GITHUB_SHA=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
cat > "$TMP_ROOT/bin/docker" <<'PY'
#!/usr/bin/env python3
import json
import os
import re
import sys
from pathlib import Path

args = sys.argv[1:]
root = Path(os.environ["SMOKE_STATE"])
with open(os.environ["SMOKE_CALLS"], "a") as stream:
    stream.write(json.dumps(args) + "\n")

def state(name):
    return root / name

def labels():
    return dict(args[index + 1].split("=", 1) for index, value in enumerate(args) if value == "--label")

if args[:2] == ["image", "inspect"]:
    profile = "BlazorAuto"
    acceptance = "BlazorAutoV1"
    if os.environ.get("FAIL_IMAGE_LABEL") == "1":
        profile = "React"
    if os.environ.get("FAIL_ACCEPTANCE_LABEL") == "1":
        acceptance = "ReactPublicV1"
    print(json.dumps({
        "com.grumplates.frontend-profile": profile,
        "com.grumplates.acceptance-profile": acceptance,
        "org.opencontainers.image.revision": os.environ["GITHUB_SHA"],
    }))
elif args[:2] == ["network", "create"] or args[0] == "run":
    name = args[-1] if args[0] == "network" else args[args.index("--name") + 1]
    if state(name).exists():
        raise SystemExit(125)
    if "redis" in name and os.environ.get("FAIL_REDIS") == "1":
        raise SystemExit(17)
    metadata = labels()
    metadata["ports"] = {}
    if "ci-postgres" in args:
        password = os.environ.get("POSTGRES_PASSWORD", "")
        if not re.fullmatch(r"[0-9a-f]{48}", password):
            raise SystemExit("The database fixture requires a generated password.")
        metadata["database_password"] = password
    for argument in args:
        if argument.startswith("ConnectionStrings__DefaultConnection="):
            passwords = [json.loads(item.read_text()).get("database_password") for item in root.iterdir()]
            if not any(password and f"Password={password};" in argument for password in passwords):
                raise SystemExit("The application did not receive the fixture database password.")
    if args[0] == "run":
        for index, value in enumerate(args[:-1]):
            if value in ("-p", "--publish"):
                mapping = args[index + 1].split(":")
                if len(mapping) == 3 and mapping[-1] in ("5432", "8080"):
                    metadata["ports"][mapping[-1] + "/tcp"] = mapping[1] or "34567"
    state(name).write_text(json.dumps(metadata))
elif args[:2] == ["network", "inspect"] and "Gateway" in args[args.index("--format") + 1]:
    print("172.20.0.1")
elif args[0] == "inspect" or args[:2] == ["network", "inspect"]:
    name = args[-1]
    if not state(name).exists():
        raise SystemExit(1)
    item = json.loads(state(name).read_text())
    print(" ".join(item.get("localcluster.ci." + key, "") for key in ("repository", "owner", "run_id", "run_attempt")))
elif args[0] == "logs":
    print("fake smoke logs")
elif args[0] == "rm" or args[:2] == ["network", "rm"]:
    state(args[-1]).unlink()
elif args[0] == "port":
    item = json.loads(state(args[1]).read_text())
    print("127.0.0.1:" + item.get("ports", {}).get(args[2], "34567"))
elif args[0] == "exec":
    print("PONG" if "redis-cli" in args else "accepting connections")
else:
    raise SystemExit("unexpected fake Docker command")
PY
cat > "$TMP_ROOT/bin/curl" <<'SH'
#!/usr/bin/env bash
url="${*: -1}"
case "$url" in
  */health/ready) printf '200' ;;
  */api/books)
    if [[ "${FAIL_API:-0}" == "1" ]]; then
      printf 'HTTP/1.1 302 Found\r\nLocation: /Account/Login\r\n\r\n'
    else
      printf 'HTTP/1.1 401 Unauthorized\r\nCache-Control: no-store\r\n\r\n'
    fi
    ;;
  */) printf '<!DOCTYPE html><script src="_framework/blazor.web.js"></script>' ;;
  *) exit 22 ;;
esac
SH
cat > "$TMP_ROOT/bin/pwsh" <<'SH'
#!/usr/bin/env bash
if [[ "$*" == *Test-DeployedSite.ps1* ]]; then
  [[ " $* " == *" -FrontendProfile BlazorAuto "* ]] || exit 36
  [[ " $* " == *" -AcceptanceProfile BlazorAutoV1 "* ]] || exit 37
  exit "${FAIL_HTTP_ACCEPTANCE:-0}"
fi
exit 0
SH
cat > "$TMP_ROOT/bin/dotnet" <<'SH'
#!/usr/bin/env bash
[[ "$*" == *"FullyQualifiedName~RenderModeE2ETests"* ]] || exit 3
[[ "${E2E_BASE_URL:-}" == http://127.0.0.1:* ]] || exit 4
results_dir=""
previous=""
for argument in "$@"; do
  if [[ "$previous" == "--results-directory" ]]; then results_dir="$argument"; fi
  previous="$argument"
done
[[ "$results_dir" == */test-results/123-2/validate/docker-smoke/BlazorAuto || "$results_dir" == */test-results/local-*-1/validate/docker-smoke/BlazorAuto ]] || exit 39
mkdir -p "$results_dir"
cat > "$results_dir/blazor-browser-smoke.trx" <<'XML'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results><UnitTestResult testName="render-mode-smoke" outcome="Passed"/></Results></TestRun>
XML
exit "${FAIL_BROWSER:-0}"
SH
cat > "$TMP_ROOT/bin/sleep" <<'SH'
#!/usr/bin/env bash
exit 0
SH
chmod +x "$TMP_ROOT/bin/"*

run_case() {
  local expected="$1" status=0
  RUNNER_TEMP="$TMP_ROOT/runner-temp" GITHUB_RUN_ID=123 GITHUB_RUN_ATTEMPT=2 bash "$SCRIPT" > "$TMP_ROOT/output" 2>&1 || status=$?
  [[ "$status" == "$expected" ]] || { cat "$TMP_ROOT/output" >&2; echo "expected $expected, got $status" >&2; exit 1; }
  [[ -z "$(find "$SMOKE_STATE" -type f -print)" ]] || { echo "owned resources leaked" >&2; exit 1; }
}

run_case 0
FAIL_IMAGE_LABEL=1 run_case 1
FAIL_ACCEPTANCE_LABEL=1 run_case 1
FAIL_REDIS=1 run_case 17
FAIL_API=1 run_case 1
FAIL_BROWSER=19 run_case 19
FAIL_HTTP_ACCEPTANCE=23 run_case 23
FRONTEND_PROFILE=React run_case 2
FRONTEND_PROFILE=BlazorAuto FRONTEND_ACCEPTANCE_PROFILE=ReactPublicV1 run_case 2

env -u GITHUB_RUN_ID -u GITHUB_RUN_ATTEMPT RUNNER_TEMP="$TMP_ROOT/runner-temp" bash "$SCRIPT" > "$TMP_ROOT/local-one"
env -u GITHUB_RUN_ID -u GITHUB_RUN_ATTEMPT RUNNER_TEMP="$TMP_ROOT/runner-temp" bash "$SCRIPT" > "$TMP_ROOT/local-two"
python3 - <<'PY'
import json
import os
from pathlib import Path

calls = [json.loads(line) for line in Path(os.environ["SMOKE_CALLS"]).read_text().splitlines()]
networks = [args[-1] for args in calls if args[:2] == ["network", "create"] and "local-" in args[-1]]
assert len(networks) == 2 and len(set(networks)) == 2
assert all(name.startswith("sample-ci-") for name in networks)
for args in calls:
    if args[0] == "run":
        name = args[args.index("--name") + 1]
        assert name.startswith("sample-ci-"), name
        if "postgres" in name:
            assert args[args.index("--network-alias") + 1] == "ci-postgres"
            assert "/var/lib/postgresql:rw,size=1073741824" in args
            assert "127.0.0.1::5432" in args
        if "redis" in name:
            assert args[args.index("--network-alias") + 1] == "ci-redis"
            assert "/data:rw,size=67108864" in args
            assert "--appendonly" in args
        if "web" in name:
            assert any(value.startswith("ConnectionStrings__DefaultConnection=Host=ci-postgres;") for value in args)
            assert "Redis__Configuration=ci-redis:6379,abortConnect=false" in args
            assert "RateLimiting__Api__PermitLimit=1000" in args
            assert "DataProtection__KeyEncryptionCertificatePath=/run/dp-cert/dp.pfx" in args
            assert any(value.endswith(":/run/dp-cert:ro") for value in args)
        assert "localcluster.ci.repository=example/repo" in args
        assert "localcluster.ci.owner=ci-smoke" in args
        assert "localcluster.ci.purpose=smoke" in args
        assert any(value.startswith("localcluster.ci.session=") for value in args)
        assert any(value.startswith("localcluster.ci.run_id=") for value in args)
        assert any(value.startswith("localcluster.ci.run_attempt=") for value in args)
    assert "prune" not in args and "volume" not in args
PY

# A foreign object with a colliding name must survive failed creation/cleanup.
printf '{"localcluster.ci.repository":"other/repo"}' > "$SMOKE_STATE/sample-ci-web-123-2"
if RUNNER_TEMP="$TMP_ROOT/runner-temp" GITHUB_RUN_ID=123 GITHUB_RUN_ATTEMPT=2 bash "$SCRIPT" > "$TMP_ROOT/collision" 2>&1; then
  echo "smoke unexpectedly replaced a foreign colliding resource" >&2
  exit 1
fi
grep -Fq '"localcluster.ci.repository":"other/repo"' "$SMOKE_STATE/sample-ci-web-123-2"
