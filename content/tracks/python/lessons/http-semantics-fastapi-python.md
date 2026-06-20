# HTTP Semantics in FastAPI

## Learning objectives

- Choose successful status codes deliberately.
- Distinguish structural validation, semantic validation, missing resources, and conflicts.
- Return no body for `204 No Content`.
- Use `HTTPException` for explicit error translation.
- Test status code and response body together.

## Prerequisites

You should understand FastAPI route handlers, Pydantic models, and service boundaries. This lesson focuses on HTTP behavior.

## Mental model

**Term: HTTP semantics** means the meaning carried by method, path, status code, headers, and body.

**Term: structural validation error** means the request body does not match the declared model. FastAPI normally returns `422`.

**Term: semantic validation error** means the JSON shape is valid, but the application rule rejects it. This API uses `400`.

**Term: conflict** means the request is valid but clashes with existing state. Creating a duplicate note can return `409`.

## Core idea

Do not make every endpoint return `200`. The status code should describe the result:

```python
@app.post("/notes", status_code=201)
def create_note(request: NoteCreate) -> NoteRead:
    ...


@app.delete("/notes/{title}", status_code=204)
def delete_note(title: str) -> None:
    ...
```

The response contract is part of the feature, not decoration.

## Worked example

Translate service outcomes at the edge:

```python
try:
    return service.create_note(request)
except DuplicateNoteError as error:
    raise HTTPException(status_code=409, detail=str(error)) from error
except ValueError as error:
    raise HTTPException(status_code=400, detail=str(error)) from error
```

Missing records are different:

```python
except NoteNotFoundError as error:
    raise HTTPException(status_code=404, detail=str(error)) from error
```

These distinctions help clients respond correctly.

## Production transfer

HTTP semantics become more important as clients multiply. A browser UI, mobile app, CLI client, and background integration can all consume the same API if the status codes and response shapes are reliable.

This also improves observability. A spike in `409` means something different from a spike in `422` or `500`.

## Common mistakes

- Returning `200` for creation when `201` communicates a new resource.
- Returning a JSON body with `204 No Content`.
- Using `404` for validation errors.
- Letting duplicate creates overwrite existing records silently.
- Catching every exception and returning `400`.
- Testing only response JSON and ignoring status codes.

## Testing strategy

Test status and body together:

```python
def test_duplicate_create_returns_conflict(client: TestClient) -> None:
    client.post("/notes", json={"title": "Python", "body": "one"})

    response = client.post("/notes", json={"title": "Python", "body": "two"})

    assert response.status_code == 409
    assert response.json()["detail"] == "note already exists: python"
```

For delete:

```python
response = client.delete("/notes/python")

assert response.status_code == 204
assert response.content == b""
```

## Debugging strategy

When an HTTP test fails:

- `422` usually means Pydantic rejected the shape before the handler ran.
- `400` should come from semantic service validation.
- `404` means the resource was absent.
- `409` means the resource state conflicts with the request.
- `500` means an exception escaped the translation boundary.

Read the status code first, then inspect the JSON.

## Exercise connection

`MapNotesHttpSemantics` asks you to implement routes where create, duplicate create, update, missing update, invalid tags, and delete each have distinct HTTP behavior.

## Project connection

This milestone moves the notes API from "working route handlers" toward a client-ready API contract.

## Check yourself

- When should create return `201`?
- Why should delete return `204` with no body?
- How is `409` different from `400`?
- Why does FastAPI return `422` before your handler runs?

## Source reference notes

- FastAPI status-code documentation anchors successful response declarations.
- FastAPI error documentation anchors `HTTPException`.
- FastAPI testing documentation anchors status/body assertions.
