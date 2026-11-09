# Frontend profile

RitzauDemo supports one frontend profile: `BlazorAuto`. The tracked `frontend-profile.txt` is the source of truth. Build and release validation reject other values; releases use acceptance profile `BlazorAutoV1`.

```powershell
python Scripts/Frontend/resolve_frontend_profile.py
python Scripts/Frontend/resolve_frontend_profile.py --acceptance-profile
```

Do not select a profile from machine environment state. Change the tracked profile only as part of a reviewed change that also updates the immutable release contract.

The Blazor client uses ASP.NET Core APIs and shares Core request and response contracts. See [Blazor feature guidance](Features/Blazor.md), the [release contract](../Deployment/Common/ReleaseContract.md), and [deployment operations](../Deployment/README.md).
