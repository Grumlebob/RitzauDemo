# Optional local observability

This guide covers the optional telemetry profile for local development. It is outside the one-week Books CRUD product test scope and is not part of the required local gate. LocalSingleNode deployments keep observability disabled.

## Start and inspect

From the repository root, start the local stack with observability:

```powershell
.\Scripts\LocalDevelopment\RunLocal.ps1 -Observability
```

Grafana is available at `http://127.0.0.1:3000`; Prometheus, Alertmanager, Loki, and Tempo use loopback ports `9090`, `9093`, `3100`, and `3200`. The local Grafana account permits anonymous admin access. Keep every telemetry port bound to loopback and do not publish or forward them.

For app errors, check the app's `/health/ready` endpoint first. Then inspect local app logs and telemetry. Useful Prometheus queries:

```promql
up
sum(rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m]))
```

Useful Loki query:

```logql
{service="web"} |= "Error"
```

## Stop or recover

Stop only the optional telemetry services while leaving the app and data services running:

```powershell
.\Scripts\LocalDevelopment\RunLocal.ps1 -StopObservability
```

This preserves local volumes. The app may still attempt telemetry export, but failed export must not affect requests. Resume the profile with `RunLocal.ps1 -Observability`. For local resource pressure, disable telemetry export or stop the optional profile; do not remove app, database, or Redis data to recover observability.
