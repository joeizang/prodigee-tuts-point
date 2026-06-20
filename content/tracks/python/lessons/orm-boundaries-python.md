# SQLAlchemy and SQLModel Boundaries

## Learning objectives

- Explain what an ORM mapping owns.
- Keep ORM rows out of FastAPI response contracts.
- Design repository methods around use cases rather than table access.
- Separate service rules from session mechanics.
- Prepare for SQLAlchemy or SQLModel without rewriting route handlers.

## Prerequisites

You should understand repositories, SQLite persistence, Pydantic response models, and FastAPI dependencies.

## Mental model

**Term: ORM model** means a Python class mapped to a database table.

**Term: session** means the unit that tracks ORM objects and database work.

**Term: mapping boundary** means the conversion between ORM rows and application/response data.

## Core idea

The route should not return ORM objects. The service should not know SQLAlchemy session details. Keep conversion explicit:

```python
def to_note_read(row: NoteRow) -> NoteRead:
    return NoteRead(title=row.title, body=row.body, tags=row.tags)
```

The repository owns row loading and row mapping.

## Worked example

A repository method should read like a use case:

```python
def list_notes(self, tag: str | None, limit: int, offset: int) -> Page[NoteRecord]:
    ...
```

That is better than exposing arbitrary table queries to the service.

## Production transfer

SQLAlchemy and SQLModel become powerful only when their boundaries are respected. They should reduce repetitive SQL, not spread database concerns through the API layer.

## Common mistakes

- Returning ORM instances directly from routes.
- Letting services depend on SQLAlchemy `Session`.
- Mixing Pydantic response models with table models too early.
- Hiding every query behind generic CRUD methods.
- Testing only ORM behavior and skipping service contracts.

## Testing strategy

Before packages are added, test the mapping plan and repository contract shape. After packages are installed, reuse the same contract tests against real SQLAlchemy/SQLModel repositories.

## Debugging strategy

If ORM code becomes confusing, inspect the boundary: table model, repository method, service method, response model.

## Exercise connection

`DesignOrmNoteMapping` asks you to produce a concrete mapping plan and repository contract that can later be implemented with SQLAlchemy or SQLModel.

## Project connection

This milestone prepares the notes API to move beyond handwritten SQL without losing the architecture already built.

## Check yourself

- Which layer should know about sessions?
- Why avoid returning ORM rows from routes?
- What makes a repository method use-case-shaped?
- How does SQLModel relate to Pydantic and SQLAlchemy?

## Source reference notes

- FastAPI SQL database guidance anchors ORM-backed app shape.
- Python class material anchors object collaboration.
- pytest assertion guidance anchors contract tests.
