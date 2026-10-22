# QuickCart: Deployment & Release Engineering

Last updated: 2026-10-21

## 1. Environments & Deployment Targets

| Environment | Host Infrastructure | Database | Migration Strategy |
|---|---|---|---|
| **Development** | Local workstation / Docker Compose | Local SQL Server 2022 | Auto-migrate on startup via `DbInitializer` |
| **Staging** | Azure App Service / Azure Container Apps | Azure SQL Database (Serverless) | Idempotent SQL script in pipeline step |
| **Production** | Azure App Service / Kubernetes Cluster | Azure SQL Database (Business Critical) | Pre-deployment gate executing `bundle.sql` |

---

## 2. Containerization: Multi-Stage Dockerfile

QuickCart utilizes a hardened, non-root multi-stage Docker build:
```dockerfile
# Build & Publish Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["QuickCart/QuickCart.csproj", "QuickCart/"]
COPY ["QuickCart.Models/QuickCart.Models.csproj", "QuickCart.Models/"]
COPY ["QuickCart.DataAccess/QuickCart.DataAccess.csproj", "QuickCart.DataAccess/"]
COPY ["QuickCart.Utility/QuickCart.Utility.csproj", "QuickCart.Utility/"]
RUN dotnet restore "QuickCart/QuickCart.csproj"
COPY . .
RUN dotnet publish "QuickCart/QuickCart.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QuickCart.dll"]
```

---

## 3. Automated CI Pipeline (`.github/workflows/ci.yml`)

Every push and pull request to `master` triggers automated verification:
1. Checkout source code.
2. Setup .NET 8.0 SDK.
3. Cache NuGet packages.
4. Restore dependencies across `QuickCart.sln`.
5. Build solution in `Release` configuration.
6. Execute 25 automated unit and integration tests (`dotnet test`).
7. Validate Docker build readiness.

---

## 4. Production Release & Rollback Procedures

Refer to [`docs/RELEASE_RUNBOOK.md`](RELEASE_RUNBOOK.md) for detailed operational release checklists, smoke tests, and rollback procedures.

### Summary Rollback Plan
- **Application Code:** Redeploy previous container image tag or commit SHA.
- **Database:** If non-breaking, no schema rollback required. For breaking changes, generate target rollback script via `dotnet ef migrations script <Target> <Source> -o rollback.sql`.
