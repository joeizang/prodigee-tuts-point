# Loops and Collection Processing

## Learning objectives

- Use `for` loops to process strings and lists one item at a time.
- Accumulate results without mutating inputs accidentally.
- Remove empty values deliberately.
- Preserve deterministic ordering when extracting unique values.

## Prerequisites

You should understand lists, dictionaries, strings, and simple branches.

## Mental model

A loop repeats the same small operation for each item in a collection. In Python, a `for` loop does not expose an index unless you ask for one. Most beginner code should start by reading the item directly.

**Term: iteration** means one pass through a loop.

**Term: accumulator** means a value that collects results while a loop runs.

**Term: deterministic order** means the same input produces the same output order every time.

For notes, loops appear in ordinary places:

- count non-empty body lines
- extract tags
- format search results
- reject invalid records
- render a list of summaries

## Counting with a loop

```python
def count_nonempty_lines(text: str) -> int:
    count = 0
    for line in text.splitlines():
        if line.strip():
            count += 1

    return count
```

This function has three important choices:

- `splitlines()` respects real line boundaries.
- `strip()` treats whitespace-only lines as empty.
- `count` is returned instead of printed.

## Building a list

```python
def clean_tags(raw_tags: list[str]) -> list[str]:
    cleaned: list[str] = []
    for tag in raw_tags:
        normalized = tag.strip().lower()
        if normalized:
            cleaned.append(normalized)

    return cleaned
```

The accumulator starts empty. Each valid item is appended. The original list is left alone.

## Unique values without surprises

When order matters, do not casually convert a list to a set and back. A set is useful for membership checks, but it is not a user-facing ordering rule.

```python
def unique_tags(raw_tags: list[str]) -> list[str]:
    seen: set[str] = set()
    result: list[str] = []

    for raw_tag in raw_tags:
        tag = raw_tag.strip().lower()
        if not tag or tag in seen:
            continue

        seen.add(tag)
        result.append(tag)

    return result
```

The result preserves first-seen order. That is a stable contract.

## Production transfer

Production APIs often process collections: request items, database rows, validation errors, logs, and response models. The same loop discipline applies: name the accumulator, avoid mutating caller-owned inputs, and make ordering intentional.

## Exercise connection

You will count non-empty lines, extract unique tags, and format summaries from note-shaped dictionaries.

## Project connection

`py-notes-cli` will soon list and search notes. Listing behavior depends on loops that produce stable, readable output from stored records.

## Check yourself

- Why should whitespace-only lines not count as content?
- Why is `set(raw_tags)` not enough when output order matters?
- What does an accumulator do?

## Source reference notes

Use the Python tutorial sections on lists, `for` statements, and sets as syntax anchors. Treat deterministic ordering as a project rule, not just a data-structure detail.
