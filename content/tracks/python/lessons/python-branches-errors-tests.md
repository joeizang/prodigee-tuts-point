# Branches, Errors, and Contracts

## Learning objectives

- Use `if` statements to separate valid and invalid data.
- Explain truthiness for strings after trimming whitespace.
- Raise `ValueError` when outside data violates the function contract.
- Test failure paths with `pytest.raises`.

## Prerequisites

You should have completed the names-and-return-values lesson first. You do not need to know loops, classes, files, FastAPI, or databases yet. This lesson only assumes that you can call a function and compare its returned value in a test.

## Mental model

**Term: branch** means a path through the code selected by a condition such as `if not clean_text`.

**Term: truthiness** means Python's rule for treating some values as true or false in conditions. Empty strings are falsey; non-empty strings are truthy.

**Term: exception** means a controlled failure signal. Here, `ValueError` tells the caller that the supplied value is not acceptable.

## Why validation appears early

Real applications receive messy input. A user can type spaces. A CLI argument can be missing. A JSON request can contain an empty field. A database row can be older than the code that now reads it.

Professional Python does not let every part of the program rediscover these problems. It creates validation boundaries.

A validation boundary answers one question:

```text
Can the rest of the program trust this value now?
```

For required note text, the rule is:

- trim whitespace
- reject the value if nothing remains
- return the clean text if it is usable
- include the field name in the error message

## Branches

Python uses indentation to show what belongs inside a branch.

```python
clean_text = raw_text.strip()
if not clean_text:
    raise ValueError("title is required")

return clean_text
```

`if not clean_text` is a compact way to say "if the cleaned string is empty." Empty strings are falsey. Non-empty strings are truthy.

The blank line before `return` is not required by Python, but it helps the reader see the separation between validation and success.

## Exceptions

`raise ValueError(...)` stops normal execution. The function does not continue to the return statement.

Use `ValueError` here because the caller supplied a value, but the value violates the contract. This is different from a syntax error, import error, or missing file.

Later, the CLI can translate this exception into a command-line message. FastAPI can translate it into an HTTP `400` response. The core function should not know about either UI.

## Testing invalid input

pytest can prove that a function rejects bad input:

```python
import pytest


def test_rejects_blank_title() -> None:
    with pytest.raises(ValueError, match="title"):
        require_note_text("title", "   ")
```

The `match` argument matters. It proves the error message says which field failed.

Do not only test the happy path. Validation code that has no failure-path tests is usually under-specified.

## A professional contract

For this beginner slice, a good function contract is small and explicit:

```python
def require_note_text(field_name: str, raw_text: str) -> str:
    ...
```

Inputs:

- `field_name`: the human-readable field name, such as `"title"` or `"body"`
- `raw_text`: the untrusted text value

Output:

- the stripped text

Failure:

- raise `ValueError` when the stripped text is empty
- include the field name in the message

Notice that the function does not decide whether the value came from a CLI, JSON body, form field, database import, or test. That makes it reusable.

## Common mistakes

- Checking `if not raw_text` before stripping, which allows `"   "` to pass.
- Returning `None` for invalid text, forcing every caller to remember another special case.
- Raising `Exception` instead of a more specific `ValueError`.
- Writing an error message that says only `"required"` without naming the field.
- Catching the exception inside the function that raised it.

## Production transfer

FastAPI and Pydantic will eventually validate many request fields for you. That does not remove the need for application validation.

Framework validation can say "this must be a string." Your application still decides "this string cannot be blank after trimming" or "this title must not duplicate an existing note."

Small validation functions teach that separation before the framework arrives.

## Exercise connection

`RequireNoteText` asks you to make one validation boundary reliable. The visible tests prove the normal behavior and one blank case. The hidden tests check tabs, newlines, body-specific messages, and reusable behavior.

## Project connection

`py-notes-cli` will use required text for titles, bodies, command arguments, API request fields, and database records. This lesson keeps that rule tiny so you can reuse it without importing CLI or FastAPI concerns into the core.

## Check yourself

1. Why should whitespace-only text be rejected after trimming?
2. What does `if not clean_text` mean for strings?
3. Why is `ValueError` a better fit than returning `None` here?
4. What does `pytest.raises(..., match="title")` prove?

## Source reference notes

Use the official Python tutorial for `if` statements and exceptions. Use pytest's exception-testing guidance for `pytest.raises`. The lesson's project-specific rule is stricter than the language itself: blank text is valid Python data, but it is invalid note data.
