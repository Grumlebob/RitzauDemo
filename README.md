# RitzauDemo

RitzauDemo is a small one-week .NET 10 demo. Its single Blazor Auto app demonstrates Books CRUD with PostgreSQL and Redis. Account registration and production observability are off by default. LocalSingleNode is the only deployment target.

## Run locally

From the repository root, start Docker Desktop and run:

```powershell
.\Scripts\LocalDevelopment\RunLocal.ps1
```

The first run creates ignored local settings and a development certificate. The app opens at `https://localhost:7186`. See [local development](docs/HowToRunLocally.md) for prerequisites, reset behavior and direct .NET commands.

## Verify changes

```bash
bash Scripts/CI/validate-frontend-profiles.sh /tmp/ritzaudemo-test-results
```

This runs the focused Books CRUD unit and database-backed integration tests. One packaged Blazor render/hydration smoke is optional and off by default. The full operational gate for code and configuration changes is in [Test](docs/Test.md).

## Product and operations

- [Requirements](docs/Requirements.md) describes the demo's product and test scope.
- [Accounts](docs/Accounts.md) documents the existing sign-in routes and disabled registration policy.
- [Deployment](Deployment/README.md) covers the isolated LocalSingleNode release, backups and recovery.
- `Scripts/LocalDevelopment` contains local app, certificate and status helpers.
- `Scripts/LocalObservability` contains the optional developer-only telemetry stack.
- `Scripts/Quality/SonarQube` contains optional local quality-analysis resources.

RitzauDemo preserves the source history and is based on the [GrumPlates template](https://github.com/Grumlebob/GrumPlates).
