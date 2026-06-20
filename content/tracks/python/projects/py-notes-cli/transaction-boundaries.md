# Transaction Boundaries

## Scenario

The API now has persistence. Real write operations often need more than one database change. This milestone introduces atomic note and audit writes.

## Requirements

- Create notes and audit events in one transaction.
- Commit only after all writes succeed.
- Roll back when the second write fails.
- Test success and forced failure.
- Keep transaction control in the persistence boundary.

## CLI/API contract

The API should never expose a partially completed write. If audit fails, the note should not appear.

## Milestone task

Implement transactional note creation with an audit table.

## Rubric

- Correctness: note and audit rows are written together.
- Testing: rollback is proven with a forced failure.
- Maintainability: transaction logic stays out of route handlers.
- Design: persistence operations own commit and rollback.
- Production readiness: future multi-table writes have a model.

## Complexity

The failure path is the feature. Success without rollback coverage is not enough.

Keep transaction ownership explicit. If route handlers begin opening transactions, the API layer becomes coupled to SQL. If repositories commit too early, service operations can leave half-finished state behind. The repository should expose one operation that succeeds or fails as a unit.

Audit rows make the lesson concrete because they create a second write that must remain consistent with the note write. That same shape appears later in production systems as outbox records, counters, permissions, and integration events.

## Stretch goals

- Add transaction context helpers.
- Add update/delete audit events.
- Add service-level transaction orchestration.
