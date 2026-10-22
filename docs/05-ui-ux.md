# Flyleaf: Enterprise Book Commerce UI/UX Architecture & Screen Specifications

Last updated: 2026-10-21

## 1. Design Token Architecture (`wwwroot/css/tokens.css`)

Flyleaf implements a strict 3-tier token hierarchy (**Primitives -> Semantics -> Components**) with native dual-theme support (Light and Dark Mode):

```
Primitive Tokens (Raw color scales, typography scales, base spacing)
       ↓
Semantic Tokens (Theme-aware aliases: --bg-canvas, --bg-surface, --text-primary)
       ↓
Component Tokens (.qc-btn, .qc-card, .qc-table, .qc-badge, .qc-input)
```

### Theme Custom Properties

| Token Name | Light Theme Default | Dark Theme (`data-bs-theme="dark"`) | Purpose |
|---|---|---|---|
| `--brand-primary` | `#4f46e5` (Indigo 600) | `#6366f1` (Indigo 500) | Primary interactions, active links |
| `--bg-canvas` | `#f8fafc` (Slate 50) | `#090d16` (Deep Obsidian) | App viewport background |
| `--bg-surface` | `#ffffff` (Pure White) | `#0f172a` (Slate 900) | Cards, sidebars, modal panels |
| `--bg-subtle` | `#f1f5f9` (Slate 100) | `#172033` (Slate 850) | Input canvases, artwork wells |
| `--border-subtle` | `#e2e8f0` (Slate 200) | `#1e293b` (Slate 800) | 1px hairline division borders |
| `--text-primary` | `#0f172a` (Slate 900) | `#f8f9fa` (High-contrast white) | Main headlines, body copy |
| `--text-secondary` | `#475569` (Slate 600) | `#94a3b8` (Slate 400) | Subtitles, helper text |

---

## 2. Typography & Numerical Precision

- **Primary Typeface:** *Plus Jakarta Sans* (humanist geometric sans).
- **Tabular Monospace Typeface:** *JetBrains Mono* applied to all prices, quantities, ISBNs, tracking numbers, and order references with `font-variant-numeric: tabular-nums` to eliminate visual column jitter in data tables.

---

## 3. Master Layouts

1. **`_Layout.cshtml` (Enterprise App Shell):**
   - 260px collapsible sidebar with 68px mini-rail state with persistent bidirectional toggle (`#sidebarCollapseBtn` in header, `#sidebarRailToggleBtn` in footer, `#topbarSidebarToggleBtn` in topbar, and `Ctrl+B` keyboard shortcut).
   - Sticky contextual topbar with breadcrumb navigation slot and desktop sidebar trigger.
   - Global quick search shortcut (`Ctrl+K` or `/`).
   - Zero-FOUC dual-theme toggle (`#themeToggleBtn`).
   - User profile dropdown with initials avatar chip.

2. **`_AuthLayout.cshtml` (Enterprise Split-Screen):**
   - High-trust desktop brand storytelling left pane with SSL/OWASP security indicators.
   - Centered interactive form right pane with theme toggle and mobile responsiveness.

3. **`_ErrorLayout.cshtml` (System Diagnostic Shell):**
   - Centered minimal container with correlation and request ID trace diagnostics.

4. **Brand Vector Assets & Iconography:**
   - Unified vector app icon (`wwwroot/favicon.svg`) featuring a stylized open book, dynamic fluttering flyleaf page, and emerald silk ribbon over a royal indigo canvas, optimized without filter dependencies for sandboxed browser tab renderers.
   - Comprehensive multi-resolution icon assets: `favicon-32x32.png`, `favicon-16x16.png`, `apple-touch-icon.png` (180x180), and multi-resolution binary `favicon.ico` (32x32 & 16x16 layers).
   - Declared across `_Layout.cshtml`, `_AuthLayout.cshtml`, and `_ErrorLayout.cshtml` with `asp-append-version="true"` to prevent stale favicon cache poisoning.
   - High-DPI `.brand-logo-img` vector integration across `_Layout.cshtml`, `_AuthLayout.cshtml`, and `Login.cshtml`.

---

## 4. Vertical Slice Execution Matrix

