# Flyleaf (formerly QuickCart) Project Guide for Claude

Last updated: 2026-10-21

## Project Overview
Flyleaf is an enterprise-grade ASP.NET Core 8 MVC web application for literary and book e-commerce, built with Clean Architecture, Entity Framework Core 8, ASP.NET Core Identity, Stripe Hosted Checkout, and Backblaze B2 S3-Compatible Cloud Storage.

- **Target Framework:** .NET 8.0 (`net8.0`), C# 12
- **Database:** Microsoft SQL Server 2022+ / Azure SQL
- **Architecture:** ASP.NET Core MVC with Areas (Customer, Admin, Identity), Repository Pattern, and Unit of Work

---

## Essential Commands

### Build & Run
```bash
# Build the entire solution
dotnet build QuickCart.sln

# Run web application (Customer/Admin portal)
dotnet run --project QuickCart

# Run with hot reload
dotnet watch --project QuickCart
```

### Testing
```bash
# Run all unit and integration tests
dotnet test QuickCart.sln

# Run tests with detailed verbosity
dotnet test QuickCart.sln --logger "console;verbosity=normal"
```

### Database & Migrations
```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project QuickCart.DataAccess --startup-project QuickCart

# Update local database
dotnet ef database update --project QuickCart.DataAccess --startup-project QuickCart

# Generate idempotent SQL script for production deployment
dotnet ef migrations script --idempotent --project QuickCart.DataAccess --startup-project QuickCart -o docs/migrations/bundle.sql
```

### Docker
```bash
# Build and run with Docker Compose
docker-compose up -d --build

# Inspect logs
docker-compose logs -f quickcart-web
```

---

## Folder Map & Architecture

```
QuickCart.sln
├── QuickCart/                      # Web Presentation Layer (MVC + Identity)
│   ├── Areas/
│   │   ├── Customer/Controllers/   # HomeController, CartController
│   │   ├── Admin/Controllers/      # Product, Category, Company, Order, UserController
│   │   └── Identity/Pages/         # Hardened Login, Register, Lockout
│   ├── Views/Shared/               # _Layout, _Notification, _EmptyState, _StatusBadge
│   ├── wwwroot/                    # css/tokens.css, js/site.js, DataTables scripts
│   └── Program.cs                  # Pipeline, Security Headers, Correlation ID, /health
├── QuickCart.Models/               # Domain Entities, DTOs & ViewModels
├── QuickCart.DataAccess/           # EF Core DbContext, Repositories, UnitOfWork, Migrations
├── QuickCart.Utility/              # SD (Static Details), EmailSender, BackblazeStorageService, OAuthService, StripeSettings
├── QuickCart.Tests/                # xUnit Test Suite (32 unit, integration, OAuth & BOLA security tests)
└── docs/                           # Central architectural and operational documentation
```

---

## Core Conventions & Gotchas

1. **Security & Anti-CSRF:**
   - Global `AutoValidateAntiforgeryTokenAttribute` is enabled on all controllers.
   - All AJAX `POST`/`PUT`/`DELETE` calls must send the request verification token (handled globally in `site.js`).

2. **Authorization & BOLA / IDOR Defense:**
   - Any access or mutation of user-owned resources (e.g., `ShoppingCart`, `OrderHeader`) **must** verify `entity.ApplicationUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)`.
   - Never rely exclusively on route parameters (`cartId`, `orderId`) without validating ownership.

3. **Asynchronous Data Access:**
   - Use asynchronous methods (`GetAllAsync`, `GetAsync`, `SaveAsync`) across all controllers and services to prevent thread-pool starvation.

4. **Identity & Privilege Escalation:**
   - Public self-registration (`Register.cshtml.cs`) strictly assigns `SD.Role_Customer`.
   - Administrative roles can only be granted by existing administrators through `/Admin/User`.

5. **Design System & UI Tokens:**
   - Use CSS custom properties defined in `wwwroot/css/tokens.css` (`var(--qc-primary)`, `var(--qc-surface)`, etc.).
   - Mandatory 4 UI states: Loading (`.qc-btn-loading`), Empty (`_EmptyState.cshtml`), Error (Toastr / validation summaries), and Populated.

6. **External OAuth Authentication (Google & GitHub):**
   - Configured via `OAuthSettings` (`GoogleClientId`, `GoogleClientSecret`, `GithubClientId`, `GithubClientSecret`) adapted from Flexfolio.
   - `ExternalLogin.cshtml.cs` implements automatic email-based account linking for verified external identities, preventing duplicate email registration crashes.

---

## Key Documentation Links
- [Requirements & Scope](docs/01-requirements.md)
- [Architecture & System Design](docs/02-architecture.md)
- [Database Schema & Migrations](docs/03-database.md)
- [API & Controller Endpoints](docs/04-api.md)
- [UI/UX & Design Tokens](docs/05-ui-ux.md)
- [Testing Strategy & Test Suite](docs/06-testing.md)
- [Deployment & Release Runbook](docs/07-deployment.md)
- [Operations, Monitoring & Health](docs/08-operations.md)
- [Roadmap & Milestone Tracking](docs/10-roadmap.md)
- [Architecture Decision Records](docs/adr/)
