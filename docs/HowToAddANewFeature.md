# Add a feature

Use this page to add a small feature to the single Blazor Auto app. Read [Requirements](Requirements.md) for product rules and [Blazor client](Features/Blazor.md) for UI conventions.

## Order

1. Define the behavior, data scope, access rules, routes, and what is out of scope.
2. For shared behavior, put domain types and API contracts in `RitzauDemo.Core/Features/{Feature}`. UI-only features keep their state and types in the selected client; add no unused server contract or DTOs.
3. When server behavior is needed, add it in `RitzauDemo/Features/{Feature}`. Skip persistence when the feature does not store data. Follow [Server feature guidance](Features/Server.md).
4. Implement UI under `RitzauDemo.Client/Features/{Feature}`. Keep the component focused and use the Core contract or feature state for server data.
5. Test behavior at its boundary. Keep Books CRUD unit and integration coverage focused; the packaged render/hydration smoke is the only optional E2E. Use [Infrastructure testing](Testing/Infrastructure.md) and [Browser testing](Testing/Browser.md).
6. Update configuration, observability, or operator docs only when the feature changes those behaviors. Run the [local gate](Test.md#local-gate).

## Boundaries

Core owns shared contracts and types; it has no EF Core, ASP.NET Core, or browser HTTP implementation. The client references Core, never the server. Server services enforce data ownership and authorization scope even when endpoints also require authorization. The client calls shared contracts through a client service or feature state; Razor components do not inject `HttpClient`. See the [server](Features/Server.md) and [Blazor](Features/Blazor.md) pages for the details and source examples.

Books is the demo's CRUD example. Keep future changes small, do not import existing data during startup, and verify schema changes through reviewed migrations.
