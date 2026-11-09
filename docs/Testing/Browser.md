# Browser smoke

RitzauDemo keeps one opt-in Playwright E2E. It starts the packaged Blazor app, waits for interactivity, and confirms the hydrated renderer. The Books CRUD integration test covers the create/list/read/update/delete API path; there is no signed-in CRUD browser journey.

The CI image smoke starts PostgreSQL, Redis and the built app with run-owned disposable resources. It checks readiness, rendered Blazor HTML, and anonymous `/api/books` authorization before running the browser smoke. A separate deployed-site acceptance checks health and account-route behavior without creating users or writing data.

Run the local app, then run the single browser test:

```powershell
.\Scripts\RunLocal.ps1 -NoBrowser
pwsh .\GrumPlates.Test\bin\Debug\net10.0\playwright.ps1 install chromium
$env:RUN_E2E='1'
$env:E2E_BASE_URL='https://localhost:7186'
dotnet test .\GrumPlates.Test\GrumPlates.Test.csproj --filter "FullyQualifiedName~RenderModeE2ETests"
Remove-Item Env:\RUN_E2E,Env:\E2E_BASE_URL -ErrorAction SilentlyContinue
```

The browser is headed by default. Set `E2E_HEADLESS=1` for a headless run. `E2E_BASE_URL` defaults to `https://localhost:7186`. Failure screenshots and traces are written under `TestResults/Playwright`.
