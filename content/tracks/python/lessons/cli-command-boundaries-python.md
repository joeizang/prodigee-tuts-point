# CLI Command Boundaries

## Learning objectives

- Treat command-line arguments as outside data.
- Convert raw argument lists into validated request dictionaries.
- Render deterministic command output from trusted note records.
- Keep parsing and rendering pure enough to test without `input`, `print`, or real files.
- Explain how a CLI boundary differs from the reusable application core.

## Prerequisites

You should understand Python functions, lists, dictionaries, strings, `ValueError`, and pytest assertions. You should also understand the earlier storage boundary: file data becomes trusted only after decoding and validation.

## Mental model

**Term: adapter** means code that connects the outside world to your application core. A CLI adapter receives command-line arguments. A FastAPI adapter receives HTTP requests. Both should translate outside data into a small request shape.

**Term: command boundary** means the point where raw command text becomes application data. For `py-notes-cli`, that means turning strings such as `["add", "--title", "Learn Python"]` into a dictionary the core can use.

**Term: rendering** means turning trusted application data into user-facing text. Rendering is the opposite direction from parsing.

**Term: deterministic output** means the same records always produce the same lines. Deterministic output makes CLI tools scriptable and tests reliable.

## Core idea

A beginner CLI often starts by reading `sys.argv` everywhere and printing from deep inside the program. That works briefly, then becomes hard to test.

Prefer a pure boundary first:

```python
def parse_add_command(args: list[str]) -> dict[str, object]:
    ...
```

That function receives arguments without the program name. It returns a request dictionary and raises `ValueError` for invalid command data.

Later, a real executable can call it:

```python
import sys

request = parse_add_command(sys.argv[1:])
```

The parsing rule stays testable because the function does not read `sys.argv` directly.

## Worked example

The simplest parser can walk the list manually:

```python
def parse_add_command(args: list[str]) -> dict[str, object]:
    title: str | None = None
    body: str | None = None
    tags = ""

    index = 0
    while index < len(args):
        option = args[index]

        if option == "--title":
            index += 1
            if index >= len(args):
                raise ValueError("--title requires a value")
            title = args[index]
        elif option == "--body":
            index += 1
            if index >= len(args):
                raise ValueError("--body requires a value")
            body = args[index]
        elif option == "--tags":
            index += 1
            if index >= len(args):
                raise ValueError("--tags requires a value")
            tags = args[index]
        else:
            raise ValueError(f"unknown option: {option}")

        index += 1

    if title is None:
        raise ValueError("--title is required")
    if body is None:
        raise ValueError("--body is required")

    return {"title": title, "body": body, "tags": tags}
```

This is not meant to replace `argparse` forever. It is meant to make the boundary visible before adding a standard-library parser. Once you understand the shape, `argparse` becomes a tool rather than magic.

## Rendering output

Rendering should also be testable:

```python
def render_note_list(notes: list[dict[str, object]]) -> str:
    lines: list[str] = []
    for index, note in enumerate(notes, start=1):
        title = note["title"]
        tags = ", ".join(note["tags"])
        lines.append(f"{index}. {title} [{tags}]")

    return "\n".join(lines)
```

The function returns a string. It does not print. A future CLI adapter can print the returned string at the outer edge.

## Production transfer

FastAPI route handlers are adapters too. A route should translate HTTP details into request data, call tested core functions, and translate the result into a response. If the core already accepts dictionaries or typed request values, the FastAPI slice can reuse it instead of duplicating CLI logic.

This is why the CLI milestone comes before FastAPI. It teaches the adapter/core separation in a smaller environment.

## Common mistakes

- Reading `sys.argv` inside functions that should be testable.
- Calling `print` inside rendering helpers instead of returning a string.
- Letting unknown options pass silently.
- Treating missing option values as blank strings instead of errors.
- Mixing file writes into command parsing.
- Making output depend on dictionary iteration order when the display order should be explicit.

## Testing strategy

Test command boundaries by passing lists directly:

```python
def test_parses_add_command() -> None:
    assert parse_add_command(["--title", "Learn", "--body", "Practice"]) == {
        "title": "Learn",
        "body": "Practice",
        "tags": "",
    }
```

Test rendering by comparing exact strings:

```python
def test_renders_notes() -> None:
    assert render_note_list([{"title": "learn python", "tags": ["python"]}]) == "1. learn python [python]"
```

Exact output tests are useful for CLI tools because users and scripts rely on stable text.

## Debugging strategy

For parsers, inspect the current index and option:

```python
print(index, repr(args[index]))
```

For rendering, inspect the list of lines before joining:

```python
print(repr(lines))
```

Remove temporary prints once tests document the behavior.

## Exercise connection

`ParseAddCommand` handles raw `add` command options. `RenderNoteList` formats trusted records for list output. Both exercises keep the boundary pure so later executable code can stay thin.

## Project connection

After this milestone, `py-notes-cli` has enough pieces for a real command adapter:

- normalize title
- parse tags
- build note records
- load notes from JSON
- parse command options
- render list output

The next milestone can compose those pieces into a small command runner.

## Check yourself

1. Why should `parse_add_command` accept `args` instead of reading `sys.argv`?
2. Why should `render_note_list` return a string instead of printing?
3. What makes CLI output deterministic?
4. How does a CLI adapter resemble a future FastAPI route handler?

## Source reference notes

Use the Python standard library `argparse` documentation as the long-term parser reference, but this milestone uses a small manual parser to make the boundary explicit. Use pytest exact-string assertions to keep command output stable.
