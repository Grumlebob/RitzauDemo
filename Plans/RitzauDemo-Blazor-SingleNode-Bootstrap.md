# RitzauDemo: Blazor-only clone and safe single-node bootstrap

Status: revised and approved for resumption on 2026-11-08. The seed import and native clone are complete. The user approved a narrowly scoped CarbonCalculator native-CI admission change, resolving the previous scope conflict. GrumPlates and CarbonCalculator admission branches passed their full local gates. The stale CarbonCalculator dispatch was reconciled on 2026-11-08 after the user's full-authority instruction: run `42107307607` and check suite `114100632846` are completed/cancelled after confirming the deleted branch, startup-failure attempt 1, and zero jobs/check runs. GrumPlates PR #450 first failed exact-head CI in the cancellation fixture. Fix `e62686441bf5190654a823578de62d0ef8dd7036` passed the complete local gate and is pushed to PR #450 at exact head; CI run `42526856844` and CodeQL run `42526856859` passed; PR #450 merged at 2026-11-08T22:04:57Z as `de6cfaec8d86373e14fee7a912122e815ddf4ce0`. Exact-main CI run `42528402393` passed `validate`, `publish-main`, and `build-test-push`; CodeQL run `42528402282` passed. Before push, old queued Dependabot validation was canceled and its auto-merge workflow was reconciled; bot PRs remain open and unmerged. RitzauDemo's existing-host safety adaptation passed the full local gate on code SHA `3c087a5f44533a31faa4d1d8cd13b48968c5e7f8`; docs-sync SHA `23886457310bacfa8797221a054db85c78288aef` also passed `repo-gate` and every `docs/Test.md` command. Draft PR #5 remains open and in draft; Actions are disabled and no product CI has started. On 2026-11-08 the user narrowed RitzauDemo's product-behavior tests for the one-week demo to focused Books CRUD unit/integration coverage and, if time allows, one optional CRUD E2E. On 2026-11-09 the user clarified that CRUD is the only product-behavior test scope to showcase: remove Grafana/observability and other extra suites, with no standalone account/auth test suites. Keep repository-admission, release, deployment, backup, and host-safety gates, and leave GrumPlates/CarbonCalculator test scope unchanged. The initial scope amendment was pushed to PR #5 in docs-only commit `b0bee356ca9fc1c97313ac4ed1315b4ae9ab7d8a`; the clarification was pushed in docs-only commit `af1cbbfb72db434b7856fda1001807b54833f018`. No native bootstrap or RitzauDemo deployment has occurred.

### CarbonCalculator admission follow-up merged — 2026-11-09

- CarbonCalculator PR #477 follow-up `3e2b22d21dde9eba5b54a345c0413170c8a7af22` passed the full local gate at source-bound report `641f592f4b054e24ade17da15871a3ba/validation.json` (937 passed, 44 documented skips, 0 failed; report SHA-256 `7f576047a317bfcc6adfd53a807f6207ce91c904e84a13f6cae8ba67ceb79f72`). Exact-head CI run `42535851049` passed `validate` and `build-test-push`; PR #477 merged at `2026-11-08T23:58:17Z` as `42d14b3db480956f751c88b58d41c52d929b1df0`. Exact-main run `42537698202` passed `validate`, `publish-main`, and `build-test-push`. Main tree `1fab219e0b1b47e27ce0a4213041010f01fb032d` equals the reviewed PR-head tree. No application deployment or existing-site change occurred.

## Fresh-chat starting point and approved amendment

The user explicitly approved the coexistence approach on 2026-11-08 and requested this updated handoff. This workspace plan and its adjacent support packet supersede the older copies committed in the RitzauDemo seed. Do not repeat the initial import, generate another seed, force-push, recreate the existing clones, or treat CarbonCalculator admission as an excluded change.

- Objective: keep existing sites serving on the shared native node and add the independently deployed RitzauDemo site. Runtime applications run concurrently; one resource-heavy native build/test block runs at a time. Queueing a build must not stop a site.
- Approved repository scope: RitzauDemo product work, GrumPlates admission integration, and CarbonCalculator **native CI admission only**, including directly required helper, workflow, tests, audit and documentation changes. CarbonCalculator application code, runtime deployment, runner registrations, route selection, desktop/native fallback, secrets and schedules retain their existing behavior.
- Product, template and CarbonCalculator primary checkouts are sibling repositories in the shared workspace. Inspect current instructions, claims, worktrees, open PRs and live jobs before editing. Use a dedicated branch/worktree per repository; do not use another task's checkout or build capacity.
- Verified seed/main: `5931a4a872de39db8a85ac886bcdf40097bc99db`. Full original local gate passed on this exact commit. Template baseline: `ee9af0b985f86c9568cbf077aeb20336841446de`. Researched CarbonCalculator main: `0d255392b99eb635030f9c90a03de8a6d37d227c`. Fetch and inspect current state; these are historical reference SHAs, not authority to overwrite newer work.
- The native product checkout was previously verified clean on `main` at the seed SHA with the correct origin and `app_name=ritzaudemo`. Host identity, login, backup and collision evidence remain in private installation notes. Follow private machine guidance before SSH or WSL work. Keep private paths and machine facts out of public commits.
- RitzauDemo repository Actions are disabled; no product runner is registered. GitHub-hosted Dependabot automation nevertheless created four PRs at the last inspection. Recheck them; leave dependency upgrades out of bootstrap and ensure old workflow versions cannot reach the new native runner during rollout.
- Initial revalidation is complete for this resumed session. Preserve the checked baselines and host evidence; continue the three-repository admission coverage in Phase 2A and support packet section 3. The original CarbonCalculator scope blocker is resolved by approval, not by completed implementation.
- The previous goal was marked `blocked`; this resumed session created a new active goal with the approved scope. Preserve unfinished phases and the mandatory native operator handoff.

The latest read-only idle snapshot showed existing applications serving with low memory pressure. Exact host measurements remain private. This supports a measured rollout; it does not prove peak build capacity or justify unlimited workloads. Old allocated swap alone is not evidence of current thrashing; record swap-in/out rates and memory pressure during acceptance.

## Execution packet and preparation completed now

Read [Support/RitzauDemo-ExecutionPacket.md](Support/RitzauDemo-ExecutionPacket.md) before implementation. It fixes the initial import/runner dependency cycle and specifies CI admission, extraction paths, product defaults, Node pin and rename invariants. Its concrete decisions supersede open-ended implementation suggestions below; the scope boundary still applies.

- [x] Generate [the source dependency inventory](Support/RitzauDemo-SourceInventory.json): exact SHA, normalized hashes, 85 React consumers, 21 removed-target consumers, 15 release-label consumers, 9 setup/admission files and 3 migration files.
- [x] Verify the inventory against unchanged source with [the read-only generator](Support/RitzauDemo-SourceInventory.py).
- [x] Run 69 offline setup, collision/ownership, settings and frontend-profile fixture checks successfully. [The reusable preflight](Support/RitzauDemo-Preflight.sh) includes correct WSL .NET PATH setup, candidate schema validation and existing lock-helper regression tests.
- [x] Recheck native workload/listeners/networks and candidate app paths without changing resources. An existing CarbonCalculator runner service is present; live heavy-work eligibility remains a specific safety check, not authorization to change it.
- [x] Resolve setup ordering: minimal product identity/resource defaults belong in the first main import, before normal PR work. A feature-branch bootstrap cannot pass the existing clean-main gate.
- [x] Identify hidden dependencies: Node version parsing from the React Dockerfile stage in both CI and smoke; lifecycle tests selecting React; an existing Common Ansible wrapper; password-resource lookup string; fixed initial migration ID.

