# Command Runner Composition

## Learning objectives

- Compose small tested helpers into one application workflow.
- Keep command runners thin enough to understand and test.
- Preserve clear boundaries between parsing, validation, storage, and output.
- Read and write JSON notes through a `Path` without hiding file effects.
- Explain why composition is the final step before a real CLI executable or FastAPI route.

## Prerequisites

You should understand the previous `py-notes-cli` slices: title normalization, tag parsing, note-record construction, JSON file loading, command parsing, and list rendering. This lesson assumes those concepts exist as small functions and focuses on how to wire them together.

## Mental model

**Term: composition** means building a larger behavior by calling smaller behaviors in a deliberate order. Good composition does not erase the boundaries you worked to create.

**Term: command runner** means the application function that coordinates one command. It is not the parser alone, not the storage layer alone, and not the renderer alone. It is the place where those pieces are sequenced.

**Term: workflow** means the ordered steps needed to complete a user action. For an add command, the workflow is parse, normalize, validate, load, append, save, and report.

**Term: thin adapter** means outside-world code that does very little: receive data, call a runner, and present the result. A future executable and a future FastAPI route should both stay thin.

## Core idea

The command runner should read like the business process:

```python
def run_add_command(args: list[str], notes_path: Path) -> str:
    request = parse_add_command(args)
    title = normalize_title(str(request["title"]))
    tags = parse_tags(str(request["tags"]))
    record = build_note_record(title, str(request["body"]), tags)
    notes = load_notes_or_empty(notes_path)
    notes.append(record)
    save_notes(notes_path, notes)
    return f"Added note: {title}"
```

Each line should be easy to explain because each helper owns one concept. The runner coordinates; it should not become a dumping ground.

## Worked example

The file boundary needs a small missing-file rule. A first run may not have a notes file yet:

```python
from pathlib import Path


def load_notes_or_empty(path: Path) -> list[dict[str, object]]:
    if not path.exists():
        return []

    return load_notes(path)
```

Saving should be deterministic:

```python
import json
from pathlib import Path


def save_notes(path: Path, notes: list[dict[str, object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(notes, indent=2, sort_keys=True) + "\n", encoding="utf-8")
```

The newline at the end is intentional. Many command-line tools write text files with a trailing newline because it keeps diffs and terminal output clean.

## Production transfer

FastAPI route handlers will use the same composition style. A route might receive a Pydantic request model, call `run_add_command`-like core behavior, and return a response model. If the runner is already thin and explicit, the FastAPI layer does not need to duplicate validation and storage logic.

This is the bridge from beginner Python into professional application structure. You are no longer writing disconnected functions; you are arranging small boundaries into a workflow.

## Common mistakes

- Reimplementing parsing, title normalization, and tag parsing inside the runner instead of calling helpers.
- Writing to the file before all validation has succeeded.
- Returning a Python dictionary when the CLI contract expects a user-facing message.
- Letting missing files crash when the first add command should create storage.
- Swallowing JSON errors and silently overwriting corrupt data.
- Hiding too much in one giant function so tests can no longer explain the failure.

## Testing strategy

Use `tmp_path` to test the workflow:

```python
from pathlib import Path


def test_add_command_creates_notes_file(tmp_path: Path) -> None:
    path = tmp_path / "notes.json"

    message = run_add_command(
        ["--title", " Learn Python ", "--body", "Practice", "--tags", "python,cli"],
        path,
    )

    assert message == "Added note: learn python"
    assert path.exists()
```

Then read the JSON file and assert the stored record. Workflow tests should verify both the returned message and the persisted state.

## Debugging strategy

If a workflow test fails, locate the failed boundary:

- Did parsing produce the expected request?
- Did normalization produce the expected title?
- Did loading return the expected existing notes?
- Did saving write the expected JSON?
- Did the returned message match the contract?

Avoid fixing by changing everything at once. Composition bugs become clear when you inspect the value between steps.

## Exercise connection

`RunAddCommand` asks you to implement the add-command workflow in one file. The visible tests cover creating a new notes file. Hidden tests cover appending to existing notes, missing required command data, invalid tags, and deterministic JSON writes.

## Project connection

After this milestone, `py-notes-cli` has a real application workflow. The next milestone can add search/list command composition or introduce an executable wrapper. FastAPI is now close because there is meaningful application logic for routes to call.

## Check yourself

1. Why should the runner call helpers instead of duplicating their logic?
2. Why should validation happen before writing the file?
3. What should happen when the notes file does not exist yet?
4. How would a future FastAPI route reuse this runner style?

## Source reference notes

Use the Python tutorial for module organization and function composition, the standard library JSON documentation for deterministic storage, and pytest `tmp_path` documentation for workflow tests that write files safely.
