# RitzauDemo: Blazor-only clone and safe single-node bootstrap

Status: execution in progress. Tracked goal 01a1bab6-b022-7f22-b9cc-af83862b268c. Started 2026-11-08.

## Execution packet and preparation completed now

Read [Support/RitzauDemo-ExecutionPacket.md](Support/RitzauDemo-ExecutionPacket.md) before implementation. It fixes the initial import/runner dependency cycle and specifies CI admission, extraction paths, product defaults, Node pin and rename invariants. Its concrete decisions supersede open-ended implementation suggestions below; the scope boundary still applies.

- [x] Generate [the source dependency inventory](Support/RitzauDemo-SourceInventory.json): exact SHA, normalized hashes, 85 React consumers, 21 removed-target consumers, 15 release-label consumers, 9 setup/admission files and 3 migration files.
- [x] Verify the inventory against unchanged source with [the read-only generator](Support/RitzauDemo-SourceInventory.py).
- [x] Run 69 offline setup, collision/ownership, settings and frontend-profile fixture checks successfully. [The reusable preflight](Support/RitzauDemo-Preflight.sh) includes correct WSL .NET PATH setup, candidate schema validation and existing lock-helper regression tests.
- [x] Recheck native workload, listeners, networks, candidate app paths and runner presence without changing resources. An additional out-of-scope CI producer requires a live admission-safety review before any new heavy work.
- [x] Resolve setup ordering: minimal product identity/resource defaults belong in the first main import, before normal PR work. A feature-branch bootstrap cannot pass the existing clean-main gate.
- [x] Identify hidden dependencies: Node version parsing from the React Dockerfile stage in both CI and smoke; lifecycle tests selecting React; an existing Common Ansible wrapper; password-resource lookup string; fixed initial migration ID.

Keep all four support files with this tracked product plan. These checks do not prove the full application gate, live collision gate, memory admission, bootstrap effects or deployment acceptance.

## Purpose and confirmed decisions

Create an independent product repository at https://github.com/Grumlebob/RitzauDemo from GrumPlates. Keep the Books example and its sample assets initially. Remove React, retain Blazor Interactive Auto, retain accounts with registration disabled, support only LocalSingleNode, and fully rename the surviving solution, projects, namespaces, and product identity to RitzauDemo.

Repository ownership and canonical URLs must remain under Grumlebob. Do not transfer repositories or introduce an organization migration. CI and deployment for the initial RitzauDemo setup remain on `<deployment-node>`, using a separate repository runner registration and host-wide admission for heavy work. Share machine capacity across projects without claiming that GitHub offers a personal-account runner pool. Keep all existing applications, databases, runners, credentials, DNS, and backups intact. The product will eventually have its own public hostname; selection is deferred. Establish LAN deployment first.

Preserve template Git history. Keep an explicit `upstream` remote for optional manual template imports; do not configure automatic upstream merges. Do not replace any existing demo site.

Execution is active under the tracked goal. The user has included RitzauDemo deployment on `<deployment-node>` in scope. Complete repository setup, CI, application setup, LAN deployment and independent acceptance. Preserve the mandatory operator bootstrap handoff below.

## Hard scope boundary — GrumPlates and RitzauDemo only

- The only repositories eligible for implementation changes in this plan are `Grumlebob/GrumPlates` and `Grumlebob/RitzauDemo`. Other projects mentioned during runner discussion are context, not a migration or modification list.
- RitzauDemo receives all product customization and deployment: React removal, single-node-only simplification, rename, identity, settings, runner registration, its own application/PostgreSQL/Redis resources on `<deployment-node>`, CI/CD configuration, backups, deployment and independent acceptance. Phases 1 and 3–8 refer to RitzauDemo unless explicitly stated otherwise.
- GrumPlates remains a reusable template with both frontend profiles and its existing deployment targets. Limit any GrumPlates changes to the reviewed CI build-admission integration needed for its runner to coexist safely with RitzauDemo. Do not remove React or cluster/cloud support from GrumPlates, change its deployment routing or identity, or dispatch a GrumPlates deployment.
- Do not modify CarbonCalculator, ImprovedDb, Portfolio, JobHunt, KinoJoin, their repositories, runners, secrets, environments, deployments, clusters, or schedules. ImprovedDb retains its own cluster. Do not register those projects on `<deployment-node>` or relocate any project here.
- Shared-host inspection may read existing `<deployment-node>` workload, listeners, ownership and health solely to protect neighbors. Discovering another producer does not authorize changing it. If safe build admission requires an out-of-scope change, stop that implementation phase and report the exact dependency instead of expanding scope.
- On `<deployment-node>`, add only RitzauDemo-owned application resources and the narrowly reviewed admission integration for GrumPlates/RitzauDemo. Preserve existing app data, runners, routes, tunnels, credentials, backup policy, Docker configuration and services. Do not restart or reconfigure shared services as a side effect without reviewing the actual effect and obtaining separate authorization for any disruption.
- No organization, URL transfer, shared runner pool rollout, multi-project cleanup, runner fleet redesign, or unrelated cluster operation belongs in this plan.

## Plan location and private installation facts

