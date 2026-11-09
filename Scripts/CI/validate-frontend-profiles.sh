#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -P "$SCRIPT_DIR/../.." && pwd)"

if [[ $# -ne 1 || -z "$1" ]]; then
  echo "Usage: validate-frontend-profiles.sh <results-directory>" >&2
  exit 2
fi

results_root="$1"
mkdir -p "$results_root"
profile="$(python3 "$REPO_ROOT/Scripts/Frontend/resolve_frontend_profile.py")"
acceptance_profile="$(python3 "$REPO_ROOT/Scripts/Frontend/resolve_frontend_profile.py" --acceptance-profile)"
results_directory="$results_root/$profile"
mkdir -p "$results_directory"

echo "Restore, build and test the $profile/$acceptance_profile demo"
dotnet restore "$REPO_ROOT/RitzauDemo.sln" "-p:FrontendProfile=$profile"
dotnet build "$REPO_ROOT/RitzauDemo.sln" \
  --configuration Release --no-restore "-p:FrontendProfile=$profile"
dotnet test "$REPO_ROOT/RitzauDemo.Test/RitzauDemo.Test.csproj" \
  --configuration Release --no-build --no-restore "-p:FrontendProfile=$profile" \
  --logger 'trx;LogFileName=books-crud.trx' \
  --results-directory "$results_directory"

python3 "$REPO_ROOT/Scripts/CI/validate_dotnet_test_results.py" \
  "$results_directory/books-crud.trx" \
  --suite "Books CRUD $profile unit/integration and optional browser smoke"
