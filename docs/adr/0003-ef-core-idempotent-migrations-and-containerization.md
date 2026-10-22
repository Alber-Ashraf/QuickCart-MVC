# 3. Idempotent SQL Migrations & Containerized Deployment

- **Status:** Accepted
- **Date:** 2026-10-21
- **Deciders:** Alber Ashraf, Principal .NET Architect

## Context
Running `Database.Migrate()` automatically inside the web application at startup can lead to race conditions, database deadlocks, and application crashes when multiple container replicas scale up simultaneously. Production deployments require repeatable, auditable, and idempotent database schema deployment scripts that can be inspected and approved by DBAs prior to rollout.

## Decision
1. For development and single-instance local testing, `DbInitializer` handles migration execution.
2. For staging and production releases, we generate an idempotent SQL script (`docs/migrations/bundle.sql`) using `dotnet ef migrations script --idempotent`.
3. Application hosting is standardized using a multi-stage `Dockerfile` and `docker-compose.yml` defining the web host and SQL Server 2022 service.

## Consequences
- **Positive:** Guarantees zero-downtime, multi-replica safe deployments; provides DBAs with pre-execution visibility; enables easy rollback auditing.
- **Negative:** Requires generating and committing the SQL bundle whenever new schema migrations are added.
