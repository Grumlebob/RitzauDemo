# Architecture

RitzauDemo is one Blazor Auto app. The Books feature is the demo path. The server enforces ownership and authorization for every private Books operation.

| Project | Responsibility |
| --- | --- |
| `RitzauDemo.Core` | Books domain rules, API contracts and request/response DTOs. |
| `RitzauDemo.Client` | Blazor routes, components, API implementations and UI state. |
| `RitzauDemo` | HTTP endpoints, Books operations, EF persistence and hosting. |
| `RitzauDemo.Test` | Books validation, role-aware PostgreSQL/Redis integration coverage, account cookie flows, one dependency-boundary test and an optional packaged render smoke. |

Books contracts and validation live in `RitzauDemo.Core/Features/Books`. Server operations and persistence live in `RitzauDemo/Features/Books`. Blazor views live in `RitzauDemo.Client/Features/Books`. Keep UI code on shared Core contracts and enforce data ownership on the server.

Identity composes the seeded roles, cookie sessions, and request protections used by Books. See [Accounts](Accounts.md) for role behavior and local seed setup.

Use the [local gate](Test.md#local-gate) before code changes. Keep operational release and host-safety checks even though product behavior tests stay focused on CRUD.
