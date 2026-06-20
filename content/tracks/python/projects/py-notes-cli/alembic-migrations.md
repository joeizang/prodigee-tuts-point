# Alembic Migrations

## Scenario

The custom SQLite migration runner taught migration fundamentals. The production Python ecosystem commonly uses Alembic with SQLAlchemy metadata, so the track now introduces revision planning, review, and test discipline.

## Requirements

- Generate revision metadata.
- Include upgrade and downgrade operations.
- Record review notes for risk.
- Avoid destructive changes without explicit review.
- Define tests that prove fresh and upgraded database behavior.

## CLI/API contract

Migrations are deployment concerns. API clients should never observe half-applied schema changes.

## Milestone task

Create an Alembic-style revision plan for adding note archiving and search metadata.

## Rubric

- Correctness: revision includes id, parent, upgrade, and downgrade.
- Testing: migration test expectations cover upgrade, downgrade, and data preservation.
- Maintainability: schema changes are reviewed as source code.
- Design: migration operations are explicit and reversible where practical.
- Production readiness: risky operations are flagged before deployment.

## Complexity

Alembic autogenerate is useful, but it is not a substitute for review. The generated file may choose a type incorrectly, miss a data backfill, or produce operations that are unsafe on a large table. Migration literacy means reading and testing the revision, not just running a command.

This milestone also separates schema intent from tool mechanics. A professional migration plan explains why the change exists, what data can be affected, how rollback behaves, and which checks prove the revision is safe enough to deploy. The exercise keeps that reasoning explicit before introducing a real Alembic dependency.

## Stretch goals

- Add real Alembic revision files after dependencies are installed.
- Add data backfill planning.
- Add migration ordering checks in CI.
