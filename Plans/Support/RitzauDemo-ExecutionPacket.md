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
| Client | Remove `GrumPlates.React/`; keep `GrumPlates.Client/` including npm/Tailwind, `.razor.js`, axe and Lighthouse tooling. |
| Server composition | Remove `FrontendComposition.React.cs`, `ReactFrontendHosting.cs`, `ReactFrontendStaticRoot.cs` and `Frontend/React/`. Keep Blazor composition and `Frontend/OpenApi/`. |
| Server csproj | Remove React compile/content/Razor-exclusion conditions. Keep Blazor packages/references, the publish-output/source guard and source `wwwroot`. Rename `_ReactPublishWebRoot`/`_ReactSourceWebRoot` to neutral names when simplifying. |
| Profile contract | Accept only `BlazorAuto` in resolver, MSBuild target, immutable manifest/profile validators and verification helpers. Preserve `BlazorAutoV1`. Unknown/empty/environment-derived selection must still fail. |
| CI lifecycle test | `Prove Testcontainers lifecycle cleanup` explicitly builds/tests `-p:FrontendProfile=React` today. Change to Blazor; retain mandatory execution and zero skips. |
| Node pin | CI currently parses `FROM node:26.13.0-bookworm-slim` in `GrumPlates/Dockerfile`. Add tracked `.node-version` containing `26.13.0` and point `actions/setup-node` at it with `package-manager-cache: false` before deleting the React stage. Retain npm 12.2.0 setup for surviving CSS tooling. |
| Packaged smoke | `Deployment/LocalCluster/Scripts/ci-docker-smoke.sh` also parses that Node stage unconditionally for the React SMTP fixture. Remove React-only SMTP/browser resources and initialization, then remove that Node Dockerfile parse. Keep Blazor packaged render/pre-hydration/browser tests and cleanup. |
| App name in smoke | Smoke defaults through `read-deploy-setting.sh` from LocalCluster. Supply `APP_NAME` from the product LocalSingleNode parser; remove the obsolete fallback when moving the smoke helper. |
| Tests | Remove React-only hosting/E2E tests. Adapt mixed `HostingResponseBehaviourTests`, `FrontendCompositionTests`, fixtures and profile unit tests. Keep shared account/Books/API tests. |
| Quality | Sonar/audit scripts have `Both`/React modes, React path/coverage fixtures and generated-contract assumptions. Simplify supported modes and tests, preserving the Blazor/.NET checks; do not retain broken optional commands. |
| Publication | Remove `publish_additional_frontend.sh`, corresponding planning/tests and Actions variables only after all callers are adapted. Keep main image/payload receipt/provenance and report-store behavior. |

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
5. Product React removal PR, preserving Blazor and shared account/Books coverage.
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
- The complete `docs/Test.md` local gate passed on that exact code SHA. The safety branch is local only; no PR or remote CI exists. Actions remain disabled and stale CarbonCalculator run `42107307607` remains queued without jobs. Do not cancel or work around that run.
- Continue only through the normal checked PR route. Do not present the stock command, push directly to `main`, or create a passing remote check. If no exact-head check path exists before the required root handoff, report that CI dependency for user resolution.

Perform static effect review before any privileged execution; running setup is not a prerequisite to understanding its effects. Do not report a verified no-disruption path until review, operator action and post-action health evidence support it. An actual required existing-host code adaptation must use the normal checked integration route; report a concrete bootstrap/CI dependency if canonical tooling cannot support that route, rather than inventing sudo context or another direct-main exception.
