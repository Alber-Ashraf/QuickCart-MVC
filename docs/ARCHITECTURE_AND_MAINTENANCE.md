# QuickCart Architecture, Observability & Maintenance Guide

## 1. System Architecture Overview

QuickCart is built as a modular ASP.NET Core 8 MVC web application adhering to Clean Architecture principles, strict separation of concerns, and the Repository / Unit of Work design pattern.

```
                      ┌────────────────────────────────────────┐
                      │              Web Browser               │
                      └──────────────────┬─────────────────────┘
                                         │ HTTPS / Cookie Auth
                                         ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│ QuickCart (ASP.NET Core 8 Web Host)                                             │
│                                                                                 │
│   Middleware Pipeline:                                                          │
│   ├── Correlation ID Middleware (X-Correlation-ID)                              │
│   ├── Security Headers Middleware (CSP, X-Frame-Options, X-Content-Type)        │
│   ├── Cookie Authentication & Lockout                                           │
│   ├── Global Anti-Forgery Token Validation                                      │
│   └── Health Check Endpoint (/health)                                           │
│                                                                                 │
│   Areas:                                                                        │
│   ├── Customer Area (HomeController, CartController, Shop Views)                │
│   ├── Admin Area (ProductController, CategoryController, Company, Order, User)  │
│   └── Identity Area (Hardened Login, Register, Lockout)                         │
└────────────────────────┬───────────────────────────────────┬────────────────────┘
                         │                                   │
                         ▼                                   ▼
┌──────────────────────────────────┐        ┌─────────────────────────────────────┐
│ QuickCart.Models                 │        │ QuickCart.DataAccess                │
│ ├── Entities (Product, Category) │        │ ├── QuickCartDbContext (EF Core 8)  │
│ ├── OrderHeader & OrderDetails   │        │ ├── UnitOfWork & Repositories       │
│ └── ViewModels (EmptyState, etc.)│        │ └── EF Core Migrations              │
└──────────────────────────────────┘        └──────────────────┬──────────────────┘
                                                               │
                                                               ▼
                                            ┌─────────────────────────────────────┐
                                            │ Microsoft SQL Server 2022+          │
                                            └─────────────────────────────────────┘
```

---

## 2. Observability & Monitoring Infrastructure

### Correlation IDs & Structured Logging
Every HTTP request is assigned a unique `X-Correlation-ID` header (or preserves an upstream correlation ID if supplied). This ID is attached to the HTTP response and injected into an active `ILogger.BeginScope` context:
```json
{
  "Timestamp": "2026-10-21T13:15:00.000Z",
  "LogLevel": "Information",
  "Message": "Order #1042 successfully updated to StatusShipped",
  "CorrelationId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "RequestMethod": "POST",
  "RequestPath": "/Admin/Order/StartShipping"
}
```

### Health Check Endpoint (`/health`)
The `/health` endpoint validates database liveness, query execution, and response latencies:
```bash
curl -i http://localhost:5000/health
```

**Response Format:**
```json
{
  "status": "Healthy",
  "totalDuration": "4.21ms",
  "timestamp": "2026-10-21T13:15:00.0000000Z",
  "entries": [
    {
      "check": "sql_server_database",
      "status": "Healthy",
      "duration": "4.12ms",
      "description": null,
      "exception": null
    }
  ]
}
```

### Alerting & Metric Thresholds

| Metric | Warning Threshold | Critical Threshold | Recommended Action |
|---|---|---|---|
| **Health Check Status** | Degraded | Unhealthy | Check SQL Server container / connection pool exhaustion |
| **HTTP 5xx Error Rate** | > 1% in 5 min | > 5% in 5 min | Inspect structured logs using `CorrelationId` |
| **P95 Request Latency** | > 500 ms | > 1500 ms | Review slow SQL queries or missing indexes |
| **Failed Logins (Lockout)**| > 20 / min | > 100 / min | Possible credential stuffing attack; review IP blocks |

---

## 3. Maintenance & Developer Workflows

### Running Automated Test Suites
```bash
# Execute all 25 unit and integration tests across the solution:
dotnet test QuickCart.sln
```

### Adding New Entities & Migrations
1. Add entity class to `QuickCart.Models`.
2. Register `DbSet<T>` in `QuickCart.DataAccess/Data/QuickCartDbContext.cs`.
3. Create repository interface in `IRepository/` and implementation in `Repository/`.
4. Register property in `IUnitOfWork` and instantiate in `UnitOfWork`.
5. Generate migration:
   ```bash
   dotnet ef migrations add AddYourEntityToDb --project QuickCart.DataAccess --startup-project QuickCart
   ```
6. Re-generate idempotent bundle script:
   ```bash
   dotnet ef migrations script --idempotent --project QuickCart.DataAccess --startup-project QuickCart -o docs/migrations/bundle.sql
   ```

### UI Token Usage
Always reference design tokens from `wwwroot/css/tokens.css` rather than hardcoding hex values:
- Use `var(--qc-primary)` for brand actions.
- Use `var(--qc-success)`, `var(--qc-warning)`, `var(--qc-danger)` for semantic alerts and status badges.
- Use `<partial name="_EmptyState" model="@emptyStateVM" />` whenever a query returns zero results.
- Use `<partial name="_StatusBadge" model="@orderStatus" />` for order statuses.

---

## 4. Security Practices & OWASP Checklist
- **Anti-CSRF:** Global `AutoValidateAntiforgeryTokenAttribute` is active; all forms must include `@Html.AntiForgeryToken()` (or standard `<form method="post">` TagHelper).
- **Object Ownership (BOLA/IDOR):** Never mutate or display user data relying solely on route IDs. Always cross-reference `entity.ApplicationUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)` unless caller possesses `SD.Role_Admin`.
- **Identity Lockout:** Keep lockout settings active (5 attempts, 5-minute lockout) to prevent brute-force attacks.
- **Sensitive Data Exposure:** Never return raw `IdentityUser` objects from API endpoints. Always project to explicit, sanitized view models.
