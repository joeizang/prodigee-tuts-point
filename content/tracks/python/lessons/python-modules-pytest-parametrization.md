# Modules, Helpers, and Parametrized Tests

## Learning objectives

- Treat a `.py` file as a module with named functions.
- Split helpers only when they clarify a rule.
- Import functions in tests without running application code at import time.
- Use `pytest.mark.parametrize` to test many examples of the same rule.

## Prerequisites

You should understand functions, branches, loops, lists, and dictionaries.

## Mental model

A Python module is a file that Python can import. If `note_summary.py` defines `format_summary`, a test can import it with `from note_summary import format_summary`.

**Term: module** means a Python file used as an importable unit.

**Term: helper function** means a smaller function used by another function to keep a rule readable.

**Term: parametrized test** means one test body that runs against multiple input and expected-output examples.

Beginner code often grows in two bad directions. One version puts everything into one huge function. Another version splits every line into a tiny helper. Good Python sits between those extremes: split when the name explains a real idea.

## Import boundaries

Importing a module should be boring. It should define functions, classes, and constants. It should not start reading files, running CLI prompts, or connecting to services.

```python
def normalize_word(word: str) -> str:
    return word.strip().lower()


def normalize_words(words: list[str]) -> list[str]:
    result: list[str] = []
    for word in words:
        normalized = normalize_word(word)
        if normalized:
            result.append(normalized)

    return result
```

`normalize_word` is a useful helper because its name captures a repeated rule. The caller can test the helper indirectly through `normalize_words`, or directly when the rule deserves its own examples.

## Parametrized tests

When the same test shape repeats, parametrization keeps the signal high:

```python
import pytest

from note_status import choose_status


@pytest.mark.parametrize(
    ("is_archived", "is_pinned", "age_days", "expected"),
    [
        (True, False, 1, "archived"),
        (False, True, 1, "pinned"),
        (False, False, 31, "stale"),
        (False, False, 3, "active"),
    ],
)
def test_choose_status(is_archived: bool, is_pinned: bool, age_days: int, expected: str) -> None:
    assert choose_status(is_archived, is_pinned, age_days) == expected
```

This does not make testing weaker. It makes repeated branch examples easier to scan.

## Production transfer

FastAPI apps depend heavily on import boundaries. Route modules import services. Tests import app factories. Tooling imports modules for analysis. If import time does real work, your tests and editor feedback become fragile.

## Exercise connection

You will split a note formatting task into small helpers and prove the behavior through tests. The expected implementation should be plain, importable, and easy for Pyright and Ruff to analyze.

## Project connection

The notes project is moving toward reusable modules. This lesson prepares you for sharing the same core functions between CLI commands and FastAPI route handlers.

## Check yourself

- What should happen when a test imports your module?
- When is a helper function worth creating?
- Why does parametrization fit branch-heavy rules?

## Source reference notes

Use the Python tutorial module section for import syntax. Use pytest parametrization documentation as the testing anchor for repeated rule examples.
