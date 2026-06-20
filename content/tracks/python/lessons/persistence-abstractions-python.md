# Persistence Abstractions

## Learning objectives

- Explain why services should depend on a persistence contract instead of a file path.
- Define a small repository boundary for notes.
- Keep JSON storage behind the repository implementation.
- Test service behavior with a fake repository.
- Prepare the codebase for SQLite without rewriting FastAPI handlers.

## Prerequisites

You should understand the notes service, FastAPI dependencies, and JSON storage from earlier milestones. This lesson is about changing the direction of dependency: the service should ask for note storage, not build storage itself.

## Mental model

**Term: repository** means an object that owns persistence operations for one kind of data. A note repository can list, add, update, and delete notes.

**Term: abstraction** means a contract that hides implementation details. The service should care that a repository can `add(note)`, not whether the data lives in JSON, SQLite, or memory.

**Term: protocol** means a structural type contract in Python. If an object has the right methods, type checkers can treat it as satisfying the protocol.

## Core idea

The service should receive the repository:

```python
class NotesService:
    def __init__(self, repository: NoteRepository) -> None:
        self.repository = repository
```

Then service methods express application behavior:

```python
def create_note(self, title: str, body: str, tags: list[str]) -> dict[str, object]:
    note = build_note(title, body, tags)
    if self.repository.get(note["title"]) is not None:
        raise ValueError(f"note already exists: {note['title']}")
    self.repository.add(note)
    return note
```

The repository owns storage. The service owns application rules.

## Worked example

A minimal protocol can stay small:

```python
from typing import Protocol


class NoteRepository(Protocol):
    def list_notes(self) -> list[dict[str, object]]: ...
    def get_note(self, title: str) -> dict[str, object] | None: ...
    def add_note(self, note: dict[str, object]) -> None: ...
```

A JSON implementation can satisfy that protocol:

```python
class JsonNoteRepository:
    def __init__(self, path: Path) -> None:
        self.path = path

    def list_notes(self) -> list[dict[str, object]]:
        return load_notes_or_empty(self.path)
```

Nothing in the service needs to know that `Path` exists.

## Production transfer

This is the moment where the application stops being "a FastAPI app that writes files" and becomes "an application with an HTTP adapter and a persistence adapter." That distinction is what makes later SQLite work controlled instead of invasive.

The route handler keeps depending on `NotesService`. The dependency provider can decide whether that service uses JSON or SQLite.

## Common mistakes

- Making the repository know HTTP status codes.
- Making the service know file paths or SQL strings.
- Creating an abstraction with too many methods before the use cases require them.
- Returning raw storage rows that route handlers must understand.
- Testing only the JSON repository and never testing service behavior with a fake.

## Testing strategy

Use a fake repository for service tests:

```python
class FakeRepository:
    def __init__(self) -> None:
        self.notes: list[dict[str, object]] = []

    def list_notes(self) -> list[dict[str, object]]:
        return list(self.notes)
```

This test does not touch the filesystem. It proves application behavior. Separate tests can prove the JSON repository persists correctly.

## Debugging strategy

When a persistence abstraction test fails, first identify the layer:

- Service failure: fake repository state is wrong.
- Repository failure: file or database state is wrong.
- Dependency failure: FastAPI is constructing the wrong concrete repository.
- Contract failure: an implementation is missing a repository method.

Do not fix a service bug by editing storage code unless the storage code broke the contract.

## Exercise connection

`AbstractNoteRepository` asks you to define a repository protocol, implement a JSON repository, and use it from a notes service. Hidden tests swap in a fake repository to prove the service is storage-agnostic.

## Project connection

This is the required step before SQLite. Without it, adding SQLite would force route handlers, tests, and service logic to change at once.

## Check yourself

- Which layer should know about `Path`?
- Which layer should know about duplicate-title rules?
- Why is a fake repository useful?
- What should stay unchanged when JSON is replaced with SQLite?

## Source reference notes

- Python class material anchors small collaborating objects.
- Python typing documentation anchors `Protocol`.
- pytest temporary-file guidance anchors repository implementation tests.
