# Error Contract Consistency

## Learning objectives

- Define one Pydantic error envelope for public API failures.
- Map domain errors to precise HTTP status codes.
- Override validation errors without losing useful field information.
- Keep internal errors safe and observable.

## Prerequisites

You should already understand FastAPI exception handlers, Pydantic validation, HTTP status codes, request IDs, and why clients need stable failure shapes.

## Mental model

Errors are part of the API. A client should not need five parsers for five failure paths. Validation, conflict, not-found, auth, readiness, and internal failures can have different status codes and error codes while still sharing one envelope.

**Term: error envelope** means the stable JSON object shape returned for API failures.

**Term: error code** means the machine-readable string that lets clients branch without parsing prose.

## Core idea

Use a single response shape:

```json
{
  "error": {
    "code": "note_conflict",
    "message": "note already exists",
    "request_id": "req-123",
    "fields": []
  }
}
```

Domain exceptions become `400`, `404`, or `409`. Request validation becomes `422` with field paths. Authentication failures remain `401` or `403`. Unexpected exceptions become `500` with a safe generic message and a request id for support.

## Worked example

```python
@app.exception_handler(NoteNotFoundError)
async def note_not_found(request: Request, error: NoteNotFoundError):
    return error_response(404, "note_not_found", str(error), request)
```

The handler centralizes shape. Routes stay focused on application work and can raise domain-specific exceptions.

## Production transfer

Stable error contracts reduce client ambiguity and improve support. Logs can correlate by request id while responses avoid secrets, stack traces, and raw exception details.

## Common mistakes

- Returning `{"detail": "..."}` for some errors and a custom shape for others.
- Leaking stack traces or API keys into 500 responses.
- Flattening validation errors until clients cannot find the bad field.
- Using `200` with an error body.

## Testing strategy

Test representative failure paths: validation, not found, conflict, unauthorized, forbidden, readiness failure, and internal error. Assert both status code and envelope shape.

## Debugging strategy

When an error shape is inconsistent, inspect exception registration order and route code that still raises raw `HTTPException` directly.

## Exercise connection

`StandardizeErrorContract` asks you to implement one envelope and handlers for domain, validation, auth, and internal failures.

## Project connection

This milestone makes the notes API a stable client-facing system instead of a collection of route-specific failure conventions.

## Check yourself

- Can every failure response be parsed with the same envelope?
- Are field validation errors still precise?
- Do internal errors hide implementation details?

## Source reference notes

Use FastAPI's error handling and request validation documentation for handler mechanics, and pytest contract tests for exact response shapes.
