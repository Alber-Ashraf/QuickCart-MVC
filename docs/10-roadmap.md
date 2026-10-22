# QuickCart: Roadmap & Milestone Tracking

Last updated: 2026-10-21

## 1. Project Status Overview

| Phase | Milestone Name | Status | Deliverables |
|---|---|---|---|
| **Phase 1** | Auto-Discovery & Audit | **Completed** | Full-stack audit, requirements analysis, risk matrix, MVP scoping. |
| **Phase 2** | Architecture & UI/UX Plan | **Completed** | Clean architecture plan, ERD, design tokens, security strategy. |
| **Phase 3** | Foundation & Security Setup | **Completed** | Package harmonization (.NET 8.0.22), anti-CSRF, security headers, Identity lockout, layout tokens, Docker/CI setup. |
| **Phase 4** | Vertical-Slice Implementation | **Completed** | Async data access, BOLA defense, product/order/user refactoring, bug fixes. |
| **Phase 5** | Comprehensive Testing | **Completed** | 25 automated xUnit tests (pricing, BOLA, order lifecycle, repository). |
| **Phase 6** | Deployment Engineering | **Completed** | Multi-stage Docker, Compose orchestration, idempotent SQL bundle (`bundle.sql`), release runbook. |
| **Phase 7** | Monitoring & Observability | **Completed** | Correlation ID middleware, `/health` endpoint, structured logging, maintenance guide. |

---

## 2. Completed Epics & Features (v2.0)

- [x] **EPIC-1: Security Hardening & OWASP Compliance**
  - [x] Global Anti-CSRF protection across all forms and AJAX calls.
  - [x] Broken Object-Level Authorization (BOLA/IDOR) defense on ShoppingCart and Order endpoints.
  - [x] Identity brute-force account lockout (5 attempts, 5 minutes).
  - [x] Elimination of public privilege escalation during self-registration.
  - [x] OWASP security headers (CSP, X-Frame-Options: DENY, X-Content-Type: nosniff).
  - [x] Purge of plaintext secrets from configuration files.

- [x] **EPIC-2: Asynchronous Performance & High Availability**
  - [x] Asynchronous Generic Repository (`GetAllAsync`, `GetAsync`, `SaveAsync`).
  - [x] Elimination of thread-pool starvation hazards in Customer & Admin controllers.
  - [x] Entity Framework Core 8 package harmonization (`8.0.22`).

- [x] **EPIC-3: Modernized UI/UX & Design Tokens**
  - [x] CSS Design Tokens (`tokens.css`) on `:root`.
  - [x] Mandatory 4 UI states (Loading button spinners, Empty state partial, Error Toastr/SweetAlert2, Populated DataTables).
  - [x] Standardized order and payment status badges (`_StatusBadge.cshtml`).

- [x] **EPIC-4: Cloud-Native DevOps & CI/CD**
  - [x] Multi-stage non-root Dockerfile and Docker Compose orchestration.
  - [x] GitHub Actions automated build, test, and container CI pipeline.
  - [x] Idempotent SQL migration bundle (`bundle.sql`) for zero-downtime releases.

- [x] **EPIC-5: Observability & Operational Diagnostics**
  - [x] `X-Correlation-ID` distributed tracing middleware and structured logging scopes.
  - [x] Database health check probe (`/health`) with JSON diagnostics.
  - [x] Release, operations, and maintenance runbooks.

---

## 3. Completed Milestone: Enterprise SaaS UI/UX Modernization (v2.1)

