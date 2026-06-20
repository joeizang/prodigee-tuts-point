# PostgreSQL Readiness

## Learning objectives

- Parse and validate database URLs.
- Distinguish SQLite and PostgreSQL runtime settings.
- Identify SQLite assumptions that fail on PostgreSQL.
- Prepare migration and test strategy for provider differences.
- Keep app code behind repository and session boundaries.

## Prerequisites

You should understand settings, repositories, migrations, and ORM boundaries.

## Mental model

**Term: provider difference** means behavior that changes between databases, such as booleans, JSON, locking, autoincrement, or case sensitivity.

**Term: connection URL** means the database connection string used by the app.

**Term: portability check** means a review that identifies SQLite-specific assumptions before moving to PostgreSQL.

## Core idea

Do not pretend SQLite and PostgreSQL are identical:

```python
postgresql+psycopg://user:password@localhost:5432/notes
```

The app should validate that production uses a PostgreSQL URL and tests should flag SQLite-only shortcuts.

## Worked example

SQLite-specific assumptions to flag:

- `INTEGER PRIMARY KEY AUTOINCREMENT`
- storing booleans as `0` or `1`
- relying on loose typing
- using SQLite-only pragmas

## Production transfer

PostgreSQL readiness is not only changing a connection string. It affects migrations, test data, transactions, pooling, deployment settings, and performance. Preparing deliberately prevents a painful rewrite.

## Common mistakes

- Assuming SQLite tests prove PostgreSQL behavior.
- Keeping SQLite-only SQL in repositories.
- Hard-coding database paths after adding URLs.
- Migrating without reviewing provider-specific types.
- Ignoring connection pooling and timeouts.

## Testing strategy

Test URL parsing, production settings requirements, and portability checks. Later, add integration tests against a real PostgreSQL service.

## Debugging strategy

If production database config fails, parse the URL first: scheme, user, host, port, and database name.

## Exercise connection

`PreparePostgresSettings` asks you to validate URLs and detect portability risks.

## Project connection

This milestone prepares the track for real PostgreSQL integration after ORM and migration fundamentals are established.

## Check yourself

- What does SQLite hide from you?
- Why should prod reject SQLite URLs?
- Which SQL patterns are provider-specific?
- What tests need a real PostgreSQL database later?

## Source reference notes

- FastAPI SQL database docs anchor database URL configuration.
- Python URL parsing docs anchor validation.
- pytest assertion docs anchor portability checks.
