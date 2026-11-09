# Accounts

The demo uses seeded `Admin` and `User` accounts. Registration, email delivery, external login, passkeys, two-factor setup, and account management are not included. The server remains authoritative for authentication, authorization, antiforgery, and cache policy.

Server composition and the sign-in pages live under `RitzauDemo/Features/Accounts`; shared account contracts live in `RitzauDemo.Core/Features/Accounts/Contracts`. The shared context and existing migrations remain under `RitzauDemo/Infrastructure/Persistence`.

`User` can read only their own private Books. `Admin` can read and create, update, or delete their own Books. Admin role does not bypass ownership. Private API requests return JSON `401`/`403` responses rather than login redirects, and responses use `Cache-Control: private, no-store`.

The authentication cookie is `Secure`, `HttpOnly`, `SameSite=Lax`, host-only, and fixed at eight hours, or fourteen days when “remember me” is selected. Login is rate limited and preserves lockout feedback. Passwords must contain 15–128 Unicode characters, contain no control characters, and differ from the application name and email; common passwords are rejected. The `logout-all` endpoint verifies the current password and rotates the security stamp. Sign-in state is revalidated on requests.

## Local seeded accounts

Local setup seeds one Admin and one User for the Blazor app. Run it once, or again if `.env` or the local certificate is missing:

```powershell
pwsh -File ./Scripts/LocalDevelopment/setup-local.ps1
```

Setup fills `Accounts__AdminSeed__*` with `admin@localhost.test` and `Accounts__UserSeed__*` with `user@localhost.test` in the ignored `.env`, each with a generated 24-character password. It prints the emails, never the passwords. It leaves a seed alone when its email or password is already set. Startup creates each account once, confirmed, with the `Admin` or `User` role. It never changes an existing password or email, and refuses an email that belongs to an account without the configured role.

Start the app and sign in at `https://localhost:7186/Account/Login`:

```powershell
.\Scripts\LocalDevelopment\RunLocal.ps1 -NoBrowser
```

`Accounts:AdminSeed` and `Accounts:UserSeed` each accept `Enabled`, `Email`, and `Password`. Their passwords must satisfy the account policy. Local setup enables both seeds; deployment defaults disable both. Keep `.env`, local databases, and generated certificates out of commits and images.

Data Protection keys and account data share PostgreSQL. Preserve the app identity, database, and wrapped key material across redeploys and backups. Follow [LocalSingleNode backup verification and recovery](../Deployment/LocalSingleNode/Operations.md#backups-and-recovery). Restart and restore acceptance uses disposable fixtures only; it does not authorize or describe a live data restore.
