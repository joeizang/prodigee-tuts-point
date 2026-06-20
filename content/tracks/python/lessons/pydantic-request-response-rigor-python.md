# Pydantic Request and Response Rigor

## Learning objectives

- Separate request models from response models.
- Use field constraints and validators for boundary-level input normalization.
- Forbid accidental extra request fields when the API contract should be strict.
- Return response models that expose only the intended fields.
- Design a stable error response shape.

## Prerequisites

You should understand FastAPI request bodies and basic Pydantic models. This lesson focuses on contract design, not first syntax.

## Mental model

**Term: request model** means the JSON shape the client is allowed to send.

**Term: response model** means the JSON shape the API promises to send back.

**Term: contract model** means a Pydantic model treated as public API, not just a convenient internal class.

**Term: semantic validation** means validation that goes beyond JSON shape. `"machine learning"` may be a string, but it is not a valid tag if tags cannot contain spaces.

## Core idea

Do not use one model everywhere. Create models for the direction of data flow:

```python
from pydantic import BaseModel, ConfigDict, Field


class NoteCreate(BaseModel):
    model_config = ConfigDict(extra="forbid")

    title: str = Field(min_length=1)
    body: str = Field(min_length=1)
    tags: list[str] = Field(default_factory=list)


class NoteRead(BaseModel):
    title: str
    body: str
    tags: list[str]
```

The create model accepts input. The read model declares output. Keeping them separate lets the API evolve without leaking internal fields.

## Worked example

A validator can normalize input at the boundary:

```python
from pydantic import field_validator


class NoteCreate(BaseModel):
    title: str = Field(min_length=1)

    @field_validator("title")
    @classmethod
    def normalize_title(cls, value: str) -> str:
        words = value.split()
        if not words:
            raise ValueError("title is required")
        return " ".join(words).lower()
```

Use this carefully. Validation should make request data trustworthy, but deeper business decisions still belong in the service layer.

## Production transfer

Good Pydantic contracts help four audiences at once:

- FastAPI can validate requests.
- OpenAPI documentation becomes accurate.
- Tests can assert exact API shape.
- Editors can provide real type feedback.

This is why response rigor matters. If the service record later includes `internal_id` or `storage_version`, the response model prevents accidental exposure.

## Common mistakes

- Reusing the persistence dictionary as the public response contract.
- Allowing extra request fields when the API should reject typos.
- Using mutable default lists instead of `Field(default_factory=list)`.
- Returning `None` for optional fields when the API contract says the field is required.
- Treating all validation as Pydantic validation and leaving no service-level rules.
- Forgetting to test serialization with `model_dump()`.

## Testing strategy

Test models directly before route tests:

```python
def test_create_model_normalizes_title_and_tags() -> None:
    note = NoteCreate(title=" Learn FastAPI ", body="Build", tags=["Python", "api"])

    assert note.model_dump() == {
        "title": "learn fastapi",
        "body": "Build",
        "tags": ["python", "api"],
    }
```

Also test rejection paths. Contract tests are cheap and catch API drift early.

## Debugging strategy

When validation surprises you:

- Inspect `model_dump()` to see the serialized shape.
- Check whether the failure is structural validation or semantic validation.
- Check whether a default should be `default_factory`.
- Check whether response filtering removed a field you expected.

## Exercise connection

`DefineNoteContracts` asks you to create strict Pydantic models for notes, updates, responses, and errors. The tests verify normalization, forbidden extra fields, default tags, and output serialization.

## Project connection

The notes API now has enough behavior that contract drift is a real risk. Strong models become the API's written agreement with clients.

## Check yourself

- Why should create and read models be separate?
- What should `extra="forbid"` protect you from?
- When should validation stay in the service layer instead of Pydantic?
- Why does response filtering matter?

## Source reference notes

- FastAPI request-body documentation anchors request models.
- FastAPI response-model documentation anchors output filtering.
- FastAPI field documentation anchors field constraints and metadata.
