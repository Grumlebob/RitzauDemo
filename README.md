# RitzauDemo

RitzauDemo is a .NET 10 demo app with one Blazor Auto frontend. Books CRUD is the product example, backed by PostgreSQL and Redis. Account registration stays disabled by default.

Run the local Docker stack:

```powershell
.\Scripts\RunLocal.ps1
```

## Start here

| Task | Guide |
| --- | --- |
| Understand contributor rules and product requirements | [AGENTS.md](AGENTS.md), [Requirements](docs/Requirements.md) |
| Run the app or change local settings | [Run locally](docs/HowToRunLocally.md) |
| Add a Blazor feature | [Feature guide](docs/HowToAddANewFeature.md) |
| Understand the Blazor frontend | [Frontend profile](docs/FrontendProfiles.md), [Blazor feature guide](docs/Features/Blazor.md) |
| Configure account settings | [Accounts runbook](docs/Accounts.md) |
| Run and verify checks | [Tests](docs/Test.md) |
| Prepare a fork or a standalone product | [Fork guide](docs/HowToForkThisRepo.md), [standalone product](docs/StandaloneProduct.md) |
| Choose and operate a deployment target | [Deployment guide](Deployment/README.md) |
| View or diagnose telemetry | [Observability operations](docs/Observability/Operations.md) |
| Run optional quality audits | [Quality tools](docs/Quality/QualityTools.md), [SonarQube](docs/Quality/SonarQubeAnalysis.md) |

Read only the guides needed for your task. The contributor rules point to required checks and integration steps.

## Repository map

- `GrumPlates.Core/Features/` contains shared feature contracts and domain types.
- `GrumPlates/Features/` contains server behavior and endpoint mappings.
- `GrumPlates.Client/Features/` contains Blazor client behavior and UI.
- `GrumPlates.Test/` contains Books CRUD unit/integration tests and one opt-in packaged browser smoke.
- `Deployment/` contains the LocalSingleNode target and shared release tooling.
- `Plans/` contains tracked plans; `Plans.local/` is ignored scratch space.
