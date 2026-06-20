# Update, List, Delete, and Search Commands

## Learning objectives

- Add multiple command workflows without turning the runner into an untested script.
- Separate **query commands** from **mutation commands**.
- Preserve deterministic storage after updates and deletes.
- Return stable, testable CLI text for list and search output.
- Explain why these operations should become reusable service functions before FastAPI arrives.

## Prerequisites

You should already have a working add-command runner, JSON storage helpers, note records shaped as dictionaries, and a deterministic list renderer. This lesson assumes that a note has `title`, `body`, and `tags` fields.

## Mental model

**Term: command dispatch** means choosing the correct workflow from the first command token. In `["search", "--tag", "python"]`, the dispatcher chooses the search workflow and passes the rest of the arguments to that workflow.

**Term: query command** means a command that reads data but does not change it. `list` and `search` are query commands.

**Term: mutation command** means a command that changes stored data. `update` and `delete` are mutation commands.

**Term: service boundary** means the reusable application operation underneath the CLI. `update_note(notes, title, body)` should not know whether the caller was a command line or an HTTP request.

## Core idea

Do not make every command parse, load, mutate, save, and render in one large block. Keep the runner shallow:

```python
def run_note_command(args: list[str], notes_path: Path) -> str:
    if not args:
        raise ValueError("command is required")

    command = args[0]
    if command == "list":
        return render_note_list(load_notes_or_empty(notes_path))
    if command == "search":
        return run_search(args[1:], notes_path)
    if command == "update":
        return run_update(args[1:], notes_path)
    if command == "delete":
        return run_delete(args[1:], notes_path)

    raise ValueError(f"unknown command: {command}")
```

The runner dispatches. Smaller functions handle each command. Service helpers do the real data work.

## Worked example

Searching by tag is a query. It should not write the notes file:

```python
def search_notes_by_tag(notes: list[dict[str, object]], tag: str) -> list[dict[str, object]]:
    wanted = tag.strip().lower()
    if not wanted:
        raise ValueError("--tag is required")

    return [
        note
        for note in notes
        if wanted in note["tags"]
    ]
```

Updating is a mutation. It should fail if the note does not exist:

```python
def update_note_body(notes: list[dict[str, object]], title: str, body: str) -> bool:
    for note in notes:
        if note["title"] == title:
            note["body"] = body.strip()
            return True
    return False
```

The command workflow can then decide whether to save or raise:

```python
if not update_note_body(notes, title, body):
    raise ValueError(f"note not found: {title}")
save_notes(notes_path, notes)
```

## Production transfer

This is the last major CLI shape before FastAPI. If your list, search, update, and delete behaviors are clean service functions, HTTP handlers can call the same operations. If the logic stays trapped inside command parsing, the API layer will either duplicate it or reach awkwardly into CLI code.

The professional move is to make the CLI an adapter, not the owner of the application rules.

## Common mistakes

- Saving during `list` or `search`.
- Treating titles case-sensitively after earlier lessons normalized titles.
- Returning different text depending on dictionary insertion accidents.
- Silently doing nothing when `update` or `delete` cannot find a note.
- Duplicating JSON load/save logic inside each command.
- Making FastAPI call `run_note_command(["update", ...])` instead of calling the shared service boundary.

## Testing strategy

Use workflow tests for commands and smaller tests for service helpers. A command test should verify both output and file state:

```python
def test_delete_removes_note_and_saves(tmp_path: Path) -> None:
    path = tmp_path / "notes.json"
    save_notes(path, [{"title": "old", "body": "body", "tags": []}])

    message = run_note_command(["delete", "--title", "old"], path)

    assert message == "Deleted note: old"
    assert load_notes_or_empty(path) == []
```

For search, assert that the file text is unchanged when the command completes.

## Debugging strategy

When a multi-command runner fails, first identify the command class:

- Dispatch problem: the first token is not recognized.
- Parse problem: a required option is missing.
- Query problem: loaded notes are filtered incorrectly.
- Mutation problem: the in-memory notes list is wrong before saving.
- Persistence problem: the saved JSON does not match the expected state.

Inspect values between those boundaries. Avoid adding print statements inside every helper; targeted assertions usually reveal the broken boundary faster.

## Exercise connection

`RunNoteCommand` asks you to implement `list`, `search`, `update`, and `delete` over a JSON file. The tests check command output, persisted state, missing-record failures, and that query commands do not rewrite the file.

## Project connection

After this milestone, `py-notes-cli` is no longer a one-command toy. It has enough behavior to justify extracting a service layer that a CLI and an API can both use.

## Check yourself

- Which commands should save the file?
- Why should `search` return an empty-list message instead of raising when no notes match?
- Where should missing-title validation live?
- Why is calling service helpers better than making FastAPI call command strings?

## Source reference notes

- The Python tutorial's data-structure material anchors list filtering and dictionary updates.
- The standard library `json` documentation anchors stable persisted state.
- pytest `tmp_path` guidance anchors isolated workflow tests.