- [x] **Phase 1: Full UI/UX Audit & Information Architecture:** Completed comprehensive screen inventory, identified 4 defect discoveries (copy-paste text, inline CSS in JS class string, emoji table headers, and order search placeholder).
- [x] **Phase 2: Enterprise Design System & Token Architecture:** Established 3-tier token hierarchy (`Primitives` -> `Semantics` -> `Components`) in `wwwroot/css/tokens.css` with native Dual-Theme support (Light/Dark mode) and *Plus Jakarta Sans* / *JetBrains Mono* tabular numerals.
- [x] **Phase 3: Design System Implementation & Master Shells:** Built responsive collapsible sidebar (260px to 68px mini-rail with `Ctrl+B`), sticky contextual topbar with breadcrumb slot, global quick search trigger (`Ctrl+K`), and dedicated split-screen auth shell (`_AuthLayout.cshtml`).
- [x] **Phase 4: Vertical Slice Redesign:** Completed end-to-end redesign of all customer storefront, catalog administration, order fulfillment, and governance screens while strictly preserving 100% of C# route contracts, `@model` bindings, and Anti-CSRF protection.
- [x] **Phase 5: UX Polish & WCAG 2.2 AA Accessibility:** Implemented accessible skip navigation links, 2px focus-visible rings with subtle glow, motion sensitivity support (`prefers-reduced-motion: reduce`), live screen reader announcements (`#qc-live-announcer`), and Escape keyboard drawer dismissal.
- [x] **Phase 6: Rebrand to Flyleaf & Cloud Storage Migration:**
  - Transitioned identity to **Flyleaf** with custom vector book icon (`wwwroot/favicon.svg`) and book-centric iconography.
  - Replaced Azure Blob Storage with **Backblaze B2 S3-Compatible Cloud Storage** (`AWSSDK.S3`), providing dual interface compatibility (`IBlobService` + `IStorageService`) and resilient zero-config development fallbacks.
  - [x] **Phase 7: Google OAuth Integration from Flexfolio:**
    - Ported `OAuthSettings`, `IOAuthService`, and `OAuthService` from `Flexfolio` (`ExchangeCodeForUserInfoAsync`, Google OpenID authorization URL & token exchange).
    - Wired ASP.NET Core `AddGoogle` handler with resilient zero-crash fallback and customized SVG/branded buttons.
    - Fixed external login button submission dropping `provider="Google"` in `site.js` anti-double-submission handler.
    - Configured reverse proxy `ForwardedHeadersOptions` and DataProtection key persistence for cloud hosting.
    - Added auto-account linking in `ExternalLogin.cshtml.cs` for verified email identities.
    - Expanded automated test suite with 25 unit tests in `OAuthTests.cs` (50/50 tests passing).
  - [x] **Phase 8: Authentic Literary Catalog Data Seeding:**
    - Replaced all legacy demo placeholders ("Fortune of Time", "Billy Spark", lorem ipsum text) with 12 authentic, world-renowned literary titles spanning 5 curated categories (*Classics*, *Sci-Fi & Fantasy*, *History & Thought*, *Technology*, *Self-Development*).
    - Downloaded and bundled high-resolution book cover assets locally under `wwwroot/images/books/` (F. Scott Fitzgerald, George Orwell, Harper Lee, J.R.R. Tolkien, James Clear, Robert C. Martin, Frank Herbert, Jane Austen, Yuval Noah Harari, Paulo Coelho, Fyodor Dostoevsky, J.D. Salinger).
    - Enhanced `DbInitializer.cs` with safe idempotent database initialization, relationship creation (`ProductImages`), and automated cleanup of detached placeholder records.
    - Added unit and idempotency test coverage in `DbInitializerTests.cs`, bringing automated test suite to 53 tests passing (53/53).

---

## 4. Future Roadmap (v2.2+)

- [ ] **Milestone 2.2: Real-Time Notifications**
  - [ ] Implement ASP.NET Core SignalR hub for live order status updates to customers.
  - [ ] Real-time order notification badges for store operators.
- [ ] **Milestone 2.3: Advanced Search & Caching**
  - [ ] Integrate Redis distributed cache for high-throughput product catalog reads.
  - [ ] Implement fuzzy search and multi-facet filtering (author, price range, ratings).
- [ ] **Milestone 2.4: Automated E2E Testing**
  - [ ] Playwright test suite simulating end-to-end user checkout and Stripe payments in CI.
