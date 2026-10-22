# Changelog

All notable changes to the QuickCart project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Last updated: 2026-10-21

---

## [2.1.0] - 2026-10-21

### UI/UX & Design Systems Modernization
- **Enterprise Design Token System:** Implemented a 3-tier token architecture (`Primitives` -> `Semantics` -> `Components`) in `wwwroot/css/tokens.css` with native Dark and Light Mode support (`[data-bs-theme="dark"]` with deep obsidian canvas `#090d16` and card surface `#0f172a`).
- **Typography & Tabular Numerals:** Standardized typography with *Plus Jakarta Sans* and *JetBrains Mono*, enforcing `tabular-nums` across all currency values, order IDs, quantities, and dates to eliminate visual jitter.
- **Enterprise Master App Shell (`_Layout.cshtml`):** Built a collapsible sidebar navigation (260px expanded to 68px mini-rail with persistent bidirectional toggles in header, rail footer, topbar, and `Ctrl+B`), sticky contextual topbar, breadcrumb navigation, zero-FOUC theme toggle, global search trigger (`Ctrl+K`), and user avatar initials dropdown. Fixed defect where shrinking the sidebar permanently hid the toggle button without an expand control.
- **Dedicated Split-Screen Auth Shell (`_AuthLayout.cshtml`):** Built high-trust split-screen authentication layout featuring enterprise storytelling, SSL indicators, and responsive mobile form wells.
- **Customer Storefront Slice:** Redesigned `Home/Index`, `Home/Details`, `Cart/Index`, `Cart/Summary`, and `Cart/OrderConfirmation` with segmented category pills, tiered volume pricing matrix, 2-column specifications, and sticky summary sidebars.
- **Admin Catalog & Taxonomy Slice:** Redesigned `Product/Index`, `Product/Upsert`, `Category/Index`, `Category/Create`, `Category/Edit`, and `Category/Delete`. Fixed copy-paste bug in empty state and replaced emoji headers with clean enterprise headers.
- **Operations & Fulfillment Slice:** Redesigned `Order/Index`, `Order/Details`, `Company/Index`, and `Company/Upsert` with segmented status filter pills (`all`, `inprocess`, `pending`, `approved`, `completed`), carrier tracking validation, and corporate account management.
- **Governance & Identity Slice:** Redesigned `User/Index`, `User/ManagePermission`, `Identity/Login`, and `Identity/Register`. Fixed inline CSS bug in `user.js` and added semantic role badges (`Admin`, `Employee`, `Company`, `Customer`).
- **Rebrand to Flyleaf & Modern SVG Brand Icon:**
  - Transitioned the application identity from "QuickCart" to **"Flyleaf"** across storefront, admin navigation, auth layout, and browser titles.
  - Designed custom scalable vector icon `wwwroot/favicon.svg` featuring open pages, dynamic fluttering flyleaf accent, and emerald ribbon over a royal indigo canvas, optimized without filter dependencies for sandboxed browser tab renderers.
  - Generated matching multi-resolution raster icons: `favicon-32x32.png`, `favicon-16x16.png`, `apple-touch-icon.png` (180x180), and binary `favicon.ico` (32x32 & 16x16) replacing browser default sheet icons.
  - Linked all favicon variants across `_Layout.cshtml`, `_AuthLayout.cshtml`, and `_ErrorLayout.cshtml` with `asp-append-version="true"` to prevent stale favicon cache poisoning.
  - Integrated high-DPI vector logo directly into `_Layout.cshtml`, `_AuthLayout.cshtml`, and `Login.cshtml`.
- **Backblaze B2 S3-Compatible Cloud Storage Migration:**
  - Migrated cloud storage from `Azure.Storage.Blobs` to Backblaze B2 S3-compatible object storage utilizing `AWSSDK.S3` (`AmazonS3Client`, `IAmazonS3`).
  - Added `BackblazeSettings` for strongly-typed configuration (`BucketName`, `Region`, `AccessKey`, `SecretKey`, `Endpoint`).
  - Created `IStorageService` extending `IBlobService` with presigned URLs and stream downloads.
  - Implemented `BackblazeStorageService` adhering to the dual interface pattern (`IBlobService` and `IStorageService`) for non-breaking compatibility with `ProductController`.
  - Configured resilient startup fallbacks in `Program.cs` preventing crashes during offline development or testing without credentials.
  - Updated environment variables, `.env.example`, `docker-compose.yml`, `appsettings.json`, and `appsettings.Production.json`.
