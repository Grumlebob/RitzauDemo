# Test

Use the full local gate for code, configuration, and other non-Markdown changes. Product behavior coverage is five focused Books CRUD unit cases and one PostgreSQL/Redis-backed create/list/read/update/delete integration test. At most one packaged Blazor render/hydration smoke is optional and off by default; it does not exercise browser CRUD. CI, release, single-node deployment, backup/recovery, and host-safety checks remain operational gates. Grafana/observability suites are out of scope while deployed observability is disabled. See [Browser testing](Testing/Browser.md) and [Infrastructure testing](Testing/Infrastructure.md) for the optional smoke and CRUD integration fixture.

## Normal run

Run `dotnet test` for the single `BlazorAuto` profile. The optional packaged render/hydration smoke stays skipped unless `RUN_E2E=1`. The retained validation script builds and tests the one product suite, then validates its TRX result. It rejects failures, empty discovery and unexplained skips; it does not pin a test count:

```bash
bash Scripts/CI/validate-frontend-profiles.sh /tmp/frontend-profile-results
```

Run this before changing product code. It needs Docker. CI builds and smoke-tests one Blazor image; the current workflow stages are in [ci.yml](../.github/workflows/ci.yml). Do not run build and test concurrently in the same checkout.

## GitHub Actions reports

The workflow policy requires an explicit `self-hosted` runner label for application and maintenance jobs; a runner group name alone does not establish runner type. Only public-repository CodeQL scans can use GitHub-hosted runners; they skip private repositories and receive no secrets or node access. Application CI never uses hosted runners. Each `actions/setup-node` step must set `package-manager-cache: false`. Each CodeQL initialization step must set `env.CODEQL_OVERLAY_DATABASE_MODE: none` to preserve full scans without overlay caches. The audit also follows local reusable workflows and composite actions.

Publication, deployment and registry retention share a non-cancelling `queue: max` group, which retains up to 100 pending requests. The policy checks every workflow or job using the publisher's group, including case-insensitive matches. The local gate uses the pinned queue-aware actionlint runner below.

CI stores TRX results and available browser screenshots and traces on the self-hosted runner. Packaged smoke TRX files use the same terminal job directory. Routine reports do not use GitHub artifact storage. Each native terminal job summary reports test totals, the runner name, a retrieval command, and observed report-store size. Run that command on the same runner after checking out the reported commit. Reports expire after 3 days for successful jobs and 7 days for failed jobs; pins and active-reader locks protect selected reports. The LocalSingleNode maintenance workflow reports store usage and prunes expired reports under the shared deployment lock.

Every CI job uses the repository's `CI_RUNNER_LABEL` and verifies `CI_RUNNER_HOST` before running. Heavy validation and publication also use the shared native admission guard. The stable `build-test-push` result gate rejects skipped or failed validation and skipped or failed publication on `main`.

Resource-heavy validation and publication steps use `with-runner-build-admission.sh` and the existing shared node browser lock. Keep one wrapper around each heavy step or full shell block, including owned cleanup. Do not nest the lock inside an admitted block; its contention test runs outside admission.

## Local Gate

For ordinary Markdown-only changes, follow the documentation exception in [AGENTS.md](../AGENTS.md#changes-and-checks). Mixed PRs use the full gate below.

Run every command successfully before pushing non-Markdown changes. Use Linux or WSL with the pinned .NET SDK, Python 3, Docker, and ShellCheck available in that environment. The virtual-environment paths below assume Linux.

```bash
git diff --check
dotnet restore RitzauDemo.sln
python3 -m unittest discover -s Scripts/Quality/tests -v
python3 -m unittest discover -s Scripts/CI/tests -v
python3 Scripts/CI/workflow_policy.py --repo-root .
dotnet format RitzauDemo.sln --verify-no-changes --verbosity minimal --no-restore
bash Scripts/CI/validate-frontend-profiles.sh /tmp/frontend-profile-results # needs Docker
bash Scripts/CI/tests/test_check_runner_capacity.sh
bash Deployment/Common/Scripts/validate-common-release.sh
python3 -m unittest discover -s Deployment/LocalSingleNode/Scripts/Tests -v
bash Deployment/Common/Scripts/audit-deployment.sh
python3 -m unittest discover -s Deployment/Common/Scripts/Tests -v
python3 -m venv /tmp/tpl-venv && /tmp/tpl-venv/bin/pip install --constraint Deployment/Common/ci-python-constraints.txt jinja2 yamllint
PATH="/tmp/tpl-venv/bin:$PATH" bash Deployment/LocalSingleNode/Scripts/validate-rendered-templates.sh
PATH="/tmp/tpl-venv/bin:$PATH" yamllint .github Deployment docker-compose.yml .yamllint.yml
find Deployment/LocalSingleNode Deployment/Common/Scripts Scripts/CI -type f -name '*.sh' -print0 | xargs -0 shellcheck --severity=warning
bash Scripts/CI/run-actionlint.sh
```

If `RitzauDemo.Client/Styles` or Razor classes change, run `npm ci && npm run css:build` from the client project and commit generated `RitzauDemo/wwwroot/tailwind.css` when it changes. The `discover` commands run the same Python suites as CI's `validate` job; the shell suites are listed there. Optional static/source audits are in [Quality tools](Quality/QualityTools.md) and [SonarQube](Quality/SonarQubeAnalysis.md); they are not merge gates.
