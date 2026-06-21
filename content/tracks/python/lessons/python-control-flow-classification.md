# Control Flow and Classification

## Learning objectives

- Read `if`, `elif`, and `else` as a decision table.
- Order branches from most specific to default.
- Return early when a rule has been satisfied.
- Test every meaningful branch instead of only the happy path.

## Prerequisites

You should understand strings, booleans, comparisons, and simple function returns.

## Mental model

Control flow is how a program chooses one path instead of another. In Python, indentation is part of the program. The indented block under an `if` runs only when the condition is true.

**Term: branch** means one path through a decision.

**Term: default case** means the path used when no earlier condition matched.

**Term: classification** means converting input facts into one named category.

For beginners, the danger is not the syntax. The danger is writing rules in an order that accidentally hides a more specific case.

## Branch order

Consider a note status rule:

```python
def choose_status(is_archived: bool, is_pinned: bool, age_days: int) -> str:
    if is_archived:
        return "archived"

    if is_pinned:
        return "pinned"

    if age_days > 30:
        return "stale"

    return "active"
```

This is readable because the most important override comes first. An archived note stays archived even if it is also pinned or old. The code makes that priority visible.

## `elif` versus repeated `if`

`elif` is useful when the branches are part of one exclusive decision:

```python
def age_bucket(age_days: int) -> str:
    if age_days <= 7:
        return "fresh"
    elif age_days <= 30:
        return "active"
    else:
        return "stale"
```

Repeated `if` statements with early returns can be just as clear. In this curriculum, use the form that makes the rule easiest to read. Do not chase cleverness.

## Testing decisions

A decision function should have tests for:

- the first branch
- every middle branch
- the default branch
- boundary values such as `7`, `8`, `30`, and `31`

Boundary tests are where many beginner bugs show up. A rule that says "30 days or fewer" should test exactly `30` and exactly `31`.

## Production transfer

HTTP route handlers are full of decisions: which status code to return, whether a request is valid, whether a record exists, and whether a user can perform an action. If you can test tiny control-flow functions, you can later test those API decisions without guessing.

## Exercise connection

You will implement a status chooser and a priority classifier. The point is not the labels themselves. The point is branch order, boundary testing, and stable return values.

## Project connection

The notes project needs deterministic behavior before it needs a database. A user should see the same status for the same note facts every time.

## Check yourself

- Which branch should run first when one rule overrides another?
- Why should `age_days == 30` have its own test?
- What makes a default case different from a hidden assumption?

## Source reference notes

Use the Python tutorial section on `if` statements for syntax. Use pytest parametrization as a way to keep many branch examples compact and readable.
