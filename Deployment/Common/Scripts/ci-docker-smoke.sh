#!/usr/bin/env bash
# Start the freshly built app image with disposable PostgreSQL and Redis
# containers, check its HTTP surface, then run the browser smoke tests.
#
# Every container and network carries localcluster.ci.* labels, and cleanup
# removes only resources whose labels match this repository and run. Nothing
# here prunes, and PostgreSQL/Redis use tmpfs, so no Docker volume is created.
set -euo pipefail
CLEANUP_ONLY=false
if [[ $# -gt 0 ]]; then
  [[ $# == 1 && "$1" == --cleanup-only ]] || { echo 'Usage: ci-docker-smoke.sh [--cleanup-only]' >&2; exit 2; }
  CLEANUP_ONLY=true
  [[ -n "${GITHUB_REPOSITORY:-}" && "${GITHUB_RUN_ID:-}" =~ ^[0-9]+$ && "${GITHUB_RUN_ATTEMPT:-}" =~ ^[0-9]+$ ]] || {
    echo 'Cleanup-only requires the exact GitHub repository, run ID and attempt.' >&2; exit 2;
  }
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -P "$SCRIPT_DIR/../../.." && pwd)"
APP_IMAGE="${APP_IMAGE:?APP_IMAGE is required}"
APP_VERSION="${APP_VERSION:-${GITHUB_SHA:?APP_VERSION or GITHUB_SHA is required}}"
IMAGE_REF="${APP_IMAGE}:${APP_VERSION}"
FRONTEND_PROFILE="${FRONTEND_PROFILE:-BlazorAuto}"
[[ "$FRONTEND_PROFILE" == "BlazorAuto" ]] || { echo "Unsupported frontend profile: $FRONTEND_PROFILE" >&2; exit 2; }
expected_acceptance_profile="$(python3 "$REPO_ROOT/Scripts/Frontend/resolve_frontend_profile.py" --override "$FRONTEND_PROFILE" --acceptance-profile)"
FRONTEND_ACCEPTANCE_PROFILE="${FRONTEND_ACCEPTANCE_PROFILE:-$expected_acceptance_profile}"
[[ "$FRONTEND_ACCEPTANCE_PROFILE" == "$expected_acceptance_profile" ]] || {
  echo "Acceptance profile does not match the selected frontend profile" >&2
  exit 2
}
if [[ -n "${GITHUB_SHA:-}" && ! "$GITHUB_SHA" =~ ^[0-9a-f]{40}$ ]]; then
  echo "GITHUB_SHA must be a full lowercase commit SHA" >&2
  exit 2
fi
APP_NAME="${APP_NAME:-ritzaudemo}"
[[ "$APP_NAME" =~ ^[a-z0-9][a-z0-9-]*$ ]] || { echo "app_name must be a lowercase slug: $APP_NAME" >&2; exit 1; }

# Keep these tags equal to TestContainerImages.cs and the CI preflight pulls.
POSTGRES_IMAGE="postgres:18.4-alpine3.23"
REDIS_IMAGE="redis:8.8.0-alpine3.23"
# The single packaged browser smoke verifies Blazor hydration.
BROWSER_SMOKE_FILTER="FullyQualifiedName~RenderModeE2ETests"

expected_repository="${GITHUB_REPOSITORY:-local}"
expected_run_id="${GITHUB_RUN_ID:-local-$(cat /proc/sys/kernel/random/uuid)}"
expected_run_attempt="${GITHUB_RUN_ATTEMPT:-1}"
session="${expected_run_id}-${expected_run_attempt}"
network="${APP_NAME}-ci-${session}"
postgres="${APP_NAME}-ci-postgres-${session}"
redis="${APP_NAME}-ci-redis-${session}"
web="${APP_NAME}-ci-web-${session}"
dp_cert_dir=""
owned_label_args=(
  --label "localcluster.ci.repository=${expected_repository}"
  --label localcluster.ci.owner=ci-smoke
  --label localcluster.ci.purpose=smoke
  --label "localcluster.ci.session=${session}"
  --label "localcluster.ci.run_id=${expected_run_id}"
  --label "localcluster.ci.run_attempt=${expected_run_attempt}"
  --label "localcluster.ci.created_at=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
)
expected_identity="${expected_repository} ci-smoke ${expected_run_id} ${expected_run_attempt}"
identity_format='{{index .Labels "localcluster.ci.repository"}} {{index .Labels "localcluster.ci.owner"}} {{index .Labels "localcluster.ci.run_id"}} {{index .Labels "localcluster.ci.run_attempt"}}'

owned_container() {
  [[ "$(docker inspect --format "${identity_format//.Labels/.Config.Labels}" "$1" 2>/dev/null || true)" == "$expected_identity" ]]
}

owned_network() {
  [[ "$(docker network inspect --format "$identity_format" "$1" 2>/dev/null || true)" == "$expected_identity" ]]
}

cleanup() {
  local status="$1"
  trap - EXIT INT TERM
  if [[ "$status" -ne 0 ]]; then
    owned_container "$web" && docker logs --tail 200 --since 10m "$web" || true
    owned_container "$postgres" && docker logs --tail 200 --since 10m "$postgres" || true
    owned_container "$redis" && docker logs --tail 200 --since 10m "$redis" || true
  fi
  for container in "$web" "$postgres" "$redis"; do
    owned_container "$container" && docker rm -f "$container" >/dev/null 2>&1 || true
  done
  owned_network "$network" && docker network rm "$network" >/dev/null 2>&1 || true
  for container in "$web" "$postgres" "$redis"; do
    if owned_container "$container"; then echo "Owned smoke container remains: $container" >&2; status=1; fi
  done
  if owned_network "$network"; then echo "Owned smoke network remains: $network" >&2; status=1; fi
  if [[ -n "$dp_cert_dir" ]]; then
    local expected_dp_prefix="${RUNNER_TEMP:-${TMPDIR:-/tmp}}/dp-cert-${session}-"
    if [[ "$dp_cert_dir" == "$expected_dp_prefix"* ]]; then
      rm -rf -- "$dp_cert_dir"
    else
      echo "Refusing to remove unexpected certificate path: $dp_cert_dir" >&2
      status=1
    fi
  fi
  exit "$status"
}
if [[ "$CLEANUP_ONLY" == true ]]; then cleanup 0; fi
image_labels="$(docker image inspect "$IMAGE_REF" --format '{{json .Config.Labels}}')"
python3 - "$FRONTEND_PROFILE" "$FRONTEND_ACCEPTANCE_PROFILE" "${GITHUB_SHA:-}" "$image_labels" <<'PY'
import json
import sys

frontend_profile, acceptance_profile, source_sha, labels_json = sys.argv[1:]
labels = json.loads(labels_json)
if not isinstance(labels, dict):
    raise SystemExit("final image labels are missing")
expected = {
    "com.ritzaudemo.frontend-profile": frontend_profile,
    "com.ritzaudemo.acceptance-profile": acceptance_profile,
}
if source_sha:
    expected["org.opencontainers.image.revision"] = source_sha
for key, value in expected.items():
    if labels.get(key) != value:
        raise SystemExit(f"final image label {key!r} does not match the selected release")
PY
trap 'cleanup "$?"' EXIT
trap 'exit 130' INT
trap 'exit 143' TERM

wait_until_stable() {
  local attempts="$1"
  shift
  local consecutive_ready=0
  for ((attempt = 1; attempt <= attempts; attempt++)); do
    if "$@" >/dev/null 2>&1; then
      consecutive_ready=$((consecutive_ready + 1))
      if [[ "$consecutive_ready" -ge 3 ]]; then
        return 0
      fi
    else
      consecutive_ready=0
    fi
    sleep 2
  done
  "$@"
}

postgres_ready() {
  docker exec "$postgres" pg_isready -U postgres -d app
}

redis_ready() {
  docker exec "$redis" redis-cli ping | grep -Fxq PONG
}

docker network create "${owned_label_args[@]}" "$network"
# Host requests reach the app from this network's gateway. Trust only that
# address for X-Forwarded-Proto, as production trusts its TLS proxy.
network_gateway="$(docker network inspect --format '{{range .IPAM.Config}}{{.Gateway}}{{end}}' "$network")"
[[ "$network_gateway" =~ ^[0-9]{1,3}(\.[0-9]{1,3}){3}$ ]] || { echo "smoke network has no IPv4 gateway" >&2; exit 1; }
# Loopback-only random host port: the browser tests clean up the users they
# register directly in this disposable database.
postgres_password="$(python3 -c 'import secrets; print(secrets.token_hex(24))')"
POSTGRES_PASSWORD="$postgres_password" docker run -d --name "$postgres" --network "$network" "${owned_label_args[@]}" \
  --network-alias ci-postgres \
  -p 127.0.0.1::5432 \
  --tmpfs /var/lib/postgresql:rw,size=1073741824 \
  -e POSTGRES_PASSWORD \
  -e POSTGRES_DB=app \
  "$POSTGRES_IMAGE"
docker run -d --name "$redis" --network "$network" "${owned_label_args[@]}" \
  --network-alias ci-redis \
  --tmpfs /data:rw,size=67108864 "$REDIS_IMAGE" \
  redis-server --save "" --appendonly no

wait_until_stable 90 postgres_ready
wait_until_stable 60 redis_ready

# Data Protection keys are always certificate-protected outside Development.
mkdir -p "${RUNNER_TEMP:-${TMPDIR:-/tmp}}"
dp_cert_dir="$(mktemp -d "${RUNNER_TEMP:-${TMPDIR:-/tmp}}/dp-cert-${session}-XXXXXXXX")"
chmod 0755 "$dp_cert_dir"
openssl req -x509 -newkey rsa:2048 -nodes -sha256 -days 2 -subj "/CN=ci-data-protection" \
  -keyout "$dp_cert_dir/key.pem" -out "$dp_cert_dir/cert.pem" >/dev/null 2>&1
dp_cert_password="$(openssl rand -hex 32)"
openssl pkcs12 -export -out "$dp_cert_dir/dp.pfx" \
  -inkey "$dp_cert_dir/key.pem" -in "$dp_cert_dir/cert.pem" -passout "pass:${dp_cert_password}" >/dev/null 2>&1
rm -f -- "$dp_cert_dir/key.pem"
chmod 0444 "$dp_cert_dir/dp.pfx"

# The HTTP and browser checks share one client IP. Raise the limits only for
# this disposable container so the smoke does not trip production budgets.
docker run -d --name "$web" --network "$network" "${owned_label_args[@]}" \
  -p 127.0.0.1::8080 \
  -v "$dp_cert_dir:/run/dp-cert:ro" \
  -e DataProtection__KeyEncryptionCertificatePath=/run/dp-cert/dp.pfx \
  -e "DataProtection__KeyEncryptionCertificatePassword=${dp_cert_password}" \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e ASPNETCORE_HTTP_PORTS=8080 \
  -e "ForwardedHeaders__KnownProxies__0=${network_gateway}" \
  -e "APP_VERSION=${APP_VERSION}" \
  -e "ConnectionStrings__DefaultConnection=Host=ci-postgres;Port=5432;Database=app;Username=postgres;Password=${postgres_password};GSS Encryption Mode=Disable" \
  -e "Redis__Configuration=ci-redis:6379,abortConnect=false" \
  -e Redis__AllowMissing=false \
  -e Database__RunMigrationsAtStartup=true \
  -e RateLimiting__Global__PermitLimit=10000 \
  -e RateLimiting__Api__PermitLimit=1000 \
  -e Observability__OpenTelemetry__Enabled=false \
  "$IMAGE_REF"

host_port="$(docker port "$web" 8080/tcp | sed -n 's/.*:\([0-9][0-9]*\)$/\1/p' | head -n 1)"
[[ -n "$host_port" ]] || { echo "web container did not publish port 8080" >&2; exit 1; }
base_url="http://127.0.0.1:${host_port}"

ready_deadline=$((SECONDS + 180))
until [[ "$(curl --silent --output /dev/null --write-out '%{http_code}' --max-time 5 "${base_url}/health/ready" || true)" == "200" ]]; do
  if (( SECONDS >= ready_deadline )); then
    echo "app did not report /health/ready within 180 seconds" >&2
    exit 1
  fi
  sleep 2
done
echo "app ready at ${base_url}"

if ! docker exec "$web" sh -c 'if command -v node >/dev/null 2>&1; then exit 1; fi; test ! -e /usr/local/bin/node && test ! -e /usr/bin/node'; then
  echo "final application image must not contain a Node.js runtime" >&2
  exit 1
fi

# Production runs behind a TLS-terminating proxy; send the same forwarded
# protocol so HTTPS redirection does not apply to these plain-HTTP checks.
forwarded_proto=(--header "X-Forwarded-Proto: https")

# The Blazor image is the only supported frontend. Validate its HTTP surface and run one browser smoke.
  home_html="$(curl --fail --silent --show-error --max-time 30 "${forwarded_proto[@]}" "${base_url}/")"
  grep -Fqi '<!DOCTYPE html>' <<< "$home_html" || { echo "home page did not return HTML" >&2; exit 1; }
  grep -Fq '_framework/blazor.web' <<< "$home_html" || { echo "home page does not load the Blazor web script" >&2; exit 1; }
  if docker exec "$web" sh -c 'test ! -e /app/wwwroot/index.html && test ! -e /app/wwwroot/assets'; then
    :
  else
    echo "Blazor image contains unexpected root static assets" >&2
    exit 1
  fi

  api_headers="$(curl --silent --show-error --max-time 30 "${forwarded_proto[@]}" --dump-header - --output /dev/null "${base_url}/api/books")"
  api_status="$(head -n 1 <<< "$api_headers" | awk '{print $2}')"
  [[ "$api_status" == "401" ]] || { echo "anonymous /api/books returned ${api_status:-no status}, expected 401" >&2; exit 1; }
  if grep -qi '^location:' <<< "$api_headers"; then
    echo "anonymous /api/books redirected instead of returning 401" >&2
    exit 1
  fi
  cd "$REPO_ROOT"
  pwsh -NoProfile -File "$REPO_ROOT/Scripts/Test-DeployedSite.ps1" \
    -FrontendProfile "$FRONTEND_PROFILE" -AcceptanceProfile "$FRONTEND_ACCEPTANCE_PROFILE" \
    -ForwardedProto https -BaseUrl "$base_url"
  echo "Blazor HTTP smoke passed"

case "${RUN_E2E:-0}" in
  1)
    cd "$REPO_ROOT"
    pwsh "$REPO_ROOT/RitzauDemo.Test/bin/Release/net10.0/playwright.ps1" install chromium
    browser_results_dir="${RUNNER_TEMP:-${TMPDIR:-/tmp}}/test-results/${expected_run_id}-${expected_run_attempt}/${GITHUB_JOB:-validate}/docker-smoke/BlazorAuto"
    mkdir -p "$browser_results_dir"
    E2E_BASE_URL="$base_url" E2E_HEADLESS=1 E2E_SLOW_MO_MS=0 \
      dotnet test "$REPO_ROOT/RitzauDemo.Test/RitzauDemo.Test.csproj" \
        --configuration Release --no-build --no-restore \
        --filter "$BROWSER_SMOKE_FILTER" \
        --logger "trx;LogFileName=blazor-browser-smoke.trx" \
        --results-directory "$browser_results_dir"
    python3 "$REPO_ROOT/Scripts/CI/validate_dotnet_test_results.py" \
      "$browser_results_dir/blazor-browser-smoke.trx" \
      --expected-skipped 0 --suite "Blazor packaged browser smoke"
    echo "Blazor packaged browser smoke passed"
    ;;
  0|"")
    echo "Skipping optional packaged Blazor render/hydration smoke; set RUN_E2E=1 to enable."
    ;;
  *)
    echo "RUN_E2E must be 0 or 1." >&2
    exit 2
    ;;
esac
echo "Blazor HTTP smoke passed"
