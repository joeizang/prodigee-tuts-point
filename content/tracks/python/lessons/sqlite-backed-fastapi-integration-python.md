# SQLite-Backed FastAPI Integration

## Learning objectives

- Wire `SqliteNoteRepository` into FastAPI dependencies.
- Keep route handlers independent from concrete persistence.
- Initialize the SQLite schema at the boundary.
- Preserve dependency overrides for tests.
- Verify that the API contract remains stable after switching storage.

## Prerequisites

You should understand the repository abstraction, SQLite repository, dependency providers, and HTTP semantics.

## Mental model

**Term: concrete adapter** means an implementation of a boundary. `SqliteNoteRepository` is a concrete persistence adapter.

**Term: integration seam** means the narrow place where the concrete adapter is selected. In FastAPI, this is usually a dependency provider.

**Term: storage swap** means replacing one repository implementation with another while keeping service and route behavior stable.

## Core idea

The dependency provider creates concrete infrastructure:

```python
def get_notes_service(request: Request) -> NotesService:
    repository = SqliteNoteRepository(request.app.state.database_path)
    repository.initialize()
    return NotesService(repository)
```

The route still depends on the service:

```python
def list_notes(service: Annotated[NotesService, Depends(get_notes_service)]):
    return service.list_notes()
```

## Worked example

Tests can still replace the service:

```python
app.dependency_overrides[get_notes_service] = lambda: FakeNotesService()
```

That is how you know SQLite is not hard-coded into the route handler.

## Production transfer

This step proves the repository boundary worked. The API can now use durable storage while keeping route tests, service tests, and dependency overrides intact. It also prepares the project for environment-based settings and migrations.

## Common mistakes

- Creating SQLite connections globally at import time.
- Initializing schema inside every route handler.
- Letting tests depend on a shared database file.
- Returning SQLite rows through the API.
- Removing dependency override support after adding SQLite.

## Testing strategy

Use `tmp_path` for real SQLite integration tests. Use dependency overrides for route-only behavior. Both are useful, but they prove different things.

## Debugging strategy

If the integrated API fails:

- Check whether the schema was initialized.
- Check whether dependency overrides still target the provider.
- Check whether repository row mapping matches response models.
- Check whether duplicate/missing exceptions still map to HTTP status codes.

## Exercise connection

`IntegrateSqliteNotesApi` asks you to create an API backed by SQLite while preserving HTTP semantics and override seams.

## Project connection

This milestone makes `py-notes` durable without sacrificing the clean service and route boundaries already built.

## Check yourself

- Where is SQLite selected?
- Why should routes still depend on a service?
- How do tests avoid shared database state?
- What API behavior should remain unchanged?

## Source reference notes

- FastAPI dependency docs anchor concrete adapter selection.
- Python `sqlite3` docs anchor persistence implementation.
- FastAPI override docs anchor test seams.
