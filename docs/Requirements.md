# Requirements

RitzauDemo is a small CRUD demonstration with Books as its example. It supports one frontend and one application instance per LocalSingleNode deployment. Accounts use seeded roles; registration and email-based account operations are not included. Detailed local commands live in [README](../README.md) and [Run locally](HowToRunLocally.md).

## Product scope

- Build and run the Blazor Auto app (`BlazorAuto`). React and alternate frontend profiles are not supported.
- Keep the Books example easy to follow. Cover request validation, role-aware ownership and CRUD, real-cookie account sessions, cache freshness, and project dependency direction with a small PostgreSQL/Redis-backed suite and one compiled architecture test.
- Use seeded `User` and `Admin` roles for private Books: Users read their own books, and Admins perform CRUD on their own books. Preserve sign-in, sign-out, lockout, ownership, and session revocation behavior.
- Deploy only to LocalSingleNode. Keep application resources, credentials, database, and Data Protection keys isolated from neighboring sites.

## Blazor behavior

- Product pages support server prerendering and Blazor Auto interactivity. Do not switch pages to server-only rendering to hide an error.
- Prerendered controls remain disabled until interactive. Keep persisted prerender data bounded and public; do not put user-specific data in the page payload.
- Client components use shared Core contracts or feature state. They do not call raw `HttpClient` directly.
- Rebuild and commit `RitzauDemo/wwwroot/tailwind.css` when Razor classes or client styles change.

## Data and runtime

- PostgreSQL stores Books and account data. Redis supports the configured runtime cache and coordination behavior.
- The app and database use separate app-owned resources and backups. Preserve the existing initial migration ID and schema when updating the demo.
- User-specific API responses use `Cache-Control: private, no-store`. API authentication failures return `401` or `403`, not an HTML login redirect.
- Local development may seed an Admin and a User. Deployments keep both seed switches disabled by default.
- Deployment observability remains disabled. Telemetry export must fail open so it cannot block application requests.

## Verification and security

- Product test counts and commands are documented in [Test](Test.md). Keep repository admission, immutable release identity, deployment, backup/recovery and shared-host safety as operational gates.
- Integration tests use disposable Testcontainers resources and leave host data untouched.
- Never log or commit secrets, authorization headers, cookies, request bodies, connection strings or vault contents.
- Trust forwarded headers only from configured proxies or networks. Keep local `.env`, generated certificates, databases, and private deployment state outside commits and images.
