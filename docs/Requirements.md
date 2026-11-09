# Requirements

Canonical rules for apps built from this template. Detailed commands live in the guides linked from [README.md](../README.md).

## Rendering

- In Blazor Auto, product routes must work during SSR/prerender, interactive server rendering, and browser WebAssembly rendering through `InteractiveAuto`.
- Do not force `InteractiveServer` on product routes to hide Auto, SSR or WebAssembly bugs. Any server-only exception needs a clear comment and focused verification.
- Interactive controls rendered during prerender stay disabled until the component is interactive (`disabled="@(!RendererInfo.IsInteractive)"`). The packaged browser smoke confirms Blazor hydration.
- In Blazor Auto, keep prerender payloads bounded. Use `PersistentComponentState` only for bounded public data, never for large response graphs.
- Blazor components use shared Core contracts or feature state, not raw `HttpClient`.

## UI

- Prefer Tailwind utilities and existing UI patterns. Rebuild and commit `GrumPlates/wwwroot/tailwind.css` when Tailwind output changes.
- Blazor component CSS is fine when it is cleaner or faster than a long utility string. Do not add another CSS framework without a plan.

## Performance and caching

- Public pages must be fast: useful SSR content, no N+1 queries, bounded API payloads, stable layouts, and cache behaviour that works across app nodes.
- Put a schema version in cache keys (for example `books:v2:...`) so a deploy that changes a cached shape does not read old entries.
- User-specific API responses are sent with `Cache-Control: private, no-store`.

## Runtime

- Redis is required outside development and test. Observability must fail open for app requests.
- API requests answer 401/403, never a redirect to the HTML login page.

## Testing

- Keep product behavior tests focused on Books CRUD: request validation unit tests, one database-backed create/list/read/update/delete integration path, and one optional packaged Blazor render smoke.
- Keep CI admission, release, deployment, backup/recovery, and host-safety checks as operational gates. Do not add separate account, Grafana, visual, or performance suites for this demo.
- Integration tests use shared Testcontainers fixtures; containers carry `localcluster.ci.*` labels and use disposable storage.

## Security

- Never log or commit secrets, auth headers, cookies, raw request bodies, connection strings or vault contents.
- Keep built-in rate limiting enabled: defaults are 600 global requests/minute, 60 API requests/minute and 20 account writes per client IP per five minutes. Static assets are exempt. Defaults are in [`appsettings.json`](../GrumPlates/appsettings.json); account-specific behavior is in [Accounts](Accounts.md).
- Trust forwarded headers only from configured proxies or networks.
- Local development can seed an Admin and a User through `Accounts:AdminSeed` and `Accounts:UserSeed`; deployments seed only the Admin. Registration remains disabled by default. See [Accounts](Accounts.md).
