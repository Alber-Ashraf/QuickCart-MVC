# QuickCart: Testing Strategy & QA

Last updated: 2026-10-21

## 1. Testing Philosophy & Architecture

QuickCart enforces an automated testing strategy combining unit tests for business logic, controller security tests for authorization boundaries, and integration tests using an in-memory SQL database provider.

- **Framework:** xUnit (`xunit` v2.5.3) & .NET Test SDK
- **Mocking & Isolation:** Moq (`Moq` v4.20.72)
- **Database Testing:** `Microsoft.EntityFrameworkCore.InMemory` (v8.0.22)
- **Execution Speed:** Full 25-test suite executes in < 600 ms.

---

## 2. Test Suite Breakdown

### Total: 25 Tests (100% Passing)

| Test Fixture | Count | Scope & Coverage |
|---|---|---|
| `PricingTests.cs` | 4 | Tiered quantity pricing calculations: base price (1-50 units), tier 1 discount (50-100 units), tier 2 discount (100+ units), zero/negative handling. |
| `CartControllerTests.cs` | 6 | BOLA/IDOR security tests: unauthorized user manipulation rejection (HTTP 403 Forbidden) on `Plus`, `Minus`, `Remove`, `OrderConfirmation`. Valid quantity mutations. |
| `OrderControllerTests.cs` | 4 | Order filtering by role (`Admin` vs. `Customer`), status transitions (`Approved` -> `Processing` -> `Shipped`), and tracking updates. |
| `CategoryControllerTests.cs` | 4 | Duplicate category name validation, display order sorting, create/update/delete validation. |
| `HomeControllerTests.cs` | 3 | Catalog search, category filter matching, product details view model binding. |
| `UserControllerTests.cs` | 2 | Administrative account locking, self-lockout defense validation (admin cannot lock self). |
| `RepositoryIntegrationTests.cs` | 1 | Entity Framework Core asynchronous repository lifecycle (Add, GetAsync, GetAllAsync, Delete). |
| `HealthCheckTests.cs` | 1 | Diagnostics health check component registration and readiness verification. |

---

## 3. Running the Test Suite

```bash
# Execute entire test suite
dotnet test QuickCart.sln

# Execute with detailed console logging
dotnet test QuickCart.sln --logger "console;verbosity=normal"

# Execute specific test fixture
dotnet test QuickCart.Tests/QuickCart.Tests.csproj --filter "FullyQualifiedName~CartControllerTests"
```

---

## 4. Known Gaps & Future QA Roadmap
- **Browser E2E:** Introduce Playwright test automation for end-to-end customer checkout flow in CI.
- **Stripe Webhooks:** Add integration tests with Stripe CLI for simulated asynchronous webhook event handling.
- **Load & Stress Testing:** Author k6 test scripts simulating 500 concurrent shoppers browsing catalog and checking out.
