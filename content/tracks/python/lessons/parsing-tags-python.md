# Parsing Tags into Data

## Learning objectives

- Convert loose user text into a list of trusted tag strings.
- Use lists, loops, membership checks, and `continue` deliberately instead of mechanically.
- Distinguish normalization from validation.
- Raise `ValueError` when outside input cannot become valid application data.
- Read pytest failures for list results and exception paths.

## Prerequisites

You should have completed the title-normalization slice or be comfortable reading a small Python function with a parameter, a return value, and pytest assertions. You do not need prior experience with lists beyond knowing that a list can hold multiple values.

## Why this matters

`py-notes-cli` will eventually search, filter, and display notes by tag. A tag starts as outside text: someone types it at a command line, pastes it from another app, or sends it later through an API request. Outside text is not application data yet.

The application needs a boundary:

- accept comma-separated input such as `" Python, FastAPI, testing "`
- trim surrounding whitespace for each tag
- lowercase each tag
- ignore empty chunks caused by repeated commas
- remove duplicates while preserving first-seen order
- reject tags that contain spaces after trimming

That rule is small, but it is not trivial. It introduces a professional Python habit: parse outside input at the edge, then let the rest of the program work with clean values.

## Mental model

**Term: outside data** means data that has not been checked by your application. Text from a CLI argument, file, database import, or HTTP request is outside data.

**Term: normalized data** means data transformed into a consistent representation. Lowercasing a tag is normalization.

**Term: validated data** means data checked against rules. Rejecting `"machine learning"` as one tag is validation because this project wants tags without internal spaces.

**Term: list** means an ordered collection. Python lists keep insertion order, which matters here because `["python", "fastapi"]` should stay in the order the user supplied.

## Core idea

Start with the contract before writing the loop.

```python
def parse_tags(raw_tags: str) -> list[str]:
    ...
```

The return type `list[str]` says the function returns a list where each item is a string. The function does not return one comma-separated string. That distinction is important. A string is convenient for input. A list is convenient for application behavior.

The simplest useful skeleton is:

```python
def parse_tags(raw_tags: str) -> list[str]:
    parsed: list[str] = []

    for chunk in raw_tags.split(","):
        tag = chunk.strip().lower()
        if not tag:
            continue

        parsed.append(tag)

    return parsed
```

Read this carefully:

- `split(",")` cuts the input into comma-separated chunks.
- `strip()` removes accidental space around one chunk.
- `lower()` makes comparisons deterministic.
- `continue` skips empty chunks.
- `append()` adds one valid value to the output list.

This is enough to understand the shape, but not enough for the milestone. It still allows duplicates and tags with spaces.

## Worked example

The project rule also requires duplicate removal and invalid-tag failures.

```python
def parse_tags(raw_tags: str) -> list[str]:
    parsed: list[str] = []

    for chunk in raw_tags.split(","):
        tag = chunk.strip().lower()
        if not tag:
            continue

        if " " in tag:
            raise ValueError("tags cannot contain spaces")

        if tag in parsed:
            continue

        parsed.append(tag)

    return parsed
```

This loop is doing three different jobs in a clear order:

1. Normalize the raw chunk into a candidate tag.
2. Validate the candidate.
3. Add it only if it is not already present.

That order matters. You want `" Python "` and `"python"` to be considered the same tag, so duplicate detection must happen after trimming and lowercasing.

## Why preserve order?

You could remove duplicates with a `set`, but a plain set is the wrong first tool for this beginner slice because the user-facing order matters.

```python
tags = ["python", "fastapi", "testing"]
```

This order can later drive display, search examples, and deterministic tests. A list plus `if tag in parsed` is direct and readable for a small number of tags. Later, when the project handles large inputs, you can learn a combined list-and-set pattern for faster membership checks while still preserving output order.

## Validation boundary

This project rejects a tag with spaces because tags are intended to be compact search labels:

```python
parse_tags("python, machine learning")
```

The second chunk becomes `"machine learning"`. It has an internal space, so the function raises `ValueError`.

The exact rule is product-specific. Some applications allow spaces in tags. The habit is broader than the rule: invalid outside data should fail at the boundary instead of silently becoming surprising application state.

FastAPI will make this idea feel familiar later. Pydantic can validate request bodies, but your application still owns the product rule. A request model can say a field is a string or list. Your domain function still decides what a valid tag means for `py-notes-cli`.

## Production transfer

The same boundary appears in production systems whenever users send repeated values: tags, categories, labels, roles, scopes, feature flags, query filters, and comma-separated import columns. In FastAPI, those values may arrive as query parameters or request-body fields. The framework can parse transport details, but your application still needs a small, testable function that defines which values are acceptable and how duplicates are handled.

## Common mistakes

- Returning the result of `raw_tags.split(",")` without trimming or lowercasing each item.
- Removing duplicates before normalization, so `"Python"` and `" python "` survive as different tags.
- Using `split()` instead of `split(",")`, which treats spaces as separators instead of commas.
- Silently keeping invalid values such as `"machine learning"` when the contract says to reject them.
- Raising an exception for empty chunks like `"python,,fastapi"` even though the milestone says to ignore empty chunks.

## Testing strategy

Use one test for the normal path and one test for the failure boundary.

```python
import pytest

from note_tags import parse_tags


def test_parses_tags_in_first_seen_order() -> None:
    assert parse_tags(" Python, FastAPI, python ") == ["python", "fastapi"]


def test_rejects_tag_with_internal_space() -> None:
    with pytest.raises(ValueError, match="spaces"):
        parse_tags("python, machine learning")
```

The first test proves normalization, duplicate removal, and order. The second proves invalid input does not sneak through.

## Debugging strategy

When a list result is wrong, print the intermediate representation while debugging:

```python
print(repr(raw_tags.split(",")))
print(repr(parsed))
```

`repr` helps reveal empty chunks and whitespace. Remove these prints when the tests explain the behavior.

## Exercise connection

The `ParseNoteTags` exercise asks you to implement `parse_tags`. The visible tests cover common parsing behavior. The hidden tests check duplicate casing, empty chunks, tabs, and invalid tags.

## Project connection

After this milestone, `py-notes-cli` has two pure boundaries:

- `normalize_title` turns title text into a stable title key.
- `parse_tags` turns tag text into structured tag data.

Those two functions are the core of later CLI, JSON, and FastAPI work. They are deliberately framework-free because the same rules should work from a command line, a test, or an HTTP request.

## Check yourself

1. Why should duplicate detection happen after lowercasing?
2. Why does this function return `list[str]` instead of `str`?
3. What is the difference between ignoring an empty chunk and rejecting an invalid tag?
4. Why is this kind of function useful before learning FastAPI?

## Source reference notes

Use the official Python tutorial for lists, loops, membership checks, and exceptions. Use the pytest documentation for list equality assertions and exception tests. The references explain Python behavior; the project contract decides the exact tag rule.
