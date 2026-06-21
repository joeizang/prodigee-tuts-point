# Python first contact

## Scenario

You are starting `py-notes-cli` with no Python background. Before the project can normalize titles, parse tags, store JSON, or expose a FastAPI route, you need the smallest reliable development loop.

This milestone gives you that loop:

```text
read a contract -> edit one function -> run tests -> read feedback -> improve
```

## Requirements

- Write small pure functions that return values instead of printing.
- Use type hints on parameters and return values.
- Trim required text before trusting it.
- Raise `ValueError` for blank required fields.
- Build a note-shaped dictionary with stable keys.
- Copy caller-provided tag lists before storing them in records.
- Use pytest output and static-analysis diagnostics as learning feedback.

## CLI/API contract

There is no CLI command and no HTTP route in this milestone. The contract is a set of reusable Python functions:

```python
def build_note_label(title: str, body: str) -> str:
    ...

def require_note_text(field_name: str, raw_text: str) -> str:
    ...

def build_note_draft(title: str, body: str, tags: list[str]) -> dict[str, object]:
    ...
```

These functions form the mental foundation for later CLI and FastAPI boundaries.

## Milestone task

Complete the three beginner exercises in order. Do not skip the tiny ones. They intentionally isolate the mechanics that later become easy to confuse:

- names and returned values
- branches and exceptions
- lists, dictionaries, and mutation safety

After each exercise, explain what the tests proved and what the editor diagnostics helped you notice.

## Rubric

- Correctness: functions return exactly the values required by the visible and hidden tests.
- Design: functions stay pure and avoid printing, file access, global state, or framework dependencies.
- Testing: pytest success includes both normal behavior and invalid-input behavior.
- Maintainability: names match the project language and make the code readable to a beginner.
- Complexity: solutions use direct Python expressions and simple branches instead of premature abstractions.

## Stretch goals

- Add one extra local test for each function before looking at the model solution.
- Rewrite one test failure in plain English before changing the code.
- Use Ruff formatting after each exercise and compare the before/after code.
- Explain why these functions can later be called from both CLI code and FastAPI handlers.

