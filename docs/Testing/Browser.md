# Browser tests

RitzauDemo keeps a small opt-in Playwright suite for the packaged Blazor app. `RenderModeE2ETests` checks server prerendering, initial Interactive Server, and a later WebAssembly reload. `BooksCrudE2ETests` covers anonymous public browsing, User read-only access, denied write routes and a safe API error state, plus an Admin empty-list and validation/create/reload/update/delete/logout journey.

The signed-in browser tests read enabled Admin and User seed credentials from the untracked repository `.env`. They can also use `E2E_ADMIN_EMAIL`, `E2E_ADMIN_PASSWORD`, `E2E_USER_EMAIL`, and `E2E_USER_PASSWORD`. Keep those values local and never put them in command history or tracked files.

The CI image smoke starts PostgreSQL, Redis and the built app with run-owned disposable resources. It checks readiness, rendered Blazor HTML, and anonymous `/api/books` authorization before running the browser smoke. A separate deployed-site acceptance checks health and account-route behavior without creating users or writing data.

Ensure `.env` has enabled Admin and User seeds. Run the local app, then run the browser suite:

```powershell
pwsh .\Scripts\LocalDevelopment\setup-local.ps1
.\Scripts\LocalDevelopment\RunLocal.ps1 -NoBrowser
pwsh .\RitzauDemo.Test\bin\Debug\net10.0\playwright.ps1 install chromium
$env:RUN_E2E='1'
$env:E2E_BASE_URL='https://localhost:7186'
dotnet test .\RitzauDemo.Test\RitzauDemo.Test.csproj --filter "FullyQualifiedName~RenderModeE2ETests|FullyQualifiedName~BooksCrudE2ETests"
Remove-Item Env:\RUN_E2E,Env:\E2E_BASE_URL -ErrorAction SilentlyContinue
```

The browser is headed by default. Set `E2E_HEADLESS=1` for a headless run. `E2E_BASE_URL` defaults to `https://localhost:7186`. Failure screenshots and traces are written under `TestResults/Playwright`.
