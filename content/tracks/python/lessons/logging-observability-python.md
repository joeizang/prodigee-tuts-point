# Logging and Observability

## Learning objectives

- Add request IDs to responses and telemetry.
- Emit structured request events.
- Capture status code, method, path, and duration.
- Log errors without leaking secrets.
- Test observability behavior.

## Prerequisites

You should understand FastAPI middleware, status codes, and API tests.

## Mental model

**Term: observability** means the app emits enough information to understand behavior after it runs.

**Term: structured log** means a dictionary-like event with named fields rather than a vague sentence.

**Term: request ID** means an identifier that ties logs and responses together.

## Core idea

Middleware can wrap every request:

```python
@app.middleware("http")
async def observe(request, call_next):
    response = await call_next(request)
    log_event(...)
    return response
```

The route should not duplicate request logging.

## Worked example

Do log:

```python
{"method": "POST", "path": "/notes", "status_code": 201}
```

Do not log API keys or full request bodies by default.

## Production transfer

When an API fails in production, tests are no longer the only source of truth. Logs and request IDs tell you which request failed, how long it took, and what status was returned.

## Common mistakes

- Logging secrets.
- Logging only errors and missing successful request telemetry.
- Generating a request ID but not returning it.
- Measuring time inconsistently.
- Testing routes but never testing telemetry.

## Testing strategy

Expose a test sink such as `app.state.events` and assert emitted events in route tests.

## Debugging strategy

If logs are missing, check middleware registration and exception paths.

## Exercise connection

`ObserveNotesApi` asks you to add request IDs, structured events, and safe error records.

## Project connection

This milestone makes the notes API operationally inspectable before deployment.

## Check yourself

- Why return the request ID header?
- Which fields are safe to log?
- Where should request logging live?
- How can tests inspect telemetry?

## Source reference notes

- Python logging docs anchor log records.
- FastAPI middleware docs anchor request wrapping.
- pytest assertions anchor telemetry tests.
