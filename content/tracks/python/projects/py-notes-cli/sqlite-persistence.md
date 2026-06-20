# SQLite Persistence

## Scenario

The notes app has outgrown JSON files. SQLite gives the project durable local storage, queryable records, and database constraints while staying simple enough for local development.

## Requirements

- Initialize the database schema.
- Use parameterized SQL for all user data.
- Map rows into the same note shape used by the service.
- Support list, get, add, update, and delete operations.
- Preserve duplicate-title behavior through a database constraint or explicit check.
- Test with isolated temporary database files.

## CLI/API contract

The repository implementation changes, but the service and FastAPI route contract should stay stable. A note is still represented as title, body, and tags.

## Milestone task

Implement `SqliteNoteRepository` behind the existing repository boundary.

## Rubric

- Correctness: repository operations persist and retrieve the expected notes.
- Testing: isolated SQLite files cover create, read, update, delete, and duplicate behavior.
- Maintainability: SQL stays inside the repository implementation.
- Design: SQLite rows are mapped before they reach the service or route handlers.
- Security: all user values use parameterized queries.

## Complexity

SQLite introduces schema and query thinking. Keep the first schema intentionally small so the learner can focus on connection handling, constraints, parameters, and row mapping.

This milestone deliberately avoids an ORM. The goal is to understand the database boundary first: what SQL runs, where parameters go, when rows are mapped, and how the repository preserves the service-facing note shape.

## Stretch goals

- Add a `note_tags` table instead of JSON-encoded tags.
- Add migrations after the first schema change.
- Add pagination and indexed search.
