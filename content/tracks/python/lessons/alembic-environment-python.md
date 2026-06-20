# Alembic Environment

## Learning objectives

- Identify the files that make Alembic usable in a FastAPI package.
- Explain how Alembic discovers SQLAlchemy metadata.
- Define safe revision commands for local development and CI.
- Plan migration smoke tests before production deployment.

## Prerequisites

You should already understand schema migrations, SQLAlchemy metadata, and the difference between generated migration suggestions and reviewed migration source code.

## Mental model

Alembic is not just a command. It is an environment that imports your application metadata, compares it to a database, and runs ordered revision files. If that environment imports the wrong metadata, points at the wrong database URL, or runs without smoke tests, the migration workflow is fragile.

**Term: target metadata** means the SQLAlchemy metadata object Alembic compares against the database schema.

**Term: revision file** means the reviewed migration source code that defines upgrade and downgrade behavior.

## Core idea

A production-shaped Alembic setup has:

- `alembic.ini` for command configuration.
- `migrations/env.py` for metadata import and online/offline execution.
- `migrations/versions/` for reviewed revision files.
- A clear command contract for `revision`, `upgrade`, `downgrade`, and CI smoke tests.

The most important line in `env.py` is the metadata connection:

```python
from notes_api.models import Base

target_metadata = Base.metadata
```

## Worked example

A safe first workflow is:

```bash
uv run alembic revision --autogenerate -m "create notes table"
uv run alembic upgrade head
uv run pytest tests/test_migrations.py
```

The generated revision is still source code. Review column types, indexes, nullable changes, data backfills, downgrade behavior, and PostgreSQL portability before it is merged.

## Production transfer

The FastAPI app should not create production tables opportunistically during request startup once Alembic owns schema changes. Startup may verify the schema version or fail fast, but deployments should run migrations deliberately.

## Common mistakes

- Importing a test metadata object instead of the application metadata.
- Running autogenerate and merging without reading the revision.
- Depending on `create_all` in production after migrations exist.
- Forgetting to test upgrade from an existing database.

## Testing strategy

Smoke tests should run `upgrade head` against a fresh database, insert rows through the app repository, and verify that downgrade behavior is documented or executable for the supported environment. Data-preserving migrations need an upgraded-existing-database test.

## Debugging strategy

When Alembic misses changes, inspect `target_metadata`. When migrations hit the wrong database, inspect URL loading. When a migration passes SQLite but fails PostgreSQL, inspect provider-specific types, indexes, defaults, and transactional DDL assumptions.

## Exercise connection

`ConfigureAlembicEnvironment` asks you to define the exact files, imports, command sequence, and smoke tests for a SQLAlchemy-backed notes API.

## Project connection

This milestone turns prior migration planning into an executable project convention that future production schema changes can reuse.

## Check yourself

- Where should Alembic import application metadata from?
- Why is autogenerate not enough by itself?
- Which migration tests should run in CI?

## Source reference notes

Use FastAPI's SQL database structure as the application side, Python module imports for reliable metadata discovery, and pytest temporary databases for migration smoke testing.
