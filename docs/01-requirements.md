# QuickCart: Requirements & Scope

Last updated: 2026-10-21

## 1. Problem Statement
Modern e-commerce applications require high availability, resilient payment orchestration, strict security posture against common web vulnerabilities (OWASP Top 10), and intuitive shopping experiences across desktop and mobile devices. QuickCart solves these challenges by providing a secure, modular ASP.NET Core 8 MVC platform with clean domain modeling, robust role-based authorization, and automated deployment capabilities.

## 2. Measurable Success Metrics
- **Performance:** P95 response time < 250ms for catalog browsing; P95 < 500ms for cart and checkout operations.
- **Reliability:** 99.9% uptime with automated zero-downtime database migrations and health check probes.
- **Security:** Zero high/critical vulnerabilities; 100% defense against BOLA/IDOR and CSRF; 5-attempt brute-force lockout.
- **Quality:** 100% passing automated test suite covering all critical pricing, authorization, and data mutation paths.

## 3. Personas & Roles

| Persona | Role | Primary Goals & Capabilities |
|---|---|---|
| **Online Shopper** | `Role_Customer` | Browse catalog, search/filter products, manage cart quantities, execute secure credit card checkout, view order history. |
| **Company Buyer** | `Role_Company` | Place bulk purchase orders with delayed net-30 invoicing terms. |
| **Store Operator** | `Role_Employee` | Review inbound orders, update processing status, assign tracking numbers, and initiate shipping. |
| **System Admin** | `Role_Admin` | Full administrative authority: product catalog CRUD, category management, company onboarding, user role assignments, refunds. |

## 4. User Stories & Acceptance Criteria

### US-1: Catalog Discovery & Tiered Pricing
- **As a** customer,
- **I want to** view products with real-time volume discounts (1-50 units, 50-100 units, 100+ units),
- **So that** I receive accurate wholesale discounts as order volume increases.
- **Acceptance Criteria:**
  - Price per unit automatically calculates based on item count.
  - Price calculation is strictly validated server-side during checkout.

### US-2: Secure Cart Management (BOLA Defense)
- **As a** logged-in customer,
- **I want to** update item counts and remove items from my shopping cart,
- **So that** other users cannot inspect, alter, or hijack my cart items.
- **Acceptance Criteria:**
  - Route actions (`Plus`, `Minus`, `Remove`) verify the authenticated user ID matches `ShoppingCart.ApplicationUserId`.
  - Unauthorized requests return HTTP 403 Forbidden.

### US-3: Stripe Hosted Checkout & Order Fulfillment
- **As a** customer,
- **I want to** pay for my order securely using Stripe Checkout,
- **So that** my sensitive credit card details are never transmitted through or stored on QuickCart servers.
- **Acceptance Criteria:**
  - Server initiates a Stripe Checkout Session with verified server-side line item totals.
  - On payment confirmation, order status updates to `Approved` and payment intent is recorded.

---

## 5. MoSCoW Prioritization & Scope

### Must Have (MVP)
- Clean ASP.NET Core 8 MVC architecture with Area separation (Customer, Admin, Identity).
- Tiered quantity pricing engine and shopping cart persistence.
- Stripe payment gateway integration with payment confirmation webhooks/callbacks.
- Role-based authorization (`Customer`, `Company`, `Employee`, `Admin`).
- Global anti-forgery protection and BOLA ownership defenses.
- Automated unit and integration test suite.

### Should Have
- Multi-stage Docker containerization and Docker Compose orchestration.
- Idempotent database migration bundle (`bundle.sql`).
- Distributed tracing with `X-Correlation-ID` and structured logging scopes.
- Operational `/health` diagnostic probe.

### Could Have (v2 Roadmap)
- Real-time order status notifications via SignalR.
- Full-text search and filtering via Elasticsearch / Azure Cognitive Search.
- Redis distributed cache for catalog caching.

### Won't Have (Out of Scope)
- Direct on-server credit card tokenization (delegated entirely to Stripe hosted pages).
- Multi-tenant multi-vendor marketplace functionality.