This sanitized execution copy lives in RitzauDemo `Plans/`. Do not modify or publish GrumPlates merely to deliver the plan. The only permitted GrumPlates code change is the narrowly scoped CI admission integration described above, through its own reviewed PR. Keep actual machine, address, login, neighbor inventory, tunnel, and key details in ignored/private installation notes.

The host alias above is a local planning input. Before publishing this plan, replace private machine identifiers with `<deployment-node>` and keep actual host, address, login, observed neighbor inventory, installation-specific overrides, tunnel state, and key paths in ignored/private installation notes. Generic non-secret product resource defaults may live in the product inventory, as in the canonical primary setup route; see the execution packet. The public repository must not inherit machine-specific details from this workspace. Product/repository names and generic placeholder examples are safe to track.

## Research already completed

- [x] Read workspace rules, caveman skill, RTK integration, machine facts, and template `AGENTS.md`.
- [x] Verify target repository through authenticated GitHub API: public, empty, caller has admin access, Actions enabled, no registered runners, no rulesets returned.
- [x] Verify template primary checkout is clean on `main`, with no additional registered worktrees.
- [x] Verify local template HEAD matches GitHub `main`: `ee9af0b985f86c9568cbf077aeb20336841446de`.
- [x] Read fork, standalone-product, frontend-profile, single-node-only, local-gate, release, controller-SSH, readiness, and setup guides.
- [x] Verify authorized controller access and GitHub repository administration through supported authentication. Never print tokens.
- [x] Verify the template uses a repository-scoped deployment runner. Its registration cannot run RitzauDemo workflows.
- [x] Inspect the runner installer: it supports a separate service and directory per application and refuses foreign registrations.
- [x] Inspect read-only capacity, health, storage and neighbor inventory. Keep observed values and application details in ignored/private installation notes; this is not deployment acceptance.
- [x] Find critical coupling: CI builds both frontend profiles; required audit, smoke, capacity, and Ansible scripts still live under LocalCluster; shared tests reference removed targets.

These are planning observations, not deployment acceptance. A clean product clone now exists at the verified source SHA; its minimal identity seed is being prepared. No product push, runner installation, node bootstrap, deployment, or cleanup has occurred. No full application gate has run. The preparation pass ran the 69 offline fixture checks above; those do not constitute build or deployment acceptance.

## Read these canonical files before their implementation phase

Paths below are relative to the template or initial product clone. Read them before removing them; retain corrected product operations documentation afterward.

1. `AGENTS.md`, `docs/HowToForkThisRepo.md`, `docs/StandaloneProduct.md`.
2. `docs/FrontendProfiles.md`, `docs/Frontend/React.md`, `docs/Accounts.md`, `docs/Test.md`, `docs/Testing/Browser.md`.
3. `Deployment/LocalSingleNode/SingleNodeOnlyClone.md`, `HowToDeployLocalSingleNode.md`, `Readiness.md`, `Operations.md`, `ControllerSsh.md`.
4. `Deployment/LocalSingleNode/AgentSetup.md`, `Scripts/lib/setup_status.py`, `Scripts/lib/setup_next.py`, `Scripts/lib/bootstrap.py`, `Scripts/lib/runner.py`.
5. `Deployment/Common/ReleaseContract.md`, `.github/workflows/ci.yml`, `Scripts/CI/workflow_policy.py`.

The existing React-only guide describes the opposite frontend removal. It is not a Blazor-only recipe. Do not follow its deletions literally.

## Execution rules and evidence

- Work in the new product checkout and specifically owned native checkout. Use a separate GrumPlates task branch only for its explicitly scoped CI admission integration. Never rename the template checkout or point its origin at RitzauDemo.
- Re-read applicable instructions when entering each checkout. Read the private machine guide before SSH, WSL, tool, or runner changes.
- Use one phase per PR after initial repository seeding. Run the current complete local gate before pushing non-Markdown changes. Require `build-test-push` on the exact PR head, then verify the integrated `origin/main` commit and applicable main CI.
- Update audits and test expectations alongside behavior changes. A missing script, skipped suite, suppressed assertion, or green wrapper around failed work is not a passing gate.
- Keep build and test sequential within a checkout. Use distinct owned paths and result directories. Inspect every failed/background command promptly.
- Stage explicit paths. Do not overwrite another session's edits, force-push main, weaken protection, reclaim locks, prune shared Docker resources, or run Compose down with volumes.
- Store no real secrets in commits, workflow arguments, terminal output, PRs, or this plan. Generate fresh application secrets rather than copying Books values.
- Record each phase's full commit SHA, PR URL, local validation outcome, exact CI run/attempt, and outstanding issues below. Deployment additionally needs SHA, manifest profile pair, image/payload digests, migration outcome, and acceptance evidence.
- Before filesystem removal, verify absolute target paths, ownership, reparse/symlink boundaries, and useful private inputs. Use supported scoped cleanup; never blanket-delete workspaces or generated-looking folders.

## Confirmed runner architecture — preserve all repository URLs

The user explicitly rejects repository URL changes. Keep every repository owned by `Grumlebob`, including RitzauDemo. Organization migration, account conversion, repository transfers and mirror repositories are outside this plan.

