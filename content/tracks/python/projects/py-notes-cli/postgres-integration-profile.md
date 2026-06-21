# PostgreSQL Integration Profile

## Scenario

SQLite and in-memory tests remain valuable, but production confidence requires PostgreSQL behavior. The project needs a named profile that runs against the production database family and proves migrations, repositories, engine settings, and readiness checks together.

## Requirements

- Define profile name and pytest marker.
- Require PostgreSQL database URL configuration.
- Run migrations before provider-real tests.
- Verify repository CRUD/search behavior.
- Verify readiness on migrated PostgreSQL.
- Isolate database state between runs.

## CLI/API contract

The profile should be opt-in and explicit: `pytest -m postgres`. It should fail clearly when the PostgreSQL URL or service is missing.

## Milestone task

Specify the PostgreSQL integration profile contract for the notes API.

## Rubric

- Correctness: profile covers migrations, repository, engine, and readiness.
- Testing: required settings and commands are asserted.
- Maintainability: provider-real tests are isolated from fast unit tests.
- Design: PostgreSQL is not treated as a SQLite string replacement.
- Production readiness: CI can run the profile intentionally.

## Complexity

Provider-real tests are slower and more operationally demanding. That is why they should be named, marked, and scoped. They do not replace unit tests; they prove the places where SQLite cannot speak for PostgreSQL.

The profile should also make failure modes obvious. Missing service, bad URL, failed migration, and readiness failure should each produce actionable test output.

The key discipline is scope control. Provider-real tests should prove the boundaries SQLite cannot prove, while ordinary service logic should remain covered by faster tests.

## Stretch goals

- Add Docker Compose service configuration.
- Add Testcontainers-style orchestration later.
- Add PostgreSQL JSON/index behavior tests.
