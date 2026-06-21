# OpenAPI Contract Quality

## Scenario

The notes API now has real routes, Pydantic models, persistence boundaries, and production settings. The next quality bar is the generated OpenAPI document. API clients should be able to inspect the schema and understand success responses, error responses, examples, tags, and operation purposes.

## Requirements

- Configure FastAPI title, version, and summary.
- Use explicit response models for public routes.
- Add operation tags and summaries.
- Document conflict, validation, not-found, and readiness errors.
- Add examples that match runtime payloads.
- Test `/openapi.json` as a contract artifact.

## CLI/API contract

The API behavior and the OpenAPI document must describe the same contract. If the route returns `NoteRead`, the schema should expose `NoteRead`. If a route can return `409`, the schema should document the error envelope.

## Milestone task

Build and test a notes API OpenAPI contract with precise route metadata and documented error schemas.

## Rubric

- Correctness: success and error schemas match runtime responses.
- Testing: OpenAPI JSON is asserted directly.
- Maintainability: route metadata is explicit and stable.
- Design: examples clarify real payloads without inventing hidden fields.
- Production readiness: client-facing schema drift is caught in tests.

## Complexity

OpenAPI quality is not about making docs prettier. It is about making the contract inspectable and stable. Generated clients, manual testers, and future maintainers rely on this document to understand what the API accepts and returns. A polished schema prevents implementation details from becoming tribal knowledge.

The most useful tests do not assert the whole generated document. They assert the high-value contract points: route presence, operation ids, tags, summaries, response codes, schema references, and examples.

## Stretch goals

- Add operation-id stability rules.
- Add OpenAPI diff checks in CI.
- Add examples for every error envelope.
