# FastAPI Dependency Boundaries

## Learning objectives

- Explain why FastAPI dependencies are application boundaries, not just a convenience feature.
- Use `Depends` to resolve a notes service inside route handlers.
- Keep service construction outside the handler body.
- Use app factories and app state without creating untestable global state.
- Override dependencies in tests with `app.dependency_overrides`.

## Prerequisites

You should understand the previous FastAPI service-adapter slice: request models, route handlers, and a `NotesService` that owns the core notes operations.

## Mental model

**Term: dependency** means a value a route handler needs in order to do its work. A notes handler might need `NotesService`.

**Term: dependency provider** means the function FastAPI calls to produce that value.

**Term: dependency override** means a test-time replacement for the provider. It lets you test HTTP behavior without depending on a real file, database, network, or clock.

**Term: app factory** means a function that builds and configures the FastAPI app. It lets each test create an isolated app.

## Core idea

Handlers should declare what they need:

```python
from typing import Annotated

from fastapi import Depends


@app.get("/notes")
def list_notes(service: Annotated[NotesService, Depends(get_notes_service)]) -> list[dict[str, object]]:
    return service.list_notes()
```

The handler does not call `NotesService(...)` directly. That construction belongs to the dependency provider.

## Worked example

An app factory can store configuration in app state:

```python
from pathlib import Path
from fastapi import FastAPI, Request


def create_app(notes_path: Path) -> FastAPI:
    app = FastAPI()
    app.state.notes_path = notes_path
    return app


def get_notes_service(request: Request) -> NotesService:
    return NotesService(request.app.state.notes_path)
```

Tests can replace the provider:

```python
app = create_app(tmp_path / "notes.json")
fake = FakeNotesService()
app.dependency_overrides[get_notes_service] = lambda: fake
```

That test now proves route-handler behavior without touching the file boundary.

## Production transfer

This pattern is the bridge to production FastAPI. Today the dependency creates a file-backed service. Later it can create a database-backed service, attach a transaction, read authenticated user context, or provide a repository.

The route handler should not care. It should receive the dependency and call the service contract.

## Common mistakes

- Constructing the service inside every route handler.
- Creating a global service instance that leaks state between tests.
- Overriding the wrong dependency function in tests.
- Putting business rules inside the dependency provider.
- Using dependencies as hidden input when an explicit function parameter would be clearer.
- Letting route tests hit real persistent files when a fake service would prove the HTTP boundary.

## Testing strategy

Test the default dependency path with `tmp_path`, then test override behavior:

```python
def test_dependency_override_controls_handler_response(tmp_path: Path) -> None:
    app = create_app(tmp_path / "notes.json")
    app.dependency_overrides[get_notes_service] = lambda: FakeNotesService()
    client = TestClient(app)

    response = client.get("/notes")

    assert response.json() == [{"title": "fake", "body": "body", "tags": []}]
```

The point is not to fake everything. The point is to prove the handler is coupled to the service contract, not to a concrete file implementation.

## Debugging strategy

When dependency tests fail:

- If the handler receives the real service, check that `app.dependency_overrides` uses the exact provider function object.
- If state is missing, check that the app was created by the factory.
- If tests affect each other, clear overrides or create a new app per test.
- If handler signatures get noisy, move construction details into the provider.

## Exercise connection

`InjectNotesService` asks you to expose a notes API where handlers receive `NotesService` through `Depends`. Hidden tests override the dependency and expect the route to use the fake service.

## Project connection

After this milestone, the notes API can grow without hard-coding service construction into every handler. That is the first real production structure step in the FastAPI track.

## Check yourself

- What should a dependency provider construct?
- What should a route handler still do itself?
- Why does an override need the exact same provider function?
- How does this pattern prepare the project for database persistence?

## Source reference notes

- FastAPI dependency documentation anchors `Depends`.
- FastAPI testing documentation anchors dependency overrides.
- FastAPI application-structure documentation anchors app factories and app state.
