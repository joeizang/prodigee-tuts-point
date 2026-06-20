# SQLite-Backed FastAPI Integration

## Scenario

The repository is ready. Now FastAPI should use SQLite through dependencies without route handlers knowing SQL exists.

## Requirements

- Provide SQLite-backed notes services through FastAPI dependencies.
- Initialize schema outside route business logic.
- Keep dependency overrides working.
- Preserve create, list, update, delete, conflict, and missing-record behavior.
- Use temporary SQLite files in tests.

## CLI/API contract

The HTTP contract should not change because storage changed. Clients still see notes, status codes, and errors.

## Milestone task

Wire `SqliteNoteRepository` into the FastAPI app and prove the API remains stable.

## Rubric

- Correctness: notes persist through SQLite-backed routes.
- Testing: real SQLite and override paths are covered.
- Maintainability: SQLite construction stays in the dependency layer.
- Design: route handlers call services, not repositories.
- Production readiness: the app can later read database paths from settings.

## Complexity

This is integration work. The hard part is keeping the new concrete implementation from spreading upward.

A good integration slice should feel almost boring from the API client's point of view. The storage file changes from JSON to SQLite, but the route paths, status codes, response bodies, and override strategy remain stable. That stability is the evidence that the abstraction is carrying its weight.

Treat schema initialization as part of the adapter boundary too. The handler should not wonder whether the database table exists. By the time the service is resolved, the repository should be ready to answer the use case.

## Stretch goals

- Add settings object injection.
- Add startup-time schema checks.
- Add repository contract tests reused by JSON and SQLite implementations.
