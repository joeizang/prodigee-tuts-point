# Lists, Dictionaries, and Records

## Learning objectives

- Read and build simple note-shaped dictionaries.
- Use lists for ordered collections of tags.
- Copy input lists so callers cannot mutate stored records accidentally.
- Recognize the difference between Python objects and JSON-safe data.

## Prerequisites

You should understand names, return values, branches, and `ValueError` before starting this lesson. You do not need to know JSON files yet. The only goal here is to build trustworthy in-memory data.

## Mental model

**Term: list** means an ordered mutable collection, such as `["python", "fastapi"]`.

**Term: dictionary** means a mapping from keys to values, such as `{"title": "learn python"}`.

**Term: record** means a dictionary shape that your application treats as one meaningful piece of data.

## Why records matter

The notes project needs a stable shape for a note before it can save anything.

For the early slices, a note is represented as a dictionary:

```python
{
    "title": "learn python",
    "body": "practice functions",
    "tags": ["python", "foundation"],
}
```

This is not the final architecture. Later you will use Pydantic models, repository protocols, SQLite rows, SQLAlchemy models, and HTTP responses. The dictionary is still useful now because it teaches a fundamental idea: application data needs a shape.

## Lists

A list stores ordered values.

```python
tags = ["python", "fastapi"]
```

Lists are mutable. That means they can be changed after creation.

```python
tags.append("pytest")
```

Mutability is useful, but it also creates accidental coupling. If a function stores the caller's list directly, the caller can mutate the stored record later.

```python
tags = ["python"]
record = {"tags": tags}
tags.append("mutated")
```

Now `record["tags"]` also contains `"mutated"`. The record did not protect itself.

The beginner fix is simple:

```python
record = {"tags": list(tags)}
```

`list(tags)` creates a shallow copy.

## Dictionaries

A dictionary maps keys to values.

```python
note = {
    "title": "learn python",
    "body": "practice functions",
}
```

In this track, use string keys for records that will eventually cross JSON or HTTP boundaries. This keeps the shape easy to inspect and easy to serialize later.

## JSON-safe data

JSON-safe data is made from values JSON understands:

- strings
- numbers
- booleans
- `None`
- lists
- dictionaries with string keys

Python can represent far more than JSON can. File handles, functions, database connections, and class instances are not JSON-safe by default.

The first record-building exercises should return dictionaries and lists, not JSON strings. Serialization comes later. This keeps "build trusted data" separate from "encode trusted data."

## A useful beginner contract

```python
def build_note_draft(title: str, body: str, tags: list[str]) -> dict[str, object]:
    ...
```

The function should:

- trim title and body
- reject blank title and body
- copy tags into a new list
- return a dictionary with stable keys

This is intentionally close to the later `SerializeNoteRecord` exercise, but it is not the same level of rigor. The goal here is to learn data shape and mutation safety before adding every validation rule.

## Testing mutation safety

The most important hidden issue is list copying:

```python
def test_copies_tags() -> None:
    tags = ["python"]
    note = build_note_draft("title", "body", tags)
    tags.append("mutated")
    assert note["tags"] == ["python"]
```

This test teaches a deep lesson early: tests should cover ownership, not only visible output.

## Common mistakes

- Returning a tuple when the contract asks for a dictionary.
- Returning a JSON string too early.
- Reusing the caller's `tags` list directly.
- Building different keys in different branches.
- Letting blank title or body values become trusted records.

## Production transfer

Pydantic response models and SQLAlchemy row mapping both rely on stable data shapes. If you cannot reason about a dictionary with strings and lists, FastAPI models will feel like magic. This lesson removes the magic before the framework arrives.

## Exercise connection

`BuildNoteDraft` asks you to build the first note-shaped value. It is a bridge between simple function returns and the project data model used by storage, CLI rendering, and FastAPI responses.

## Project connection

The next milestones will normalize note titles, parse tags, serialize records, and load JSON files. `BuildNoteDraft` is the low-pressure rehearsal for those slices: make a stable shape, copy mutable inputs, and return data that another boundary can trust.

## Check yourself

1. Why is `list(tags)` safer than storing `tags` directly?
2. Why should record keys be stable strings?
3. Why should this function return a dictionary instead of a JSON string?
4. What kind of bug does a mutation-safety test catch?

## Source reference notes

Use the official Python tutorial for list and dictionary behavior. Use the Python `json` documentation only to understand which values are JSON-compatible; this lesson does not serialize anything yet. Use pytest assertions to compare the full returned record shape.
