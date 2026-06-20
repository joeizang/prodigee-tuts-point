# Transaction Boundaries

## Learning objectives

- Explain why multi-step writes need transactions.
- Use SQLite commit and rollback behavior deliberately.
- Keep audit and note writes atomic.
- Test rollback instead of assuming it works.
- Keep transaction control inside persistence boundaries.

## Prerequisites

You should understand SQLite repositories, services, and tests with temporary database files.

## Mental model

**Term: transaction** means a group of database changes that succeed or fail together.

**Term: atomic** means no partial result is visible. Either the note and audit record both exist, or neither exists.

**Term: rollback** means undoing changes made in a failed transaction.

## Core idea

When one use case writes multiple tables, wrap the work:

```python
try:
    connection.execute("BEGIN")
    insert_note(connection, note)
    insert_audit(connection, "created", note["title"])
    connection.commit()
except Exception:
    connection.rollback()
    raise
```

Do not rely on hope for atomicity.

## Worked example

A failure after the first write should leave no note:

```python
with pytest.raises(RuntimeError):
    repository.create_note_with_audit(note, fail_after_note=True)

assert repository.list_notes() == []
assert repository.list_audit_events() == []
```

That test is the evidence.

## Production transfer

Real APIs often write more than one thing: a record, an audit event, a queue message, a counter, or a relationship. Transactions make those operations predictable.

## Common mistakes

- Committing after the first insert.
- Auditing outside the transaction.
- Catching errors without rollback.
- Mixing transaction ownership between service and repository.
- Testing only success paths.

## Testing strategy

Test success and forced failure. The failure test matters more because it proves rollback.

## Debugging strategy

If rollback fails, check where commit happens. Any commit before all writes succeed can make rollback useless.

## Exercise connection

`TransactionalNoteWrites` asks you to write note and audit rows atomically.

## Project connection

This milestone prepares the notes API for production workflows where persistence side effects must stay consistent.

## Check yourself

- What belongs in one transaction?
- Where should rollback happen?
- Why is a forced-failure test valuable?
- Should routes manage SQL transactions directly?

## Source reference notes

- Python `sqlite3` docs anchor transaction behavior.
- `contextlib` docs anchor explicit resource boundaries.
- pytest temp storage docs anchor rollback tests.
