# RitzauDemo implementation decisions and dependency map

Read this with `../RitzauDemo-Blazor-SingleNode-Bootstrap.md`. This revised packet incorporates the user's 2026-11-08 approval for CarbonCalculator native-CI admission, alongside GrumPlates admission and RitzauDemo product work. Its original dependency map uses source SHA `ee9af0b985f86c9568cbf077aeb20336841446de`; review drift before applying it. Other projects and all unrelated CarbonCalculator behavior remain excluded.

## 1. Completed preparation and how to repeat it

The source remains clean. A read-only dependency inventory contains 85 React consumers outside the client directory, 21 removed-target consumers outside those targets, 15 image-label consumers, 9 bootstrap/admission files, and 3 migration files. These categories overlap; they are search results, not automatic deletion lists.

`RitzauDemo-SourceInventory.json` records exact paths and normalized source hashes. The generator reads tracked files only. It never edits the source and refuses an output inside the source repository. Run the following from WSL, replacing paths with owned source/support directories:

```bash
python3 -B "$support/RitzauDemo-SourceInventory.py" "$source" \
  --verify "$support/RitzauDemo-SourceInventory.json"
bash "$support/RitzauDemo-Preflight.sh" "$source"
```

Define `support` and `source` as absolute paths first. The preflight sets `DOTNET_ROOT` to `$HOME/.dotnet` when unset, adds it to PATH and disables Python bytecode output. This matters: a non-login WSL command initially failed because `dotnet` was absent from PATH. The corrected preflight passed all 69 tests: 4 setup CLI, 46 single-node fixture tests, 3 settings fixtures, and 16 frontend-profile tests, using SDK 10.0.303.

These are offline fixture tests. Their mocked maintenance/removal calls do not change the host. They do not prove the full local gate, live backup freshness, peak build memory or a deployable release.

Keep the original inventory as historical evidence. A changed current main, including the approved admission PRs themselves, will no longer match its hashes. Review the actual diff and generate any new inventory to a distinct owned output path outside the source; do not reset a repository to the baseline or overwrite the historical JSON to hide drift. The CarbonCalculator admission coverage table is separate from this template React/target dependency inventory. Do not rerun the full application gate merely for these Markdown planning edits; implementation uses each repository's current required checks.

## 2. Resolve the bootstrap dependency cycle once

**Resume checkpoint (2026-11-08): steps 1–6 below are already complete.** RitzauDemo main is seeded at `5931a4a872de39db8a85ac886bcdf40097bc99db`, full original local gate passed, and its separate native clone exists. Revalidate current state; never repeat the initial import or use it to justify another direct-main push. The workspace's updated main plan is authoritative over the seed-time copies. The user now approves CarbonCalculator native CI admission alongside GrumPlates/RitzauDemo; all other CarbonCalculator behavior stays in its existing scope.

The original draft's order was invalid: `setup_status.status()` requires a clean checkout on branch `main` at `origin/main` before it offers the operator bootstrap command. An identity branch with a new slug cannot pass that check. Bootstrapping the untouched template would select `books`, colliding with the existing installation.

Use a **minimal bootstrap seed as part of the first import**, before any target main exists:

1. Verify RitzauDemo is empty and disable its Actions before pushing.
2. Clone the exact template baseline locally, configure origin/upstream and the `gh` default, and retain its history.
3. On local main, create one minimal bootstrap seed commit containing only product app slug/resource defaults, shared release identity, frontend selection, the sanitized plan/support packet, and any settings example changes required to validate those inputs. Do not remove React or targets, rename projects, or redesign CI in this commit.
4. Run the full existing local gate against that commit. Source names and all profiles/targets are still intact. Fix any gate failure before the import.
5. Push local main once, containing template history and that bootstrap seed, with Actions disabled. Record template SHA, seed SHA, and local gate results. This is the one-time initial import exception needed before a repository runner exists; it does not waive exact-head CI for any later PR or permit later direct-main pushes.
6. Clone RitzauDemo's main separately on the native node. Verify clean main equals origin/main and the selected app is `ritzaudemo`. Prepare tools/authentication and candidate collision evidence. Do not run the state machine's deployment action automatically.
7. Complete the canonical operator bootstrap handoff. Verify runner registration and private machine facts; set variables. Keep new heavy CI disabled until admission safety is proven.
8. Implement the narrowly scoped GrumPlates and CarbonCalculator native admission integrations through separate normal PRs, with each repository's full current local gate and exact-head/main verification. Preserve GrumPlates profiles/targets and CarbonCalculator routing, reports and product behavior. Follow section 3 for the product CI rollout.
9. Enable target Actions only after admission and variables pass. Put remaining product identity/branding/readme changes in the first normal PR. Require exact-head `build-test-push`, merge, and require a successful **main push** validation/publication. That creates the push provenance demanded by initial setup; `workflow_dispatch` is not a substitute for the initial setup checkpoint.
10. All subsequent cleanup, target reduction and rename work uses normal PRs. Never treat the initial import exception as permission to bypass failed checks.

If enforced GitHub policy prevents this initial import, stop and report the exact policy. Do not weaken protection or fabricate checks. If the target is no longer empty, this initial-import route no longer applies; reconcile existing history first.

### Stable product resource defaults

