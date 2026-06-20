# Text as Data in Python

## Learning objectives

- Run and read a small Python function that transforms text without changing the original input.
- Explain how Python names, string objects, and return values work in the first `py-notes-cli` milestone.
- Define a narrow title-normalization contract before writing code that depends on it.
- Use tests and editor diagnostics to separate syntax mistakes from behavior mistakes.

## Prerequisites

You do not need previous Python experience. You should know what a file is, what text is, and that a function can receive input and return output. This lesson intentionally moves slowly because the goal is not to memorize Python punctuation; the goal is to build the first mental model you will reuse in every later Python and FastAPI exercise.

## Mental model

**Term: name** means a label that points at an object. In Python, `title = "  Learn Python  "` does not create a box named `title` that contains characters. It binds the name `title` to a string object.

**Term: string** means an immutable text object. Immutable means methods such as `.strip()` and `.lower()` return new strings; they do not edit the existing string in place.

**Term: function contract** means the promise your function makes about input, output, and edge cases. A beginner mistake is to start typing operations before deciding the contract. Professional Python starts by making the rule testable.

For this milestone, the contract is deliberately narrow:

- trim whitespace from both ends of a note title
- collapse internal whitespace runs to one space
- lowercase the result
- reject empty titles with a clear exception

This is not the universal rule for every title in every product. It is the project rule for the first slice of `py-notes-cli`.

## Core idea

Python code is indentation-sensitive. The indented lines under `def normalize_title(raw_title: str) -> str:` are the function body.

```python
def normalize_title(raw_title: str) -> str:
    stripped = raw_title.strip()
    lowered = stripped.lower()
    return lowered
```

The annotations `raw_title: str` and `-> str` are **type hints**. They help tools such as Pyright explain what your code appears to expect. Python still runs at runtime, but the editor can catch many mismatches earlier when functions are annotated.

The important part is the object behavior:

```python
title = "  Learn Python  "
clean = title.strip()

print(title)  # still has spaces
print(clean)  # spaces removed
```

`strip()` returns a new string. If you ignore the return value, nothing useful happens:

```python
title = "  Learn Python  "
title.strip()

print(title)  # still "  Learn Python  "
```

That mistake is common because the line reads like a command. In Python, many string methods are better understood as expressions that produce another value.

## Worked example

The first project needs a slightly stronger rule than `strip().lower()`. Users can paste titles with tabs, newlines, or several spaces between words. Splitting without arguments treats any run of whitespace as a separator.

```python
def normalize_title(raw_title: str) -> str:
    words = raw_title.split()
    if not words:
        raise ValueError("title must contain at least one non-space character")

    return " ".join(words).lower()
```

Read the function from the inside out:

- `raw_title.split()` creates a list of meaningful word chunks.
- `if not words` handles an empty list.
- `" ".join(words)` rebuilds the title with one space between chunks.
- `.lower()` makes the result deterministic for search and comparison.

## Production transfer

Text normalization appears in search, tags, slugs, filenames, usernames, import pipelines, and API validation. The exact rule changes by product. The professional habit does not: define the boundary, write tests for examples and edge cases, and keep the transformation small enough to review.

FastAPI will use the same discipline later. A request body will arrive as outside data. Pydantic and FastAPI can help validate it, but you still need to know what rule your application wants.

## Common mistakes

- Forgetting `return`, so the function returns `None`.
- Calling `.strip()` without assigning or returning the result.
- Treating `split(" ")` as the same as `split()`; it is not the same for tabs, newlines, or repeated spaces.
- Returning an empty string for whitespace-only input when the project contract says to reject it.
- Fixing one visible example while ignoring the hidden edge case the contract already described.

## Testing strategy

Use tests to pin down the contract before trusting the implementation.

```python
import pytest

from note_titles import normalize_title


def test_normalizes_outer_and_inner_whitespace() -> None:
    assert normalize_title("  Learn   Python\tNow  ") == "learn python now"


def test_rejects_blank_titles() -> None:
    with pytest.raises(ValueError, match="title"):
        normalize_title("   ")
```

The first test checks normal behavior. The second test checks a failure path. Both are part of the contract.

## Debugging strategy

When the exercise fails, read the traceback from the bottom upward. The bottom usually tells you the immediate failure. Then add one temporary `print(repr(value))` while debugging whitespace-sensitive behavior. `repr` makes spaces, tabs, and newlines visible enough to reason about.

```python
print(repr(raw_title))
print(repr(raw_title.split()))
```

Remove temporary prints once the test explains the behavior.

## Exercise connection

The `NormalizeNoteTitle` exercise asks you to implement this exact contract. The visible tests cover the normal path. The hidden tests check blank input, tabs, newlines, and repeated internal spacing.

## Project connection

`py-notes-cli` will store and search note titles. If title normalization is vague, later features such as duplicate detection, tag filtering, and deterministic output will become inconsistent. This first milestone makes the text boundary explicit before file I/O, JSON, and command parsing arrive.

## Check yourself

1. Why does `.strip()` not change the original string object?
2. Why is `split()` better than `split(" ")` for this milestone's whitespace rule?
3. What should happen when a note title contains only spaces?
4. What editor feedback would help you notice that a function may return `None`?

## Source reference notes

Use the official Python tutorial for functions, strings, lists, and exceptions. Use the pytest documentation for assertions and exception tests. These sources anchor API behavior; the project contract decides which subset of behavior the exercise requires.
