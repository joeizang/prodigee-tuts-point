# Pydantic Contract Rigor

## Scenario

The API is growing. Before adding more endpoints, the request and response shapes need to become explicit contracts instead of incidental dictionaries.

## Requirements

- Define separate create, update, read, and error models.
- Normalize title and tag data at the request boundary where appropriate.
- Forbid unexpected fields on request models.
- Use stable response serialization.
- Keep internal service records from leaking extra fields.

## CLI/API contract

The CLI can render text from service records. The API must return JSON contracts. Pydantic models are the API boundary; they should not be confused with storage dictionaries.

## Milestone task

Design and test Pydantic models for notes before expanding the HTTP surface further.

## Rubric

- Correctness: model instances validate and serialize to the expected shape.
- Testing: valid and invalid contract cases are covered directly.
- Maintainability: request, update, response, and error models are separate.
- Design: model validation is boundary-focused and does not absorb all service rules.
- API readiness: response models can safely filter internal fields.

## Complexity

This is contract design. The code is smaller than a route handler, but the naming and validation choices matter more.

A useful rule is to ask what would break a client. If a mobile app depends on `tags` always being present, the response model should make that true even when the internal service record was assembled from older data. If a client misspells `titel`, the request model should reject it instead of silently creating a malformed note. These are small details, but they are the details that make an API feel stable.

## Stretch goals

- Add examples for OpenAPI documentation.
- Add a cursor/pagination response wrapper for future list endpoints.
- Add versioned response models before changing public shape.
