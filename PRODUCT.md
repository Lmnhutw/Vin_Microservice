# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- Primary: learners and junior engineers studying how an e-commerce system is split, operated, and evolved as microservices.
- Secondary: reviewers who need to understand the architecture and verify the main commerce flows from a polished browser experience.

These audiences are inferred from the repository and the explicit brief that this is a learning project.

## Product Purpose

Vin Shop is a working e-commerce learning environment. It lets a user discover products, authenticate, manage a cart, apply promotions, and follow a purchase workflow while exposing the service boundaries and distributed-system tradeoffs behind those actions.

Success means the storefront feels complete enough to use, while the codebase and runtime make each microservice responsibility, data owner, synchronous dependency, event, failure state, and recovery strategy easy to inspect.

## Positioning

Vin Shop is not positioned as a production retailer. Its distinctive mechanism is that every important customer action doubles as an observable microservice lesson: the UI explains the current business state, and the backend makes the service-to-service path traceable.

## Operating Context

- The complete stack runs locally with Docker Compose.
- The Angular client is served through the ASP.NET Core BFF.
- Internal APIs use ASP.NET Core and own SQL Server databases and EF Core migrations.
- Synchronous HTTP is used when the caller needs an immediate answer.
- Asynchronous messaging is used for side effects and distributed workflows.
- Swagger, health endpoints, logs, traces, tests, and architecture documentation are part of the learning experience.

## Capabilities and Constraints

- Preserve the existing Vin Shop name and the implemented Auth, Product, Coupon, Cart, Email, BFF, SQL Server, and Azure Service Bus concepts.
- Angular is the canonical UI. Razor views are legacy fallback surfaces until Angular reaches feature parity.
- Current target frameworks remain .NET 8 and Angular 19 unless a separate upgrade is approved.
- Product, coupon, cart, auth, and notification data remain owned by their respective services.
- New Order, Payment, and Inventory capabilities must demonstrate event-driven consistency without sharing service databases.
- Local development must remain practical on one machine.
- No real customers, commercial claims, payment processing, inventory guarantees, or delivery promises may be fabricated.

## Brand Commitments

- Product name: Vin Shop.
- Voice: clear, composed, direct, and educational without sounding like documentation inside customer-facing controls.
- The interface should feel intentional and contemporary rather than like a scaffolded Bootstrap demo.

## Evidence on Hand

- Product and category data from ProductAPI.
- Coupon data from CouponAPI.
- Auth and role behavior from AuthAPI.
- Cart behavior and totals from ShoppingCartAPI.
- Notification logs and the existing email-cart message flow from EmailAPI.
- Existing product image URLs plus a local product placeholder.
- No verified testimonials, customer logos, benchmarks, shipping terms, stock guarantees, or real payment provider assets exist.

## Product Principles

1. A customer flow should work before it becomes an architecture demo.
2. Every service boundary must have a visible reason: ownership, independent failure, asynchronous work, or an explicit consistency tradeoff.
3. Failures are product states, not hidden exceptions.
4. Local setup, traces, contracts, and tests are part of the deliverable.
5. Prefer incremental, inspectable changes over a big-bang rewrite.

## Accessibility & Inclusion

Target WCAG 2.2 AA for the web UI: keyboard-operable controls, visible focus, semantic landmarks, sufficient contrast, reduced-motion support, accessible form errors, and layouts that remain usable from small mobile screens through desktop.