Keep all four support files with this plan when copying it into RitzauDemo. These checks do not prove the full application gate, live collision gate, memory admission, bootstrap effects or deployment acceptance.

## Purpose and confirmed decisions

Create an independent product repository at https://github.com/Grumlebob/RitzauDemo from GrumPlates. Keep the Books example and its sample assets initially. Remove React, retain Blazor Interactive Auto, retain accounts with registration disabled, support only LocalSingleNode, and fully rename the surviving solution, projects, namespaces, and product identity to RitzauDemo.

Repository ownership and canonical URLs must remain under Grumlebob. Do not transfer repositories or introduce an organization migration. CI and deployment for the initial RitzauDemo setup remain on the shared native node, using a separate repository runner registration and host-wide admission for heavy work. Share machine capacity across projects without claiming that GitHub offers a personal-account runner pool. Keep all existing applications, databases, runners, credentials, DNS, and backups intact. The product will eventually have its own public hostname; selection is deferred. Establish LAN deployment first.

Preserve template Git history. Keep an explicit `upstream` remote for optional manual template imports; do not configure automatic upstream merges. Do not replace any existing demo site.

Execution began at the user's explicit request on 2026-11-08. Carry the plan through repository setup, CI, application setup, deployment and independent acceptance; do not treat deployment as optional. Preserve the specific operator bootstrap requirement below. Stop implementation if an out-of-scope producer prevents safe admission; do not solve that blocker by changing another project.

## Hard scope boundary — product work plus three-repository CI admission

- Eligible repositories are `Grumlebob/RitzauDemo` for product work, plus `Grumlebob/GrumPlates` and `Grumlebob/CarbonCalculator` solely for the admission integration defined here. The CarbonCalculator exception is explicitly user-approved; it is not permission for unrelated work.
- RitzauDemo receives all product customization and deployment: React removal, single-node-only simplification, rename, identity, settings, runner registration, its own application/PostgreSQL/Redis resources on the shared native node, CI/CD configuration, backups, deployment and independent acceptance. Phases 1 and 3–8 refer to RitzauDemo unless explicitly stated otherwise.
- GrumPlates remains a reusable template with both frontend profiles and its existing deployment targets. Limit any GrumPlates changes to the reviewed CI build-admission integration needed for its runner to coexist safely with RitzauDemo. Do not remove React or cluster/cloud support from GrumPlates, change its deployment routing or identity, or dispatch a GrumPlates deployment.
- CarbonCalculator changes are limited to making its native CI/manual heavy work participate in the shared host lock and preserving its existing tests, reports, release provenance and routing. Do not change its product logic, runtime resources, CI topology, registrations, secrets, environments or schedules. ImprovedDb, Portfolio, JobHunt and KinoJoin remain excluded in full; preserve their existing topology.
- Shared-host inspection may read existing workloads, listeners, ownership and health solely to protect neighbors. Discovering another producer does not authorize changing it. If safe build admission requires an out-of-scope change, stop that implementation phase and report the exact dependency instead of expanding scope.
- On the shared native node, add RitzauDemo-owned application resources and the narrowly reviewed admission integration for GrumPlates/RitzauDemo/CarbonCalculator. Preserve existing app data, runners, routes, tunnels, credentials, backup policy, Docker configuration and services. Do not restart or reconfigure shared services as a side effect without reviewing the actual effect and obtaining separate authorization for any disruption.
- No organization, URL transfer, shared runner pool rollout, multi-project cleanup, runner fleet redesign, or unrelated cluster operation belongs in this plan.

## Plan location and private installation facts

This file currently lives in the parent workspace's `Plans/`, outside the template checkout. Do not modify or publish GrumPlates merely to deliver this plan. The only planned template code change is the narrowly scoped CI admission integration described above, through its own reviewed PR. When creating RitzauDemo, copy a sanitized version into its own tracked `Plans/` and retain progress there.

The host alias above is a local planning input. Before publishing this plan, replace private machine identifiers with `<deployment-node>` and keep actual host, address, login, observed neighbor inventory, installation-specific overrides, tunnel state, and key paths in ignored/private installation notes. Generic non-secret product resource defaults may live in the product inventory, as in the canonical primary setup route; see the execution packet. The public repository must not inherit machine-specific details from this workspace. Product/repository names and generic placeholder examples are safe to track.

## Research already completed

- [x] Read workspace rules, caveman skill, RTK integration, machine facts, and template `AGENTS.md`.
- [x] Verify target repository through authenticated GitHub API: public, empty, caller has admin access, Actions enabled, no registered runners, no rulesets returned.
- [x] Verify template primary checkout is clean on `main`, with no additional registered worktrees.
- [x] Verify local template HEAD matches GitHub `main`: `ee9af0b985f86c9568cbf077aeb20336841446de`.
- [x] Read fork, standalone-product, frontend-profile, single-node-only, local-gate, release, controller-SSH, readiness, and setup guides.
- [x] Verify passwordless SSH and native GitHub repository administration access on the selected host. Never print tokens.
- [x] Verify the existing GrumPlates deployment runner is online. Its repository registration cannot run RitzauDemo workflows.
- [x] Inspect the runner installer: it supports a separate service and directory per application and refuses foreign registrations.
- [x] Inspect a read-only capacity snapshot on the target node. Host metrics, disk and existing Books, React Books and CarbonCalculator health were recorded privately; the idle snapshot is not peak-capacity evidence.
- [x] Find critical coupling: CI builds both frontend profiles; required audit, smoke, capacity, and Ansible scripts still live under LocalCluster; shared tests reference removed targets.

These are planning observations, not deployment acceptance. The target import and separate native clone are complete. Full `docs/Test.md` local gate passed on seed SHA `5931a4a872de39db8a85ac886bcdf40097bc99db`; the four profile suites passed with zero failures (197/149 for BlazorAuto and 189/135 for React; only 15 documented opt-in E2E skips in each non-account suite). The earlier 69-fixture preflight also passed. No runner installation, node bootstrap, deployment, or app cleanup has occurred. The canonical native collision parser now passes; deploy-user readiness remains unavailable, and candidate resources are not reserved.

## Read these canonical files before their implementation phase

Paths below are relative to the template or initial product clone. Read them before removing them; retain corrected product operations documentation afterward.

1. `AGENTS.md`, `docs/HowToForkThisRepo.md`, `docs/StandaloneProduct.md`.
2. `docs/FrontendProfiles.md`, `docs/Frontend/React.md`, `docs/Accounts.md`, `docs/Test.md`, `docs/Testing/Browser.md`.
3. `Deployment/LocalSingleNode/SingleNodeOnlyClone.md`, `HowToDeployLocalSingleNode.md`, `Readiness.md`, `Operations.md`, `ControllerSsh.md`.
4. `Deployment/LocalSingleNode/AgentSetup.md`, `Scripts/lib/setup_status.py`, `Scripts/lib/setup_next.py`, `Scripts/lib/bootstrap.py`, `Scripts/lib/runner.py`.
5. `Deployment/Common/ReleaseContract.md`, `.github/workflows/ci.yml`, `Scripts/CI/workflow_policy.py`.

