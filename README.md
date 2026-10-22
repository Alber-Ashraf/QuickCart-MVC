# 📚 Flyleaf (formerly QuickCart-MVC)

Enterprise-grade ASP.NET Core 8 MVC literary e-commerce platform built with Clean Architecture, Entity Framework Core 8, ASP.NET Core Identity, Stripe Hosted Checkout, and Backblaze B2 S3-Compatible Cloud Storage.

[![CI Build & Test](https://github.com/Alber-Ashraf/QuickCart-MVC/actions/workflows/ci.yml/badge.svg)](https://github.com/Alber-Ashraf/QuickCart-MVC/actions)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)
![Tests](https://img.shields.io/badge/Tests-25%20Passed-brightgreen.svg)
![OWASP Hardened](https://img.shields.io/badge/Security-OWASP%20Hardened-green.svg)

---

## 🌟 Key Capabilities

- 🛍️ **Customer Experience:** Responsive product catalog with search, category filtering, tiered quantity pricing discounts, persistent shopping cart, and Stripe payment gateway integration.
- 🔐 **Hardened Security:** Anti-CSRF protection, claim-based BOLA/IDOR resource ownership defenses, account lockout after 5 failed attempts, privilege escalation prevention, and OWASP security headers (CSP, HSTS, X-Frame-Options).
- 👔 **Administrative Suite:** Real-time DataTables management for Products, Categories, Companies, Orders, and User accounts with role assignments and account locking.
- ⚡ **High-Performance Architecture:** Fully asynchronous EF Core 8 queries, Repository & Unit of Work patterns, structured logging with correlation IDs, and `/health` operational monitoring.
- 🎨 **Modern Design System:** Bootstrap 5.3 + custom CSS design tokens (`tokens.css`), SweetAlert2 confirmations, Toastr notifications, and 4 mandatory UI states (Loading, Empty, Error, Populated).
- 🐳 **Cloud-Native & Production-Ready:** Multi-stage Docker containerization, Docker Compose for local orchestration, and idempotent SQL migrations (`docs/migrations/bundle.sql`).

---

## 🛠️ Technology Stack

| Layer | Technology |
|---|---|
| **Framework & Language** | .NET 8.0 (`net8.0`), C# 12, ASP.NET Core MVC with Razor |
| **Data Access & ORM** | Entity Framework Core 8.0.22, SQL Server 2022+ |
| **Identity & Auth** | ASP.NET Core Identity, Role-Based Access Control (RBAC), External Facebook OAuth |
| **Payments & Cloud** | Stripe.net (v48.1.0) Hosted Checkout & Refunds, Backblaze B2 S3 Storage (AWSSDK.S3) |
| **Frontend & UI** | Bootstrap 5.3.3, Bootstrap Icons, DataTables 2.2.2, SweetAlert2, Toastr |
| **Testing** | xUnit, EF Core In-Memory Database, Moq |
| **DevOps & Containers** | Docker, Docker Compose, GitHub Actions CI |

---

## 🚀 Quick Start

### 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft SQL Server 2022](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or Docker)
- [Git](https://git-scm.com/)

### 2. Clone and Configure
```bash
git clone https://github.com/Alber-Ashraf/QuickCart-MVC.git
cd QuickCart-MVC

# Copy example environment configuration
cp .env.example .env
```

### 3. Run with Docker Compose (Fastest)
```bash
docker-compose up -d --build
```
Navigate to `http://localhost:5000` to view the store.

### 4. Or Run Locally via .NET CLI
```bash
# Restore and build
dotnet build QuickCart.sln

# Update database schema
dotnet ef database update --project QuickCart.DataAccess --startup-project QuickCart

# Run application
dotnet run --project QuickCart
```

---

## 🧪 Automated Testing

QuickCart includes 25 automated unit and integration tests covering security authorization, pricing calculations, catalog search, and data access:

```bash
# Run the test suite
dotnet test QuickCart.sln
```

---

## 📁 Solution Structure

```
QuickCart.sln
├── QuickCart/                      # Web Presentation Layer (Controllers, Views, Areas)
│   ├── Areas/
│   │   ├── Customer/               # Customer storefront, cart, checkout
│   │   ├── Admin/                  # Administration of products, orders, users
│   │   └── Identity/               # Secure login, registration, lockout
│   └── Program.cs                  # Middleware pipeline, correlation ID, /health
├── QuickCart.Models/               # Domain entities, ViewModels, DTOs
├── QuickCart.DataAccess/           # DbContext, Repository pattern, Unit of Work, Migrations
├── QuickCart.Utility/              # Static details (SD), Email sender, Backblaze B2 storage
├── QuickCart.Tests/                # xUnit test project
└── docs/                           # Architecture, database, deployment, and API docs
```

---

## 📚 Documentation Index

Detailed architectural and operational documentation is available in [`docs/`](docs/):

- [01. Requirements & Scope](docs/01-requirements.md)
- [02. Architecture & System Design](docs/02-architecture.md)
- [03. Database Schema & Migrations](docs/03-database.md)
- [04. API & Controller Specification](docs/04-api.md)
- [05. UI/UX & Design Tokens](docs/05-ui-ux.md)
- [06. Testing Strategy & QA](docs/06-testing.md)
- [07. Deployment & Release Runbook](docs/07-deployment.md)
- [08. Operations & Monitoring](docs/08-operations.md)
- [10. Roadmap & Milestones](docs/10-roadmap.md)
- [Architecture Decision Records (ADRs)](docs/adr/)

---

## 💡 Author
Developed with ❤️ by Alber Ashraf
