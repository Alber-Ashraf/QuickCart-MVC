# 1. Clean Architecture & Asynchronous Generic Repository Pattern

- **Status:** Accepted
- **Date:** 2026-10-21
- **Deciders:** Alber Ashraf, Principal .NET Architect

## Context
QuickCart previously utilized synchronous Entity Framework Core queries directly from some controllers, risking thread-pool starvation under high concurrent traffic. Additionally, data access queries and business transactions lacked a unified abstraction layer, resulting in duplicated query logic and fragmented transaction boundaries.

## Decision
We adopted Clean Architecture principles with an asynchronous Generic Repository Pattern and Unit of Work abstraction:
1. `IRepository<T>` defines non-blocking asynchronous data access operations (`GetAllAsync`, `GetAsync`, `AddAsync`, `Remove`).
2. `IUnitOfWork` coordinates entity repositories (`Product`, `Category`, `ShoppingCart`, `OrderHeader`, `OrderDetail`, `ApplicationUser`) and manages unified database transactions via `SaveAsync()`.
3. Presentation layer controllers depend strictly on `IUnitOfWork` rather than direct `DbContext` instances.

## Consequences
- **Positive:** Eliminates thread-pool starvation hazards; centralizes EF Core querying conventions; provides clean test isolation for unit and integration testing.
- **Negative:** Adds a thin layer of abstraction over EF Core (mitigated by keeping the repository focused on query composition rather than re-implementing EF features).
