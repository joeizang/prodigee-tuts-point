# Migration Operations

## Learning objectives

- Define the deployment command order for database-backed FastAPI services.
- Distinguish migration execution from application startup.
- Write startup gates that verify schema readiness without mutating production schema.
- Document rollback and manual-review policy.

## Prerequisites

You should already understand Alembic environment wiring, readiness checks, SQLAlchemy metadata, and why production startup should not casually call `create_all`.

## Mental model

Migrations are deployment operations, not request-time behavior. The application should verify that schema is ready, but the deployment pipeline should run schema changes before the ASGI server accepts traffic.

**Term: migration gate** means a startup verification that blocks traffic when schema state is not acceptable.

**Term: operational rollback** means the documented response when a migration or deploy fails, including whether downgrade is supported.

## Core idea

The safe order is explicit:

```text
uv run alembic upgrade head
uv run pytest tests/test_migrations.py
uv run uvicorn notes_api.main:create_app --factory
```

The app startup check verifies database connectivity and expected schema version. It does not run migrations in production. That keeps schema changes reviewable and deployable as their own step.

## Production transfer

This contract becomes part of release documentation and CI. If migration smoke tests fail, the app is not deployed. If startup sees an old schema, it fails before accepting traffic.

## Common mistakes

- Running migrations implicitly in route handlers.
- Running `Base.metadata.create_all()` in production startup.
- Treating downgrade as always safe without review.
- Starting workers before schema upgrade completes.

## Testing strategy

Test command order, startup gate behavior, rollback policy, and whether production schema ownership belongs to Alembic instead of the app.

## Debugging strategy

When startup fails, inspect schema version, Alembic head, database URL, and deployment command logs before debugging route handlers.

## Exercise connection

`PlanMigrationOperations` asks you to produce the migration operation policy that deployment and startup code must follow.

## Project connection

This closes the gap between migration code and production operations.

## Check yourself

- Which command runs before the server starts?
- Should production startup create tables?
- What happens when schema is old?

## Source reference notes

Use FastAPI deployment guidance for startup behavior and pytest contract tests for release command expectations.
