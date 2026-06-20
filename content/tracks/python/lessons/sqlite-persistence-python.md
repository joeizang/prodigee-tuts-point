# SQLite Persistence with sqlite3

## Learning objectives

- Create a SQLite schema from Python.
- Use parameterized queries instead of string interpolation.
- Map SQLite rows into note dictionaries.
- Store tags deterministically without changing the service contract.
- Keep SQLite behind the repository boundary.

## Prerequisites

You should understand repository boundaries and FastAPI dependency injection. This lesson replaces the JSON repository implementation, not the service or route contract.

## Mental model

**Term: SQLite** means an embedded relational database stored in a local file.

**Term: schema** means the tables, columns, constraints, and indexes the database uses.

**Term: parameterized query** means SQL where values are passed separately from the SQL text. This prevents injection bugs and quoting mistakes.

**Term: row mapping** means converting database rows into application dictionaries or models.

## Core idea

The repository initializes its own schema:

```python
def initialize(self) -> None:
    with sqlite3.connect(self.path) as connection:
        connection.execute(
            """
            CREATE TABLE IF NOT EXISTS notes (
                title TEXT PRIMARY KEY,
                body TEXT NOT NULL,
                tags TEXT NOT NULL
            )
            """
        )
```

Every query uses placeholders:

```python
connection.execute(
    "INSERT INTO notes (title, body, tags) VALUES (?, ?, ?)",
    (note["title"], note["body"], json.dumps(note["tags"])),
)
```

## Worked example

Row mapping should stay deterministic:

```python
def map_row(row: sqlite3.Row) -> dict[str, object]:
    return {
        "title": row["title"],
        "body": row["body"],
        "tags": json.loads(row["tags"]),
    }
```

The service still sees the same note shape it saw with JSON storage.

## Production transfer

SQLite is a strong local persistence step because it introduces real database concerns without requiring a server. You learn schema creation, constraints, transactions, and parameterized SQL in a controlled setting.

Later, SQLAlchemy or another database layer can replace raw `sqlite3`, but the repository boundary should remain. The service should not care how notes are stored.

## Common mistakes

- Building SQL with f-strings and user input.
- Forgetting to initialize the schema before queries.
- Returning SQLite rows directly to FastAPI.
- Committing after each helper accidentally instead of using connection context managers.
- Storing Python lists directly instead of serializing tags.
- Changing service behavior while swapping persistence.

## Testing strategy

Use a temporary database file:

```python
repository = SqliteNoteRepository(tmp_path / "notes.db")
repository.initialize()
repository.add_note({"title": "python", "body": "body", "tags": ["api"]})

assert repository.get_note("python") == {"title": "python", "body": "body", "tags": ["api"]}
```

Then run the same service tests against the SQLite repository that you previously ran against the JSON repository.

## Debugging strategy

When SQLite behavior fails:

- Check schema creation first.
- Check parameter values separately from SQL text.
- Query the rows before mapping.
- Check JSON serialization of tags.
- Confirm the repository returns the same note shape as JSON storage.

## Exercise connection

`SqliteNoteRepository` asks you to implement schema initialization, add/get/list/update/delete, duplicate detection, and deterministic row mapping.

## Project connection

After this milestone, `py-notes` has a real database implementation behind the same repository boundary. The FastAPI layer should not need to change.

## Check yourself

- Why does SQLite belong behind the repository?
- Why should tags be serialized deterministically?
- What problem do parameterized queries solve?
- What should still be true if the JSON repository and SQLite repository are swapped?

## Source reference notes

- Python `sqlite3` documentation anchors connections, parameters, and rows.
- Python resource-management material anchors context managers.
- pytest temporary directory docs anchor isolated database tests.