Use these as generic RitzauDemo defaults in its own `Deployment/LocalSingleNode/inventory/group_vars/all.yml`, not as copied facts from another app. The product defaults are non-secret operational configuration, as in the template's own inventory. Actual hostname/IP/login, observed neighbor inventory, keys, credentials and any installation-specific overrides remain private.

```yaml
---
app_name: ritzaudemo
app_port: 28080
postgres_port: 25432
redis_port: 26379
deploy_root: /opt/ritzaudemo
docker_subnet: 172.30.40.0/24
lan_http_port: 28081
lan_hostnames: []
backup_root: /opt/ritzaudemo-backups
backup_keep_days: 7
observability_enabled: false
accounts_registration_enabled: false
accounts_admin_seed_enabled: false
accounts_admin_seed_email: ""
accounts_public_origin: ""
accounts_email_host: ""
accounts_email_port: 587
accounts_email_security: StartTls
accounts_email_from: ""
```

A native snapshot found none of the four candidate ports listening, no overlap between the candidate subnet and inspected runtime networks, and no existing candidate app/backup/secret directory or app ownership marker. No resources were reserved. Re-run the **canonical** collision check near setup, including Compose files, Caddy claims, runtime networks and ancestry checks. Listener/network inspection alone is not that gate. The runner directory still requires the installer's privileged ownership check.

Do not invent an ignored primary settings route: Python's `LOCALSINGLENODE_SETTINGS_FILE` override alone does not update Ansible `vars_files`, the root subprocess environment, or the primary CD resolver. The stock primary route reads the tracked product inventory; the private additional-instance route is for another profile of the same repository and must not be used here. If a future requirement demands private overrides, implement and test end-to-end propagation as a separate change.

## 3. Exact CI admission design

Reuse `Deployment/Common/Scripts/with-node-browser-lock.sh` and its existing shared lock file `/tmp/localsinglenode-ci-browser.lock`. This already has bounded `flock` acquisition, automatic release, a command descriptor close, and failure propagation. Do not add a competing lock or hold a background `flock` process across Actions steps.

Expand admission coverage in GrumPlates, RitzauDemo and CarbonCalculator's native route. Preserve current runner registrations, hosts and routing, including CarbonCalculator's desktop preference/native fallback. Use existing host/profile configuration to scope the wrapper to the shared native host; desktop/cluster execution must retain its behavior. The portable code must not hard-code a private host name. Apply exactly one wrapper to each resource-heavy step or full multi-command block. Use the existing wrapper directly for individual commands:

```bash
bash Deployment/Common/Scripts/with-node-browser-lock.sh dotnet restore GrumPlates.sln
```

For a full shell block:

```bash
bash Deployment/Common/Scripts/with-node-browser-lock.sh bash -euo pipefail <<'ADMITTED'
# Keep this step's actual commands and environment behavior here.
ADMITTED
```

Use existing project names before the rename phase and product names afterward. Keep Actions `working-directory` behavior correct for npm steps, with an absolute helper path when needed. Updates through `GITHUB_ENV`/`GITHUB_OUTPUT` remain file writes available to later steps.

Known gaps in `ci.yml` at the researched SHA:

- Validation: restore, format, Docker image pulls, Testcontainers lifecycle proof, Blazor npm install/audit/CSS, React npm install/audit/contract/lint/unit/build, and the full image-build/smoke loop. Existing profile validation and React browser steps already have wrappers.
- Publication: restore, tool restore, migration bundle creation, browser test binary build, image build and additional frontend build/publication. Existing packaged browser smoke has a wrapper. Keep reservation checks, provenance and cleanup behavior intact.
- Manual gates: wrap the resource-heavy gate execution once; do not recursively wrap its already wrapped subcommands using the same lock.

**Never nest the same lock.** If an entire image-build/smoke block is wrapped, remove the inner browser-lock call from that block while leaving the actual smoke command. The additional-frontend helper also contains a browser wrapper; remove or relocate it consistently when the caller owns admission. Test the lock's actual scope instead of adding an environment flag that silently skips acquisition for arbitrary callers.

Do not hold this lock around an entire Actions job containing the lock's own contention tests or around scripts which acquire it internally. Do not hold it while dispatching/waiting for another workflow. No lock order may form a cycle with the existing deployment lock; inventory the nested reservation/smoke calls first.

The existing helper's default wait is 3,600 seconds; job timeout includes waiting time. Size queue/job limits from observed duration. A timeout must fail clearly, not mark work successful or trigger an automatic rerun storm.

### Approved CarbonCalculator integration

CarbonCalculator main was inspected at `0d255392b99eb635030f9c90a03de8a6d37d227c`. Native CI is eligible: `CI_RUNNER_ROUTING_ENABLED=true`, an existing native label/host is configured, `auto` can fall back to native, and non-main dispatch supports `native-canary`. Its current browser helper protects selected React/browser/smoke commands, but restore, lifecycle tests, dependency install and image/publication work have gaps. Revalidate these facts against current main before editing.

The user approved fixing these gaps through native admission only. Inspect `.github/workflows/ci.yml`, `.github/workflows/select-ci-runner.yml`, `Scripts/CI/select_ci_runner.py`, `Scripts/CI/runner_profiles.py`, the existing lock helper and related tests. Preserve selector semantics and route variables. Preserve `CARBON_LOCK_TIMING_DIRECTORY` timing output and publisher/deployment lock contracts. Add coverage assertions proving every eligible native heavy path participates, and prove desktop/cluster behavior remains intact. Do not rewrite the helper from the template over CarbonCalculator's extensions.

