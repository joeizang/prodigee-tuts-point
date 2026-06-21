# OpenAPI Contract Quality

## Learning objectives

- Configure FastAPI metadata so generated docs identify the API clearly.
- Use response models to keep OpenAPI and runtime responses aligned.
- Document expected error responses instead of leaving them implicit.
- Test the generated OpenAPI schema as part of the API contract.

## Prerequisites

You should already understand Pydantic request and response models, HTTP semantics, FastAPI route handlers, and why response models are part of the public API.

## Mental model

OpenAPI is executable documentation for clients. It is not marketing copy and it is not decoration. If the generated schema says a route returns one shape while runtime returns another, client code and human expectations drift apart.

**Term: operation metadata** means the tags, summary, description, operation id, status code, and response documentation attached to one route.

**Term: response model** means the Pydantic model FastAPI uses to serialize output and generate the response schema.

## Core idea

Treat the OpenAPI document as a testable artifact. Every public route should have a stable response model, intentional status code, useful operation metadata, and documented error response schema. Examples should clarify real payloads, not invent a different contract from the code.

## Worked example

```python
@app.post(
    "/notes",
    response_model=NoteRead,
    status_code=201,
    tags=["notes"],
    summary="Create a note",
    responses={409: {"model": ErrorEnvelope}},
)
def create_note(request: NoteCreate) -> NoteRead:
    ...
```

The schema can then be tested through `client.get("/openapi.json")`. A test can assert that `POST /notes` has the correct tag, summary, `201` response, and `409` error schema.

## Production transfer

Production clients may generate SDKs or validation code from OpenAPI. That means schema regressions can become client regressions. Keep the OpenAPI document reviewed and tested like route behavior.

## Common mistakes

- Returning dictionaries while documenting Pydantic models that do not match.
- Forgetting documented error responses.
- Letting route names become unstable operation ids.
- Adding examples that contain fields the API never returns.

## Testing strategy

Use route tests for runtime behavior and OpenAPI tests for documentation behavior. Check key schema names, response codes, tags, summaries, examples, and error response references.

## Debugging strategy

When OpenAPI looks wrong, check route decorators first. Then inspect the response model, status code, `responses` dictionary, and Pydantic model configuration.

## Exercise connection

`DocumentOpenApiContract` asks you to build a FastAPI app whose generated schema accurately documents notes routes and error responses.

## Project connection

This milestone makes the notes API usable by humans and generated clients without forcing readers to infer contracts from implementation code.

## Check yourself

- Which routes have explicit response models?
- Which error responses are documented?
- Can tests catch OpenAPI drift?

## Source reference notes

Use FastAPI path operation configuration and response model documentation as the framework anchor, and pytest assertions to lock down the generated schema.
