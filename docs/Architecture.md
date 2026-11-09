# Architecture

RitzauDemo is one Blazor Auto app. The Books feature is the demo path. The server enforces ownership and authorization for every private Books operation.

| Project | Responsibility |
| --- | --- |
| `GrumPlates.Core` | Books domain rules, API contracts and request/response DTOs. |
| `GrumPlates.Client` | Blazor routes, components, API implementations and UI state. |
| `GrumPlates` | HTTP endpoints, Books operations, EF persistence and hosting. |
| `GrumPlates.Test` | Books CRUD request validation, database-backed integration coverage and one optional packaged render smoke. |

Books contracts and validation live in `GrumPlates.Core/Features/Books`. Server operations and persistence live in `GrumPlates/Features/Books`. Blazor views live in `GrumPlates.Client/Features/Books`. Keep UI code on shared Core contracts and enforce data ownership on the server.

Identity and account settings remain available for protected Books operations, but are outside the product test showcase. See [Accounts](Accounts.md) for registration defaults and deployment settings.

Use the [local gate](Test.md#local-gate) before code changes. Keep operational release and host-safety checks even though product behavior tests stay focused on CRUD.
