# ADR 0001: Same-origin BFF with independently owned service databases

## Status

Accepted

## Decision

Angular talks only to `VinWeb` at `/api/bff`. The BFF owns browser cookie/session handling and transforms browser-shaped requests into internal API calls. Each backend service owns its own EF Core context, migrations, and SQL database; neither BFF nor another service can join a service database.

## Consequences

- Browser tokens and internal URLs do not need to become a frontend concern.
- A cross-service read is explicit over HTTP or an event projection, so students can see the cost of a boundary.
- There is deliberate duplication of order price snapshots, payment records, and inventory reservation facts. That is safer than cross-database joins for a distributed workflow.