The existing React-only guide describes the opposite frontend removal. It is not a Blazor-only recipe. Do not follow its deletions literally.

## Execution rules and evidence

- Work in the existing product checkout and specifically owned native checkout. Use separate GrumPlates and CarbonCalculator task branches/worktrees only for their scoped admission integrations. Never rename the template checkout or point another repository's origin at RitzauDemo.
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

Use the existing supported per-repository installer for RitzauDemo and implement effective host-wide build admission in Phase 2A. On the shared native node, admit only one heavy CI/manual build at a time until measured capacity supports more. Other runner services may remain online while their jobs wait for admission. Waiting listeners and jobs still consume some memory, so measure baseline and queued-job overhead as well as build peaks. Do not start heavyweight setup, dependency installation, Docker builds or test containers before admission.

Do not add registrations for other projects or build a general runner pool. This plan adds only RitzauDemo's registration and compatible admission calls in the existing GrumPlates and CarbonCalculator workflows. Existing projects keep their CI/deployment hosts and routing. The template checkout is `GrumPlates`, not the historical `BlazorAutoApp` path.

A central dispatcher, organization migration, dynamic runner scheduler, or multi-host rollout is outside scope. Coordinate the three known native producers through one shared lock. A newly discovered producer outside these three remains a scope boundary; inspect it read-only and report a concrete dependency if it prevents safety. Do not pause its runner, move its jobs or alter its cluster.

Phase 2 covers GrumPlates/RitzauDemo/CarbonCalculator native build coexistence. No fleet-wide runner change is authorized. CarbonCalculator native build eligibility is now an implementation requirement, not the former out-of-scope blocker.

