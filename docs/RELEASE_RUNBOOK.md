# QuickCart Release Runbook & Deployment Guide

## Overview
This runbook defines the operational standards and step-by-step procedures for deploying, maintaining, and rolling back QuickCart in staging and production environments.

---

## 1. Environment Architecture & Prerequisites
- **Runtime:** .NET 8.0 LTS (`mcr.microsoft.com/dotnet/aspnet:8.0`)
- **Database:** Microsoft SQL Server 2022+ (Azure SQL or Containerized)
- **External Dependencies:**
  - Stripe API (Payment processing & refunds)
  - Backblaze B2 S3-Compatible Cloud Storage
  - Azure Web App / Container Apps / Kubernetes (K8s)

---

## 2. Configuration & Secret Management
Production configuration **must never** be committed to source control. Inject secrets through Environment Variables or Azure Key Vault / GitHub Secrets.

| Environment Variable | Description | Example / Note |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Application Environment | `Production` |
| `ConnectionStrings__DefaultConnection` | SQL Server Connection String | `Server=...;Database=...;User Id=...;Password=...;` |
| `Stripe__SecretKey` | Stripe API Secret Key | `sk_live_...` or `sk_test_...` |
| `Stripe__PublishableKey` | Stripe Client Publishable Key | `pk_live_...` or `pk_test_...` |
| `Backblaze__BucketName` | Backblaze B2 Bucket Name | `flyleaf-books` |
| `Backblaze__Region` | Backblaze B2 Region | `us-east-005` |
| `Backblaze__AccessKey` | Backblaze Application Key ID | `YOUR_BACKBLAZE_KEY_ID` |
| `Backblaze__SecretKey` | Backblaze Application Key Secret | `YOUR_BACKBLAZE_APPLICATION_KEY` |
| `Backblaze__Endpoint` | S3-Compatible API Endpoint | `https://s3.us-east-005.backblazeb2.com` |
| `AdminUser__Email` | Default Seed Admin Account Email | `admin@quickcart.com` |
| `AdminUser__Password` | Default Seed Admin Initial Password | Strong 14+ character password |

---

## 3. Database Migration Deployment
QuickCart uses Entity Framework Core idempotent SQL migration scripts to ensure zero-downtime, safe schema transitions:

### Executing Idempotent SQL Script (Recommended for Production)
The pre-compiled idempotent SQL bundle lives in `docs/migrations/bundle.sql`.
```bash
# Apply using sqlcmd or Azure DevOps / GitHub Actions pipeline step:
sqlcmd -S <server-name> -d QuickCartDb -U <db-user> -P <db-password> -i docs/migrations/bundle.sql
```

### Automated In-App Migration
In staging or single-instance setups, `DbInitializer.Initialize()` automatically executes:
```csharp
if (_db.Database.GetPendingMigrations().Any()) {
    _db.Database.Migrate();
}
```

---

## 4. Docker Deployment Procedures

### Standalone Docker Build & Run
```bash
# 1. Build optimized container image
docker build -t quickcart:latest .

# 2. Run container mapping host port 5000 to internal container port 8080
docker run -d \
  -p 5000:8080 \
  --name quickcart-web \
  --env-file .env \
  quickcart:latest
```

### Full-Stack Docker Compose Deployment
```bash
# 1. Copy sample environment file and configure secrets
cp .env.example .env

# 2. Start all services (SQL Server + Web Application)
docker-compose up -d --build

# 3. View container health and application logs
docker-compose ps
docker-compose logs -f quickcart-web
```

---

## 5. Post-Deployment Smoke Test & Verification Checklist

Execute the following checks immediately following every release:

- [ ] **HTTP 200 Root Check:** `GET /` returns `200 OK` with valid HTML.
- [ ] **Security Headers:** Response headers contain:
  - `X-Frame-Options: DENY`
  - `X-Content-Type-Options: nosniff`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Content-Security-Policy`
- [ ] **Database Connectivity:** Products catalog renders properly on home page.
- [ ] **Authentication & Identity:** User login and registration pages render without 500 errors.
- [ ] **Static Assets:** CSS tokens (`/css/tokens.css`), DataTables, and site JS load without 404s.
- [ ] **Stripe Connection:** Test checkout flow initiates Stripe Checkout session redirect without throwing exceptions.

---

## 6. Rollback Procedures

### Containerized Deployment (Docker / Kubernetes / Azure App Service)
1. **Re-tag or Redeploy Previous Release:**
   ```bash
   # Revert to previous immutable image digest or tag:
   docker pull myregistry.azurecr.io/quickcart:v1.0.X
   docker stop quickcart-web
   docker rm quickcart-web
   docker run -d -p 5000:8080 --name quickcart-web --env-file .env myregistry.azurecr.io/quickcart:v1.0.X
   ```

2. **Database Rollback:**
   - If a breaking schema change occurred:
     ```bash
     # Generate targeted rollback script:
     dotnet ef migrations script <TargetMigrationName> <SourceMigrationName> -o rollback.sql
     ```
   - For backwards-compatible migrations, code rollback alone is sufficient without rolling back database schema.
