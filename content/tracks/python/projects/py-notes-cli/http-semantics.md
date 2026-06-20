# HTTP Semantics

## Scenario

The notes API now needs client-grade behavior. A client should be able to tell the difference between created, updated, missing, duplicate, invalid, and deleted outcomes without parsing vague text.

## Requirements

- Return `201` for successful creation.
- Return `409` for duplicate note creation.
- Return `400` for semantic validation failures.
- Preserve FastAPI's `422` structural validation behavior.
- Return `404` for missing records.
- Return `204` with no body for successful deletion.

## CLI/API contract

The service raises meaningful errors. The HTTP adapter maps those errors to status codes and response bodies. The CLI can still render messages from the same outcomes.

## Milestone task

Implement route handlers that translate notes service outcomes into deliberate HTTP responses.

## Rubric

- Correctness: every tested outcome maps to the expected status code and body.
- Testing: status codes and response bodies are asserted together.
- Maintainability: error translation remains at the HTTP boundary.
- Design: conflict, missing, validation, and structural errors are not collapsed into one category.
- Client readiness: responses are predictable enough for a UI or CLI client to consume.

## Complexity

The complexity is not syntax. It is deciding what each outcome means to an HTTP client.

Keep this mapping close to the route layer. The notes service should be able to say "duplicate", "not found", or "invalid", but the HTTP adapter decides whether that becomes `409`, `404`, or `400`. That separation keeps the same service usable from the CLI, tests, and later background jobs without smuggling HTTP concepts into the core.

## Stretch goals

- Add structured error models for all error responses.
- Add `Location` headers for creation.
- Add optimistic concurrency before update conflicts.