Source: [GitHub runner scopes](https://docs.github.com/en/actions/concepts/runners/self-hosted-runners).

## Phase 0 — Refresh facts and establish isolated work areas

1. [x] Confirm the target is empty. No target branch existed before import.
2. [x] Recheck template cleanliness and GitHub main SHA. The exact baseline is `ee9af0b985f86c9568cbf077aeb20336841446de`; source checkout remained unchanged.
3. [x] Confirm the target product workspace was new and contained no other task or private inputs. No pre-existing folder was removed.
4. [x] Create a private installation record outside tracked files for host, neighbor, backup, capacity and candidate evidence.
5. [x] Recheck existing app health, backup restore verification, runner activity, Docker workload, disk, memory and host identity. Existing applications remained healthy.
6. [x] Inventory occupied TCP listeners, Docker published ports/subnets, Caddy sites, app and backup roots, runner services and installed app records without exposing credentials.
7. [ ] Select distinct product candidates and validate them through the canonical collision/readiness parser. The canonical port, subnet, directory and Caddy collision checks passed on the native host. The deploy-user readiness script remains unavailable because deploy-user SSH access is not configured. Candidate defaults remain unreserved.

Exit: known source SHA, empty target, isolated destination, and privately recorded candidates. Canonical collision checks passed; deploy-user readiness remains outstanding. No deployment is authorized by readiness alone.

## Phase 1 — Seed the independent repository with minimal bootstrap identity

Follow execution packet section 2 exactly. The target is empty and has no runner, so bootstrap identity belongs in its first import, with full local validation. Subsequent code/configuration changes use normal PRs and exact-head CI.

1. [x] Verify the target was empty and disable target Actions before import. Template Actions were not changed.
2. [x] Clone the selected template SHA with history into the isolated product directory. Rename `origin` to `upstream`, add the product `origin`, and set the `gh` default to RitzauDemo.
3. [x] Verify both remotes, baseline SHA and `gh` default. No template branches/tags were pushed and the source checkout remains unchanged.
4. [x] Create the minimal bootstrap seed on local `main`: product defaults, `app_image=ghcr.io/grumlebob/ritzaudemo`, `migration_bundle_name=ritzaudemo-migrate`, `migration_runtime=linux-x64` and `frontend-profile.txt=BlazorAuto`. Account registration/seeding and deployed observability remain disabled.
5. [x] Keep internal GrumPlates project names, both frontends and all deployment targets intact. Copy the sanitized plan and support packet. No cleanup or rename was part of the seed.
6. [x] Use one stable `App:Name` identity across settings and deployment; caching, Data Protection and account identity resolve to `ritzaudemo`. Remaining branding changes stay for the first normal PR.
7. [x] Generate an independent ignored local `.env`; no source credentials/data were copied. Development ports are distinct.
8. [x] Run the full unchanged `docs/Test.md` gate on exact seed SHA `5931a4a872de39db8a85ac886bcdf40097bc99db`. All commands passed. Both profiles and account suites passed; only documented opt-in E2E skips remained.
9. [x] Push local `main` once with template history and the validated minimal seed while target Actions were disabled. No tags or other branches were pushed.
10. [x] Clone target `main` separately on the native node. Clean `main` equals `origin/main` at the seed SHA; origin, `app_name=ritzaudemo` and host identity were verified.
11. [ ] Prepare and review the exact operator bootstrap handoff. Complete Phase 2 before enabling heavy CI. A reviewed package/Avahi safety adaptation is local but not integrated. Do not present the stock command or use a direct-main exception; follow the normal checked integration route and resolve the CI dependency first.
12. [ ] After runner/admission/variables are ready, enable target Actions. Make remaining identity/branding/readme updates in the first normal PR, require exact-head `build-test-push`, merge, and verify successful main-push CI/publication. This supplies initial setup's push provenance.

Exit: correct import, validated native bootstrap identity and, after Phase 2, green first normal identity PR/main release. No product deployment yet. Do not repeat the import exception.

## Phase 2 — Make runner coexistence safe before enabling CI

### A. Coordinate all three native build producers on the shared node

Two runner services can start jobs independently. Idle service memory is not the primary risk; simultaneous builds, Testcontainers, Docker image builds and browser suites are. GitHub concurrency groups are repository-scoped. Existing deployment/browser locks do not automatically serialize all CI validation.

1. [x] Read-only inspect the heavy producers actually running on the shared node. Determine whether existing admission controls already cover a third-party producer on this host. No other cluster was changed.
2. [x] Build a static coverage table for GrumPlates, RitzauDemo and CarbonCalculator: eligible native jobs/manual commands, heavy setup, dependency install, restore/format/build, Testcontainers, browser/image smoke, migration/publication builds, resource lifetime and cleanup. See [the support packet's Phase 2A table](Support/RitzauDemo-ExecutionPacket.md#phase-2a-static-coverage-table--branch-review-2026-11-08). The table flags toolchain-install behavior for measurement and does not claim live lock or queue proof.
3. [ ] Reuse `Deployment/Common/Scripts/with-node-browser-lock.sh` and `/tmp/localsinglenode-ci-browser.lock` on the intended native host. Verify all runner accounts open the same lock inode with compatible ownership/permissions. Do not introduce a second competing lock or a detached holder across Actions steps. Remove nested acquisitions when an outer block owns admission. Preserve CarbonCalculator's lock timing/report extensions and test them; do not overwrite its helper blindly.
4. [ ] Prepare one admission PR in GrumPlates and one in CarbonCalculator, using their dedicated task branches/worktrees. GrumPlates retains both frontends and all targets. CarbonCalculator changes only native admission calls and directly required helpers/tests/audits/docs; its desktop selection, native fallback, application and deployments remain intact. Scope host admission through existing verified host/profile configuration; never commit the real host name or make cluster/desktop jobs depend on a lock on another machine.
5. [ ] Follow support packet section 3's staged integration and controlled first-product-PR procedure. Run the full current local gate in each repository before push. CarbonCalculator requires `pwsh -File Scripts/Testing/Validate.ps1 -Mode full` in the prepared Linux environment with a fresh successful full-gate report; the old repo-gate summary alone is insufficient. Require `build-test-push` on each exact PR head, merge normally, and verify integrated main and applicable publication. No direct-main exception is added.
6. [ ] Port compatible admission to RitzauDemo's first normal identity/CI PR. Keep its repository Actions disabled until existing native producers are integrated, runner setup is reviewed/completed, and the first-PR scheduling guard is ready. A guard must prevent the seed workflow and stale Dependabot branches from reaching a live heavy runner during rollout. CarbonCalculator participation is approved and required; do not stop merely because it needs these scoped changes.
7. [ ] Use bounded waits, visible queue status and automatic release after command exit and task-owned cleanup. Keep lightweight status/checkout/report work outside heavy blocks. Keep resources that survive between commands within one admitted shell block through cleanup. Do not wrap lock contention tests in the same lock. Verify timeout, failure and cancellation behavior; reconcile orphan work before another heavy block starts.
8. [ ] Recheck available RAM, swap-in/out, memory pressure, disk/inodes on actual Docker/workspace roots and application health after admission. Retain readiness thresholds. Record idle listeners and queued-job overhead. Do not infer saturation from old allocated swap or infer peak capacity from an idle snapshot.
9. [ ] Prove the shared queue using lightweight contenders from the three repository contexts, then a complete admitted native validation/publication workload while all existing sites serve requests. Observe maximum concurrent heavy blocks = 1, lock wait durations, peak memory, pressure, swap rates, OOM events, container restarts and HTTP health/latency. Do not deliberately run uncontrolled simultaneous heavy builds to test safety.
10. [ ] Record three-repository PR URLs, exact tested/integrated SHAs and CI attempts, complete coverage table and private host evidence. Approval resolved the scope conflict; build safety is proven only when implementation passes. If a single admitted build cannot fit with serving apps, measure and report a specific remediation; do not silently move builds, change routing, lower checks or redesign the runner fleet.

### A2. Isolate and budget the new runtime stack

1. [ ] Verify unique Compose project, app/PostgreSQL/Redis containers, networks, volumes, ports, data roots, credentials, Data Protection keys, Caddy site and backups. Published database/cache/app backends stay loopback-only; expose only the chosen LAN listener through the supported proxy route.
2. [ ] Add measured CPU/memory/PID budgets for RitzauDemo through its tracked settings, supported Compose rendering, validators and tests in the first suitable normal product PR. Choose values from the full app/migration/smoke workload plus headroom; do not copy idle usage as a hard limit. Verify effective Docker `HostConfig` and runtime limits, not just YAML. Preserve a host reserve for OS, existing services and one admitted build.
3. [ ] Bound RitzauDemo Redis allocation consistently with its container memory budget and preserve intended cache/persistence behavior. Verify .NET/PostgreSQL startup, migrations and request load within the chosen limits. Container OOM kills or restart loops are failures, not acceptable containment evidence.
4. [ ] Existing application limits remain unchanged in this task. Record their observed load; change a neighbor's runtime budget only under separate explicit scope. Limiting a runner service alone does not bound Docker-daemon-created build/test containers; admission and actual container budgets must cover those workloads.
5. [ ] Deploy only after all later product phases pass. Phase 7 must repeat the coexistence workload with the new site running and verify independent acceptance plus backup/recovery. The shared build lock never surrounds HTTP request handling or stops an application while a job waits.

### B. Register RitzauDemo's own runner

1. [x] Prepare a separate native checkout for RitzauDemo using its own origin. The existing Books checkout was not repointed.
2. [x] Use the canonical tracked product defaults: `app_name: ritzaudemo`, distinct resource candidates and `observability_enabled: false`. Private machine facts and credentials remain outside Git; no partial private-settings route was added.
3. [ ] Read setup status and bootstrap code against the prepared candidate. The process must add a new app to an existing host, preserve every existing registration/site, and keep the current hostname. Never run fresh-node setup with a new hostname.
4. [x] Resolve the setup authorization boundary before mutation. `AgentSetup.md` requires an explicit native-node setup request and an operator-run root command. The controller did not run the state machine or bootstrap through SSH.
5. [ ] Read execution packet section 8 and review shared-host effects before presenting the operator command. Prepare everything it needs first: correct checkout/ref, validated candidate inputs, repository authentication status, collision/backup/capacity evidence, and reviewed shared-host effects. The validated minimal identity must already be in the first main import from Phase 1. Do not attempt feature-branch bootstrap or bypass the clean-main gate.
6. [ ] If setup lacks CI capacity, use the documented runner/bootstrap-first route. Finish customization via a normal exact-head CI PR afterward. Do not automatically execute `setup-next-step` actions that dispatch CI/deployment before their prerequisites and authorization are satisfied.
7. [ ] At the canonical `human` step, present the exact generated command and message. The operator runs it natively. Do not fabricate a generic sudo command, run sudo for the operator, copy registration tokens, or add sudoers rules.
8. [ ] Request the installer’s CI option when CI and CD use this same registration. Expected identities from the inspected installer are:

   - Runner name: `<deployment-node>-ritzaudemo`.
   - Directory: `/home/deploy/actions-runner-ritzaudemo`.
   - CD label: `localsinglenode-ritzaudemo`.
   - CI label: `ci-ritzaudemo` when installed with `--ci-runner`.

9. [ ] Verify the service is online in the RitzauDemo API and its repository URL, name, labels, service account, and workspace are correct. Existing Books runner must remain registered and healthy in GrumPlates. A runner does not need an organization to share a physical machine with another registration.
10. [ ] Set RitzauDemo host/CD variables deliberately: `CI_RUNNER_HOST=<verified-hostname>`, `LOCALSINGLENODE_RUNNER_LABEL=localsinglenode-ritzaudemo`, `LOCALSINGLENODE_HOST=<verified-hostname>`, `DEPLOY_TARGETS=localsinglenode`. Final canonical `CI_RUNNER_LABEL` is `ci-ritzaudemo`, but apply the temporary quarantine/bootstrap variable procedure in packet section 3 before making it schedulable. Leave `ADDITIONAL_FRONTEND_PROFILES` unset. Do not report canonical setup complete while temporary variables remain.
11. [ ] Inspect all workflows for fallback labels, deployment environments, permissions and schedules. Restrict deployment to trusted main. Retain rejection of foreign-fork PR code on deployment runners. Do not expose this privileged shared host to arbitrary PR workflows.
12. [ ] Follow the enforced first-PR scheduling guard in packet section 3. Enable Actions only when existing native producers are admitted and only the prepared admitted product workflow can reach the new runner. Require exact-head checks and exact-main push publication, then retire the temporary bootstrap mechanism through a normal checked PR. Reconcile stale bot workflows before enabling the canonical label. Manual dispatch does not replace initial main-push provenance.

Exit: RitzauDemo's runner is online; GrumPlates/RitzauDemo/CarbonCalculator native heavy work participates in the shared queue; routing and existing sites are preserved; new runtime budgets are validated; identity main and publication are green. No product deployment yet.

## Phase 3 — Remove React and slim product tests while preserving Blazor

Apply the approved lean product test scope below in this phase. The broad inherited test suite is not a product requirement for this one-week demo.

1. [ ] Branch from verified product main. Capture the actual dependency inventory with `rg`, including hidden tracked configuration. Classify matches before deleting anything.
2. [ ] Inspect and remove React client assets/tooling under `GrumPlates.React/`, `GrumPlates/Frontend/FrontendComposition.React.cs`, `ReactFrontendHosting.cs`, `ReactFrontendStaticRoot.cs`, and `GrumPlates/Frontend/React/` after removing their consumers.
3. [ ] Keep `GrumPlates.Client/`, its npm lockfile, Tailwind, generated `GrumPlates/wwwroot/tailwind.css`, Blazor render modes, account UI, Books UI, and shared Core contracts. Removing React does not remove all Node/npm tooling.
4. [ ] Simplify `Directory.Build.props`, `Directory.Build.targets`, project references, server csproj compile/content rules, frontend composition, and configuration. Preserve publish safeguards that prevent output from overwriting source `wwwroot`.
5. [ ] Keep the tracked profile and immutable release profile pair `BlazorAuto` / `BlazorAutoV1`. Make profile validation reject unsupported React selection. Update resolver tests, manifest validation, release verifiers, acceptance scripts, frontend publication planning, and status helpers together. Keep generic shared contracts only when deliberately needed.
6. [ ] Simplify `GrumPlates/Dockerfile`: remove React dependency/assets/publish/final stages; retain `final-blazor`, non-root execution, image identity labels, and Blazor assets. Update local Compose target resolution and `Scripts/RunLocal.ps1` consistently.
7. [ ] Remove React-only browser fixtures and scripts: `Scripts/Testing/RunReactPublicE2E.ps1`, React SMTP fixture if no remaining consumer, and `GrumPlates.Test/E2E/Features/React/`. Remove React hosting-only tests; adapt mixed frontend composition/security checks only where required for the retained Blazor application.
8. [ ] Reduce product-behavior tests to the approved Books CRUD scope below. Keep any required private-book ownership/access assertion inside the CRUD integration path; do not retain standalone account/auth suites. Remove exhaustive account lifecycle, caching, observability, visual, layout, concurrency, and architecture suites that do not protect CRUD or approved operational requirements. Do not delete backend endpoints merely because React consumed them.
9. [ ] Rewrite `.github/workflows/ci.yml` to validate/build/test/smoke/publish Blazor only. Remove React npm/unit/browser/contract steps, both-profile loops, additional-profile publishing, Grafana/observability test requirements, and obsolete reports. Keep required `build-test-push`, lean CRUD unit/integration results, image smoke/release provenance as operational checks, cleanup, and failure propagation.
10. [ ] Update `Scripts/CI/validate-frontend-profiles.sh` to retain its entry point but validate only Blazor and the retained lean unit/integration results, with no unexpected skips. Remove the separate inherited account/non-account suite requirements. Update CI Python tests, release tests, report validators, and local-gate docs to match; do not simply stop calling validation.
11. [ ] Update Dependabot, CodeQL language matrix if appropriate to surviving JavaScript, editor/run settings, quality scripts, browser docs, README, and frontend documentation. Check `.claude`, `.codex`, `.qlty`, and `.run` if present. Keep Tailwind tooling needed to build Blazor styles; remove axe/Lighthouse and browser-test dependencies unless a retained build/runtime consumer requires them.
12. [ ] Search for `React`, `ReactPublicV1`, `final-react`, `ADDITIONAL_FRONTEND_PROFILES`, and removed file paths. Resolve every live dependency. Keep only justified historical/provenance references.
13. [ ] Fix both Node Dockerfile parsers and remove the React-selected Testcontainers lifecycle test and its React-only fixtures as specified in execution packet section 4. Run the updated local gate with only the lean CRUD unit/integration suite; run the single optional CRUD E2E only if it fits the demo schedule. Keep packaged Blazor smoke and required release/host-safety checks. Review test discovery, then merge via exact-head CI and verify exact-main publication.

Exit: no React app or selectable React release remains; Books CRUD works through Blazor; product tests match the one-week demo scope.

### Approved one-week product test scope — clarified 2026-11-09

Keep automated product-behavior coverage limited to the Books CRUD example:

- Unit tests cover only core CRUD validation and small deterministic rules used by that path.
- Integration tests cover one database-backed create/list/read/update/delete path. Include an ownership/access assertion within this path only where required by the existing private-book policy; do not keep separate account or authentication test suites.
- Keep at most one opt-in Playwright E2E for a signed-in Books CRUD happy path. If it cannot be made reliable within the demo week, omit it and use the required independent browser acceptance during LAN deployment.
- Remove product tests for Grafana dashboards/alerts and observability stacks, visual snapshots, Lighthouse/performance, React, elaborate responsive/navigation behavior, cross-node cache invalidation, account/email/outbox lifecycle, deep migration edge cases, and broad architecture/layout details unrelated to CRUD.
- Remove test fixtures, containers, and dependencies used only by deleted suites. Do not keep a suite merely to preserve inherited test counts.

This is the complete automated product-behavior test scope for the demo: focused CRUD unit and integration tests, plus zero or one CRUD E2E. Keep required CI admission, release, deployment, backup/recovery, and shared-host safety checks as operational gates rather than extra product test suites. Do not reduce GrumPlates or CarbonCalculator test suites. Disabled registration, private API boundaries, migration identity, release provenance, and existing-site preservation remain requirements; verify each with the smallest relevant operational or acceptance check.

## Phase 4 — Remove unused deployment targets from RitzauDemo only

1. [ ] Branch from verified RitzauDemo main. Confirm no GrumPlates file or live cluster is being changed. Inventory all `LocalCluster`, `Cloud`, `localcluster`, `cloud`, and removed path references in workflows, scripts, Python imports, tests, lint configurations, and documentation.
2. [ ] Use execution packet section 5 for exact destinations and the existing Common Ansible implementation. Extract still-needed functionality before deleting target folders. Known critical candidates under `Deployment/LocalCluster/Scripts/` include `ci-docker-smoke.sh`, `install-ansible.sh`, `audit-deployment.sh` and `Component/lib/audit_deployment.py`, capacity scripts, and their dependencies/tests. Classify and relocate to `Deployment/Common` or a product CI location as appropriate.
3. [ ] Trace `Scripts/CI/check-runner-capacity.sh` and additional-frontend publication helpers. Replace secondary-runner-specific capacity assumptions with retained shared-host safeguards. Remove additional-profile helpers if Phase 3 removed their purpose.
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
9. [ ] Recheck existing Books/React Books/CarbonCalculator application health and runner registration. Record that no existing service, data volume or public route was adopted or replaced. With RitzauDemo serving, repeat one full admitted native validation/publication workload and a waiting lightweight contender; collect peak RAM, swap rates, pressure, OOM/restart deltas and HTTP health/latency for every site. Compare with baseline, verify effective RitzauDemo limits and host reserve, and record numeric acceptance against readiness and workload requirements. Do not declare coexistence proven from idle stats or container startup alone.
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
- Implement shared native admission in GrumPlates/RitzauDemo/CarbonCalculator as explicitly approved. Broader routing and other project changes remain excluded. Keep new heavy CI guarded until coverage and capacity pass; a different excluded producer remains a concrete scope dependency.
- No other product questions are required to start. Preserve history, keep Books and account behavior, perform full rename, and avoid introducing product features during bootstrap.

## Final completion checklist

- [x] No out-of-scope repository, runner, deployment, cluster or schedule changed. ImprovedDb and every other project retain their existing topology.

- [x] New origin/default repository verified; GrumPlates retains both frontend profiles and all deployment targets. Only its separately reviewed CI admission integration may change.
- [ ] Product main has integrated plan and all cleanup changes, with applicable exact-main checks green.
- [ ] React app/toolchain removed; Blazor client/Tailwind retained; unsupported profiles rejected.
- [ ] Only RitzauDemo is simplified to LocalSingleNode plus needed shared tooling. GrumPlates and other projects retain their deployment targets; product audit and gate remain effective.
- [ ] Solution/projects/namespaces/runtime paths/labels use RitzauDemo, with intentional exceptions documented.
- [ ] Books works; account routes remain available; registration and account seeds stay disabled.
- [ ] RitzauDemo runner verified; GrumPlates/RitzauDemo/CarbonCalculator native admission works; any additional producer is proven safe; runtime limits, loaded coexistence and peak memory evidence are recorded; routing and existing sites are preserved.
- [x] No private installation values or credentials in Git history added by this task.
- [ ] Authorized deployment independently accepted; backup/recovery evidence and immutable release identity recorded.
- [x] Public hostname explicitly deferred because no hostname or public-exposure authorization was supplied.
- [ ] Finished branches/worktrees and task-only resources removed through supported scoped cleanup after integration/ownership proof. Preserve primary checkouts, keys, private configuration, app data and backups. Record deleted branch SHAs.

## Progress record

### 2026-11-08 checkpoint

- Seed: `5931a4a872de39db8a85ac886bcdf40097bc99db`; pushed once to empty RitzauDemo `main` with Actions disabled.
- Post-import automation: the Actions permission remains disabled. A GitHub-hosted Dependabot dynamic run and four Dependabot PRs appeared automatically; no RitzauDemo self-hosted CI run occurred. Leave those PRs unmerged until exact-head checks are available.
- Validation: full documented local gate passed on that exact seed commit in a clean Linux checkout. BlazorAuto suites: 197 non-account tests passed with 15 expected opt-in skips and 149 account tests passed. React suites: 189 non-account tests passed with 15 expected opt-in skips and 135 account tests passed. Remaining restore, formatting, policy, deployment, lint and actionlint gates passed.
- Native clone: clean `main` at the same SHA, correct origin and `app_name` verified. No runner installation, native bootstrap, app deployment or application cleanup occurred.
- Historical stop, superseded by later approval: CarbonCalculator could run native heavy CI outside the shared lock while its repository was excluded. The user has now approved its narrow native admission integration. Implement Phase 2A rather than repeating this scope stop. Actions remain disabled pending the controlled rollout; no build-safety proof is implied by approval.
- Outstanding: canonical candidate collision check, native operator setup handoff, Phase 2A admission, runner/variables, Blazor-only cleanup, deployment/acceptance, backup/recovery proof. No public hostname or exposure authorization was supplied; that phase remains deferred.

The sanitized plan copy in the initial seed is an execution snapshot. Update it through the first safe exact-head PR; do not make a second direct-main push.

### Approved resumption amendment — 2026-11-08

The user accepted the coexistence design: applications serve concurrently, native heavy builds queue across the three repositories, and RitzauDemo gets isolated resources and measured budgets. CarbonCalculator native-CI admission is now explicitly in scope. The latest idle snapshot supports proceeding with measured validation, while full build/load capacity remains unproven. This turn updates the handoff only; no CI integration, native setup, runner registration, runtime limit or deployment is claimed complete.

For each phase, fill in: date; branch; full PR-head SHA; local gate commands/results; PR URL; integrated main SHA; exact CI run/attempt; sanitized result; next action. Leave unchecked work unchecked. Keep installation-specific evidence private and put only sanitized conclusions here.

### Phase 2A local checkpoint — 2026-11-08

- GrumPlates admission branch `task/ci-admission-ritzau-20261108`, commit `b17c20655dfb968133db7bb704106e2fc69a7959`, is pushed. The complete `docs/Test.md` local gate passed, including both frontend profiles, account tests, deployment audits, Python suites, rendered templates, ShellCheck and actionlint. No PR or CI run was created.
- CarbonCalculator admission branch `task/ritzau-ci-admission-20261108`, commit `1f29e82f9e0a152f396dee8118f63308d1fc3674`, is pushed. `pwsh -File Scripts/Testing/Validate.ps1 -Mode full -ResultsDirectory TestResults/ci-admission-ritzau-20261108-r1` passed with a fresh report bound to that exact checkout SHA; repository-policy, deployment and .NET phases passed, including 728 non-account and 209 account tests with zero failures or skips in accounts. No PR or CI run was created.
- CarbonCalculator run `42107307607` remains queued from deleted branch `task/fieldwork-polish-20261105`, head `a94eeb4340642a8e3081ada880d5976c3aa9d2a8`. The Jobs API returns no jobs and the branch lookup returns 404. Preserve the run; do not cancel it or start native PR CI until it is reconciled or completed. Existing repository routing and runner registrations remain unchanged.
- RitzauDemo Actions remain disabled and the four existing Dependabot PRs remain open. Product admission/identity code commit `744aeeecab9e8b4bb506fdd9775b178e0511473c` on `task/ritzaudemo-bootstrap-20261108` contains the first-PR native admission/bootstrap-label guard and README/UI identity draft. A first gate attempt at `66d99e1ba9a7e2f5b4a70d5e20fe1ad2360c5550` exposed stale canonical-label expectations in the shared runner-target tests; commit `744aeee` updates those tests. The complete `docs/Test.md` gate then passed on exact SHA `744aeee` in a fresh WSL clone: 99 quality tests, 107 CI tests, BlazorAuto 197 non-account/149 account, React 189 non-account/135 account, 15 documented E2E opt-in skips per non-account profile and zero account skips, 276 common deployment tests, deployment audit, LocalCluster/LocalSingleNode suites, rendered templates, YAML lint, ShellCheck and actionlint. Workflow policy, format, release/cloud/observability validation also passed. This checkpoint predates the safety adaptation below. No product bootstrap, runner registration or deployment occurred.
- Existing sites, data, backups, routes, runners and services remain unchanged. The local validation containers were removed by their test lifecycles; only the pre-existing shared containers remain. Build/load coexistence is still unproven.

### Existing-host bootstrap safety checkpoint — 2026-11-08

- Read-only native `setup-status` still reports the human root step from the clean seed checkout. Do not show its generated command yet: it comes from the unmodified seed on `origin/main`.
- The canonical native collision check passed for ports, Docker subnet, directories and Caddy ownership. Candidate paths remain absent. Existing apps were running with no unhealthy containers at the latest read-only check. Detailed host inventory stays in private installation notes.
- The deploy-user `native-readiness.py` check could not run because deploy-user SSH access is unavailable. Root-only UFW rule inspection also remains unavailable without the operator context.
- Static review found that the stock `mint_base` `dist` upgrade would update shared container-runtime packages on this existing host. The product branch now gates that broad upgrade off for `PrepareSingleNode.yml`, adds `apt-get --no-upgrade` for bootstrap prerequisites, and changes Avahi from forced restart to `started`. Docker package installation stays `state: present`; a focused test protects this behavior. No root bootstrap ran.
- Commit `3c087a5f44533a31faa4d1d8cd13b48968c5e7f8` on `task/ritzaudemo-bootstrap-20261108` contains this safety adaptation and tests. The complete `docs/Test.md` local gate passed on that exact code SHA in a fresh WSL clone: 99 quality tests, 107 CI tests, BlazorAuto 197 non-account / 149 account, React 189 non-account / 135 account, 15 documented E2E opt-in skips for each non-account profile and zero account skips, 278 common tests, 63 LocalCluster tests, 217 LocalSingleNode tests, deployment audit, common release/cloud/observability checks, rendered templates, YAML lint, ShellCheck and actionlint. Workflow policy and format checks passed. Existing local shared containers remained; no gate-owned container remained after completion.
- At this checkpoint, safety branch `task/ritzaudemo-bootstrap-20261108` was pushed and draft PR [#5](https://github.com/Grumlebob/RitzauDemo/pull/5) was open at head `4654ef9ba256f1648871faa6adc49d47e9c02c1d`; status checks were empty because product Actions were disabled. Keep it draft and do not merge without `build-test-push` on the exact PR head. The stale CarbonCalculator run `42107307607` was still queued with zero jobs. The contemporaneous no-cancel instruction was later superseded by the user's explicit full-authority direction; the orphaned run and its check suite were canceled after verifying the missing branch and zero jobs/checks. Do not rerun the deleted-branch dispatch. Four Dependabot PRs remained open. No runner, bootstrap, deployment, routes or neighbor settings changed.
- Next dependency: integrate the safety adaptation through a normal PR with exact-head CI before generating the native operator handoff. The current product checkout has no registered runner, Actions are disabled, and native CI remains deferred while the stale run is unresolved. Do not bypass branch protection or use the stock root command.

### Product docs-sync gate checkpoint — 2026-11-08

- Exact docs-sync SHA `23886457310bacfa8797221a054db85c78288aef` passed every `repo-gate` step, including restore, format, all four frontend profile suites, repository scripts, React checks, image build, and packaged Blazor HTTP/HTTPS browser smoke.
- The same checkout passed the full `docs/Test.md` command set. Profile results: BlazorAuto 197 non-account tests passed with 15 documented opt-in skips and 149 account tests passed; React 189 non-account tests passed with 15 documented opt-in skips and 135 account tests passed. Python discovery passed: Quality 99, CI 107, Common 278, LocalCluster 63, and LocalSingleNode 217 tests. Workflow policy, rendered templates, YAML lint, ShellCheck, actionlint, deployment audit, common release, cloud, and observability checks passed.
- The first local gate invocation omitted the documented task-scoped WSL host override and timed out one React account Testcontainers readiness check after 1 hour 7 minutes. Re-running the full gate with that launcher override passed. No Docker engine or machine networking settings changed; the failed attempt's logs and results remain archived locally.
- At this checkpoint the local docs-sync commit was one commit ahead of remote draft PR #5 head `4654ef9ba256f1648871faa6adc49d47e9c02c1d`. Product Actions remain disabled, so the PR still has no exact-head checks. Four Dependabot PRs remain open and out of scope. Keep PR #5 in draft; no admission PR was integrated and no native CI was started.
- CarbonCalculator run `42107307607` remains queued with no jobs from a deleted branch. Preserve it. No runner, bootstrap, deployment, routes, neighbor settings, or existing site data changed during these gates.

### Draft PR #5 update — 2026-11-08

- Docs commits `23886457310bacfa8797221a054db85c78288aef` and `4569db7578845c974aae3944e3bbff0f1e123b04` were pushed by fast-forward to the existing draft PR #5. Remote PR head now matches `4569db7`; the local branch is clean. The full `repo-gate` and complete `docs/Test.md` command set passed at that exact head.
- PR #5 remains draft with an empty status-check rollup because RitzauDemo Actions are disabled. Keep the draft; exact-head CI and integration are still pending. Four Dependabot PRs remain open and out of scope.
- No new GitHub runs were started in GrumPlates, CarbonCalculator, or RitzauDemo. CarbonCalculator run `42107307607` is still queued with no jobs. Preserve it and do not start native PR CI until it is reconciled or completes.
- Recheck found inconsistent CarbonCalculator run state: the aggregate run endpoint and associated check suite `114100632846` still say `queued`, with zero check runs; attempt 1 reports `completed` with `startup_failure`; both job listings are empty, and the old dispatch branch is absent. Preserve the run. This mismatch does not establish that it is reconciled or safe to proceed.
- Read-only native checks confirm the two existing LocalSingleNode runner services use `deploy`, and the common lock path is owned and writable by that account. The host identity matched the private machine inventory. This supports access for existing producers; RitzauDemo has no runner service, and repository-helper/workflow-level contention and cancellation behavior remain unproven.
- A controlled two-process check on the real lock path confirmed exclusive acquisition, contender rejection while held, release at process exit, and successful reacquisition. The lock inode/owner/mode remained stable. This validates the shared file's current `flock` behavior; repository workflows, cancellation cleanup, and a third RitzauDemo runner remain untested.
- No native bootstrap, runner registration, deployment, or site/data/route change occurred. The native operator handoff remains blocked on an exact-head CI path.

### Draft PR #5 checkpoint — `59e0c23` — 2026-11-08

- Docs-only commit `59e0c235e5f3941513147df85681edb561563b7d` was pushed by fast-forward. PR #5 remains open/draft at this exact head; its status-check rollup is empty because RitzauDemo Actions remain disabled. The branch-specific Actions run list is empty.
- The complete local `repo-gate` passed at this SHA. Profiles: BlazorAuto 197 non-account passes / 15 documented opt-in skips and 149 account passes; React 189 non-account passes / 15 documented opt-in skips and 135 account passes. All restore, format, Python, deployment validation, rendering/lint, ShellCheck, playbook, actionlint, script-test, React, image-build and packaged smoke stages passed.
- The full `docs/Test.md` command set passed at this SHA: Quality 99, CI 107, Common 278, LocalCluster 63 and LocalSingleNode 217 tests; workflow policy passed.
- The first gate launch at this SHA omitted the documented non-login WSL .NET environment, so .NET-dependent stages failed with `dotnet: command not found`. Its logs were preserved. The corrected task launcher set `DOTNET_ROOT=$HOME/.dotnet`, added it to `PATH`, sourced the gate Docker environment and set `TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal`; the complete gate then passed. No Docker engine or machine network settings changed.
- CarbonCalculator run `42107307607` remains queued with zero jobs; attempt 1 reports `startup_failure`, and check suite `114100632846` remains queued with zero check runs. Its dispatch branch is absent. Preserve it and keep native PR CI deferred until reconciliation or completion.
- This commit changed documentation only. No native bootstrap, runner registration, deployment, site/data/route change, or new native CI run occurred. The two-process lock check is recorded above; three-repository workflow contention, cancellation cleanup and loaded capacity remain outstanding.

### Stale-run reconciliation and first admission PR — 2026-11-08

- CarbonCalculator run `42107307607` was a `workflow_dispatch` from 2026-11-05 at head `a94eeb4340642a8e3081ada880d5976c3aa9d2a8`; its branch returned 404. Attempt 1 completed with `startup_failure`, while the aggregate run and check suite `114100632846` stayed queued with no jobs/checks. After the user's explicit full-authority instruction, the stale run was canceled. Both run and suite now report `completed/cancelled`, still with no jobs/checks. No source branch was recreated or rerun.
- Immediately before the first admission PR, all three repositories had no queued, waiting, or in-progress Actions runs. The GrumPlates native runner was online/idle. A read-only check of the verified host found no active Runner.Worker, and only the existing app/database/cache containers were running and healthy. A bounded native build reservation was recorded in the GrumPlates clone's ignored claim ledger.
- GrumPlates admission branch `task/ci-admission-ritzau-20261108` passed its complete `docs/Test.md` local gate at `b17c20655dfb968133db7bb704106e2fc69a7959`. Normal PR [#450](https://github.com/Grumlebob/GrumPlates/pull/450) opened at that exact head. CI run `42521702333` failed in `Test runner build admission`: after cancellation, the fixture's `kill -0` check saw the process group still present immediately after `wait`. The downstream required gate failed because `validate` failed; all CodeQL jobs passed. The runner is idle, and a follow-up host process check found no remaining lock-test processes. The fixture now polls for running, non-zombie group members and bounds lock reacquisition to five seconds. Follow-up commit `e62686441bf5190654a823578de62d0ef8dd7036` passed the full local gate; its first rerun attempt used a split task-local test command, then the corrected wrapper passed. PR #450 now matches exact head `e62686441bf5190654a823578de62d0ef8dd7036`; CI run `42526856844` is in progress and CodeQL run `42526856859` passed. Merge only after required exact-head checks are green; then verify exact-main CI/publication before opening the CarbonCalculator admission PR.
- A fresh pre-push queue check found legacy Dependabot run `42523384022` active on the old workflow; it completed successfully. Queued PR #453 validation run `42523421032` was canceled and completed cancelled. Auto-merge run `42526557993` was canceled. PRs #451–#453 remain open with no auto-merge request. The remaining updater run `42523334321` uses GitHub-hosted Actions. Before PR #450 was pushed, no queued job remained and the native runner was idle.
- CarbonCalculator admission branch `task/ritzau-ci-admission-20261108` remains locally validated at `1f29e82f9e0a152f396dee8118f63308d1fc3674`, with its full report recorded above; it has no PR yet. RitzauDemo Actions remain disabled; draft PR #5 remains unchecked. No bootstrap, registration, site, data, route, runner, or deployment change occurred.
- Parent plan and execution packet were synced into the RitzauDemo branch as docs-only commit `a86b00c8acfb41beada455373b4f724f4d4cdf20` (`[skip ci] docs: sync active admission checkpoint`). The 2026-11-08 test-scope amendment was then pushed as docs-only commit `b0bee356ca9fc1c97313ac4ed1315b4ae9ab7d8a`; PR #5 remains draft and Actions stay disabled.

### GrumPlates admission integration verified — 2026-11-08

- PR #450 head `e62686441bf5190654a823578de62d0ef8dd7036` passed exact-head CI `42526856844` and CodeQL `42526856859`, then merged at `2026-11-08T22:04:57Z` as main SHA `de6cfaec8d86373e14fee7a912122e815ddf4ce0`.
- Exact-main CI run `42528402393` passed `validate`, `publish-main`, and `build-test-push` at that SHA. Exact-main CodeQL run `42528402282` also passed.
- Dependabot PR #451 at `651f8a532835297947f3bac0f578ae30cce36cc0` passed CI run `42528614333`; PRs #451–#453 remain open with no auto-merge request. Auto-merge workflow run `42531613094` completed without merging them.
- Fresh checks found no queued or running CI in GrumPlates, CarbonCalculator, or RitzauDemo. The native runner is online and idle. Run-owned containers were removed; Books, React Books, CarbonCalculator, and their databases/caches remained up, with PostgreSQL containers healthy.
- CarbonCalculator main still equals validated base `0d255392b99eb635030f9c90a03de8a6d37d227c`; its clean admission branch remains full-gate validated at `1f29e82f9e0a152f396dee8118f63308d1fc3674`. Create its normal native-CI-only PR after one more live queue and runner check.
- RitzauDemo PR #5 remains open and draft at `902f98c5123f0a5054ac32f7bda7f498be5c4469`; Actions remain disabled. The approved one-week test-scope amendment is in the branch. Product test pruning remains planned for Phase 3. No bootstrap, product runner registration, or deployment occurred.

## Suggested handoff prompt

> Create a new /goal to complete the remaining work in `Plans/RitzauDemo-Blazor-SingleNode-Bootstrap.md` and `Plans/Support/RitzauDemo-ExecutionPacket.md`. Read their fresh-chat checkpoint first, then workspace/repository rules and the current private machine guide. The user explicitly approved narrow CarbonCalculator native-CI admission alongside GrumPlates admission and RitzauDemo product work. Preserve all three repository URLs, existing CI routing, deployments, data and other projects. The seed is already pushed at `5931a4a872de39db8a85ac886bcdf40097bc99db` and the native clone exists; revalidate rather than re-seed or direct-push main. Start with Phase 2A: integrate/test one shared native build queue across all three repositories, preserve CarbonCalculator desktop/native routing, and follow the guarded first RitzauDemo PR rollout. Do not repeat the obsolete CarbonCalculator scope blocker. Complete the remaining Blazor-only cleanup, single-node extraction, rename, isolated runtime budgets, LAN deployment, loaded coexistence trial, independent acceptance and backup/recovery evidence through reviewed PRs and required checks. Public hostname remains deferred until supplied and explicitly authorized. Prepare the exact canonical native operator handoff and stop for its required human action; never run its sudo command from the controller. Stop otherwise only for a concrete unresolved dependency after all available authorized work. Record evidence and keep incomplete items unchecked.
