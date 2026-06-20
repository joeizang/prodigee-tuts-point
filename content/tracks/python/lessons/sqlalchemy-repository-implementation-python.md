# SQLAlchemy Repository Implementation

## Learning objectives

- Name the production dependency set for a SQLAlchemy-backed notes API.
- Design an ORM row model without exposing it as the public response contract.
- Define the session boundary owned by the repository layer.
- Map ORM rows to service records deliberately.

## Prerequisites

You should already understand repository boundaries, Pydantic response models, transactions, and why route handlers should stay thin. The previous ORM boundary milestone planned the shape; this lesson turns that plan into implementation decisions.

## Mental model

SQLAlchemy is the persistence mechanism, not the application model. The API still speaks in requests, service records, and responses. The repository is the translator that knows how to open a session, query rows, write changes, commit or roll back, and return plain application records.

**Term: ORM row model** means the Python class SQLAlchemy maps to a database table.

**Term: repository implementation** means the concrete class that hides SQLAlchemy sessions and queries behind use-case methods.

## Core idea

A professional SQLAlchemy integration has four named parts:

- A table row model, such as `NoteRow`, with columns and indexes.
- A session factory, usually created from an engine.
- A repository implementation, such as `SqlAlchemyNoteRepository`.
- Mapping functions that convert between rows and service records.

The service should not import `Session`, call `select`, or return ORM objects. The route should not know whether the repository uses SQLite, PostgreSQL, SQLAlchemy, SQLModel, or a test double.

## Worked example

The dependency set is explicit:

```text
dependencies = [
  "sqlalchemy>=2.0.36",
  "sqlmodel>=0.0.22",
  "alembic>=1.14.0",
  "psycopg[binary]>=3.2.0",
]
```

The model shape is explicit:

```python
class NoteRow(Base):
    __tablename__ = "notes"

    title: Mapped[str] = mapped_column(String(200), primary_key=True)
    body: Mapped[str] = mapped_column(Text, nullable=False)
    tags: Mapped[list[str]] = mapped_column(JSON, nullable=False)
    archived: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
```

The repository owns SQLAlchemy:

```python
class SqlAlchemyNoteRepository:
    def __init__(self, session_factory: Callable[[], Session]) -> None:
        self._session_factory = session_factory
```

## Production transfer

In production, this becomes real module structure: `models.py` owns ORM rows, `repositories.py` owns session use, `services.py` owns business rules, `schemas.py` owns Pydantic contracts, and `dependencies.py` wires a repository into FastAPI routes.

## Common mistakes

- Returning `NoteRow` from a route because FastAPI can serialize some objects.
- Calling `Session` directly inside services.
- Letting tests assert SQLAlchemy internals instead of repository behavior.
- Treating SQLModel table models and response models as interchangeable.

## Testing strategy

Keep contract tests against the repository interface. Add integration tests that create a fresh database, create schema from metadata or migrations, write a note, read it back, update it, delete it, and verify the service still returns plain records.

## Debugging strategy

When persistence behavior is wrong, inspect the boundary first. Check whether the repository commits, whether row-to-record mapping normalizes fields, whether sessions are closed, and whether tests accidentally reuse database state.

## Exercise connection

`ImplementSqlAlchemyRepository` asks you to specify the concrete production implementation shape: packages, model fields, repository methods, session ownership, and mapping rules.

## Project connection

This milestone is the bridge between abstract repository design and the real SQLAlchemy implementation that a production FastAPI app would use.

## Check yourself

- Which layer is allowed to import SQLAlchemy `Session`?
- Why should route handlers avoid returning ORM rows?
- Where should row-to-response conversion happen?

## Source reference notes

Use FastAPI's SQL database guidance for the framework integration shape, SQLAlchemy 2 style for typed mappings, and pytest fixtures for database isolation.
