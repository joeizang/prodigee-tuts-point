# FastAPI Service Adapter

## Scenario

The notes logic is now useful enough to expose over HTTP. This milestone starts FastAPI by treating it as another adapter over the existing service boundary, not as a rewrite.

## Requirements

- Create a `FastAPI` app with an app factory.
- Use Pydantic request models for create and update operations.
- Return deterministic JSON response shapes.
- Support create, list, search by tag, update by title, and delete by title.
- Translate invalid service data to `400` and missing records to `404`.
- Test handlers with `TestClient`.

## CLI/API contract

The API receives JSON and path/query parameters instead of command tokens. The underlying operations should still be the same: validate, load, mutate or query, save when needed, and return trusted records.

## Milestone task

Build `create_app(notes_path)` and route handlers that call reusable notes operations. Keep the handlers thin enough that a reader can see the HTTP boundary and the service call at a glance.

## Rubric

- Correctness: API responses match status-code and JSON contracts.
- Design: route handlers do not duplicate storage and mutation rules.
- Validation: Pydantic handles structural request validation and service code handles semantic validation.
- Testing: each test creates its own app with a temporary notes path.
- Maintainability: request models, service operations, and route handlers stay distinct.
- Professional readiness: the app can grow toward dependency injection and a real database later.

## Complexity

The new complexity is adapter translation: HTTP method, path, query, request body, response body, and status code. Keep application rules out of that translation layer.

## Stretch goals

- Add response models for every endpoint.
- Add OpenAPI examples.
- Split service code and API code into separate modules.