GitHub repository runners are registered to one repository. There is no native personal-account-wide shared runner registration. This registration limit does not require a separate physical machine for each repository. Multiple repository registrations can use the same host, provided resource-heavy work is coordinated.

Use the existing supported per-repository installer for RitzauDemo and implement effective host-wide build admission in Phase 2A. On `<deployment-node>`, admit only one heavy CI/manual build at a time until measured capacity supports more. Other runner services may remain online while their jobs wait for admission. Waiting listeners and jobs still consume some memory, so measure baseline and queued-job overhead as well as build peaks. Do not start heavyweight setup, dependency installation, Docker builds or test containers before admission.

Do not add registrations for other projects or build a general runner pool. This plan adds only RitzauDemo's registration on `<deployment-node>` and, if needed, a compatible build-admission change in GrumPlates. Existing projects keep their current CI and deployment hosts. The actual template checkout here is `GrumPlates`, not the historical `BlazorAutoApp` path supplied during discussion.

A central dispatcher, organization migration, dynamic runner scheduler, or multi-host rollout is outside scope. If the two-repository arrangement cannot be made safe without touching another project, report that blocker and leave the new heavy CI disabled. Do not solve it by changing another repository, pausing its runner, moving its jobs, or altering its cluster.

Phase 2 below covers only GrumPlates/RitzauDemo coexistence. No fleet-wide runner change is authorized, and this session remains planning-only.

