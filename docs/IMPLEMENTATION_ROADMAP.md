# Vin Shop implementation roadmap

Purpose: full-stack review, redesign, refactor, and feature implementation

Assumptions:

- Vin Shop is a local learning project, not a production retailer.
- Angular 19 is the canonical UI and ASP.NET Core 8 remains the backend target.
- Existing Auth, Product, Coupon, Cart, Email, BFF, SQL Server, and Azure Service Bus behavior must remain available while the system evolves.
- Mobile, tablet, and desktop are first-class targets.
- Changes land incrementally behind build, test, contract, and GitNexus impact gates.

Final Prompt:

You are working in the existing `Vin_Microservice` repository. Complete the full Vin Shop roadmap as an incremental, repository-aware implementation.

## Objective

- Turn the Angular client into a polished, accessible e-commerce storefront.
- Make the backend an explicit microservice learning system whose ownership boundaries, synchronous dependencies, events, consistency model, failures, recovery, and observability are easy to inspect.

## Scope

- `VinWeb/ClientApp`: application shell, catalogue, product detail, cart, auth, checkout, admin, shared UI, routing, state, API integration, responsive behavior, accessibility, and tests.
- `VinWeb`: same-origin BFF, browser auth/session, UI-shaped endpoints, error normalization, resilience, and correlation.
- Existing APIs: Auth, Product, Coupon, ShoppingCart, Email, and MessageBus.
- New capabilities: Order, Payment simulator, Inventory, shared versioned integration contracts, outbox/inbox, saga state, observability, health, local orchestration, tests, CI, and architecture documentation.

## Constraints

- Run GitNexus upstream impact analysis before editing existing symbols. Stop and report HIGH or CRITICAL risk before editing.
- Preserve service data ownership. A service must never read or write another service database.
- Keep controllers thin and put workflow behavior in cohesive application services or feature slices.
- Keep public contracts backward compatible during migration windows.
- Do not perform a big-bang rewrite or introduce abstractions without a concrete teaching or operational purpose.
- Do not fabricate stock, shipping, customer, benchmark, or real-payment claims.

## Frontend requirements

- Follow `PRODUCT.md` and `DESIGN.md`.
- Implement loading, empty, partial, success, error, disabled, auth-expired, and retry states.
- Complete catalogue search/filter/sort, product details, quantity selection, cart quantity, coupon, notification request, auth recovery, return URL, role-aware admin, and checkout/order tracking.
- Keep API transport, auth state, notifications, and feature state outside presentation markup.
- Meet WCAG 2.2 AA expectations, keyboard operation, visible focus, 44px touch targets, reduced motion, and mobile/tablet/desktop layouts.
- Add Angular unit/component tests and Playwright coverage for critical browser flows.

## Backend requirements

- Normalize API errors with RFC 9457-compatible ProblemDetails while supporting existing clients during transition.
- Configure AuthAPI JWT authentication and role/policy authorization correctly.
- Use typed `HttpClient` registrations with explicit timeout, retry rules, circuit breaking, correlation propagation, and cancellation.
- Separate liveness and readiness health endpoints and include critical dependencies in readiness.
- Add structured logs, metrics, and OpenTelemetry traces across BFF, APIs, SQL, HTTP, and messaging.
- Define versioned integration events. Add transactional outbox, idempotent inbox/consumer processing, retry, and dead-letter behavior.
- Implement Order as the saga owner, Payment as a deterministic simulator, and Inventory as the stock owner. Model pending, confirmed, rejected, and compensation states explicitly.
- Use SQL Server constraints and indexes that match the actual access patterns. Use additive, backward-compatible EF Core migrations.

## Local platform and documentation requirements

- Keep one-command Docker Compose startup.
- Add the broker and observability stack needed to inspect the end-to-end flow locally.
- Document service ownership, API and event contracts, consistency guarantees, failure injection, retry/DLQ inspection, migrations, and rollback.
- Add architecture decision records for BFF, service boundaries, messaging, saga ownership, and outbox/inbox.
- Add CI for restore, build, unit tests, integration tests, Angular tests, contract checks, and container validation.

## Output requirements

- Implement code and migrations, not only recommendations.
- Update the roadmap as slices land.
- Briefly report the changed behavior, architectural reason, blast radius, verification evidence, and remaining risks after each slice.

## Verification criteria

- Angular production build passes and critical UI routes are visually checked at desktop and mobile sizes.
- The .NET solution builds and all unit/integration tests pass.
- Each service exposes useful OpenAPI and live/ready health endpoints.
- Login, authorization, catalogue, cart ownership, coupon, order success, payment rejection, inventory rejection, duplicate event, retry, and DLQ scenarios are covered.
- A single order can be followed by correlation/trace ID through the BFF, services, database work, events, and final state.
- GitNexus `detect_changes` reports only expected symbols and flows before commit.
- The requirement-by-requirement completion audit has direct evidence for every roadmap item.
