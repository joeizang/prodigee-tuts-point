# Command runner composition

## Scenario

The project has all the pieces for adding a note, but they are still separate. This milestone composes them into one workflow that accepts add-command arguments, writes a note to a JSON file, and returns a deterministic message.

## Requirements

- Parse add-command arguments.
- Normalize the title.
- Parse comma-separated tags.
- Build a JSON-safe note record.
- Load existing notes when the file exists.
- Treat a missing notes file as an empty note list.
- Append the new record after validation succeeds.
- Save notes as deterministic UTF-8 JSON.
- Return `Added note: <normalized-title>`.

## CLI/API contract

The milestone contract is one Python function:

```python
def run_add_command(args: list[str], notes_path: Path) -> str:
    ...
```

The function receives already-split command arguments and a storage path. It returns the message a CLI adapter can print later.

## Milestone task

Implement `run_add_command` and any helper functions you need. Keep the workflow testable: no `sys.argv`, no `print`, and no hardcoded storage path.

## Rubric

- Correctness: creates a notes file, appends to existing notes, normalizes title/tags, and returns the expected message.
- Design: composes parsing, normalization, record-building, loading, and saving in a readable order.
- Validation: rejects invalid command arguments and invalid tags before writing.
- Testing: verifies returned output and persisted JSON state.
- Complexity: uses direct composition and JSON file storage rather than introducing a framework, database, or subprocess.
- Maintainability: keeps the runner reusable by a future executable wrapper and a future FastAPI route.

## Stretch goals

- Split helpers into modules once the exercise environment supports multi-file Python workspaces.
- Add an id or created-at field and decide how deterministic tests should handle time.
- Add a `run_list_command` workflow that reuses `load_notes` and `render_note_list`.
