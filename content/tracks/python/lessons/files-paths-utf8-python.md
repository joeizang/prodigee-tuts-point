# Files, Paths, and UTF-8 Text

## Learning objectives

- Use `pathlib.Path` to name files without hardcoding platform-specific separators.
- Read and write text with an explicit `encoding="utf-8"`.
- Separate file I/O from pure transformation functions.
- Use pytest `tmp_path` to test file behavior without touching real project files.
- Explain why storage boundaries are where many beginner bugs become production bugs.

## Prerequisites

You should understand Python functions, strings, lists, dictionaries at a basic level, and pytest assertions. You do not need prior file I/O experience. This lesson intentionally treats file access as a boundary, not as a magic side effect to sprinkle throughout the program.

## Mental model

**Term: path** means the location of a file or directory. In Python, prefer `Path` objects from `pathlib` instead of manually joining strings.

**Term: file boundary** means the line where your program crosses from memory into the filesystem. Code inside the boundary can fail for reasons unrelated to your business rule: missing files, permissions, invalid JSON, or unexpected encodings.

**Term: UTF-8** means the text encoding this project uses for notes. Explicit encodings make behavior predictable across operating systems.

**Term: pure core** means functions that receive values and return values without reading files, printing, prompting, or mutating global state.

## Core idea

`Path` gives you a small object for filesystem locations:

```python
from pathlib import Path

notes_path = Path("notes.json")
```

Reading text should name the encoding:

```python
raw_text = notes_path.read_text(encoding="utf-8")
```

Writing text should do the same:

```python
notes_path.write_text("hello\n", encoding="utf-8")
```

This looks simple, but the design decision is larger: do not hide file reads inside every useful function. Keep file access at the edge and pass normal Python values into the pure core.

## Worked example

Suppose you have a pure function that validates a note title:

```python
def require_nonblank_title(title: str) -> str:
    normalized = " ".join(title.split()).lower()
    if not normalized:
        raise ValueError("title is required")

    return normalized
```

That function is easy to test. A file boundary can wrap it:

```python
from pathlib import Path


def load_title(path: Path) -> str:
    raw_title = path.read_text(encoding="utf-8")
    return require_nonblank_title(raw_title)
```

Now the responsibilities are visible:

- `load_title` handles the filesystem.
- `require_nonblank_title` handles the product rule.

When a test fails, you can tell which layer is wrong.

## Production transfer

The same separation matters in FastAPI. A route handler is an I/O boundary because it receives outside data and returns an HTTP response. If route handlers directly contain all parsing, validation, storage, and formatting logic, every test becomes large and brittle. If route handlers call small pure functions, the application remains testable as it grows.

File I/O also teaches the operational mindset you will need for databases later. A database call is another boundary: useful, necessary, and worth isolating from pure rules.

## Common mistakes

- Building paths with string concatenation such as `"data/" + filename`.
- Relying on the operating system's default encoding instead of passing `encoding="utf-8"`.
- Reading a file inside a function that should only transform data.
- Writing tests that modify real files in the repository.
- Catching every exception too early and hiding useful failure information from tests.

## Testing strategy

Use pytest `tmp_path` for file tests:

```python
from pathlib import Path


def test_reads_utf8_text(tmp_path: Path) -> None:
    path = tmp_path / "note.txt"
    path.write_text("café", encoding="utf-8")

    assert path.read_text(encoding="utf-8") == "café"
```

`tmp_path` gives every test its own temporary directory. That keeps tests isolated and repeatable.

## Debugging strategy

When file tests fail, print or inspect:

```python
print(path)
print(path.exists())
print(repr(path.read_text(encoding="utf-8")))
```

Use `repr` for text because it reveals newlines and whitespace. Remove temporary prints after the test captures the behavior.

## Exercise connection

The storage milestone includes `LoadNotesFile`, which reads a UTF-8 JSON file through a `Path`. The exercise expects you to treat file reading as the boundary and validation as explicit Python logic.

## Project connection

`py-notes-cli` will need a real notes file. This lesson prevents the project from turning into functions that secretly read and write files from everywhere. The project should have a small storage boundary that later CLI and FastAPI adapters can share.

## Check yourself

1. Why is `Path` better than string concatenation for file locations?
2. Why should this project pass `encoding="utf-8"` explicitly?
3. What does `tmp_path` protect your tests from?
4. Why should file I/O wrap pure functions instead of replacing them?

## Source reference notes

Use the Python standard library documentation for `pathlib` and text file APIs. Use pytest documentation for `tmp_path`. The project applies those APIs to a notes storage boundary.
