# Alembic Environment Wiring

## Scenario

The app has moved from custom migration planning to the real migration tool used by SQLAlchemy projects. The next step is not only writing a revision file; it is creating an Alembic environment that imports application metadata, reads settings, and supports repeatable commands.

## Requirements

- Define the Alembic file layout.
- Import the application SQLAlchemy metadata from one stable module.
- Name revision, upgrade, downgrade, and smoke-test commands.
- Preserve reviewed migration files under version control.
- Stop relying on production `create_all` once migrations own schema changes.

## CLI/API contract

API clients should never see partial schema updates. Migrations run before the app serves traffic, and app startup should fail fast if the expected schema is unavailable.

## Milestone task

Design the Alembic environment contract for the notes API, including file paths, metadata imports, command sequence, and tests.

## Rubric

- Correctness: `target_metadata` points to the notes API model metadata.
- Testing: fresh upgrade and existing-database upgrade smoke tests are named.
- Maintainability: generated revisions are treated as reviewed source code.
- Design: Alembic owns schema lifecycle instead of route startup code.
- Production readiness: unsafe destructive operations require explicit review.

## Complexity

Alembic is easy to start and easy to misuse. Autogenerate can compare metadata, but it cannot know operational intent. It may miss data migration requirements, produce provider-specific defaults, or create changes that are unsafe on a large table. The mature habit is to review the generated revision, test it from a fresh database and an existing database, and make the deployment command explicit.

This milestone also protects the beginner learning path. SQLite still works for local learning, but migration discipline is framed in a way that transfers to PostgreSQL instead of depending on SQLite-only shortcuts.

## Stretch goals

- Add real `migrations/env.py` when Alembic is available.
- Add CI migration smoke tests.
- Add schema-version startup checks.
