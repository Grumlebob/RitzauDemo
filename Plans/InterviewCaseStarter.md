# Interview case starter: vertical slices, RBAC, and focused tests

Status: in progress. R00–R14 (upstream handoff) are verified on the integrated product base. R15–R16 remain in progress pending the product port and final integration. Prepared 2026-11-09. This document authorizes no deployment.

## 1. Outcome and scope

Prepare a small application that can be adapted to a forthcoming interview case under a short deadline. The demonstrable technologies are Blazor Interactive Auto, EF Core/PostgreSQL, vertical slices, Redis/HybridCache, role-based authorization, and Testcontainers. Keep the implementation easy to follow and the automated tests deliberately focused.

The user explicitly chose the following:

- Reorganize server operations into vertical slices.
- Keep caching, including the useful implementation and explanation of invalidation. Do not execute the earlier suggestion to remove Redis or caching.
- Simplify the Books UI in **GrumPlates first**, through its separate plan, and port the completed result into RitzauDemo afterward.
- Simplify account capabilities with `User` read-only and `Admin` performing CRUD. Enforce the difference on endpoints and direct server operations.
- Keep a single meaningful architectural test and its `Architecture/` folder.
- Keep ordinary project documentation minimal. Interview documentation and a README rewrite are not deliverables.

This plan is product-specific. Do not apply the test reduction, account reduction, or product RBAC decisions to GrumPlates.

### Relationship to other work