Source: [GitHub runner scopes](https://docs.github.com/en/actions/concepts/runners/self-hosted-runners).

## Phase 0 — Refresh facts and establish isolated work areas

1. [x] Confirm the target was empty through the GitHub API before clone. If it has content later, fetch and inspect it; do not overwrite or force-push it.
2. [x] Recheck template cleanliness and GitHub main SHA: clean `main` at `ee9af0b985f86c9568cbf077aeb20336841446de`, matching GitHub `main`.
3. [x] Confirm isolated product directory was absent before clone; no other task or private inputs were overwritten.
4. [x] Create a private installation record outside tracked files. It records the selected node, app ownership, Docker storage/workspace filesystems and candidate settings.
5. [x] Recheck existing app health, fresh backup restoration, runner activity, Docker workload, free disk, memory pressure and host identity. All existing app backups restored in isolated verification containers.
6. [x] Inventory occupied TCP listeners, Docker published ports/subnets, Caddy sites, app/backup roots, runner services and installed application records without exposing credentials.
7. [ ] Select distinct candidate app/database/Redis/LAN listener ports, subnet, app/backup roots, and later public origin port. Do not use the Books defaults on this shared host. Validate candidates through the existing collision/readiness parser.

Exit: known source SHA, empty target or reconciled target contents, isolated destination, and privately recorded collision-free candidates. No deployment is authorized by passing readiness.

## Phase 1 — Seed the independent repository with minimal bootstrap identity

Follow execution packet section 2 exactly. The target is empty and has no runner, so bootstrap identity belongs in its first import, with full local validation. Subsequent code/configuration changes use normal PRs and exact-head CI.

1. [ ] Recheck target emptiness; disable target Actions before the first push. Never alter template Actions settings.
2. [x] Clone template SHA `ee9af0b985f86c9568cbf077aeb20336841446de` with history. Rename `origin` to `upstream`, add `origin=https://github.com/Grumlebob/RitzauDemo.git`, and set the `gh` default to RitzauDemo.
3. [x] Verify both remotes, baseline SHA and `gh` default. No template branches/tags were pushed; source checkout remains unchanged.
4. [x] On local `main`, set `app_name=ritzaudemo`, distinct product defaults, `app_image=ghcr.io/grumlebob/ritzaudemo`, `migration_bundle_name=ritzaudemo-migrate`, and `migration_runtime=linux-x64`. Existing `frontend-profile.txt` is `BlazorAuto`. Account registration/seeding and deployed observability remain disabled.
5. [x] Keep internal GrumPlates project names, both frontends and all deployment targets intact. Copy the sanitized plan and all four support files. No cleanup or rename is part of the seed.
6. [x] Set `App:Name` to `ritzaudemo` in app settings, the local `.env.example`, and the deployed `app_name`. Compose renders `APP_NAME` as `App__Name`; caching, Data Protection and account identity use this same value. No unused identity variable was added.
7. [x] Generate an independent ignored local `.env` with a new database password and distinct development ports. No source credentials or data were copied.
8. [ ] Run the full unchanged `docs/Test.md` local gate against the committed bootstrap seed. Both profiles/all targets still apply. Record seed SHA and results.
9. [ ] Push main once with template history plus the validated minimal seed, while Actions are disabled. This is the initial-import exception only, needed to bootstrap the runner. If remote main now exists or protection blocks import, stop and reconcile; never force or weaken policy.
10. [ ] Clone target main separately on the native node. Require clean branch main at origin/main, `app_name=ritzaudemo`, correct origin and host identity. Do not bootstrap a feature branch or untouched Books identity.
11. [ ] Prepare and review the exact operator bootstrap handoff. Complete Phase 2 before enabling heavy CI. Native bootstrap can affect shared packages/firewall/SSH/Avahi/Caddy; review execution packet section 8 before presenting the command.
12. [ ] After runner/admission/variables are ready, enable target Actions. Make remaining identity/branding/readme updates in the first normal PR, require exact-head `build-test-push`, merge, and verify successful main-push CI/publication. This supplies initial setup's push provenance.

Exit: correct import, validated native bootstrap identity and, after Phase 2, green first normal identity PR/main release. No product deployment yet. Do not repeat the import exception.

## Phase 2 — Make runner coexistence safe before enabling CI

### A. Coordinate GrumPlates and RitzauDemo heavy work on `<deployment-node>`

Two runner services can start jobs independently. Idle service memory is not the primary risk; simultaneous builds, Testcontainers, Docker image builds and browser suites are. GitHub concurrency groups are repository-scoped. Existing deployment/browser locks do not automatically serialize all CI validation.

1. [x] Read-only inspect the heavy producers actually running on `<deployment-node>`. Do not inspect or change other clusters merely because their projects were mentioned. Determine whether existing admission controls already cover any third-party producer on this host. A separate out-of-scope producer can route heavy native CI when its desktop route is busy or offline and supports a native canary; its restore, dependency installation, tests, and image/publication builds are not fully behind the shared lock. No workflow was in progress at inspection.
2. [ ] Follow execution packet section 3 for wrapper coverage/nesting rules. Identify heavy GrumPlates and RitzauDemo CI/manual sections that need admission. Include dependency installation, image publication builds, tests and cleanup lifetime as applicable. Keep existing deployment/browser lock names and define a consistent acquisition order to avoid deadlocks.
3. [ ] Reuse `Deployment/Common/Scripts/with-node-browser-lock.sh` and `/tmp/localsinglenode-ci-browser.lock`. Do not introduce a second lock or a detached holder spanning Actions steps. Never nest the same wrapper; remove inner wrapping when a full step block is admitted. Scope GrumPlates behavior to the intended host; do not change its cluster runner placement, supported profiles or deployment targets. Add or reuse the smallest portable helper needed, with reviewed compatible calls in the two repositories.
4. [ ] Make the GrumPlates integration in a separate template PR using its full existing local gate, exact-head CI and main verification. Keep both profiles and every template deployment target green. Port only the needed helper/calls to RitzauDemo. Do not apply product deletions back to the template.
5. [ ] Verify every heavy GrumPlates/RitzauDemo producer on this host participates before enabling the new heavy jobs. A lock used only by RitzauDemo cannot prevent overlap with GrumPlates. Do not pause or reconfigure another project's runner to claim success.
6. [ ] If another active host producer does not participate in compatible admission, do not claim host-wide serialization. Read-only check its existing scheduling/resource isolation. Proceed only with demonstrated compatible admission or measured enforced limits that prove safety without changing that project. Otherwise keep new heavy CI disabled and report the specific out-of-scope dependency. A one-time idle observation is insufficient.
7. [ ] Use bounded admission waits with clear status, automatic release on process exit, and no manual deletion of held locks. Start no heavy setup or test containers before admission. Measure idle/waiting-job overhead as well as build peak memory.
8. [ ] Check available RAM, swap pressure, free disk on actual Docker/workspace roots and current application health after obtaining admission. Use the readiness policy as a floor. Measure one complete admitted build alongside existing apps; do not lower thresholds to pass.
9. [ ] Verify overlapping lightweight contenders serialize, timeout/failure releases admission, and existing applications stay healthy. Then verify a complete real CI run. Do not mutate neighboring applications as part of the test.
10. [ ] Record the two participating repositories, lock ownership/path privately, integration commits, coverage of actual `<deployment-node>` producers and memory evidence. If one build does not fit, stop rollout and report measured remediation without modifying other projects or hosts.

**Current stop (2026-11-08):** An out-of-scope producer can run heavy native CI outside the planned two-repository admission lock. No compatible admission or enforced resource limit is proven. Keep RitzauDemo Actions disabled and stop Phase 2 before registering or enabling its runner. Exact workflow/routing evidence stays in private installation notes. Do not modify the out-of-scope producer.\n\n### B. Register RitzauDemo's own runner

1. [ ] Prepare a separate native checkout for RitzauDemo using its own origin. Do not repoint the existing Books checkout: it still has a historical repository URL and belongs to that installation.
2. [ ] Use the canonical tracked product defaults from execution packet section 2: `app_name: ritzaudemo`, distinct resource candidates, and `observability_enabled: false`. Keep private machine facts and credentials outside Git. Do not assume a Python settings override propagates through root bootstrap, Ansible or primary CD; do not invent a partial private-settings route.
3. [ ] Read setup status and bootstrap code against the prepared candidate. The process must add a new app to an existing host, preserve every existing registration/site, and keep the current hostname. Never run fresh-node setup with a new hostname.
4. [ ] Resolve the setup authorization boundary before mutation. `AgentSetup.md` requires an explicit native-node setup request and an operator-run root command. The controller must not silently run its state machine or bootstrap just because SSH works.
5. [ ] Read execution packet section 8 and review shared-host effects before presenting the operator command. Prepare everything it needs first: correct checkout/ref, validated candidate inputs, repository authentication status, collision/backup/capacity evidence, and reviewed shared-host effects. The validated minimal identity must already be in the first main import from Phase 1. Do not attempt feature-branch bootstrap or bypass the clean-main gate.
6. [ ] If setup lacks CI capacity, use the documented runner/bootstrap-first route. Finish customization via a normal exact-head CI PR afterward. Do not automatically execute `setup-next-step` actions that dispatch CI/deployment before their prerequisites and authorization are satisfied.
7. [ ] At the canonical `human` step, present the exact generated command and message. The operator runs it natively. Do not fabricate a generic sudo command, run sudo for the operator, copy registration tokens, or add sudoers rules.
8. [ ] Request the installer’s CI option when CI and CD use this same registration. Expected identities from the inspected installer are:

   - Runner name: `<deployment-node>-ritzaudemo`.
   - Directory: `/home/<install-user>/actions-runner-ritzaudemo`.
   - CD label: `localsinglenode-ritzaudemo`.
   - CI label: `ci-ritzaudemo` when installed with `--ci-runner`.

9. [ ] Verify the service is online in the RitzauDemo API and its repository URL, name, labels, service account, and workspace are correct. Existing Books runner must remain registered and healthy in GrumPlates. A runner does not need an organization to share a physical machine with another registration.
10. [ ] Set RitzauDemo variables deliberately: `CI_RUNNER_LABEL=ci-ritzaudemo`, `CI_RUNNER_HOST=<verified-hostname>`, `LOCALSINGLENODE_RUNNER_LABEL=localsinglenode-ritzaudemo`, `LOCALSINGLENODE_HOST=<verified-hostname>`, `DEPLOY_TARGETS=localsinglenode`. Leave `ADDITIONAL_FRONTEND_PROFILES` unset.
11. [ ] Inspect all workflows for fallback labels, deployment environments, permissions and schedules. Restrict deployment to trusted main. Retain rejection of foreign-fork PR code on deployment runners. Do not expose this privileged shared host to arbitrary PR workflows.
12. [ ] Enable Actions only after admission controls and variables are verified. Open the identity PR, including compatible target admission calls; run CI on its exact head, merge only after required checks, then verify exact-main push validation and publication. A manually dispatched validation may diagnose a missing run, but does not replace initial setup's required main-push provenance.

Exit: GrumPlates and RitzauDemo runner services online, their heavy work serialized on `<deployment-node>`, any other actual host producers proven safe without out-of-scope edits, green identity main, fresh RitzauDemo publication, and existing apps healthy. No product deployment yet.

## Phase 3 — Remove React from RitzauDemo while preserving Blazor

1. [ ] Branch from verified product main. Capture the actual dependency inventory with `rg`, including hidden tracked configuration. Classify matches before deleting anything.
2. [ ] Inspect and remove React client assets/tooling under `GrumPlates.React/`, `GrumPlates/Frontend/FrontendComposition.React.cs`, `ReactFrontendHosting.cs`, `ReactFrontendStaticRoot.cs`, and `GrumPlates/Frontend/React/` after removing their consumers.
3. [ ] Keep `GrumPlates.Client/`, its npm lockfile, Tailwind, generated `GrumPlates/wwwroot/tailwind.css`, Blazor render modes, account UI, Books UI, and shared Core contracts. Removing React does not remove all Node/npm tooling.
4. [ ] Simplify `Directory.Build.props`, `Directory.Build.targets`, project references, server csproj compile/content rules, frontend composition, and configuration. Preserve publish safeguards that prevent output from overwriting source `wwwroot`.
5. [ ] Keep the tracked profile and immutable release profile pair `BlazorAuto` / `BlazorAutoV1`. Make profile validation reject unsupported React selection. Update resolver tests, manifest validation, release verifiers, acceptance scripts, frontend publication planning, and status helpers together. Keep generic shared contracts only when deliberately needed.
6. [ ] Simplify `GrumPlates/Dockerfile`: remove React dependency/assets/publish/final stages; retain `final-blazor`, non-root execution, image identity labels, and Blazor assets. Update local Compose target resolution and `Scripts/RunLocal.ps1` consistently.
7. [ ] Remove React-only browser fixtures and scripts: `Scripts/Testing/RunReactPublicE2E.ps1`, React SMTP fixture if no remaining consumer, and `GrumPlates.Test/E2E/Features/React/`. Remove React hosting-only tests; adapt mixed frontend composition/security tests to assert the retained Blazor behavior.
8. [ ] Preserve shared accounts, account security/integration tests, private Books authorization, caching, OpenAPI metadata needed by remaining APIs, and client contracts. Do not delete backend endpoints merely because React consumed them.
9. [ ] Rewrite `.github/workflows/ci.yml` to validate/build/test/smoke/publish Blazor only. Remove React npm/unit/browser/contract steps, both-profile loops, additional-profile publishing, and only their obsolete reports. Keep required `build-test-push`, .NET account/non-account results, image smoke, release provenance, cleanup, and failure propagation.
10. [ ] Update `Scripts/CI/validate-frontend-profiles.sh` to retain its entry point but validate only Blazor and enforce account zero-skip rules. Update CI Python tests, release tests, report validators, and local-gate docs to match. Do not simply stop calling validation.
11. [ ] Update Dependabot, CodeQL language matrix if appropriate to surviving JavaScript, editor/run settings, quality scripts, browser docs, README, and frontend documentation. Check `.claude`, `.codex`, `.qlty`, and `.run` if present. Keep remaining Blazor Tailwind/axe/Lighthouse dependencies.
12. [ ] Search for `React`, `ReactPublicV1`, `final-react`, `ADDITIONAL_FRONTEND_PROFILES`, and removed file paths. Resolve every live dependency. Keep only justified historical/provenance references.
13. [ ] Fix both Node Dockerfile parsers and the React-selected lifecycle test as specified in execution packet section 4. Run the complete updated local gate, packaged Blazor smoke, and affected Blazor/account browser checks. Review test discovery so removed React tests do not hide lost shared coverage. Merge via exact-head CI and verify exact-main publication.

Exit: no React app or selectable React release remains; Books and accounts still work through Blazor.

## Phase 4 — Remove unused deployment targets from RitzauDemo only

1. [ ] Branch from verified RitzauDemo main. Confirm no GrumPlates file or live cluster is being changed. Inventory all `LocalCluster`, `Cloud`, `localcluster`, `cloud`, and removed path references in workflows, scripts, Python imports, tests, lint configurations, and documentation.
2. [ ] Use execution packet section 5 for exact destinations and the existing Common Ansible implementation. Extract still-needed functionality before deleting target folders. Known critical candidates under `Deployment/LocalCluster/Scripts/` include `ci-docker-smoke.sh`, `install-ansible.sh`, `audit-deployment.sh` and `Component/lib/audit_deployment.py`, capacity scripts, and their dependencies/tests. Classify and relocate to `Deployment/Common` or a product CI location as appropriate.
3. [ ] Trace `Scripts/CI/check-runner-capacity.sh` and additional-frontend publication helpers. Replace template-host-specific capacity assumptions with retained shared-host safeguards. Remove additional-profile helpers if Phase 3 removed their purpose.
4. [ ] Update CI, `docs/Test.md`, agent rules, workflow policy, test fixtures and all callers for relocated scripts. Preserve a meaningful single-node deployment audit covering ownership, settings, release verification, secrets, locks, workflows, backups, and acceptance.
5. [ ] Update shared tests that currently load both removed targets, especially `test_account_deploy_settings.py`, `test_settings_validation.py`, `test_node_journal_limit.py`, frontend release fixtures, and retention/release-status fixtures. Retain all assertions protecting the remaining target.
6. [ ] Keep shared `Deployment/Common` roles/scripts, provenance/payload handling, tool pins, Caddy integration, host admission, deployment/browser locks, backup support, runner prerequisites, release status, and scoped registry retention used by LocalSingleNode.
7. [ ] Delete Cloud/LocalCluster-only CD and maintenance workflows, target-only Dependabot entries, deployment folders and guides only after every live consumer has moved or been removed deliberately.
8. [ ] Keep the shared lock name `/tmp/localcluster-deploy.lockdir` and shared CI resource ownership labels unchanged. Other apps use that lock. Update obsolete documentation links/messages without changing interoperable identifiers.
9. [ ] LocalSingleNode does not support deployed observability. Keep `observability_enabled: false`. Local developer observability may remain if useful; remove only target-specific observability wiring after tracing consumers.
10. [ ] Remove same-repository additional-profile app setup/support now that only one profile exists, or explicitly retain harmless generic support with tests. Do not use the additional-instance route for RitzauDemo: it is a different repository, not the other profile of Books.
11. [ ] Re-run searches, YAML/Ansible rendering, syntax/lint, common and LocalSingleNode tests, audit, workflow lint, image smoke, and full revised local gate. Merge via exact-head CI; verify exact-main publication and unchanged existing host health.

Exit: LocalSingleNode plus necessary shared tooling only. No broken references or missing audit coverage.

## Phase 5 — Rename the surviving product completely

1. [ ] Branch from verified main. Use Git-aware renames for `GrumPlates.sln`, server, `.Client`, `.Core`, and `.Test` folders/projects. Include the retained Blazor client; the generic standalone-product rename table does not enumerate every retained project.
2. [ ] Update solution entries, project references, namespaces/usings, assembly/root namespace names, internals visibility, test source searches, reflection/assembly assertions, migration designer/snapshot namespaces, static web asset references, and runtime DLL entry points.
3. [ ] Preserve baseline migration ID `20261103193952_InitialSchema`, migration operations, schema and Books semantics. Keep embedded password-resource names synchronized with their loader string as specified in execution packet section 6. A naming change must not generate an empty/replacement database or a new initial migration. Build a migration bundle and compare its ordered migration IDs with the pre-rename list.
4. [ ] Update Dockerfile COPY paths/entry point, Compose/build scripts, Tailwind output path in `RitzauDemo.Client/package.json`, browser launcher paths, CI paths, report tooling, release bundle commands, Dependabot, quality settings and editor configurations.
5. [ ] Rename image label namespace `com.grumplates.*` to `com.ritzaudemo.*` consistently in image construction, manifest writer/validator, release verification, smoke tests, deployment audit, retention ownership checks and fixtures. Do not change shared cross-repository ownership labels or locks.
6. [ ] Update UI title, navigation branding and product descriptions. Keep Books routes, entities, sample assets and tests as the temporary product example. No visual redesign or new product feature is required here.
7. [ ] Search case-insensitively for `GrumPlates`, `grumplates`, old assembly paths and `Books`. Classify matches: provenance/shared identifier, retained sample feature, or defect. Books matches are expected. Old product names should remain only where justified.
8. [ ] Restore/build/test using `RitzauDemo.sln`; run format verification, full local gate, migration bundle check, Blazor packaged smoke and affected browser tests. Compare API surface before/after using surviving .NET metadata tooling; do not run a deleted React `api:check` script.
9. [ ] Merge via exact-head CI; verify exact-main release metadata and executable identity use the renamed product.

Exit: the retained code and operational paths use RitzauDemo consistently; database migrations and Books behavior are preserved.

## Phase 6 — Finish product documentation and repository hygiene

1. [ ] Replace README and `docs/Requirements.md` with current product purpose, Books placeholder status, Blazor-only operation, supported deployment target, local commands, tests, account policy, backup/rollback links and one template provenance link.
2. [ ] Remove inherited template plans/support files and completed fork/clone guides from RitzauDemo after their steps are embodied in current product docs. Keep `Plans/README.md` and this sanitized product plan.
3. [ ] Update `AGENTS.md` for new paths and gates. Retain stylesheet rebuild rules, PR requirements, exact-head/exact-main verification, shared-host safety, secret handling, and scoped cleanup.
4. [ ] Deliberately keep or remove optional quality/editor/agent material with its consumers. Do not turn optional quality tools into new blockers or remove useful checks as accidental cleanup.
5. [ ] Review full diff, tracked file list and secret-sensitive paths. Ensure no source `.env`, private state, runtime data, backup, generated node facts, SSH key, token or old repository remote config was copied into commits.
6. [ ] Verify required branch checks/rules and Actions settings in the public target repository through API. Configure supported protection rather than assuming repository rules were cloned.
7. [ ] Verify only intended product workflows/schedules remain enabled, GHCR package ownership is correct, and `gh` defaults to RitzauDemo.
8. [ ] Use the Markdown-only exception only for genuinely documentation-only PRs. Mixed changes require the complete code gate. Complete normal PR integration and record main verification.

Exit: a new contributor can run, test and operate RitzauDemo without template-only paths or private machine information.

## Phase 7 — Deploy and verify the independent LAN app

During execution, this phase is a required part of the requested RitzauDemo outcome after Phases 0–6 pass. The user has explicitly included RitzauDemo deployment in scope; do not ask for redundant scope confirmation. Complete the mandatory operator bootstrap handoff when applicable. Recheck shared-host capacity and backups close to deployment.

1. [ ] Revalidate candidate ownership/collisions, backup freshness, host workload, heavy-build admission and runner identity. Confirm original apps' baseline health.
2. [ ] Complete supported per-app setup and controller handoff without reinitializing the host. Use a distinct controller alias only if required; preserve current working SSH trust. Verify hostname and install user through pinned trust.
3. [ ] Check final rendered settings: unique Compose project/resources, loopback app/PostgreSQL/Redis bindings, dedicated LAN listener, independent credentials and DP keys, accounts registration/seeding disabled, observability disabled.
4. [ ] Select the full SHA of a successful main publishing run. Verify `publish-main` succeeded on its exact attempt and the GHCR image, payload, receipt, migration bundle hash and manifest all agree on repository, SHA and `BlazorAuto` / `BlazorAutoV1`.
5. [ ] Dispatch only `CD - Deploy LocalSingleNode` from trusted main for `app_instance=primary`, exact `target_sha`, and migrations enabled for the fresh database. Immediately record run ID and inspect that same run through completion. Do not redispatch on watcher failure.
6. [ ] Require workflow acceptance and release identity verification. Confirm database initialization, actual image digest, profile pair and persisted current marker. Setup or container startup alone is not deployment success.
7. [ ] Run independent controller acceptance using renamed `Scripts/Test-DeployedSite.ps1`, manifest frontend/acceptance values, selected LAN address and listener port. Verify actual rendered Books UI, assets, Blazor interactivity and account route behavior in a browser.
8. [ ] Create and verify this app's backup. Validate recovery using its documented isolated replacement-database procedure; never overwrite a live database to test recovery.
9. [ ] Recheck existing Books/React Books/CarbonCalculator application health and runner registration. Record that no existing service, data volume or public route was adopted or replaced.
10. [ ] Document rollback to a previously published immutable release after subsequent deployments. For first deployment failure, preserve diagnostic run/volumes and stop only this app's owned resources if needed. Do not invent a rollback release that does not exist.

Exit: independently accepted LAN Books demo, correct release identity, verified backup/recovery evidence, and healthy neighboring apps.

## Phase 8 — Add the dedicated public hostname later

1. [ ] Obtain the hostname and explicit public-exposure authorization. This decision does not block cloning, cleanup, runner work or LAN acceptance.
2. [ ] Inspect existing Cloudflare access privately before requesting credentials. If authentication is unavailable, operator supplies scoped access through the supported protected flow, never chat.
3. [ ] Follow `Deployment/LocalSingleNode/PublicSetup.md`. The agent creates/reconciles the dedicated owned tunnel, DNS record, unique origin port, protected connector credential and app-owned Caddy site. Do not ask the operator to construct DNS/tunnels manually when the tools can do it.
4. [ ] Preserve existing demo routes/tunnels. Keep tunnel ownership state and credentials outside Git. Validate intended hostname and ownership before applying changes.
5. [ ] Deploy the exact published main release through the supported workflow, record run ID, require LAN and HTTPS acceptance, verify certificate validation and a real browser interaction.
6. [ ] Update the application public origin and any enabled account callbacks consistently. Registration remains disabled unless separately requested. SMTP/OAuth are not prerequisites for the initial Books demo.

Exit: dedicated public HTTPS hostname works and neighboring sites remain healthy.

## Manual requirements and unresolved decisions

### Certain operator step if the current per-repository setup route is selected

The operator must authorize setup in the native-node context and run the exact bootstrap command when setup reports `actor=human`. This is a repository policy requirement, not a missing SSH capability. `Deployment/LocalSingleNode/AgentSetup.md` says: “For `human`, show `human_message` and `command` verbatim and stop” and “Never run sudo yourself”. The installer also requires the native operator bootstrap context. Do not bypass this by fabricating `SUDO_USER`, installing as root through SSH, or changing sudo policy.

Prepare and validate all inputs before asking for this step. Existing controller access and GitHub administration work, so they are not manual blockers. Creating an organization, making another GitHub account, manually registering a runner through copied tokens, and manually creating DNS/tunnels are not required.

### Remaining decisions

- Select the dedicated public hostname before Phase 8. Deferred by the user; no need to block earlier work or ask repeatedly.
- Runner architecture is settled: preserve personal-account repository URLs, use repository registrations on shared hosts, and coordinate heavy execution. No organization or transfer decision remains.
- Choose GrumPlates/RitzauDemo build admission after read-only inspection of actual `<deployment-node>` producers. Broader routing and other project changes are excluded. If safety requires an excluded change, report the dependency and keep new heavy CI disabled.
- No other product questions are required to start. Preserve history, keep Books and account behavior, perform full rename, and avoid introducing product features during bootstrap.

## Final completion checklist

- [ ] No out-of-scope repository, runner, deployment, cluster or schedule changed. ImprovedDb and every other project retain their existing topology.

- [ ] New origin/default repository verified; GrumPlates retains both frontend profiles and all deployment targets. Only its separately reviewed CI admission integration may change.
- [ ] Product main has integrated plan and all cleanup changes, with applicable exact-main checks green.
- [ ] React app/toolchain removed; Blazor client/Tailwind retained; unsupported profiles rejected.
- [ ] Only RitzauDemo is simplified to LocalSingleNode plus needed shared tooling. GrumPlates and other projects retain their deployment targets; product audit and gate remain effective.
- [ ] Solution/projects/namespaces/runtime paths/labels use RitzauDemo, with intentional exceptions documented.
- [ ] Books works; account routes remain available; registration and account seeds stay disabled.
- [ ] RitzauDemo runner verified; GrumPlates/RitzauDemo admission works, other actual host producers proven safe without modifying them, and peak memory evidence recorded.
- [ ] No private installation values or credentials in Git history added by this task.
- [ ] Authorized deployment independently accepted; backup/recovery evidence and immutable release identity recorded.
- [ ] Public hostname completed only when chosen and authorized; otherwise explicitly marked deferred.
- [ ] Finished branches/worktrees and task-only resources removed through supported scoped cleanup after integration/ownership proof. Preserve primary checkouts, keys, private configuration, app data and backups. Record deleted branch SHAs.

## Progress record

For each phase, fill in: date; branch; full PR-head SHA; local gate commands/results; PR URL; integrated main SHA; exact CI run/attempt; sanitized result; next action. Leave unchecked work unchecked. Keep installation-specific evidence private and put only sanitized conclusions here.

## Suggested handoff prompt

> Execute `Plans/RitzauDemo-Blazor-SingleNode-Bootstrap.md` with its `Support/RitzauDemo-ExecutionPacket.md`. Read applicable agent rules first. Verify the source inventory before making changes; preparation tests are not the full implementation gate. Follow the initial-import bootstrap order, then use one reviewed PR per phase. Change only RitzauDemo and the narrowly specified GrumPlates CI admission integration. Preserve other projects and all existing deployment topologies. Complete RitzauDemo deployment and independent acceptance, not only cloning. Stop at the mandatory operator bootstrap handoff or a concrete unresolved safety/access dependency. Do not solve a blocker by changing another project or skipping checks. Record phase evidence and keep unchecked work visible.
