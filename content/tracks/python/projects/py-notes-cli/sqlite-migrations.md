# SQLite Migrations

## Scenario

The SQLite schema will change as the project grows. Manual table edits are not a production strategy. The app needs versioned migrations and startup verification.

## Requirements

- Create a schema version table.
- Apply migrations in order.
- Support fresh databases and existing older databases.
- Make migration execution idempotent.
- Verify that the expected version is present at startup.

## CLI/API contract

Migrations are a startup concern. Clients should never see a partially migrated database through route behavior.

## Milestone task

Implement a small SQLite migration runner with two schema versions and verification.

## Rubric

- Correctness: fresh and old databases reach the target schema.
- Testing: idempotent reruns and old-database upgrades are covered.
- Maintainability: schema changes are centralized as migrations.
- Design: migrations run transactionally before normal repository use.
- Production readiness: startup checks fail fast on unexpected versions.

## Complexity

Migrations are risky because they touch existing data. A migration runner must not assume every database is empty. It must handle old state deliberately, preserve records, and leave a clear version marker. Testing both fresh and pre-existing databases is mandatory.

The startup check is as important as the migration itself. Code and schema must agree before the app accepts requests, otherwise failures arrive as confusing runtime bugs.

Keep migrations small, named by version, reviewed carefully, documented explicitly, and tested against real SQLite files.

## Stretch goals

- Add migration filenames.
- Add downgrade policy documentation.
- Add backups before destructive migrations.