1. Read [RitzauDemo-Blazor-SingleNode-Bootstrap.md](RitzauDemo-Blazor-SingleNode-Bootstrap.md) and its execution packet before implementation. That plan owns product identity, removal of React, single-node extraction, runner/bootstrap work, and deployment. Do not duplicate those operations here.
2. The UI source plan is [GrumPlates: Reusable CRUD UI](https://github.com/Grumlebob/GrumPlates/blob/main/Plans/ReusableCrudUi.md). Its URL becomes available when that planning document is integrated. The plan was also created in the GrumPlates checkout during this planning task.
3. Land each product change on top of the relevant completed bootstrap changes. Do not merge an old template snapshot over the renamed product.
4. The UI port can wait while slice, RBAC, and test work proceeds. Do not rebuild the replacement UI independently in both repositories.
5. This later user request permits the separate GrumPlates UI plan despite the older bootstrap plan's narrower GrumPlates scope. It does not authorize unrelated template changes.

## 2. Research completed before writing the plan

The following files were inspected; repeat the inventory against the execution base because another task is carrying out the bootstrap:

| Area | Observed source and consequence |
| --- | --- |
| Identity | `Features/Accounts/AccountSeed.cs` already provisions `AdminSeed` as `Admin` and `UserSeed` as `User`. Existing accounts are not silently updated. Reuse this mechanism. |
| Ownership | `Features/Books/Services/BooksServerService.cs` scopes private reads and writes through `UserBooks.OwnerUserId`. Admin currently has no ownership bypass. Preserve this. |
| Endpoints | `Features/Books/Endpoints/BooksEndpoints.cs` has authentication, account markers, rate limiting, private/no-store responses, and CRUD mappings. It does not currently distinguish Admin from User. |
| Slice gap | Core has `Features/Books/UseCases/{Operation}` contracts, but server EF work is collected in `BooksServerService` and HTTP work in `BooksEndpoints.cs`. |
| Cache | Private keys include user identity. `BooksCacheKeys`, `BooksCacheOptions`, `ICacheInvalidator`, and Redis invalidation are existing behavior, not disposable scaffolding. |
| Public catalog | `AuthorBooksServerService`, `IAuthorBooksApi`, and `/api/author-books` are separate public read paths. Preserve their public/private distinction. |
| Render modes | Server and browser implement the same Core contracts. Blazor server rendering can call services without traversing HTTP endpoint authorization. Enforcement must cover both paths. |
| Tests | Existing `BooksCachingTests`, `BooksCrossNodeCacheInvalidationTests`, `PrivateBooksOwnershipTests`, and `AccountCookieTests` contain useful behavior to select and consolidate. |
| Test authentication | `TestAuthenticationHandler` already accepts test-only role claims through `X-Test-Roles`. This must remain confined to test hosting. |
| Architecture | `Architecture/Boundaries/DependencyBoundariesTests.cs` already inspects compiled assembly references. Reuse that approach instead of introducing a source-folder assertion. |
| Accounts | Registration, recovery, email changes, passkeys, external login, account deletion/export, mail transport/outbox, and their Blazor pages create most account breadth. |
| UI coupling | Books state currently exposes shelf-specific types. Porting the UI requires state/type cleanup, not merely swapping Razor markup. |

At inspection the primary product checkout still exposed `GrumPlates*` project names. Native Git configuration also selected a different Linux task worktree. Read-only inventory used explicit Git directory/worktree arguments without changing shared configuration. Do not infer that this primary checkout is the current implementation base. Resolve ownership and select the bootstrap's integrated product head first. Keep machine-specific recovery commands in private notes.

### Useful preparation already done

- [x] Locate the operation, identity, ownership, cache, and render-mode seams.
- [x] Identify an existing compiled architecture test to reduce rather than replace with a new framework.
- [x] Identify the role-aware test authentication seam and existing real-cookie ownership coverage.
- [x] Separate upstream UI work from downstream product cleanup.
- [x] Define acceptance scenarios and a port manifest below so execution does not begin with another design exercise.

## 3. Fixed implementation rules

Use the repository's current names after bootstrap. In the remainder, `RitzauDemo`, `.Client`, `.Core`, and `.Test` mean the renamed projects; old paths above are source anchors, not instructions to restore old names.

- Keep the current EF schema and migration history unless an explicitly described capability removal requires a new migration. Do not regenerate or squash an existing deployed initial migration.
- Keep `IDbContextFactory<AppDbContext>` and dispose a context per operation. Do not inject a long-lived context into a Blazor circuit.
- Keep existing Core request/response contracts and `IBooksApi`/`IAuthorBooksApi` transport seams. Do not create a generic repository, unit-of-work wrapper, mediator bus, or CRUD framework.
- Preserve cancellation, EF projections/`AsNoTracking`, URL normalization, response statuses, validation, rate limiting, antiforgery, endpoint metadata, telemetry, and cache behavior while moving code.
- Authorization is enforced on the server. UI visibility is only a convenience. Role checks cannot exist only on HTTP endpoints when server rendering calls a handler directly.
- An Admin role never silently means access to another user's private records.
- Keep real PostgreSQL and Redis integration fixtures for the behavior being demonstrated. Do not replace them with EF InMemory or mocked cache results.
- Do not weaken cookie flags, password hashing, lockout, Data Protection, CSRF, trusted proxies, or API 401/403 behavior to simplify the account feature.
- Do not replace Interactive Auto with Interactive Server. Keep prerender controls disabled until interactive.
- Do not delete operational safety tests, deployment audits, or workflow gates simply to meet an application test budget.
- Preserve unrelated uncommitted work and active task resources. Execute in an owned checkout and stage explicit paths.

## 4. Confirmed RBAC contract

Decision confirmed by the user on 2026-11-09: **User is read-only; Admin performs CRUD.** Use the existing `User` and `Admin` roles. Keep ownership filtering unchanged; the Admin role does not grant access to another user's private records.

| Operation | Anonymous | User | Admin |
| --- | --- | --- | --- |
| Read public author catalog | Allowed | Allowed | Allowed |
| List/get own books | 401 | Allowed | Allowed |
| Create/update/delete own books | 401 | 403 | Allowed |
| Read another user's private book | 401 | 404 | 404 |
| Update/delete another user's private book | 401 | 403 | 404 |

Use an authenticated read policy allowing `User` or `Admin`, and an `AdminOnly` write policy. Apply the write policy to POST, PUT, and DELETE. Check role permission before looking up a write target: a User receives 403 regardless of whether the record exists. Once an Admin passes the role check, owner filtering returns 404 for another user's record. Anonymous API requests receive 401. Keep existing private/no-store headers and account request protections.

User UI shows lists/details and no Create/Edit/Delete controls. Admin UI exposes the CRUD controls. Direct navigation to a write page and direct server handler calls must also enforce Admin permission. Do not add an administration summary endpoint, dashboard, new role, or permission-mode switch.

Use Admin clients for successful mutation tests and separate User clients for read/denial tests. Prepare User-owned records through controlled fixture/demo setup, never by granting the User role temporary write access or exposing a seed endpoint.

## 5. Target server organization

```text
RitzauDemo/Features/Books/
  UseCases/
    CreateBook.cs
    GetBooks.cs
    GetBook.cs
    UpdateBook.cs
    DeleteBook.cs
    GetAuthorBooks.cs
    GetAuthorBook.cs
  Caching/
    BooksCacheKeys.cs
    BooksCacheOptions.cs
    AuthorBooksCacheKeys.cs
    BooksCachePolicy.cs          # only shared cache options/invalidation behavior
  Services/
    BooksServerService.cs        # thin IBooksApi adapter only
    AuthorBooksServerService.cs  # thin IAuthorBooksApi adapter only
  Persistence/
    BookEntityTypeConfiguration.cs
  DependencyInjection.cs
```

Each operation file contains its concrete handler and HTTP mapping/response translation. Keep shared request/response DTOs in the existing Core use-case folders so the browser can reference them. The endpoint and server adapter call the **same handler**; neither repeats EF queries or business decisions. Handlers depend only on what they use. Do not inject an omnibus feature service back into the extracted handlers.

Keep endpoint concerns such as HTTP status translation at the endpoint. Keep user scope and any role-dependent operation permission in server handler/application code usable from both execution paths. The thin adapter may remain named `BooksServerService` to avoid pointless transport churn.

`BooksCachePolicy` is a suggested concrete helper, not a required abstraction. Extract only the repeated TTL construction and invalidation behavior currently embedded in the service. Preserve key formats and tags during the refactor. A cache shape change, if genuinely necessary later, gets a schema-version change and explicit acceptance coverage.

## 6. Execution packets

Perform packets in order unless their dependency explicitly allows waiting. Do not mark a packet complete after editing alone. Record verification in section 10. A failed check must be fixed or reported with the packet incomplete.

### R00 — Establish the execution base

- [x] Read `AGENTS.md`, current `docs/Requirements.md`, current `docs/Test.md`, and the bootstrap plan status.
- [x] Inventory branch, HEAD, `origin/main`, worktree registration/ownership, and tracked/untracked/ignored changes without changing another task's Git settings.
- [x] Select an owned task branch on the integrated renamed product base. If bootstrap is still editing the same files, wait for its integration or use a clearly separate draft branch; do not co-edit.
- [x] Record exact base SHA and current project names. Identify every obsolete path in this plan and map it to the live path.
- [x] Confirm the current local gate and CI audit location after single-node extraction. Never skip an audit because its old LocalCluster path no longer exists.

Exit: reproducible base, no conflicting owner, and an executable gate list. Bootstrap completion is an agent dependency, not automatically a user action.

### R01 — Create a behavior and retirement ledger

- [x] Inventory all Books API routes, names, request/response shapes, policy metadata, and normal/error statuses.
- [x] Map each `IBooksApi` and `IAuthorBooksApi` method to endpoint, server implementation, browser implementation, cache keys/tags, and current tests.
- [x] Inventory account endpoints, Razor routes, navigation links, DI registrations, hosted workers, settings, schema references, and tests.
- [x] Classify every application test file as keep, consolidate, remove-with-capability, retain-until-UI-port, or operational-safety/outside-budget. Include a reason and replacement scenario for consolidation.
- [x] Put the compact mapping in the implementation PR or ignored scratch notes. Avoid a new permanent catalog of every source file.

Exit: no planned deletion has an unidentified consumer or uncovered surviving security behavior.

### R02 — Extract the list query as the reference slice

- [x] Add `UseCases/GetBooks.cs` with a concrete handler containing the current user-scoped EF projection and cached read.
- [x] Move list endpoint mapping/translation into that file; retain route, endpoint name, rate limiting, account markers, authorization, telemetry, and no-store metadata.
- [x] Make `BooksServerService.GetAsync` delegate to the handler. Register the handler explicitly in feature DI.
- [x] Retain the existing list integration and cache-isolation tests during this move.
- [x] Build, then run focused list and cache tests sequentially.

Exit: HTTP, prerender/server rendering, and browser calls use unchanged contracts and behavior through one handler.

### R03 — Extract the detail query

- [x] Move get-by-id query and cached read into `UseCases/GetBook.cs`.
- [x] Preserve owner filtering and the existing not-found response. Do not change unauthorized records into a record-existence disclosure.
- [x] Delegate adapter and endpoint to the handler. Remove the old service query only after both call sites switch.
- [x] Run detail, ownership, and cache-isolation coverage.

Exit: list/detail EF code no longer lives in the adapter.

### R04 — Extract create and shared cache behavior

- [x] Move create EF work, owner association, normalization, save, and post-save invalidation into `UseCases/CreateBook.cs`.
- [x] Extract only duplicated cache policy/invalidation code into a concrete feature helper if needed by subsequent writes.
- [x] Preserve commit-before-invalidation ordering and existing invalidation failure handling. Do not hold an EF transaction open while publishing Redis messages.
- [x] Preserve 201 and Location metadata at the endpoint. Keep request validation before persistence.
- [x] Verify persistence, owner association, validation failure without mutation, and invalidation of an already warmed list.

Exit: create is the complete reference command slice, including caching.

### R05 — Extract update and delete

- [x] Move update into `UseCases/UpdateBook.cs`, retaining owner scope, route/body ID mismatch handling, and list/item invalidation.
- [x] Move delete into `UseCases/DeleteBook.cs`, retaining owner scope, not-found behavior, and list/item invalidation.
- [x] Switch each adapter method and mapping individually. Remove only dead methods and imports.
- [x] Verify update, delete, mismatch/not-found cases, warmed-cache freshness, and another user's denied mutation.

Exit: `BooksServerService` contains delegation only; no EF query, cache algorithm, or duplicated business logic remains there.

### R06 — Extract public catalog reads and close the refactor

- [x] Extract `GetAuthorBooks` and `GetAuthorBook` with their existing public contracts and cache keys.
- [x] Keep seed behavior, public payload bounds, and public/private table relationships unchanged.
- [x] Keep `AuthorBooksServerService` as a thin adapter and register all handlers explicitly.
- [x] Remove old endpoint/service bodies only after a reference search confirms all consumers moved.
- [x] Run the complete current local gate before publishing the refactor PR. Do not reduce tests in the same operation as the first behavior-preserving move.

Exit: a reviewer can trace one operation in one server use-case file. Record exact PR-head CI and integrated-main verification.

### R07 — Implement the confirmed role policy contract

- [x] Use the confirmed section 4 matrix: User reads, Admin performs CRUD, and ownership remains enforced. No further permission decision is required.
- [x] Introduce shared role/policy names only where both clients and server actually need them. Keep authorization implementation out of Core.
- [x] Define the authenticated read policy for `User` or `Admin` and the `AdminOnly` write policy in existing account/authorization composition. Do not add a separate permission framework or custom role database.
- [x] Check how the authenticated principal reaches both HTTP and direct server handlers. Use current-user/authorization services, never browser-supplied role strings.

Exit: the confirmed matrix has concrete policy registrations and a shared server enforcement path.

### R08 — Implement the visible RBAC difference

- [x] Apply the read policy to private GET routes and `AdminOnly` to POST, PUT, and DELETE using `.RequireAuthorization(...)`, with equivalent enforcement on directly callable server operations.
- [x] Keep private/no-store headers, account request protections, and existing response contracts. Add no summary endpoint or administration subsystem.
- [x] Preserve owner filtering after role authorization: User writes return 403; an Admin targeting another user's record receives 404.
- [x] Verify anonymous 401, User read success and write 403, and Admin CRUD success against actual endpoints. Verify every denied write leaves data unchanged.
- [x] Hide Create/Edit/Delete controls for User, protect create/edit routes, and verify direct server behavior. Hiding a link is insufficient.

Exit: the two seeded roles demonstrably receive different endpoint results, with no ownership bypass.

### R09 — Make both sample roles usable

- [x] Reuse existing Admin/User seeds. Do not silently change passwords, role memberships, or emails of an existing account.
- [x] Give the read-only User a small deterministic set of own sample records through idempotent, explicitly enabled demo setup; seed test records directly in fixtures. Do not overwrite existing data, expose a seed endpoint, or enable User writes for setup.
- [x] Check local launch, Docker configuration, and the retained single-node environment template for both seed options.
- [x] Support an explicitly enabled User demo seed in the intended demo environment if deployment currently exposes only Admin seed settings. Default production safety must not become a public demo password.
- [x] Supply credentials through existing private configuration, generating safe values where authorized. Keep credentials out of tracked files, plan, logs, and PRs.
- [x] Verify a real login for each role, User read-only behavior, Admin CRUD, serialized role claims after prerender/interactivity, logout, and private UI state reset between users.

Exit: both roles work in the intended demo execution path without a test-header authentication bypass.

### R10 — Reduce account pages and routes

- [x] Preserve sign-in, sign-out, access-denied, lockout feedback, current-user identity, and the security validation/revocation needed by retained sessions.
- [x] Remove registration, recovery/reset, email-change, passkey/2FA, external-login, export/deletion, and optional management screens that the selected seeded-account demo no longer offers.
- [x] Update navigation and form handlers with each route removal. Search for link strings, endpoint mappings, imports, and page model types before deletion.
- [x] Do not leave a hidden API for a removed capability merely because its navigation disappeared. Do not leave login branches that require a now-removed 2FA/external route.
- [x] Confirm whether existing configured accounts use a removed authentication capability; do not alter live account data or strand a known user silently. Resolve actual data dependencies before removal.
- [x] Verify retained sign-in/out and rejected writes with real cookies/antiforgery after each coherent removal group.

Exit: the available account surface matches the selected demo, and retained login still works end to end.

### R11 — Remove unused account backend machinery

- [x] After R10, remove unused handler registrations, mail transport/outbox worker, mail-only health checks, token/link helpers, and SMTP fixtures whose consumers are gone.
- [x] Check `AccountOperations`, transaction helpers, session/revocation logic, principal guards, and Blazor adapters before removing shared pieces. Keep security functionality still used by login.
- [x] Remove obsolete settings and their validators from application defaults, examples, deployment rendering, audits, and tests together.
- [x] Prefer leaving an unused historical table until a reviewed forward migration is justified. Never edit old migration bodies or delete live operational data as cleanup.
- [x] Remove unused NuGet references only after checking all projects and fixtures, including MailKit when no consumer remains.
- [x] Run focused account and startup/migration checks, then the current full gate for the account cleanup PR.

Exit: no active dependency on removed capabilities; no security control lost as an accidental side effect.

### R12 — Keep one meaningful architecture test

- [x] Create or reduce to `RitzauDemo.Test/Architecture/DependencyDirectionTests.cs` with exactly one ordinary `[Fact]` expressing the permitted project dependency direction.
- [x] Inspect compiled references: Core must not reference ASP.NET Core, EF Core, or Npgsql; Client must reference Core and must not reference Server, EF Core, or Npgsql.
- [x] Resolve assemblies through existing marker types. Match actual assembly identities after rename; do not search source text for forbidden words.
- [x] Report offending assembly names on failure. Verify the inspected assemblies and reference sets are nonempty so an empty scan cannot pass.
- [x] Remove other application architecture/source-shape tests only after the R01 ledger assigns surviving behavior elsewhere. Preserve operational workflow/safety assertions outside this single application architecture test if still required.
- [x] Remove architecture helper code and packages with no remaining consumers. Keep the `Architecture/` directory because it contains the real test.

Exit: one small test protects a real dependency boundary and can be explained at the interview. No empty folder or decorative assertion.

### R13 — Consolidate application tests by scenario

- [x] Implement the scenario matrix in section 7 by selecting/refactoring existing tests. Do not rewrite all fixtures or invent a test DSL.
- [x] Update successful write fixtures to authenticate explicitly as Admin, including cache/invalidation tests. Seed User-owned data directly; use distinct Admin owners when testing allowed-role cross-user write denial.
- [x] Keep a shared PostgreSQL fixture, deterministic database reset, and explicit cache reset/isolation between scenarios. A database reset alone does not clear warmed HybridCache/Redis state.
- [x] Keep a real Redis fixture for cache behavior. Share infrastructure where possible without introducing process-global environment races.
- [x] Consolidate repeated account/API assertions and remove tests for capabilities actually removed in R10/R11.
- [x] Retain current modal/geometry/navigation tests until the upstream UI port removes those behaviors. Their removal is not independent permission to break the current UI.
- [x] Keep one bounded cross-node invalidation scenario if the retained implementation still claims cross-node behavior. Remove redundant permutations, not the only proof of that claim.
- [x] Preserve operational/deployment checks required by the current gates. If those dominate runtime, record that separately instead of weakening required checks.

Exit: the normal application test tree is easy to browse; every removed test has a removal/consolidation reason. Counts are an aid, not a hard cap.

### R14 — Prepare the UI port without applying it

- [x] Wait for GrumPlates `ReusableCrudUi.md` implementation PRs to be integrated and verified on its exact main.
- [x] Record upstream base/head SHAs, relevant PRs, and explicit changed paths. Verify the actual code, not merely a closed PR status.
- [x] Map `GrumPlates` namespaces, project names, Core contracts, feature DI, user state, account links, and authorization visibility to the renamed product.
- [x] Identify which source files carry only UI changes and which mix template/backend changes. Prepare an explicit path-by-path adaptation list.
- [x] Record the role-specific UI adjustment required by R07/R08. Do not import a template permission model over product RBAC.

#### Verified GrumPlates handoff

The upstream UI is integrated and verified as of 2026-11-09. [GrumPlates PR #455](https://github.com/Grumlebob/GrumPlates/pull/455) was based on `b2dd255cea8c95f496b241b5517d246938e2d681`, with PR head `39349002c1008b5b9196ef8e5f9321272e4bb729`, and squash-integrated on `origin/main` as `555cc103e33ae18838129cd79be8ec738a8a6d68`. The exact-head `build-test-push` run `42683644193` and CodeQL run `42683644261` passed. Exact-main `build-test-push` run `42686724906` and CodeQL run `42686724951` passed.

The [upstream path manifest and route contract](https://github.com/Grumlebob/GrumPlates/blob/main/Plans/ReusableCrudUi.md#7-port-manifest-for-ritzaudemo) list each added, updated, and removed file. The UI PR changes Blazor pages, list state, form/detail components, styles, focused tests, small template docs, and Sonar mapping fixtures. It does not change Core/API contracts, backend handlers, service registrations, or migrations. The UI uses `IBooksApi.SearchAsync(SearchBooksRequest)` from the separately integrated PR #454 and the existing `IAuthorBooksApi`. The source routes are `/` and `/books` for public catalog plus private own-books list, `/books/catalog/{id:int}` for public detail, `/books/author/{SeedKey}` as a bounded legacy redirect, and `/books/new`, `/books/{id:int}`, and `/books/{id:int}/edit` for private CRUD.

#### RitzauDemo adaptation map

| Upstream paths or contract | RitzauDemo adaptation |
| --- | --- |
| New and updated `GrumPlates.Client/Features/Books/Routes/*` and `Shared/*` pages, forms, and rows | Port selected components into `RitzauDemo.Client/Features/Books`; use `RitzauDemo.Client`/`RitzauDemo.Core` namespaces, current DTOs, and product routes. Keep the existing `/books/author/{SeedKey}` public author route and product `/Account/Login?returnUrl=...` links. |
| Updated `AuthorBookcase`, `UserBookcase`, their state types, and `_Imports.razor` | Adapt existing product state and imports. Reuse current `IBooksApi`, `IAuthorBooksApi`, and state registrations in `RitzauDemo.Client/Program.cs`; retain server feature registrations and handlers. |
| Source private list uses owner-scoped `IBooksApi.SearchAsync(SearchBooksRequest)` | Product `IBooksApi` currently exposes `GetAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, and `DeleteAsync`, with no `SearchAsync`. In R15, confirm the list bound before choosing local filtering or a minimal owner-scoped search/pagination adapter; preserve ownership, `BooksCachePolicy`, and invalidation behavior. Do not copy the template API wholesale. |
| Removed `BookModal`, `BookPage`, `BookCover*`, `BookcaseShelf*`, and mapper files | Remove corresponding product presentation code only after a repository-wide consumer search; preserve public catalog data and non-UI API behavior. |
| Added/updated Books E2E, state, pre-hydration, and visual tests; removed modal/geometry tests | Port a small set of semantic navigation, CRUD, role, and render-mode checks within the existing test budget. Keep User read-only, Admin CRUD on owned records, direct write-route denial, and server-handler enforcement. Do not import the full template E2E suite. |
| Updated `GrumPlates/wwwroot/app.css` and generated `tailwind.css` | Adapt authored styles as needed and rebuild `RitzauDemo/wwwroot/tailwind.css` with the product lockfile. Never copy generated upstream CSS wholesale. |
| `Scripts/Quality/sonar_*`, template architecture/feature docs, and `Plans/ReusableCrudUi.md` | Do not copy these files. Update only product-specific docs if the port makes current behavior inaccurate; retain the single product architecture test and its current quality tooling. |
| No Core/API/backend/DI/migration changes in the UI PR | Preserve existing product handlers, both client adapters, role policies, real-cookie account behavior, and cache configuration. The product's `User` role stays read-only; `Admin` may CRUD only its own books. UI visibility supplements server/direct-handler authorization. |

Exit: reviewed port manifest; no broad upstream merge is necessary.

### R15 — Port the verified CRUD UI

- [ ] Apply the explicit UI/state/type paths from R14 in an owned product PR. Do not cherry-pick unrelated template workflows, deployment targets, React support, accounts, or naming.
- [ ] Reuse current product handlers, contracts, caching, and RBAC. Adjust only adapters/signatures required by the new UI.
- [ ] Replace the old shelf/modal routes and remove dead shelf geometry, cover artwork, modal lifecycle helpers, tests, and assets only after checking remaining consumers.
- [ ] Port the small applicable state/browser acceptance coverage; do not import the entire upstream test suite.
- [ ] Rebuild product Tailwind output with the product lockfile. Do not copy upstream generated CSS wholesale.
- [ ] Verify anonymous public catalog, User read-only pages with no mutation controls, Admin own CRUD, direct write-route denial, validation, empty/error states, reload, and logout in Interactive Auto's actual execution modes.

Exit: product UI is simpler and domain replacement no longer depends on book-rendering geometry.

### R16 — Minimal docs and final integration

- [ ] Correct only stale commands, deleted-feature references, and requirements affected by this work. Keep essential startup/test/operator instructions accurate.
- [ ] Do not add an interview guide, stack brochure, documentation index, or broad README rewrite.
- [ ] Review complete code/configuration diffs. Update deployment audit rules in the same PR whenever checked behavior changes.
- [ ] Run the repository's complete current local gate before each code PR push. Build and test sequentially; record the actual commands/outcomes.
- [ ] Merge only when `build-test-push` is green on the exact PR head. Retarget dependent PRs before deleting their base branch.
- [ ] Fetch and verify integrated `origin/main` content and required exact-main CI after each code PR. Record full SHAs and CI run references.
- [ ] Close owned worktrees/branches and task-created Docker resources under `AGENTS.md`, preserving useful inputs and other task dependencies.

Exit: all implemented packets are verified on main. No deployment is needed to mark code integration complete unless separately authorized and added to scope.

## 7. Focused acceptance matrix

Aim for roughly 10–15 application test methods including the architecture test, with small theories where inputs are genuinely parallel. Do not make a count-based CI rule. Additional security cases are justified when necessary. Operational script tests are outside this budget. Suggested files are `BooksCrudTests`, `BooksAccessTests`, `BooksCachingTests`, `AccountSessionTests`, and `Architecture/DependencyDirectionTests`, plus minimal retained browser coverage.

| Scenario | Required proof |
| --- | --- |
| Create and reload | Admin receives HTTP 201 and Location; EF row/owner persist; subsequent normal read returns the record. |
| List/detail | Both roles can read own records with correct DTO values, deterministic ordering, and no other user's records. Seed User-owned records through fixtures. |
| Update | Admin's persisted change appears on a subsequent read; route/body mismatch is rejected. |
| Delete/missing record | Admin delete succeeds; later get returns 404; repeated/missing operations have documented outcomes. |
| Validation | Representative invalid Admin title/URL writes return validation errors without writes. Reuse existing rules; do not invent case requirements. |
| Private ownership | Another user's reads return 404. Admin writes to another owner's record return 404. User writes return 403 before ownership lookup. Include a warmed-cache read to catch key leakage. |
| Endpoint RBAC | Private writes give anonymous 401, User 403, and Admin success. Cover POST/PUT/DELETE with a small theory and assert denied requests leave data unchanged. Test direct handler authorization as well. |
| Cookie session | Real seeded login supplies expected role, read access works, logout prevents further authenticated access. Test an Admin cookie-authenticated write without valid CSRF and confirm rejection, independently of User role denial. |
| Cache hit and write invalidation | Prove a real warmed read is cached (bounded query instrumentation or existing controlled DB-change technique), then an API mutation makes list/item reads fresh. Avoid sleeps for TTL expiry. |
| Distributed invalidation | If retained, two hosts sharing real Redis invalidate one warmed reader after a writer mutation; use bounded polling and dispose both hosts. |
| Architecture | The single compiled dependency-direction test in R12. |
| Interactive Auto smoke | One focused Admin CRUD browser journey and User read-only/direct-write-route denial checks, with actual prerender/initial-server/later-WASM verification. Reuse existing render-mode/prehydration checks; do not merely assert the configured mode label. |

The matrix is a coverage inventory, not an instruction to create one giant lifecycle test. Keep failures localized. No test for passive DTO construction, an exact file count, source prose, or every geometry permutation.

## 8. PR boundaries and recovery

Suggested code PRs: A = R00–R06 (behavior-preserving slices); B = R07–R11 (confirmed RBAC/accounts); C = R12–R13 (focused tests); D = R14–R15 (UI port, after upstream); each includes relevant R16 steps. Split further when account removals are too broad. Do not combine account capability removal, a schema redesign, and the UI port in one review.

At every boundary, keep a working app. On failure, stop the affected packet, retain a bounded diagnostic, and fix forward on the owned branch. Do not reset someone else's checkout or restore old project names. If a future case requirement conflicts with the sample domain, record a separate case adaptation task rather than guessing the case now.

## 9. Blockers and decisions

- **Human action required to write these plans:** none identified.
- **Code dependencies:** bootstrap integration and, for R15 only, upstream UI integration. Agents can carry out ordinary implementation, validation, PR work, and porting; do not present these as manual blockers.
- **Product decisions:** User read-only and Admin CRUD are confirmed. No open permission decision remains.
- **Deployment:** outside this plan. The older bootstrap runbook may have a separate operator authentication/sudo handoff. Do not import that blocker into this code plan or dispatch deployment implicitly.
- **Actual future case:** not needed for preparation. Its entities and business rules remain deliberately unspecified until received.

## 10. Execution ledger and completion

For each PR, record: packet IDs, base SHA, full PR-head SHA, PR URL, local gate result, exact-head CI run/attempt, integrated main SHA, exact-main verification, and any intentionally retained task dependency. For branch cleanup, record deleted branch full SHAs here or in its PR. For the UI port, also record upstream source SHAs and the final adapted path list.

| PR / packets | Head and integration | Checks | Remaining dependency |
| --- | --- | --- | --- |
| PR #17 / R00–R06 | Base `dff50c37cfd6cde870ef314c25fac518d12a40a3`; head `2ed3d08788fdb53800b2ffe0c8895f3ae0d09176`; integrated on `origin/main` at `1b99bcc0a9a07fd29dc1b6099e27af3339ec0ca0` | PR CI run `42641461708` and exact-main CI run `42642496818` succeeded | None |
| [PR #18](https://github.com/Grumlebob/RitzauDemo/pull/18) / R07–R13 | Base `1b99bcc0a9a07fd29dc1b6099e27af3339ec0ca0`; head `1d91200ebd0af27b9b18fb9a0e95934e7ed4362b`; squash-integrated on `origin/main` at `9f56957a16a65a10afd4be2d29afb9b324c42ee4` | Full local gate passed; focused real-cookie account tests passed 2/2; exact-head `build-test-push` run `42666220451` and CodeQL run `42666220487` succeeded; exact-main CI run `42667087078` and CodeQL run `42667087396` succeeded | None |

Completion requires visible slices with thin transport adapters, unchanged useful caching, real role differences, preserved ownership/security, a focused Testcontainers suite, one architecture test, a verified upstream UI port, minimal accurate docs, green required code checks on integrated main, and owned-resource closeout. Do not mark the whole plan complete while the UI port or another required packet is merely scheduled.
