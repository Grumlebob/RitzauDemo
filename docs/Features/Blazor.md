# Blazor client

The Blazor client is the only frontend. Build it with the `BlazorAuto` profile. Pages prerender on the server and then use server or WebAssembly interactivity.

## Features and API calls

Keep client UI and state under `RitzauDemo.Client/Features/{Feature}`. Put shared requests and response types in `RitzauDemo.Core/Features/{Feature}`. Client services implement those contracts; Razor components do not inject raw `HttpClient`.

Put server behavior under `RitzauDemo/Features/{Feature}` and enforce authorization and data ownership there. Handle loading, empty, error, unauthorized and not-found states where reachable. Keep the Books CRUD example as the reference for client and server requests.

## Rendering and style

Keep prerender data small and public. Do not serialize private user data into the page. Disable interactive controls until Blazor becomes interactive. Use the existing Tailwind utilities and rebuild `RitzauDemo/wwwroot/tailwind.css` when Razor classes or client styles change:

```powershell
Set-Location .\RitzauDemo.Client
npm ci
npm run css:build
```

The optional packaged E2E smoke checks Blazor hydration. Product behavior tests otherwise stay focused on Books CRUD; see [Test](../Test.md).
