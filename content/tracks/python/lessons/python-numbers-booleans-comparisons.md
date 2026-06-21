# Numbers, Booleans, and Comparisons

## Learning objectives

- Use integers and booleans without treating every input as text forever.
- Read comparison expressions as questions that produce `True` or `False`.
- Combine simple comparisons into named decisions.
- Keep beginner business rules explicit enough for tests to explain.

## Prerequisites

You should be comfortable binding names, returning values, and reading a simple pytest assertion failure.

## Mental model

Python programs make decisions by asking questions. A comparison such as `age_days > 30` asks a question and produces a boolean answer. The answer is not the string `"true"` or `"false"`. It is the Python object `True` or `False`.

**Term: integer** means a whole number such as `0`, `7`, or `42`.

**Term: boolean** means one of exactly two values: `True` or `False`.

**Term: comparison** means an expression that asks a question, such as `count == 0` or `priority_score >= 10`.

For a notes tool, numbers and booleans appear quickly:

- how many days old is this note?
- is the note pinned?
- is the body too long?
- did the user ask for archived notes?

Those values should stay typed. If `is_pinned` is a boolean, keep it as a boolean. If `age_days` is a number, keep it as a number. Converting everything to strings too early makes later rules harder to test and easier to break.

## Comparisons

The most common comparison operators are:

- `==` asks whether two values are equal.
- `!=` asks whether two values are not equal.
- `<` and `<=` compare smaller values.
- `>` and `>=` compare larger values.

```python
age_days = 45

is_recent = age_days <= 7
is_stale = age_days > 30
```

The names `is_recent` and `is_stale` are useful because they explain the question. A beginner mistake is to write a large condition inline, then forget what the pieces mean. Professional Python often improves readability by naming decisions.

## Boolean design

Boolean names should usually read like answers:

```python
has_body = body.strip() != ""
is_archived = archived_at is not None
should_promote = is_pinned and age_days <= 7
```

Avoid names like `body_check` or `flag`. Those names force the reader to inspect the implementation before they understand the rule.

## Small classification rules

A classification function turns inputs into a stable category:

```python
def classify_age(age_days: int) -> str:
    if age_days <= 7:
        return "fresh"

    if age_days <= 30:
        return "active"

    return "stale"
```

This style is intentionally plain. Each branch answers one question. The final `return` is the default case.

## Production transfer

Typed comparisons are the small version of production policy. Later, FastAPI and Pydantic will validate request models before your route handlers run. The same discipline starts here: keep numbers numeric, booleans boolean, and categories explicit.

## Exercise connection

The next exercises ask you to classify note priority and status. They are deliberately small, but they teach a permanent habit: turn fuzzy rules into named, executable decisions.

## Project connection

`py-notes-cli` will eventually sort, filter, archive, and render notes. Those features depend on clear numeric and boolean decisions before they depend on persistence or HTTP.

## Check yourself

- What value does `age_days > 30` produce?
- Why is `is_stale` clearer than `flag`?
- Why should a boolean not be stored as the string `"true"`?

## Source reference notes

Use the Python tutorial sections on numbers, comparisons, and control flow as the syntax anchor. Use pytest assertion documentation as the quality anchor for turning each business rule into evidence.
