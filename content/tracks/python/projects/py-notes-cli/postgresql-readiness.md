# PostgreSQL Readiness

## Scenario

SQLite is excellent for learning and local persistence, but production systems often use PostgreSQL. This milestone prepares the app for that move before a database service is introduced.

## Requirements

- Validate SQLite and PostgreSQL database URLs.
- Require PostgreSQL URLs for production.
- Identify SQLite-specific SQL and schema assumptions.
- Preserve repository/session boundaries.
- Define future integration-test expectations.

## CLI/API contract

Public API behavior should stay stable when the database provider changes. The configuration and persistence adapter change; clients should not.

## Milestone task

Build PostgreSQL readiness checks for settings and portability.

## Rubric

- Correctness: database URLs are parsed and validated.
- Testing: production, development, invalid URL, and portability cases are covered.
- Maintainability: provider-specific details stay under persistence boundaries.
- Design: SQLite assumptions are named before they cause migration bugs.
- Production readiness: the app has a clear path to real PostgreSQL integration tests.

## Complexity

Provider readiness is mostly about refusing false confidence. SQLite tests are useful, but they do not prove PostgreSQL locking, type behavior, JSON behavior, indexing, or migration behavior. A mature project names those gaps and adds the right tests before deployment depends on them.

This milestone is deliberately earlier than the real PostgreSQL adapter. Learners should first recognize where provider assumptions leak into settings, schema design, raw SQL, test fixtures, and repository code. Once those seams are named, adding SQLAlchemy engines, PostgreSQL containers, and integration tests becomes a controlled extension instead of a rewrite.

## Stretch goals

- Add a docker-compose PostgreSQL test profile.
- Add SQLAlchemy engine settings for PostgreSQL.
- Add provider-specific migration checks.
