# FastAPI Service Adapters

## Learning objectives

- Build a FastAPI app around existing notes service operations.
- Use Pydantic models for request bodies and response contracts.
- Keep route handlers thin and readable.
- Map domain validation failures to useful HTTP status codes.
- Test the API with FastAPI's `TestClient`.

## Prerequisites

You should understand the CLI command runner, note storage, and query/mutation service functions. You do not need previous web-framework experience, but you should be comfortable with functions, dictionaries, lists, and `Path`.

## Mental model

**Term: adapter** means code at the edge of the system. A CLI adapter receives command-line text. A FastAPI adapter receives HTTP requests. Neither adapter should own the core notes rules.

**Term: request model** means a Pydantic class that describes input JSON. FastAPI reads JSON into that model before your handler runs.

**Term: response model** means the shape your route promises to return. It is part of your API contract.

**Term: route handler** means the function FastAPI calls for a matched HTTP method and path.

## Core idea

The handler should look like a boundary translation:

```python
@app.post("/notes", response_model=NoteResponse, status_code=201)
def add_note(request: CreateNoteRequest) -> dict[str, object]:
    try:
        return service.add_note(request.title, request.body, request.tags)
    except ValueError as error:
        raise HTTPException(status_code=400, detail=str(error)) from error
```

The route receives HTTP data, calls the service, and translates validation failures. It should not reimplement note normalization, JSON storage, or search.

## Worked example

Pydantic models make request intent explicit:

```python
from pydantic import BaseModel, Field


class CreateNoteRequest(BaseModel):
    title: str = Field(min_length=1)
    body: str = Field(min_length=1)
    tags: list[str] = []
```

A tiny app factory keeps tests isolated:

```python
from pathlib import Path
from fastapi import FastAPI


def create_app(notes_path: Path) -> FastAPI:
    service = NotesService(notes_path)
    app = FastAPI()

    @app.get("/notes")
    def list_notes() -> list[dict[str, object]]:
        return service.list_notes()

    return app
```

Each test can pass a temporary path and get a fresh app.

## Production transfer

FastAPI is not replacing the CLI architecture. It is proving that the architecture was good. The same service boundary can sit behind command handlers today and HTTP handlers tomorrow.

This is also where Python type hints become more valuable. Pydantic and FastAPI use those hints to validate request data, generate OpenAPI documentation, and help your editor understand handler contracts.

## Common mistakes

- Putting JSON file load/save code directly inside every route handler.
- Returning unvalidated Python objects with inconsistent keys.
- Letting a `ValueError` become a generic 500 response.
- Creating global mutable state that makes tests affect each other.
- Treating Pydantic models as the domain model instead of as boundary input.
- Skipping API tests because the service already has tests.

## Testing strategy

Use `TestClient` for adapter behavior:

```python
from fastapi.testclient import TestClient


def test_create_note_returns_created_response(tmp_path: Path) -> None:
    client = TestClient(create_app(tmp_path / "notes.json"))

    response = client.post("/notes", json={"title": "Learn FastAPI", "body": "Reuse services"})

    assert response.status_code == 201
    assert response.json()["title"] == "learn fastapi"
```

Keep some service tests too. API tests prove HTTP behavior; service tests prove core behavior without web-framework noise.

## Debugging strategy

Separate failures by layer:

- 422 response: request JSON did not satisfy the Pydantic model.
- 400 response: your service rejected semantically invalid data.
- 404 response: a route or record lookup failed.
- 500 response: an exception escaped that should probably have been translated.
- Wrong JSON body: the response model or service return shape is wrong.

Use the response status and response JSON before reading framework internals.

## Exercise connection

`CreateNotesApi` asks you to build `create_app(notes_path)` with real FastAPI route handlers. The tests create notes, list notes, search by tag, update by title, delete by title, and assert validation/error behavior.

## Project connection

This milestone starts the FastAPI part of the Python track without abandoning the CLI work. The API is a second adapter over the same notes operations.

## Check yourself

- What belongs in a Pydantic request model?
- What belongs in the service helper instead of the route handler?
- Why does the app factory receive `notes_path`?
- When should a route return 400 instead of 422 or 404?

## Source reference notes

- FastAPI request-body documentation anchors Pydantic input models.
- FastAPI path and response model documentation anchors route contracts.
- FastAPI testing documentation anchors `TestClient` usage.
