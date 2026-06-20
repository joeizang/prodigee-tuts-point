# Async Boundary Discipline

## Learning objectives

- Decide when FastAPI handlers should be `async def`.
- Explain why blocking SQLite calls are not magically async.
- Isolate blocking work with threadpool boundaries.
- Document async policy for a small API.
- Test async-shaped route behavior through `TestClient`.

## Prerequisites

You should understand FastAPI route handlers, SQLite repositories, service boundaries, and API tests.

## Mental model

**Term: event loop** means the runtime mechanism that schedules async tasks.

**Term: blocking work** means code that holds the current thread while it waits, such as standard `sqlite3` file I/O.

**Term: async boundary** means the place where async request handling meets synchronous work.

## Core idea

Do not make code async because it looks modern. Standard `sqlite3` is synchronous. If an async route calls it directly, the route can block the event loop.

Use a boundary:

```python
from starlette.concurrency import run_in_threadpool


@app.get("/notes")
async def list_notes(service: NotesService = Depends(...)):
    return await run_in_threadpool(service.list_notes)
```

## Worked example

An async policy can be explicit:

```python
ASYNC_BOUNDARY_POLICY = {
    "route_handlers": "async when coordinating request work",
    "sqlite_repository": "sync, isolated with run_in_threadpool",
    "service": "sync application rules",
}
```

That policy helps learners avoid random async conversions.

## Production transfer

Async discipline matters when the app starts doing network I/O, background work, or heavier database usage. Later, an async database driver might remove the threadpool boundary. Until then, be honest about what blocks.

## Common mistakes

- Marking every function async without awaiting anything.
- Calling blocking SQLite code directly in async routes.
- Making repository methods async while still using sync sqlite3.
- Forgetting that tests can pass even when the event loop design is poor.
- Mixing sync and async APIs without a written policy.

## Testing strategy

Use `TestClient` for behavior, and test that the policy/exported helpers make the boundary explicit. Behavior tests prove the route works; policy tests protect the architecture decision.

## Debugging strategy

If async routes feel slow, locate the blocking operation. If it uses sync disk, sync database, or CPU-heavy work, isolate it or keep the route synchronous.

## Exercise connection

`AsyncBoundaryPolicy` asks you to expose async FastAPI routes that call synchronous SQLite-backed service methods through `run_in_threadpool`.

## Project connection

This milestone prevents async from becoming cargo cult. The notes API gains a clear rule for sync SQLite today and async evolution later.

## Check yourself

- Is standard `sqlite3` async?
- What does `run_in_threadpool` protect?
- Which layer should stay sync?
- When would you switch to an async database driver?

## Source reference notes

- FastAPI async docs anchor route handler choices.
- Python `asyncio` docs anchor event-loop concepts.
- FastAPI testing docs anchor route behavior tests.
