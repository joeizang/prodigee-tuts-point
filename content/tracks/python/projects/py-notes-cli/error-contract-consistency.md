# Error Contract Consistency

## Scenario

The API currently knows how to return precise status codes. The next step is to make every failure response parse the same way. Clients should not receive `{"detail": ...}` from one endpoint, a list from validation, and a custom object from another endpoint.

## Requirements

- Define one `ErrorEnvelope` response model.
- Include machine-readable error codes.
- Include request IDs for support correlation.
- Preserve validation field paths.
- Hide stack traces and secret values.
- Register exception handlers centrally.

## CLI/API contract

Every public API failure should return:

```text
{"error": {"code": "...", "message": "...", "request_id": "...", "fields": [...]}}
```

Status codes still matter. The envelope standardizes shape; it does not flatten every failure into one category.

## Milestone task

Implement consistent FastAPI error handlers for validation, not-found, conflict, auth, readiness, and internal failures.

## Rubric

- Correctness: status codes and error codes match failure type.
- Testing: representative failure paths assert exact envelope shape.
- Maintainability: handlers are centralized.
- Design: field-level validation details remain useful.
- Production readiness: internal errors are safe and request-correlated.

## Complexity

Error contracts are easy to defer until clients exist, but by then incompatible shapes become harder to change. Standardizing early lets the app grow without creating a patchwork of failure formats. The important nuance is preserving useful detail while keeping one outer shape.

This milestone also ties into observability. The request id in the response should match logs and telemetry, giving support a safe way to investigate failures without exposing internals.

## Stretch goals

- Add localized human messages later.
- Add public error-code documentation.
- Add negative tests for secret leakage.
