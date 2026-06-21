# Names, Values, and First Tests

## Learning objectives

- Read a small Python function without guessing what each line does.
- Explain the difference between a name, an object, a parameter, and a return value.
- Use pytest failure output as feedback instead of treating it as noise.
- Understand why the project starts with pure functions before files, commands, databases, or FastAPI.

## Prerequisites

You do not need previous Python experience. You should know what plain text is and you should be comfortable editing a file. The lesson assumes that you may not yet know what a variable, function, test, or type hint really means in Python.

## Mental model

**Term: name** means a label in Python code that refers to an object.

**Term: object** means a runtime value such as a string, list, dictionary, function, or integer.

**Term: return value** means the value a function gives back to its caller.

The beginner rule is: do not imagine names as boxes. Imagine names as labels attached to objects. That model will make string methods, list copying, dictionaries, imports, dependency overrides, and FastAPI request objects easier to reason about later.

## Why this comes first

If you are new to Python, the first danger is not syntax. The first danger is building the wrong mental model.

Many beginners imagine a variable as a box. That picture works for a few minutes and then starts causing confusion. In Python, a name is better understood as a label bound to an object.

```python
title = "Learn Python"
```

The name `title` now refers to a string object. If you later write another assignment, you bind the name to a new object.

```python
title = "Learn Python"
title = "Practice FastAPI"
```

The second line does not edit the first string. It makes `title` point at another string object.

This distinction matters because strings are immutable. String operations return new values.

```python
title = "  Learn Python  "
clean_title = title.strip()
```

`clean_title` refers to `"Learn Python"`. The original `title` still refers to `"  Learn Python  "`.

## Your first project rule

`py-notes-cli` will eventually store notes, search them, expose them through FastAPI, and persist them in SQLite and PostgreSQL. That is too much for the first step.

The first step is smaller:

- receive text
- make a deterministic output
- return the output
- prove the behavior with tests

That is why the first exercise asks for a note label, not a complete app.

## Function anatomy

```python
def build_note_label(title: str, body: str) -> str:
    clean_title = title.strip()
    clean_body = body.strip()
    return f"{clean_title}: {clean_body}"
```

Read it in pieces:

- `def` starts a function definition.
- `build_note_label` is the function name.
- `title` and `body` are parameters.
- `: str` and `-> str` are type hints.
- The indented lines are the function body.
- `return` sends a value back to the caller.

The function does not print. Printing writes to the console; returning gives another part of the program a value it can test, store, format, or send over HTTP.

## pytest as a teacher

A pytest assertion compares what happened with what should have happened.

```python
def test_builds_label() -> None:
    assert build_note_label("Python", "functions") == "Python: functions"
```

If the function returns `"Python - functions"` instead, pytest tells you the exact mismatch. That is not a punishment. It is a precise description of the contract you have not met yet.

When a test fails, ask three questions:

1. What value did the test pass into the function?
2. What value did the function return?
3. What value did the contract expect?

Do not start by rewriting the whole function. Start by locating the smallest difference.

## Editor feedback

Pyright and BasedPyright read your type hints. Ruff reads your style and lint rules. They do not replace tests.

Use them for different kinds of feedback:

- Pyright: "This value may not have the type you promised."
- Ruff: "This code is unused, unclear, or not formatted according to the project rules."
- pytest: "The behavior did or did not match the example contract."

For this track, a Python exercise is not complete just because tests pass. The project quality gate also expects clean static analysis.

## Common mistakes

- Forgetting `return`, which makes Python return `None`.
- Printing the answer instead of returning it.
- Changing the test to match your code instead of changing the code to match the contract.
- Treating type hints as decoration instead of a promise to the reader and editor.
- Using names such as `x` and `thing` when the project already has clearer words such as `title`, `body`, `label`, and `note`.

## Exercise connection

`BuildNoteLabel` gives you the smallest complete feedback loop in this track. You will edit one function, run tests, read the result, and fix the function until the behavior is exact.

The goal is not to finish quickly. The goal is to make the loop familiar:

```text
read contract -> edit function -> run tests -> read diagnostics -> improve
```

That loop remains the same when the code grows into CLI commands, FastAPI routes, repositories, migrations, and PostgreSQL integration tests.

## Production transfer

Production Python is still made from small readable functions. A FastAPI route handler, a repository method, and a migration helper all depend on the same basics: bind clear names, transform values deliberately, and return the value the next boundary expects.

## Project connection

This milestone starts `py-notes-cli` with a label function because labels are easy to inspect. Later milestones will normalize titles, build note dictionaries, render command output, persist JSON, and serve HTTP responses. Those larger behaviors all reuse the same return-value discipline.

## Check yourself

1. What does a Python name refer to?
2. Why does `strip()` need to be assigned or returned?
3. Why is returning better than printing inside a reusable function?
4. What should you read first when pytest reports an assertion failure?

## Source reference notes

Use the official Python tutorial to anchor the meaning of names, functions, and return values. Use the pytest documentation to practice reading assertion failures. The project contract narrows those general language rules into the exact behavior required by `BuildNoteLabel`.