- **Google OAuth Integration from Flexfolio:**
  - Ported Google OAuth architecture from `Flexfolio` (`OAuthSettings`, `IOAuthService`, `OAuthService`).
  - Implemented direct authorization URL generation and token exchange (`ExchangeCodeForUserInfoAsync`) for Google OpenID.
  - Configured ASP.NET Core `AddGoogle` handler with resilient zero-crash initialization when credentials are unset in local/test environments.
  - Enhanced `ExternalLogin.cshtml.cs` with verified email auto-linking to existing accounts, eliminating duplicate email conflicts.
  - Added styled SVG vector brand button for Google on `Login.cshtml` and `Register.cshtml`.
  - Added 8 unit tests in `OAuthTests.cs` (33/33 tests passing).
- **OAuth Hardening, CSP Resolution & Reverse Proxy Forwarding:**
  - **Google OAuth Button Submission Fix:** Resolved defect where clicking "Continue with Google" appeared unresponsive. Diagnosed root cause in `site.js` where synchronous `submitBtn.prop('disabled', true)` stripped `name="provider"` and `value="Google"` from the form body before browser serialization. Updated `site.js` to defer button disabling via `setTimeout(..., 0)` while immediately preventing duplicate clicks with `pointer-events: none`, added `no-spinner` and explicit `asp-page`/`asp-route-provider` attributes on `Login.cshtml` and `Register.cshtml`, and added defensive provider resolution in `ExternalLogin.cshtml.cs`.
  - **Reverse Proxy Forwarded Headers:** Registered `ForwardedHeadersOptions` with `XForwardedFor`, `XForwardedProto`, and `XForwardedHost` at the head of the HTTP pipeline in `Program.cs`, allowing cloud/IIS reverse proxies (e.g. `flyleaf.runasp.net`) to correctly forward HTTPS schemes and prevent OAuth correlation cookie drops.
  - **DataProtection Key Persistence:** Configured `AddDataProtection().PersistKeysToFileSystem()` to persist encryption keys in `App_Data/DataProtection-Keys`, ensuring session tokens and OAuth correlation cookies survive application pool recycles and restarts.
  - **Self-Referential Redirect Loop Defense:** Implemented `SD.CleanReturnUrl` across `Login.cshtml.cs`, `ExternalLogin.cshtml.cs`, and `Register.cshtml.cs`, neutralizing recursive `ReturnUrl` buildup on external authentication callbacks and rejecting open redirects.
  - **Content Security Policy Enhancement:** Added `https://cdnjs.cloudflare.com`, `https://cdn.jsdelivr.net`, and `https://cdn.datatables.net` to `connect-src` in `Program.cs`, resolving CSP violations when DevTools loads script source maps (`toastr.js.map`).
  - **Test Suite Expansion:** Added 16 unit tests in `OAuthTests.cs` verifying `SD.CleanReturnUrl` against malicious redirects, self-referential loops, and valid local paths (49/49 tests passing).
- **Authentic Literary Catalog Data Seeding:**
  - Replaced all placeholder dummy data ("Fortune of Time", "Billy Spark", lorem ipsum text) with 12 authentic, world-renowned literary titles spanning 5 curated categories (*Classics*, *Sci-Fi & Fantasy*, *History & Thought*, *Technology*, *Self-Development*).
  - Seeded books with real metadata: verified ISBN-13 identifiers, canonical authors (F. Scott Fitzgerald, George Orwell, Harper Lee, J.R.R. Tolkien, James Clear, Robert C. Martin, Frank Herbert, Jane Austen, Yuval Noah Harari, Paulo Coelho, Fyodor Dostoevsky, J.D. Salinger), rich editorial descriptions, and realistic tiered volume pricing ($1–$100).
  - Downloaded high-resolution book covers and bundled them as permanent local web assets in `wwwroot/images/books/` to eliminate external image dependencies and CSP friction.
  - Enhanced `DbInitializer.cs` with idempotent category and product seeding, relationship creation with `ProductImages`, and safe cleanup of unreferenced placeholder records.
  - Added unit and idempotency test coverage in `DbInitializerTests.cs`, bringing automated test suite to 53 tests passing (53/53).
