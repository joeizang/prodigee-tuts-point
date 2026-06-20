# SQLite Schema Migrations

## Learning objectives

- Track SQLite schema versions.
- Apply migrations idempotently.
- Keep migrations transactional.
- Verify the current schema at startup.
- Test fresh and existing database upgrades.

## Prerequisites

You should understand SQLite repositories, transactions, and production startup.

## Mental model

**Term: migration** means a controlled schema change.

**Term: schema version** means the database's current migration level.

**Term: idempotent** means running the migration process again does not corrupt data or duplicate schema objects.

## Core idea

Create a version table:

```python
CREATE TABLE IF NOT EXISTS schema_version (version INTEGER NOT NULL)
```

Then apply only migrations newer than the current version.

## Worked example

Migration 1 creates notes. Migration 2 adds `archived`.

```python
ALTER TABLE notes ADD COLUMN archived INTEGER NOT NULL DEFAULT 0
```

Tests should prove old data survives.

## Production transfer

Deployment needs startup checks. If code expects `archived` but the database does not have it, the app should fail before serving traffic.

## Common mistakes

- Changing schema in repository methods.
- Running migrations without version tracking.
- Dropping user data during upgrades.
- Making migrations non-idempotent.
- Testing only fresh databases.

## Testing strategy

Test fresh database migration, old database upgrade, idempotent rerun, and startup verification.

## Debugging strategy

Use `PRAGMA table_info(table)` and inspect `schema_version` first.

## Exercise connection

`MigrateSqliteSchema` asks you to implement a small migration runner and verifier.

## Project connection

This milestone makes the SQLite-backed API deployable across schema changes.

## Check yourself

- Where is schema version stored?
- Why test old database upgrades?
- Why should migrations be transactional?
- What should startup verify?

## Source reference notes

- `sqlite3` docs anchor schema inspection.
- `contextlib` docs anchor transaction resource boundaries.
- pytest tmp dirs anchor isolated migration databases.
