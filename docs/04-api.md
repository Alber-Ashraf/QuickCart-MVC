# QuickCart: API & Controller Specification

Last updated: 2026-10-21

## 1. Overview & Protocol Standards
QuickCart primarily exposes server-rendered Razor MVC endpoints alongside asynchronous JSON endpoints designed for jQuery DataTables, SweetAlert2 modals, and Toastr notification workflows.

- **Anti-CSRF Protection:** Global `AutoValidateAntiforgeryTokenAttribute` protects all state-changing endpoints (`POST`, `PUT`, `DELETE`). AJAX calls provide tokens via the `RequestVerificationToken` HTTP header.
- **Content Type:** JSON endpoints consume and produce `application/json; charset=utf-8`.
- **Response Structure for AJAX:**
  ```json
  {
    "success": true,
    "message": "Action completed successfully."
  }
  ```

---

## 2. Customer Area Endpoints (`/Customer/...`)

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/` or `/Customer/Home/Index` | `GET` | Anonymous | Browse product catalog with category search. |
| `/Customer/Home/Details?productId={id}` | `GET` | Anonymous | Product specifications, image, and volume pricing table. |
| `/Customer/Home/Details` | `POST` | `Authorize` | Add product and count to shopping cart. |
| `/Customer/Cart/Index` | `GET` | `Authorize` | View authenticated user's shopping cart. |
| `/Customer/Cart/Plus?cartId={id}` | `POST` | `Authorize` | Increment item count (enforces BOLA check). |
| `/Customer/Cart/Minus?cartId={id}` | `POST` | `Authorize` | Decrement item count or delete (enforces BOLA check). |
| `/Customer/Cart/Remove?cartId={id}` | `POST` | `Authorize` | Remove item from cart (enforces BOLA check). |
| `/Customer/Cart/Summary` | `GET` | `Authorize` | Checkout overview, shipping address form, total calculation. |
| `/Customer/Cart/Summary` | `POST` | `Authorize` | Initiates Stripe Checkout Session redirect. |
| `/Customer/Cart/OrderConfirmation?id={id}` | `GET` | `Authorize` | Verifies Stripe payment status and clears cart. |

---

## 3. Admin Area Endpoints (`/Admin/...`)

| Endpoint | Method | Required Role | Description |
|---|---|---|---|
| `/Admin/Product/GetAll` | `GET` | `Admin`, `Employee` | JSON list of products with categories for DataTables. |
| `/Admin/Product/Upsert?id={id}` | `GET/POST`| `Admin` | Create or update product with image upload. |
| `/Admin/Product/Delete/{id}` | `DELETE` | `Admin` | Delete product and clean up image file. |
| `/Admin/Category/GetAll` | `GET` | `Admin` | JSON list of categories for DataTables. |
| `/Admin/Category/Upsert?id={id}` | `GET/POST`| `Admin` | Create or update product categories. |
| `/Admin/Order/GetAll?status={status}` | `GET` | `Admin`, `Employee` | Filterable JSON order list for DataTables. |
| `/Admin/Order/Details?orderId={id}` | `GET` | `Admin`, `Employee` | Comprehensive order detail and shipping panel. |
| `/Admin/Order/StartProcessing` | `POST` | `Admin`, `Employee` | Transition order status to `Processing`. |
| `/Admin/Order/StartShipping` | `POST` | `Admin`, `Employee` | Transition status to `Shipped` with carrier and tracking. |
| `/Admin/Order/CancelOrder` | `POST` | `Admin`, `Employee` | Cancel order and trigger automated Stripe refund. |
| `/Admin/User/GetAll` | `GET` | `Admin` | Sanitized user list (excludes sensitive password hashes). |
| `/Admin/User/LockUnlock` | `POST` | `Admin` | Toggle account lockout (self-lockout defended). |

---

## 4. Identity & External OAuth Authentication (`/Identity/...`)

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/Identity/Account/Login` | `GET/POST` | Anonymous | Hardened login page with brute-force lockout defense and OAuth links. |
| `/Identity/Account/Register` | `GET/POST` | Anonymous | Self-registration strictly granting `Role_Customer`. |
| `/Identity/Account/ExternalLogin` | `POST` | Anonymous | Initiates OAuth challenge for specified provider (`Google`, `Facebook`). |
| `/Identity/Account/ExternalLogin?handler=Callback` | `GET` | Anonymous | OAuth provider callback; validates tokens, auto-links existing users by verified email, and signs in. |
| `/Identity/Account/ExternalLogin?handler=Confirmation` | `POST` | Anonymous | Completes profile registration if external account is not yet registered. |
| `/signin-google` | `GET/POST` | Anonymous | Internal ASP.NET Core Google OAuth token exchange handler. |

---

## 5. Diagnostics & Health Probe

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/health` | `GET` | Anonymous | Real-time database liveness probe returning JSON status. |