Follow CarbonCalculator `AGENTS.md`, task claims and `Scripts/Agents/DeliveryTask.ps1` for its isolated checkout. Use `pwsh -File Scripts/Testing/Validate.ps1 -Mode full` in the prepared Linux environment and inspect its fresh source-bound report; require successful full validation, then `build-test-push` on the exact PR head and integrated main. Do not deploy CarbonCalculator, alter its schedules, registrations, routes or product code. For GrumPlates, retain the complete `docs/Test.md` gate with both profiles/all targets. Product deletions never flow upstream.

### Resource lifetime, lock identity and evidence

- Verify every native runner account opens the same file/inode; inspect access permissions without widening unrelated host access. Existing deployment/browser lock paths and ownership labels remain interoperable.
- Dependency installers, Docker pulls/builds, tests and their child containers must enter admission before consuming heavy resources. Keep a full resource lifetime inside one admitted shell block through cleanup. Separate blocks may interleave only after releasing their heavy resources. Do not create a detached holder to span Actions steps.
- Preserve failure and cancellation exit codes and report/cleanup behavior. Exercise contention, timeout, failure and interruption with lightweight fixtures before a real admitted build. Confirm cancellation leaves no active owned build/test work; a released lock alone is insufficient if daemon work survives. Reconcile owned orphans before resuming; never delete a held lock.
- Inventory nested deployment/publication reservation calls and choose one acyclic lock order across all three repositories. Do not hold admission while waiting for a workflow that must acquire it, and run lock contention tests outside that lock.
- A runner systemd memory limit alone does not prove limits for containers created by the Docker daemon. Measure aggregate host use and verify actual container/build limits. Keep the serving applications running during the queue test.

### Phase 2A static coverage table — branch review, 2026-11-08

This table reviews the prepared branches, not integrated workflows or live host behavior. GitHub `workflow_dispatch` enters the same CI jobs shown below. The repository local gates run on the controller; they are not separate commands on the shared native node.

| Repository and route | Validation admission | Publication admission | Cleanup, routing, and current proof |
| --- | --- | --- | --- |
| GrumPlates | `validate` wraps Python environment and package setup, template render and YAML lint dependencies, .NET restore/format/package audits, Docker preflight, both frontend profiles, Testcontainers lifecycle proof, Blazor/Tailwind and React npm install/audit/contract/lint/unit/build, Chromium regression, and image build/smoke. | Main-only `publish-main` wraps .NET/tool restore, migration bundle, browser-test binaries, image build/smoke, tag/push, and additional-frontend publication. | Smoke and image cleanup run through admission. Existing deployment-lock reservation, release receipts, provenance, capacity report, and report storage retain their own paths. `localcluster` labels bypass the shared lock; `localsinglenode` labels acquire it. Policy tests enumerate the named heavy steps. |
| RitzauDemo | Native `validate` wraps the same two-profile template gate and heavy dependency, restore, format, Testcontainers, browser, and image paths retained before React removal. | Main-only `publish-main` wraps restore/tool restore, migration bundle, browser-test binaries, image build/smoke, tag/push, and additional-frontend publication. | Cleanup steps use admission. Validation, publication, and result gate use the temporary `CI_ADMISSION_BOOTSTRAP_RUNNER_LABEL`; `CI_RUNNER_LABEL` stays quarantined. Non-native Dependabot notification remains on the canonical label. Actions are disabled, so this is static review only. |
| CarbonCalculator | Selector output supplies `CI_ADMISSION_POOL`. Native validation wraps Python/Caddy setup and lint dependencies, .NET restore/format/package audits, Docker preflight, host/test builds, React validation and browser tests, Testcontainers lifecycle proof, image build/smoke, and owned cleanup. Desktop route bypasses the native lock. | Main-only publication uses its own selection output and wraps .NET/tool restore, migration bundle, browser-test binaries, pinned npm/browser-tool installs, image build/smoke, tag/push, and image/resource cleanup. Desktop route bypasses the native lock. | Routing selectors, route variables, `CARBON_LOCK_TIMING_DIRECTORY`, publisher/deployment reservations, receipts, reports, and desktop fallback remain unchanged. Routing and workflow tests enumerate heavy steps and verify native versus desktop helper behavior. |

Static workflow review found no nested call to `with-node-browser-lock.sh` inside an admitted block. Registry deletion reservation steps use the deployment lock separately before/after admitted publication work; no step holds both locks or waits for another workflow while admitted. Lightweight runner checks, result gates, receipts, report storage, and cleanup metadata remain outside admission where their existing contracts apply. `actions/setup-dotnet` and `actions/setup-node` toolchain steps also remain outside the shell wrapper; measure their first-install/cache behavior and disk impact during native acceptance before deciding whether they need serialization. Do not count this table as proof of same-inode access, cross-repository contention, cancellation cleanup, or loaded capacity.