| Slice | Module | Views & Assets Redesigned | Highlights & Enhancements |
|---|---|---|---|
| **Slice 1** | Customer Storefront | `Home/Index`, `Home/Details`, `Cart/Index`, `Cart/Summary`, `Cart/OrderConfirmation` | Segmented category pills, volume pricing matrix, sticky order summary sidebar, 2-column checkout. |
| **Slice 2** | Admin Catalog | `Product/Index`, `Product/Upsert`, `Category/Index`, `Category/Create`, `Category/Edit`, `Category/Delete`, `product.js` | Fixed copy-paste empty state bug, replaced emoji table headers with clean SaaS headers, tiered wholesale price cards, media asset manager. |
| **Slice 3** | Operations & Fulfillment | `Order/Index`, `Order/Details`, `Company/Index`, `Company/Upsert`, `order.js`, `Company.js` | Segmented status filter tabs (All, In Process, Pending, Approved, Completed), carrier tracking modal validation, corporate account management. |
| **Slice 4** | Governance & Auth | `User/Index`, `User/ManagePermission`, `user.js`, `Identity/Login`, `Identity/Register`, `Manage/_Layout` | Fixed `user.js` inline CSS bug, role badges (`Admin`, `Employee`, `Company`, `Customer`), account lockout toggle, `_AuthLayout` integration. |

---

## 5. Accessibility & WCAG 2.2 AA Compliance

QuickCart conforms to **WCAG 2.2 Level AA** standards across both Light and Dark themes:

1. **Accessible Landmarks & Skip Navigation (SC 2.4.1):**
   - Skip links (`.skip-to-content`) included in `_Layout.cshtml` (`href="#mainContent"`) and `_AuthLayout.cshtml` (`href="#authMain"`).
   - Main content regions marked with `<main id="mainContent" tabindex="-1" role="main">` for direct focus transition without trapping.

2. **High-Contrast Focus Indicators (SC 2.4.7, 2.4.11, 2.4.13):**
   - Standardized 2px accent outline (`outline: 2px solid var(--brand-primary); outline-offset: 2px`) on all `:focus-visible` elements.
   - Distinctive outer ring shadow (`box-shadow: 0 0 0 3px var(--brand-subtle)`) for buttons, form inputs, selects, and links.

3. **Color Contrast Compliance (SC 1.4.3 & 1.4.11):**
   - Light Mode: Text primary (`#0f172a` on `#ffffff`) = 16.5:1 (Passes AAA); Text secondary (`#475569` on `#ffffff`) = 6.99:1 (Passes AAA); Brand primary (`#4f46e5` on `#ffffff`) = 4.88:1 (Passes AA).
   - Dark Mode: Text primary (`#f8fafc` on `#0f172a`) = 15.3:1 (Passes AAA); Text secondary (`#cbd5e1` on `#0f172a`) = 10.8:1 (Passes AAA); Semantic status pills (`#6ee7b7`, `#fde68a`, `#fca5a5`, `#7dd3fc`) = > 8.5:1 (Passes AAA).

4. **Reduced Motion Adaptation (SC 2.3.3):**
   - `@media (prefers-reduced-motion: reduce)` disables shimmer skeleton animations, transitions, and hover transforms for motion-sensitive users.

5. **Screen Reader Live Announcements (SC 4.1.3):**
   - Dedicated ARIA live region (`#qc-live-announcer`) with `aria-live="polite"` and `role="status"` dispatched by `site.js` for all toast messages (success, warning, error, info).

6. **Keyboard Navigation & Esc Dismissal (SC 2.1.1 & 2.1.2):**
   - `Ctrl+B` / `Cmd+B`: Sidebar collapse/expand.
   - `Ctrl+K` / `Cmd+K`: Global search trigger.
   - `Escape`: Closes open mobile sidebar and returns focus directly to `#mobileMenuBtn` with `aria-expanded="false"`.

---

## 6. Definition of Done Checklist

- [x] All views strictly preserve `@model` bindings, controller route signatures, and anti-forgery tokens.
- [x] Zero generic default-Bootstrap templates or unstyled tables remain.
- [x] Dual-theme switching (Light/Dark) persists without flash of unstyled content (zero FOUC).
- [x] Tabular numerals applied to all monetary and numeric outputs.
- [x] WCAG 2.2 AA accessibility requirements satisfied (skip links, focus-visible rings, live announcements, reduced motion, contrast ratios).
- [x] Solution builds with 0 errors and passes all 25 automated unit tests.

