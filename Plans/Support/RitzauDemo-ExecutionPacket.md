# RitzauDemo implementation decisions and dependency map

Read this with `../RitzauDemo-Blazor-SingleNode-Bootstrap.md`. This packet resolves implementation choices for the researched source SHA `ee9af0b985f86c9568cbf077aeb20336841446de`. If the source changes, review the inventory diff before applying these instructions. This is not authorization to change another project.

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

## 2. Resolve the bootstrap dependency cycle once

The original draft's order was invalid: `setup_status.status()` requires a clean checkout on branch `main` at `origin/main` before it offers the operator bootstrap command. An identity branch with a new slug cannot pass that check. Bootstrapping the untouched template would select `books`, colliding with the existing installation.

Use a **minimal bootstrap seed as part of the first import**, before any target main exists:

1. Verify RitzauDemo is empty and disable its Actions before pushing.
2. Clone the exact template baseline locally, configure origin/upstream and the `gh` default, and retain its history.
3. On local main, create one minimal bootstrap seed commit containing only product app slug/resource defaults, shared release identity, frontend selection, the sanitized plan/support packet, and any settings example changes required to validate those inputs. Do not remove React or targets, rename projects, or redesign CI in this commit.
4. Run the full existing local gate against that commit. Source names and all profiles/targets are still intact. Fix any gate failure before the import.
5. Push local main once, containing template history and that bootstrap seed, with Actions disabled. Record template SHA, seed SHA, and local gate results. This is the one-time initial import exception needed before a repository runner exists; it does not waive exact-head CI for any later PR or permit later direct-main pushes.
6. Clone RitzauDemo's main separately on the native node. Verify clean main equals origin/main and the selected app is `ritzaudemo`. Prepare tools/authentication and candidate collision evidence. Do not run the state machine's deployment action automatically.
7. Complete the canonical operator bootstrap handoff. Verify runner registration and private machine facts; set variables. Keep new heavy CI disabled until admission safety is proven.
8. Implement the narrowly scoped GrumPlates admission integration through a normal template PR, with its existing complete gate. Preserve both profiles and all targets.
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

Expand admission coverage in GrumPlates and RitzauDemo rather than redesigning runners. Apply exactly one wrapper to each resource-heavy step or full multi-command block. Use the existing wrapper directly for individual commands:

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

### Important shared-host limit

Read-only preparation found an additional out-of-scope CI producer on the shared host. Do not assume its heavy jobs run elsewhere or that the two-repository lock covers every producer. Prove current eligibility and compatible admission without retrieving credentials or changing that project.

Before enabling new heavy CI, read-only verify its current eligible workload/routing and existing lock coverage through available authenticated configuration or live job evidence. Do not retrieve or expose credentials. If a third producer can build concurrently outside compatible admission, report a concrete out-of-scope dependency; do not alter CarbonCalculator or enable RitzauDemo heavy CI on an unproved assumption. The existing browser lock already covers some cooperating work but not all unwrapped image/.NET builds. No fleet-wide safety claim has been established by this preparation.

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
| `Scripts/CI/check-runner-capacity.sh` | Replace cluster-only delegation with actual product runner/workspace/Docker filesystem checks. Do not copy template-host names or call a deleted script. Preserve disk/inode failure behavior. |
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

1. Refresh baseline/inventory and capacity. Prepare seed defaults; run the full unchanged gate.
2. Initial import with minimal seed; native operator bootstrap; new runner online but heavy CI gated.
3. Narrow template CI admission PR; port compatible calls; verify actual host workload safety.
4. First product identity PR and successful main-push release.
5. Product React removal PR.
6. Product target reduction/extraction PR.
7. Product full rename PR.
8. Product docs/hygiene PR.
9. Required RitzauDemo deployment and independent LAN acceptance.
10. Dedicated hostname when selected; public HTTPS acceptance.

For each PR, record head SHA, complete local gate, exact-head CI, integrated main SHA, exact-main publication outcome and remaining blockers. Do not present source fixture tests as the full gate. Do not dispatch a deployment from a documentation-only SHA lacking its own release; select the recorded published SHA explicitly.

No code repository or native resource was changed during preparation. The files created are this packet, the main plan, the reusable preflight script, the source-inventory generator and its JSON output. Keep them together until copied into RitzauDemo. They remain useful handoff material, not disposable diagnostic logs.

## 8. Native bootstrap is broader than runner registration

`bootstrap.py` performs apt operations, runs `PrepareSingleNode.yml`, imports approved SSH public keys and can invoke SSH hardening. The playbook includes base/Docker/firewall/Caddy roles and explicitly restarts Avahi. It is not merely an app runner registration. Inspect package versions, firewall defaults, SSH behavior and Caddy handler effects before presenting the operator command. Existing service availability is an acceptance requirement.

If the stock procedure would replace foreign shared configuration, upgrade/restart a shared service unexpectedly or disrupt a neighbor, do not run it as written. Prepare a narrowly reviewed existing-host adaptation in RitzauDemo, with fixture checks and a concrete operator message describing its effects, before the root handoff. Keep the operator-run requirement; do not bypass it by calling the runner installer with forged context. A harmless app-owned addition or validated Caddy site reload is different from restarting Docker or changing another app's settings.

This bootstrap-effects review is not complete because no privileged setup has run. It must not be reported as an already verified no-disruption path.