Current branch evidence at the 2026-11-08 checkpoint: GrumPlates `b17c20655dfb968133db7bb704106e2fc69a7959` and CarbonCalculator `1f29e82f9e0a152f396dee8118f63308d1fc3674` passed their recorded full local gates. RitzauDemo safety code passed the full local gate at `3c087a5f44533a31faa4d1d8cd13b48968c5e7f8`; docs-sync SHA `23886457310bacfa8797221a054db85c78288aef` then passed every `repo-gate` step and the complete `docs/Test.md` command set. At that validation checkpoint, local SHA `2388645` was one commit ahead of draft PR #5 remote head `4654ef9ba256f1648871faa6adc49d47e9c02c1d`. Product Actions were disabled and exact-head checks empty. Four Dependabot PRs remained open and excluded. No admission PR was integrated. CarbonCalculator run `42107307607` remained queued with no jobs; preserve it and do not cancel it or start native PR CI until it is reconciled or completes. Shared-lock account access and two-process real-file contention were checked later; the three-context queue test remains pending.

Product PR checkpoint, 2026-11-08: the user-approved lean test-scope plan was pushed to draft PR #5 in docs-only commit `b0bee356ca9fc1c97313ac4ed1315b4ae9ab7d8a`. The remote branch and PR API matched this commit at verification. Actions remain disabled and no product CI started. Historical admission, runner, and stale-run details remain recorded in the dated checkpoints below.

Live runner API recheck, 2026-11-08: GrumPlates and CarbonCalculator each have an online, idle LocalSingleNode runner; their separate LocalCluster and desktop runners remain registered. RitzauDemo has no registered runner. The private machine inventory maps the two existing LocalSingleNode producers to the same host, so the planned shared `/tmp/localsinglenode-ci-browser.lock` is on a common filesystem. No in-progress runs exist in any of the three repositories. This verifies runner placement and availability; service identity and lock ownership were checked separately below. Actual lock access and two-process contention were checked separately; multi-repository queue behavior remains unproven.

Read-only native check, 2026-11-08: the verified SSH target matched its expected native identity. Both existing LocalSingleNode runner services use the `deploy` account. The shared lock file is owned by `deploy` and owner-writable. No files, services, registrations, or permissions were changed. This supports access for the two current runners; RitzauDemo has no service yet. Live contention was checked separately below; cancellation cleanup and capacity remain unproven.

Live lock check, 2026-11-08: under the existing `deploy` account, one process acquired the production browser lock with nonblocking `flock`; a second process failed to acquire it while held; after the holder exited, a new process acquired it. The lock path retained the same inode, owner, and mode afterward. This proves real-file mutual exclusion and release for two processes on the host. It did not invoke either repository workflow/helper, test cancellation cleanup, or include a RitzauDemo runner.

Latest product PR checkpoint, 2026-11-08: docs-only SHA `59e0c235e5f3941513147df85681edb561563b7d` is pushed to draft PR #5, whose head matches and remains unchecked because Actions are disabled. The exact-head `repo-gate` and complete `docs/Test.md` command set pass: BlazorAuto 197 non-account passes / 15 documented skips and 149 account passes; React 189 non-account passes / 15 documented skips and 135 account passes; Quality 99, CI 107, Common 278, LocalCluster 63 and LocalSingleNode 217; workflow policy passes. The first local attempt lacked `DOTNET_ROOT`/PATH and failed .NET stages; after correcting the task launcher with the documented SDK path, gate Docker environment and host override, all stages passed. No native PR workflow ran. CarbonCalculator run `42107307607` and check suite `114100632846` remain queued despite attempt 1's `startup_failure`; both report no jobs/check runs, and the dispatch branch is absent. Preserve the run and defer native PR CI. The prepared admission branches remain unintegrated; workflow-level queue/cancellation and capacity evidence remain outstanding.

### Staged rollout, including the first RitzauDemo PR

1. Prepare both existing-repository admission changes and the coverage table locally. Reserve native build capacity through the current shared task coordination mechanism; inspect current claims and live jobs. Integrate one PR at a time. Do not cancel other sessions' jobs or use one idle snapshot as permanent serialization proof. During migration, wait for legacy unadmitted work to finish and keep new RitzauDemo heavy jobs disabled. If a legacy job starts, defer the new native canary until it finishes. Record this bounded migration window separately from the final steady-state guarantee.
2. Run each repository's full local gate on separately reserved controller capacity. Check the managed desktop CI environment and live jobs identified in private machine guidance. Do not overlap local gates or use another task's shared clone. Then obtain the exact PR-head CI result, merge, and verify main. A CarbonCalculator validation selected for desktop can retain that route; verify the native wrapper path independently with lightweight native contention and a subsequent admitted native workload.
3. Prepare the first RitzauDemo admission/identity PR while Actions remain disabled. Include the sanitized revised plan/packet and complete current local gate. Native operator bootstrap and collision review remain mandatory before installing its runner; do not bootstrap a feature branch or silently execute root commands from the controller.
4. Prevent legacy seed or stale Dependabot workflow code from reaching the first live product runner. A concrete controlled design to implement and test is to keep `CI_RUNNER_LABEL` pointed at an unregistered quarantine label while the first PR's workflow uses a separate explicit `CI_ADMISSION_BOOTSTRAP_RUNNER_LABEL` for the verified new registration. Audit every workflow/fallback label, including manual and scheduled heavy work; no legacy path may use the bootstrap label. Use that variable consistently in the first PR's workflow/policy/tests and preserve host verification. This is a proposed temporary workflow change, not an existing installer feature. Canonical setup status may remain incomplete until its usual label is restored; never bypass its checks or auto-run its next-step action to work around that. Never weaken required checks or fabricate results. Enable Actions only when the guard is ready and both existing native producers are admitted.
5. Trigger only the prepared first PR's CI and require its exact-head `build-test-push`; merge normally and verify main validation/publication under the guarded workflow. Resolve stale Dependabot branches through the target repository's normal workflow: rebase/recreate against guarded main and verify their workflow content, or close superseded bot PRs with recorded reason. Do not merge dependency upgrades as bootstrap work. Old queued jobs must stay unschedulable or be explicitly reconciled as owned/superseded target jobs; do not release them onto a live runner.
6. In a separate normal product PR, return the admitted workflow to canonical `CI_RUNNER_LABEL`, set that variable to the verified registration only after no unadmitted legacy branch/job can use it, and retire the bootstrap variable/calls. Obtain exact-head/main checks again. If the canonical tooling offers a simpler equally enforced branch-safe bootstrap, use it and record the proof; do not substitute an uncontrolled enable-and-watch window.
7. Verify all three native producers and manual gate entry points now cooperate on the same lock; desktop/cluster routing and existing app health remain correct. Complete the full native validation/publication capacity trial before calling admission finished. A single admitted build that cannot fit is a measured capacity issue; do not silently change topology or reduce required checks.

