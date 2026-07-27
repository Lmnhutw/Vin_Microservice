# ADR 0002: Order owns the saga; all integration events use outbox and inbox

## Status

Accepted

## Decision

Order starts checkout and owns the public saga state. Services publish versioned envelopes to the `vin-events` topic through a transactional outbox and process them with an idempotent inbox. Inventory rejection transitions Order to `CompensatingPayment`; Order requests a refund and waits for Payment's refund event before reaching `Rejected`.

## Consequences

- The design is at-least-once and observable, not falsely exactly-once.
- A broker outage cannot lose a committed order: its outbox remains pending until delivery succeeds or is locally marked dead-lettered for inspection.
- Compensation is explicit in the state model. It is not a hidden synchronous rollback across service databases.
