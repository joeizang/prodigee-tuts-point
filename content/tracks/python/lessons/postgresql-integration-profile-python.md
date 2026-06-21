# PostgreSQL Integration Profile

## Learning objectives

- Define what must be proven against PostgreSQL instead of SQLite.
- Separate fast unit tests from provider-real integration tests.
- Name the fixtures, settings, markers, and CI gates for PostgreSQL.
- Connect migrations, repositories, readiness, and engine settings in one profile.

## Prerequisites

You should understand SQLAlchemy repository tests, Alembic migrations, PostgreSQL engine settings, and readiness checks.

## Mental model

SQLite is useful for speed, but PostgreSQL is the production provider. Provider-real confidence requires tests that run against PostgreSQL semantics: connection URLs, pooling, JSON behavior, indexes, migrations, and transaction behavior.

**Term: integration profile** means the named test mode that uses real external dependencies.

**Term: provider-real test** means a test that runs against the same database family used in production.

## Core idea

The PostgreSQL profile should require:

- `APP_DATABASE_URL=postgresql+psycopg://...`
- `pytest -m postgres`
- migration upgrade before repository tests
- readiness checks against the migrated database
- teardown or isolated schema per run

```text
APP_DATABASE_URL=postgresql+psycopg://user:password@localhost:5432/notes_test
uv run alembic upgrade head
uv run pytest -m postgres
```

## Production transfer

This profile can run in CI with a service container or on a developer machine with Docker. It should not replace fast unit tests; it complements them at the provider boundary.

## Common mistakes

- Assuming SQLite tests prove PostgreSQL behavior.
- Running PostgreSQL tests without applying migrations.
- Sharing dirty databases across test runs.
- Hiding provider requirements in ad hoc local scripts.

## Testing strategy

Test profile configuration itself: marker name, required environment variables, command sequence, fixture responsibilities, and covered behaviors.

## Debugging strategy

When PostgreSQL tests fail, check service availability, URL parsing, migration status, schema isolation, and transaction cleanup first.

## Exercise connection

`DefinePostgresIntegrationProfile` asks you to specify the provider-real test profile for the notes API.

## Project connection

This is the step where PostgreSQL moves from configuration readiness to executable confidence.

## Check yourself

- Which tests must run against PostgreSQL?
- How does the database become migrated?
- How does CI know the profile is enabled?

## Source reference notes

Use FastAPI database testing guidance and pytest fixture/marker patterns as anchors.