### Runtime coexistence acceptance

RitzauDemo receives an isolated stack and measured CPU/memory/PID budgets through supported settings/rendering. Verify effective Docker limits and Redis allocation; avoid copying idle memory as a production hard cap. Existing applications' runtime limits are not changed under the CarbonCalculator CI exception. Record host reserve, peak workload and application behavior rather than inventing fixed budgets here. Docker's default is no resource constraints: [official resource documentation](https://docs.docker.com/engine/containers/resource_constraints/).

Before rollout, capture health, latency, memory, available RAM, swap-in/out, pressure, OOM and restart baselines. Define the representative request workload, latency/error acceptance budget, canonical capacity floors and required host reserve before the trial; record them with their rationale and do not loosen them retrospectively to pass. After LAN deployment, run that workload while one full native build executes and another lightweight contender waits. Require one heavy owner at a time, all sites healthy with no new OOM/restart failures, the recorded latency/error and capacity budgets satisfied, and verified app-specific backup/isolated recovery. Save numeric observations and chosen limits privately, with sanitized conclusions in the plan. Do not run uncontrolled concurrent heavy builds or require deployed observability infrastructure for this measurement.

## 4. React deletion map and hidden dependencies

Use the complete inventory's `react_consumers_outside_client` list as the review queue. Delete React-only paths only after adapting mixed consumers.

| Area | Exact instruction |
| --- | --- |
| Client | Remove `GrumPlates.React/`; keep `GrumPlates.Client/` and npm/Tailwind needed for Blazor styles. Remove axe/Lighthouse and browser-test tooling unless a retained runtime/build dependency requires it. |
| Server composition | Remove `FrontendComposition.React.cs`, `ReactFrontendHosting.cs`, `ReactFrontendStaticRoot.cs` and `Frontend/React/`. Keep Blazor composition and `Frontend/OpenApi/`. |
| Server csproj | Remove React compile/content/Razor-exclusion conditions. Keep Blazor packages/references, the publish-output/source guard and source `wwwroot`. Rename `_ReactPublishWebRoot`/`_ReactSourceWebRoot` to neutral names when simplifying. |
| Profile contract | Accept only `BlazorAuto` in resolver, MSBuild target, immutable manifest/profile validators and verification helpers. Preserve `BlazorAutoV1`. Unknown/empty/environment-derived selection must still fail. |
| CI lifecycle test | Remove the inherited RitzauDemo `Testcontainers lifecycle cleanup` project test. Keep shared runner admission, cancellation, cleanup, and release checks in their approved repositories. |
| Node pin | CI currently parses `FROM node:26.13.0-bookworm-slim` in `GrumPlates/Dockerfile`. Add tracked `.node-version` containing `26.13.0` and point `actions/setup-node` at it with `package-manager-cache: false` before deleting the React stage. Retain npm 12.2.0 setup for surviving CSS tooling. |
| Packaged smoke | `Deployment/LocalCluster/Scripts/ci-docker-smoke.sh` also parses that Node stage unconditionally for the React SMTP fixture. Remove React-only SMTP/browser resources and initialization, then remove that Node Dockerfile parse. Keep one small packaged Blazor smoke for startup/rendering. Do not retain multiple browser suites; if one browser-driven CRUD path is chosen, make it the single E2E. |
| App name in smoke | Smoke defaults through `read-deploy-setting.sh` from LocalCluster. Supply `APP_NAME` from the product LocalSingleNode parser; remove the obsolete fallback when moving the smoke helper. |
| Tests | Remove React-only tests and apply the one-week product test scope in section 4A. Keep only focused Books CRUD unit/integration coverage and at most one optional CRUD E2E. Put any required ownership/access assertion inside CRUD integration; remove standalone account/auth suites. |
| Quality | Sonar/audit scripts have `Both`/React modes, React path/coverage fixtures and generated-contract assumptions. Simplify supported modes and tests, preserving the Blazor/.NET checks; do not retain broken optional commands. |
| Publication | Remove `publish_additional_frontend.sh`, corresponding planning/tests and Actions variables only after all callers are adapted. Keep main image/payload receipt/provenance and report-store behavior. |

### 4A. One-week demo product test scope — approved 2026-11-08, clarified 2026-11-09

Keep RitzauDemo automated product-behavior tests limited to the Books CRUD demo:

- Unit tests cover only core CRUD validation and small deterministic rules used by that path.
- Integration tests cover one database-backed create/list/read/update/delete path. Include an ownership/access assertion within this path only where required by the existing private-book policy; do not keep separate account or authentication test suites.
- Keep at most one opt-in Playwright E2E for a signed-in Books CRUD happy path. Omit it if it cannot be made reliable within the demo week; the deployment phase still performs independent browser acceptance.
- Remove product tests for Grafana/observability, visual snapshots, Lighthouse/performance, React, elaborate navigation/responsive behavior, cross-node cache invalidation, account/email/outbox lifecycle, deep migration edge cases, and broad architecture/layout details unrelated to CRUD.
- Remove fixtures and dependencies used only by dropped suites. Do not retain tests to preserve inherited counts.

This is the complete automated product-behavior test scope for the demo: focused CRUD unit and integration tests, plus zero or one CRUD E2E. Keep required CI admission/workflow policy, release identity/provenance, rendered settings, ownership/collision, backup/recovery, and shared-host health gates as operational gates rather than additional product test suites. Do not reduce GrumPlates or CarbonCalculator suites. Registration-disabled behavior, private API boundaries, migration identity, release provenance, and existing-site preservation remain requirements; verify each with its smallest relevant operational or acceptance check.

## 5. Single-node extraction map

No deletion below applies to the GrumPlates template. Work in the RitzauDemo copy.

| Existing path | Product destination/decision |
| --- | --- |
| `Deployment/LocalCluster/Scripts/install-ansible.sh` | Wrapper only. Point callers at existing `Deployment/Common/Scripts/install-ansible.sh`; do not duplicate its implementation. |
| `Deployment/LocalCluster/Scripts/ci-docker-smoke.sh` | Move adapted Blazor-only implementation to `Deployment/Common/Scripts/ci-docker-smoke.sh`. Both locations have the same depth relative to repository root; still verify relative paths. |
| `Deployment/LocalCluster/Scripts/Tests/test-ci-docker-smoke.sh` | Move adapted tests to `Deployment/Common/Scripts/Tests/test-ci-docker-smoke.sh`. Fix smoke/Dockerfile path references and fixture expectations. |
| `Deployment/LocalCluster/Scripts/audit-deployment.sh` and `Component/lib/audit_deployment.py` | Move to `Deployment/Common/Scripts/audit-deployment.sh` and `Component/lib/audit_deployment.py`. Replace cluster/cloud assertions with meaningful retained-target assertions before removing source targets. Review required/forbidden path lists. |
| `Scripts/CI/check-runner-capacity.sh` | Replace cluster-only delegation with actual product runner/workspace/Docker filesystem checks. Do not copy secondary-runner names or call a deleted script. Preserve disk/inode failure behavior. |
| Shared common tests | Adapt `test_account_deploy_settings.py`, `test_settings_validation.py`, `test_node_journal_limit.py`, release/status/retention tests and their live path imports. Never delete all common tests because they mention two removed targets. |
| Local gate render command | Replace cluster render validation with `Deployment/LocalSingleNode/Scripts/validate-rendered-templates.sh`, using the same constrained Jinja/yamllint environment. Keep LocalSingleNode playbook syntax validation. |
| CI/frontend release tests | `Scripts/CI/tests/test_frontend_release.py` constructs LocalCluster fixture paths. Update these together with moved smoke/release helpers. |
| Shared deployment lock | Keep `/tmp/localcluster-deploy.lockdir` and existing CI ownership labels unchanged. Update documentation links only. |

The inventory's 21 removed-target consumers are a minimum literal-path list. Also inspect relative path imports, script-root calculations, fixtures and audit hard-coded path inventories. Searches are review aids, not proof that every dynamic consumer was found.

## 6. Rename invariants and acceptance targets

Rename `GrumPlates.sln`, `GrumPlates`, `GrumPlates.Client`, `GrumPlates.Core`, and `GrumPlates.Test` to corresponding `RitzauDemo` names. Rename project files, assembly names, root namespaces, solution references, Docker entry point, static assets, tailwind output, test filters, editor paths and quality settings together.

Hidden checks:

- `GrumPlates/Properties/AssemblyInfo.cs` grants `InternalsVisibleTo("GrumPlates.Test")`.
- Server csproj embeds `LogicalName="GrumPlates.Accounts.CommonPasswords"`; `Features/Accounts/Security/AccountPasswordValidator.cs` loads exactly that resource string. Rename both or retain both intentionally; a mismatch breaks password validation at runtime.
- There is one baseline migration ID: `20261103193952_InitialSchema`. Keep its ID and `Up`/`Down` operations. Only namespaces/types change. Do not generate a new migration for the rename.
- `com.grumplates.frontend-profile` and `com.grumplates.acceptance-profile` have 15 known consumers. Rename to `com.ritzaudemo.*` everywhere in the product, including fake Docker labels and release validator tests. Keep `localcluster.ci.*` shared ownership labels unchanged.
- Preserve API contract operation IDs/routes and Books sample assets. Use .NET OpenAPI metadata generation/architecture assertions after React's npm `api:check` disappears.

Mandatory runtime acceptance: Books public listing/detail; Blazor prerender and interactivity; framework/static CSS assets; readiness/liveness; anonymous access boundaries for private APIs; account page rendering with registration disabled; wrong profile/manifest rejection; wrong repository/digest rejection; successful migration bundle identity; app-specific backup verification; unchanged neighboring app health. Tests needing a login use isolated test fixtures, not a new permanent demo account.

## 7. Work order and handoff contract

1. Resume the existing seed/main/native clone; revalidate source SHAs, current claims/jobs, private host facts, backups, collisions and capacity. Keep the historical inventory as a baseline. Review upstream drift rather than overwriting newer work or regenerating a seed.
2. Prepare and integrate separate GrumPlates and CarbonCalculator admission PRs through their complete current local gates and required exact-head/main checks. Follow the staged migration in section 3; preserve CarbonCalculator routing and all existing deployments.
3. Prepare RitzauDemo's first admitted identity/CI PR and its temporary scheduling guard while Actions are disabled. Finish static shared-host bootstrap review, canonical collisions and the mandatory native operator handoff before installing its runner. Verify registration and host/CD variables; keep legacy product workflows unschedulable.
4. Enable only the controlled first product validation path, merge via exact-head checks and verify main-push release. Reconcile stale bot/queued workflow code, then retire bootstrap routing through a checked PR. Validate three-repository native admission and peak build capacity while existing sites serve.
5. Product React removal and test-slimming PR, preserving Blazor and focused Books CRUD coverage.
6. Product target reduction/extraction PR; preserve admission and operational safeguards.
7. Product full rename PR; preserve migration IDs, payload provenance and shared ownership identifiers.
8. Product measured runtime budgets plus documentation/hygiene through normal PRs; budgets may be included earlier when safely validated.
9. Required RitzauDemo LAN deployment, independent rendered/browser acceptance, loaded three-repository coexistence evidence, and app-specific backup/isolated recovery verification.
10. Dedicated hostname only when supplied and public exposure explicitly authorized; otherwise record deferral.

For each PR, record head SHA, complete local gate, exact-head CI, integrated main SHA, exact-main publication outcome and remaining blockers. Do not present source fixture tests as the full gate. Do not dispatch a deployment from a documentation-only SHA lacking its own release; select the recorded published SHA explicitly.

The original preparation created this packet, the main plan, reusable preflight and source inventory generator/JSON. Subsequent execution imported the seed, created the native product checkout, and prepared the local product admission/identity branch. A narrowly scoped existing-host package/Avahi safety adaptation now also passes the full local gate on code SHA `3c087a5f44533a31faa4d1d8cd13b48968c5e7f8`; no runner/bootstrap/deployment followed. Retain all four support files with the revised plan. The sanitized current plan and packet plus the historical inventory, generator and preflight are synchronized in the local product branch; the parent workspace copy remains the fresh-chat authority until its first normal PR.

## 8. Native bootstrap is broader than runner registration

`bootstrap.py` performs apt operations, runs `PrepareSingleNode.yml`, imports approved SSH public keys and can invoke SSH hardening. The playbook includes base/Docker/firewall/Caddy roles and explicitly restarts Avahi. It is not merely an app runner registration. Inspect package versions, firewall defaults, SSH behavior and Caddy handler effects before presenting the operator command. Existing service availability is an acceptance requirement.

If the stock procedure would replace foreign shared configuration, upgrade/restart a shared service unexpectedly or disrupt a neighbor, do not run it as written. Prepare a narrowly reviewed existing-host adaptation in RitzauDemo, with fixture checks and a concrete operator message describing its effects, before the root handoff. Keep the operator-run requirement; do not bypass it by calling the runner installer with forged context. A harmless app-owned addition or validated Caddy site reload is different from restarting Docker or changing another app's settings.

### Existing-host review checkpoint — 2026-11-08

- Native `setup-status` reaches the human root step from the clean seed checkout. Its generated command is not a safe handoff while `origin/main` still contains the stock behavior.
- The canonical native collision check passed. Existing apps were running without unhealthy containers. The deploy-user readiness script could not run because deploy-user SSH access is unavailable; UFW rules also require operator-level inspection.
- Read-only apt simulation showed that the stock broad distribution upgrade would update the shared container runtime. Product code commit `3c087a5f44533a31faa4d1d8cd13b48968c5e7f8` gates that upgrade off in the LocalSingleNode prepare playbook, prevents prerequisite apt installs from upgrading existing packages, and avoids an unnecessary Avahi restart. Docker's package task remains `state: present`.
- The complete `docs/Test.md` local gate passed on that exact code SHA. Safety branch `task/ritzaudemo-bootstrap-20261108` is pushed and draft PR [#5](https://github.com/Grumlebob/RitzauDemo/pull/5) is open; its checks are empty because Actions remain disabled. Keep the PR in draft and do not merge without `build-test-push` on the exact head. At this checkpoint stale CarbonCalculator run `42107307607` remained queued without jobs. The contemporaneous no-cancel instruction was later superseded by the user's explicit full-authority direction; the orphaned run and suite were canceled after verifying the missing branch and zero jobs/checks. Do not rerun the deleted-branch dispatch.
- Continue only through the normal checked PR route. Do not present the stock command, push directly to `main`, or create a passing remote check. If no exact-head check path exists before the required root handoff, report that CI dependency for user resolution.

Perform static effect review before any privileged execution; running setup is not a prerequisite to understanding its effects. Do not report a verified no-disruption path until review, operator action and post-action health evidence support it. An actual required existing-host code adaptation must use the normal checked integration route; report a concrete bootstrap/CI dependency if canonical tooling cannot support that route, rather than inventing sudo context or another direct-main exception.

### Stale-run reconciliation and admission rollout checkpoint — 2026-11-08

- Run `42107307607` was an orphaned CarbonCalculator `workflow_dispatch` from 2026-11-05: the source branch was absent, attempt 1 ended in `startup_failure`, and the aggregate run/check suite remained queued with zero jobs/check runs. Following the user's explicit full-authority instruction, the run and suite were canceled and verified `completed/cancelled`. Keep the original run and SHA in history; do not recreate or rerun that deleted-branch dispatch.
- At the time PR CI began, no repository had queued, waiting, or in-progress Actions runs. The existing GrumPlates native runner was online/idle; read-only host checks found no active Runner.Worker and only the existing app/database/cache containers running and healthy. The bounded native capacity reservation is in the local GrumPlates claim ledger.
- GrumPlates follow-up commit `e62686441bf5190654a823578de62d0ef8dd7036` passed the complete local gate. Its first rerun attempt split the task-local `EXTRA_SCRIPT_TESTS` command; a corrected task-local wrapper ran the fixture and the full gate passed. The commit is pushed to PR #450 at the exact head. CI run `42526856844` is in progress; CodeQL run `42526856859` passed. Merge only after exact-head checks are green, then verify exact-main CI/publication before opening the CarbonCalculator admission PR.
- Pre-push checks found legacy Dependabot run `42523384022` active on the old workflow; it completed successfully. Queued PR #453 validation run `42523421032` was canceled and completed cancelled. Auto-merge workflow `42526557993` was canceled. PRs #451–#453 remain open with no auto-merge request. Dependabot updater `42523334321` uses GitHub-hosted Actions. Before PR #450 was pushed, native queue was empty and its runner was idle.
- CarbonCalculator branch `task/ritzau-ci-admission-20261108` remains full-gate validated at `1f29e82f9e0a152f396dee8118f63308d1fc3674`, with no PR/CI yet. RitzauDemo Actions remain disabled; draft PR #5 is open and unchecked. Parent plan and packet were synced in docs-only commit `a86b00c8acfb41beada455373b4f724f4d4cdf20`; the test-scope amendment followed in docs-only commit `b0bee356ca9fc1c97313ac4ed1315b4ae9ab7d8a`. No native bootstrap, runner registration, deployment, or existing-site change occurred.

### GrumPlates admission integration verified — 2026-11-08

- GrumPlates PR #450 exact head `e62686441bf5190654a823578de62d0ef8dd7036` passed CI run `42526856844` and CodeQL run `42526856859`, then merged at `2026-11-08T22:04:57Z` as `de6cfaec8d86373e14fee7a912122e815ddf4ce0`.
- Exact-main run `42528402393` passed `validate`, `publish-main`, and `build-test-push`; CodeQL run `42528402282` passed. Dependabot PR #451 run `42528614333` also passed, but PR #451 and PRs #452–#453 remain open without auto-merge requests. Auto-merge workflow run `42531613094` completed without merging a bot PR.
- All three repositories had empty queued/running job lists at recheck; `node-demo-books` was online and idle. Host inspection during CI found existing web apps still running and PostgreSQL healthy. Run-owned Testcontainers/Compose resources were removed after their checks.
- CarbonCalculator main still equals `0d255392b99eb635030f9c90a03de8a6d37d227c`; admission branch `1f29e82f9e0a152f396dee8118f63308d1fc3674` is clean and full-gate validated, with no PR or CI yet. Recheck queues and runner immediately before opening its approved native-CI-only PR.
- RitzauDemo draft PR #5 is open at `902f98c5123f0a5054ac32f7bda7f498be5c4469`; Actions remain disabled. Product test pruning remains pending for Phase 3 under the approved lean Books CRUD scope. No bootstrap, product runner registration, deployment, or existing-site change occurred.

### Lean product test scope and CarbonCalculator follow-up — 2026-11-09

- The user clarified that the one-week demo should showcase only Books CRUD unit/integration tests and at most one optional CRUD E2E. Remove separate account/auth suites and other inherited product suites listed in section 4A, including Grafana/observability. Keep necessary ownership/access assertions inside the CRUD integration path, and keep shared CI/release/deployment/backup/host-safety checks as operational gates. The parent plan and support packet state this narrower scope; it was synced to RitzauDemo draft PR #5 in docs-only commit `af1cbbfb72db434b7856fda1001807b54833f018`. PR #5 remains draft with Actions disabled.
- CarbonCalculator PR #477's first head `1f29e82f9e0a152f396dee8118f63308d1fc3674` failed exact-head run `42532517774` in the runner-build-admission cancellation fixture because the test checked the process group before remaining processes had exited. Follow-up commit `3e2b22d21dde9eba5b54a345c0413170c8a7af22` adds bounded process-group cleanup polling. Its full local gate passed in the separate Ubuntu-24.04 worktree at report `641f592f4b054e24ade17da15871a3ba/validation.json` (SHA-256 `7f576047a317bfcc6adfd53a807f6207ce91c904e84a13f6cae8ba67ceb79f72`): 937 passed, 44 documented skips, 0 failed, source identity exact. It is pushed at PR #477's exact head; run `42535851049` is in progress on `carboncalculator-ci-desktop`. Merge only after exact-head checks are green.
