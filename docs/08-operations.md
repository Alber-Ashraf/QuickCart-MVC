# QuickCart: Operations, Monitoring & Maintenance

Last updated: 2026-10-21

## 1. Observability Architecture
QuickCart implements end-to-end distributed tracing, structured logging, and health diagnostics to ensure production reliability and rapid incident triage.

### Correlation ID Flow
1. Incoming HTTP requests are intercepted by the `X-Correlation-ID` middleware.
2. If an upstream gateway provided `X-Correlation-ID`, it is preserved; otherwise, a new `Guid.NewGuid().ToString("N")` is assigned.
3. The ID is written to the outbound response header.
4. The ID is pushed into the `ILogger.BeginScope` dictionary alongside `RequestMethod` and `RequestPath`.
5. All downstream controller, service, and EF Core log events inherit the correlation ID.

---

## 2. Health Monitoring (`/health`)

The application exposes an operational health endpoint at `/health` reporting database readiness and query execution duration.

### Sample Probe Response
```json
{
  "status": "Healthy",
  "totalDuration": "3.85ms",
  "timestamp": "2026-10-21T14:00:00.000Z",
  "entries": [
    {
      "check": "sql_server_database",
      "status": "Healthy",
      "duration": "3.75ms",
      "description": null,
      "exception": null
    }
  ]
}
```

---

## 3. Alerting Thresholds & Operational Runbook

| Alert Name | Severity | Condition | Triage Procedure |
|---|---|---|---|
| **DatabaseUnhealthy** | Critical | `/health` returns status != `Healthy` for 2 consecutive checks | Verify SQL Server container/server availability, connection string, and connection pool metrics. |
| **High5xxErrorRate** | High | > 2% of requests return 5xx over a 5-minute window | Query logs by `CorrelationId` to identify failing controller actions or unhandled exceptions. |
| **P95LatencySpike** | Medium | Catalog P95 latency > 1000ms for 5 minutes | Inspect SQL query execution plans, check index fragmentation, review server CPU/Memory pressure. |
| **BruteForceLockoutSpike** | Warning | > 20 account lockouts in 5 minutes | Review IP source addresses; consider enabling Cloudflare WAF rate limiting on `/Identity/Account/Login`. |

---

## 4. Routine Maintenance

- **Security & Package Updates:** Run `dotnet list package --vulnerable` monthly. Update NuGet dependencies within the .NET 8 LTS release train.
- **Database Backup:** Schedule nightly full SQL Server backups and 15-minute transaction log backups.
- **Index Maintenance:** Run SQL index reorganization monthly on high-write tables (`OrderHeaders`, `OrderDetails`, `ShoppingCarts`).
