# PostgreSQL Engine Configuration

## Scenario

The notes API can now name PostgreSQL readiness risks. This milestone turns those names into engine, session, and production-guard decisions for the SQLAlchemy integration.

## Requirements

- Parse and validate SQLite and PostgreSQL database URLs.
- Select provider-specific SQLAlchemy engine options.
- Keep session creation inside dependency/repository wiring.
- Reject SQLite in production environments.
- Preserve the sync/async boundary decision explicitly.

## CLI/API contract

The API contract remains stable across database providers. Changing from SQLite to PostgreSQL should change settings, engine construction, and integration tests, not route semantics or response shapes.

## Milestone task

Produce provider-aware SQLAlchemy engine settings for the notes app and define how FastAPI dependencies yield sessions safely.

## Rubric

- Correctness: PostgreSQL and SQLite URLs produce different engine options.
- Testing: invalid providers, missing databases, and production SQLite guards are covered.
- Maintainability: database settings are centralized.
- Design: session factories are injected instead of created inside routes.
- Production readiness: PostgreSQL pool and connection health options are named.

## Complexity

Database configuration looks small, but it controls runtime behavior under load. PostgreSQL needs pool settings, connection health checks, credentials, and production safety guards. SQLite needs development-friendly connection arguments but should not be mistaken for the production target. The right abstraction keeps these choices explicit and testable.

The sync/async choice also matters. Synchronous SQLAlchemy can be a reasonable first production step when database work is simple and isolated. The project should still document the decision so later async SQLAlchemy work is a deliberate migration rather than an accidental mix of blocking calls and async route handlers.

## Stretch goals

- Add a PostgreSQL integration-test profile.
- Add async SQLAlchemy comparison exercises later in the track.
- Add deployment documentation for migration-before-startup ordering.
