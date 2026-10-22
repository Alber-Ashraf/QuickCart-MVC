# 2. Claim-Based BOLA / IDOR Defense on User-Owned Resources

- **Status:** Accepted
- **Date:** 2026-10-21
- **Deciders:** Alber Ashraf, Principal .NET Architect

## Context
Broken Object-Level Authorization (BOLA / IDOR) is one of the most critical vulnerabilities in e-commerce applications. Previously, endpoints such as `CartController.Plus`, `Minus`, `Remove`, and `OrderConfirmation` accepted resource IDs via route parameters without validating whether the entity belonged to the currently authenticated user. An attacker could manipulate integer IDs to inspect, increment, or delete cart items belonging to other customers.

## Decision
We implemented strict claim-based ownership validation across all controller actions that access or mutate user-owned data:
1. Extract authenticated user ID via `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
2. Retrieve targeted entity and verify `entity.ApplicationUserId == currentUserId`.
3. If IDs do not match and the caller does not hold `SD.Role_Admin`, immediately return `Forbid()` (HTTP 403 Forbidden).
4. Verified with comprehensive automated unit tests in `CartControllerTests.cs`.

## Consequences
- **Positive:** Completely prevents unauthorized resource tampering and horizontal privilege escalation.
- **Negative:** Requires an extra check in controller actions prior to executing mutations.
