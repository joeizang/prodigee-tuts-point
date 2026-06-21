# Control flow and collection processing

## Scenario

The notes tool now needs rules that are larger than a single string operation but still small enough to test without files, commands, databases, or HTTP.

This milestone turns beginner Python syntax into project behavior. You will classify notes, count meaningful body content, normalize tag lists, format summaries, and split helpers into importable modules.

## Requirements

- Classify note priority from pinned state and note age.
- Choose note status with explicit branch precedence.
- Count non-empty body lines while ignoring whitespace-only lines.
- Extract normalized unique tags while preserving first-seen order.
- Format one note summary without mutating the note record.
- Keep helper modules importable without side effects.

## CLI/API contract

These functions are not CLI commands yet. They are core behavior that future CLI commands and FastAPI route handlers will share.

Expected behavior examples:

```text
priority(True, 400) -> high
status(False, False, 31) -> stale
count_nonempty_lines("one\n\n two ") -> 2
extract_unique_tags(["Python", " python "]) -> ["python"]
format_summary({"title": "Python", "body": "Loops", "tags": ["basics"]}) -> Python - Loops [basics]
```

## Milestone task

Create a small `note_rules.py` module locally that combines the milestone exercises into one project-facing surface:

- `classify_note_priority`
- `choose_note_status`
- `count_nonempty_lines`
- `extract_unique_tags`
- `format_note_summary`

Then write a few parametrized tests that prove the rules work together for at least three note examples.

## Rubric

- Correctness: branch precedence, numeric boundaries, empty text, duplicate tags, and no-tag summaries are all covered.
- Design: helpers are named after domain rules instead of implementation details.
- Testing: parametrized tests cover repeated examples without hiding branch intent.
- Maintainability: modules import cleanly and do not perform I/O at import time.
- Complexity: solutions stay plain and readable; no framework or database is introduced.

## Stretch goals

- Add an explicit `is_recent_note(age_days: int) -> bool` helper and decide whether it improves readability.
- Add tests proving input tag lists are not mutated.
- Write one paragraph explaining which rules belong in pure Python and which later belong in CLI or FastAPI adapters.
