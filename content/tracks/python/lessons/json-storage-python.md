# JSON as a Storage Boundary

## Learning objectives

- Represent a note as a JSON-safe Python dictionary.
- Use `json.dumps` and `json.loads` to cross the text/data boundary.
- Validate decoded JSON before trusting it as application data.
- Keep JSON formatting deterministic for tests and review.
- Connect dictionary/list validation to future Pydantic and FastAPI request models.

## Prerequisites

You should understand Python strings, lists, dictionaries, functions, and exceptions at a beginner level. You should also understand that files contain text, while application logic usually wants structured Python values.

## Mental model

**Term: JSON** means a text format for data. JSON can represent objects, arrays, strings, numbers, booleans, and null. Python's `json` module converts between JSON text and Python values.

**Term: JSON-safe value** means a value that can be encoded as JSON: strings, numbers, booleans, `None`, lists, and dictionaries with string keys.

**Term: decoded data** means the Python value returned by `json.loads`. Decoded data is still outside data until your application validates its shape.

**Term: deterministic formatting** means the same data produces the same text layout every time. That makes tests, diffs, and debugging easier.

## Core idea

A note record can start as a dictionary:

```python
record = {
    "title": "learn python",
    "body": "Practice storage boundaries.",
    "tags": ["python", "storage"],
}
```

Encoding turns it into text:

```python
import json

text = json.dumps(record, indent=2, sort_keys=True)
```

Decoding turns JSON text back into Python values:

```python
loaded = json.loads(text)
```

The dangerous part is assuming `loaded` is automatically valid. It is not. It might be a list, a string, a dictionary without `title`, or a dictionary where `tags` contains numbers.

## Worked example

Validation should check the shape before the rest of the application trusts the data:

```python
def validate_note_record(value: object) -> dict[str, object]:
    if not isinstance(value, dict):
        raise ValueError("note record must be an object")

    title = value.get("title")
    body = value.get("body")
    tags = value.get("tags")

    if not isinstance(title, str) or not title.strip():
        raise ValueError("note title is required")

    if not isinstance(body, str):
        raise ValueError("note body must be text")

    if not isinstance(tags, list) or not all(isinstance(tag, str) for tag in tags):
        raise ValueError("note tags must be a list of strings")

    return {"title": title, "body": body, "tags": list(tags)}
```

This function is intentionally plain. Pydantic will later automate parts of this style for HTTP request bodies, but learning the checks manually first makes the framework easier to understand.

## Production transfer

FastAPI request bodies are usually JSON. When FastAPI receives a request, it parses JSON text and validates it with Pydantic models. This storage milestone teaches the same boundary in a smaller context:

- outside text arrives
- Python decodes it
- application rules validate it
- trusted values move inward

Once you understand this with files, FastAPI will feel like a transport change rather than a completely new programming model.

## Common mistakes

- Treating decoded JSON as trusted application data without shape checks.
- Returning a JSON string when the rest of the application needs a dictionary.
- Mutating the caller's tag list instead of copying it into the record.
- Producing nondeterministic JSON formatting that makes tests and diffs noisy.
- Catching `json.JSONDecodeError` and returning empty data, which hides corrupt files.

## Testing strategy

Test the structure, not just the text:

```python
def test_builds_note_record() -> None:
    record = build_note_record("learn python", "body", ["python"])

    assert record == {
        "title": "learn python",
        "body": "body",
        "tags": ["python"],
    }
```

For JSON formatting, compare decoded values unless the formatting itself is part of the contract.

## Debugging strategy

Use `type` and `repr` when decoded JSON surprises you:

```python
print(type(loaded))
print(repr(loaded))
```

If a nested record fails validation, inspect the specific field before changing the validator.

## Exercise connection

`SerializeNoteRecord` builds a trusted JSON-safe note dictionary. `LoadNotesFile` reads a JSON file and rejects malformed records before they enter the application core.

## Project connection

After this milestone, `py-notes-cli` has enough core behavior to store notes:

- title normalization
- tag parsing
- JSON-safe note records
- validated file loading

The next CLI milestone can build commands on top of these pieces instead of mixing command parsing with storage rules.

## Check yourself

1. Why is decoded JSON still outside data?
2. Why should `tags` be copied into a new list?
3. When should a test compare JSON text instead of decoded Python values?
4. How does this prepare you for FastAPI request models?

## Source reference notes

Use the Python standard library documentation for `json`. Use the Python tutorial for dictionaries and lists. Use pytest documentation for nested assertions. Later FastAPI and Pydantic will automate some validation mechanics, but the boundary thinking starts here.
