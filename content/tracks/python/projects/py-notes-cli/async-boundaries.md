# Async Boundaries

## Scenario

FastAPI supports async route handlers, but the app currently uses synchronous SQLite. This milestone makes the async boundary deliberate.

## Requirements

- Document which layers are sync and async.
- Use async handlers only where the boundary is explicit.
- Isolate blocking SQLite-backed service calls with `run_in_threadpool`.
- Keep service and repository methods synchronous while using stdlib `sqlite3`.
- Test async-shaped routes through `TestClient`.

## CLI/API contract

The HTTP behavior stays the same. The improvement is operational discipline: the event loop is not blocked directly by sync SQLite work.

## Milestone task

Implement async FastAPI routes that call sync service methods through a threadpool boundary and expose an async policy.

## Rubric

- Correctness: async-shaped routes return expected API behavior.
- Testing: route behavior and exported policy are covered.
- Maintainability: sync and async responsibilities are documented.
- Design: blocking repository work is isolated at the route boundary.
- Production readiness: the project has a migration path to async drivers later.

## Complexity

Async is not a style preference. It is a concurrency contract. This milestone teaches when to use it and when to avoid pretending sync work is async.

The lesson is intentionally conservative: standard `sqlite3` remains synchronous. The production-quality move is to isolate that blocking work today and leave a clear migration path to an async database driver tomorrow, rather than spreading `async` across every function.

## Stretch goals

- Add timing instrumentation around blocking calls.
- Compare sync route handlers and async threadpool handlers.
- Later replace sqlite3 with an async database stack.
