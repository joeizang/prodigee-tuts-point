# Note storage

## Scenario

`py-notes-cli` now has pure helpers for title normalization and tag parsing. The next step is a storage boundary: notes must become JSON-safe records, and existing notes must be loaded from a UTF-8 JSON file without trusting malformed data.

## Requirements

- Build note records with `title`, `body`, and `tags` keys.
- Reject blank titles and non-text bodies.
- Copy tag lists so callers cannot mutate stored records by accident.
- Read notes from a `Path` using UTF-8.
- Decode JSON text into Python data.
- Require the file to contain a list of note objects.
- Reject malformed note records with `ValueError`.

## CLI/API contract

This milestone still avoids command parsing. The contracts are Python functions:

```python
def build_note_record(title: str, body: str, tags: list[str]) -> dict[str, object]:
    ...

def load_notes(path: Path) -> list[dict[str, object]]:
    ...
```

The first function builds trusted application data. The second crosses the file and JSON boundary, then validates what came back.

## Milestone task

Implement both storage exercises. Then explain how these functions could be reused by a future CLI command and a future FastAPI route without duplicating validation rules.

## Rubric

- Correctness: produces the expected record shape and validates loaded JSON records.
- Design: keeps file I/O at the storage boundary and returns Python data structures.
- Validation: rejects blank titles, non-text bodies, invalid tags, non-list files, and malformed records.
- Testing: uses visible examples but implements the hidden failure paths too.
- Complexity: uses direct `Path`, `json`, dictionary, and list operations instead of a database, framework, or broad abstraction.
- Maintainability: keeps storage helpers small enough for later CLI and FastAPI adapters to call directly.

## Stretch goals

- Add a `dump_notes` helper with deterministic `json.dumps(..., indent=2, sort_keys=True)`.
- Add a schema version field and decide where migration logic should live.
- Compare file-based storage with a future SQLite persistence boundary.