- **WCAG 2.2 AA Accessibility & Polish:**
  - Added accessible skip navigation links in `_Layout.cshtml` and `_AuthLayout.cshtml` anchoring to `<main id="mainContent" tabindex="-1">` and `<div id="authMain" tabindex="-1">`.
  - Implemented 2px accent focus-visible rings (`:focus-visible`) with 3px subtle outer glow across all interactive elements.
  - Implemented screen reader live announcements (`#qc-live-announcer` with `aria-live="polite"`, `role="status"`) dispatched from `site.js` for Toastr alerts.
  - Added motion sensitivity support (`@media (prefers-reduced-motion: reduce)`) disabling shimmer animations and non-essential transitions.
  - Added `Escape` key dismissal for the mobile sidebar drawer with automatic focus return to `#mobileMenuBtn` and ARIA synchronization.


---

## [2.0.0] - 2026-10-21

### Security
- **BOLA / IDOR Defense:** Enforced claim-based resource ownership validation on `CartController` (`Plus`, `Minus`, `Remove`, `OrderConfirmation`) and `OrderController` (`Details`), preventing unauthorized resource tampering.
- **Privilege Escalation Mitigation:** Removed public self-assignment of `Admin` and `Employee` roles from `Register.cshtml.cs`. All new public registrations are strictly assigned `Role_Customer`.
- **Brute-Force & Credential Stuffing Defense:** Configured ASP.NET Core Identity account lockout (`MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 5 minutes`, `lockoutOnFailure: true`).
- **Global Anti-CSRF:** Enabled `AutoValidateAntiforgeryTokenAttribute` globally in MVC filters; added automatic anti-forgery header binding in jQuery AJAX prefilters (`site.js`).
- **OWASP Security Headers:** Configured middleware injecting `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, and strict `Content-Security-Policy`.
- **Cookie Hardening:** Configured `HttpOnly = true`, `SameSite = Lax`, and `SecurePolicy = Always` across application cookies and sessions.
- **Sensitive Data Scrubbing:** Sanitized user administration JSON responses in `UserController`, removing `PasswordHash`, `SecurityStamp`, and internal tokens. Added self-lockout prevention for active administrators.
- **Secret Hardening:** Purged plaintext database credentials, Stripe keys, and Azure storage secrets from `appsettings.json` and `appsettings.Production.json`. Created `.env.example` template.

### Added
- **Automated Test Suite:** Created `QuickCart.Tests` with 25 unit and integration tests using xUnit and EF Core In-Memory, covering BOLA authorization, tiered quantity discounts, catalog search, category validation, and repository operations.
- **Design System & CSS Tokens:** Introduced `wwwroot/css/tokens.css` with semantic CSS custom properties (`--qc-primary`, `--qc-surface`, `--qc-border-radius`, etc.).
- **Mandatory 4 UI States:** Added shared partials `_EmptyState.cshtml` and `_StatusBadge.cshtml`, along with button loading spinner handlers (`qc-btn-loading`) in `site.js`.
- **Zero-Downtime Deployment Assets:** Added multi-stage `Dockerfile`, `docker-compose.yml`, `.dockerignore`, `.editorconfig`, and automated GitHub Actions CI workflow (`.github/workflows/ci.yml`).
- **Idempotent Migration Script:** Generated production-ready idempotent migration script `docs/migrations/bundle.sql` via `dotnet ef migrations script --idempotent`.
- **Observability & Diagnostics:** Added distributed tracing middleware extracting/generating `X-Correlation-ID`, structured logging scopes, and JSON-based `/health` diagnostic endpoint.

### Changed
- **Asynchronous Architecture:** Refactored `IRepository` and `Repository` to support full asynchronous data querying (`GetAllAsync`, `GetAsync`, `SaveAsync`), eliminating thread-pool starvation across all Customer and Admin controllers.
- **Package Harmonization:** Harmonized all Microsoft.EntityFrameworkCore and Identity packages to `8.0.22` across all solution projects, resolving `NU1605` package downgrade warnings.
- **Bug Fixes:**
  - Fixed inverted carrier assignment logic bug in `OrderController.UpdateOrderDetail`.
  - Fixed `NullReferenceException` on model validation error in `ProductController.Upsert` by repopulating `CategoryList`.
  - Corrected client-server JSON key typos (`sucess` -> `success`, `massege` -> `message`) across controllers and DataTables JavaScript files.

---

## [1.0.0] - 2024-05-10

### Added
- Initial release of QuickCart MVC e-commerce platform.
- Customer catalog, shopping cart, and Stripe hosted checkout.
- Admin dashboard for Categories, Products, Companies, and Orders.
- Azure Blob Storage integration for product imagery.
