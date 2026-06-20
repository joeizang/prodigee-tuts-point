# Title normalization

## Scenario

You are building the first slice of `py-notes-cli`, a small command-line notes tool. Before the tool can store notes, search notes, or detect duplicates, it needs one reliable rule for note titles.

## Requirements

- Normalize note titles by trimming outer whitespace.
- Collapse internal whitespace runs to one single space.
- Lowercase the normalized title.
- Reject titles with no non-space characters.
- Keep the function pure: no printing, file reads, prompts, or global state.

## CLI/API contract

This milestone does not expose a real CLI yet. The contract is a Python function:

```python
def normalize_title(raw_title: str) -> str:
    ...
```

The function returns normalized text for valid input and raises `ValueError` for blank input.

## Milestone task

Implement `normalize_title` so visible and hidden tests pass. Then explain why the function belongs in a reusable core module instead of being embedded directly in future command-line parsing code.

## Rubric

- Correctness: handles outer whitespace, internal whitespace, casing, and blank input.
- Design: keeps the function pure and returns a value instead of printing.
- Testing: uses visible tests as examples but implements the full written contract, including hidden whitespace and blank-input cases.
- Complexity: uses direct string/list operations instead of building a parser or writing unnecessary loops.
- Maintainability: keeps the error behavior simple enough for later CLI code to translate into a user-facing message.

## Stretch goals

- Add a second helper that preserves display casing while producing a normalized search key.
- Compare the tradeoff between lowercasing at write time and lowercasing only at search time.
- Write two additional tests for titles copied from multiline text.
