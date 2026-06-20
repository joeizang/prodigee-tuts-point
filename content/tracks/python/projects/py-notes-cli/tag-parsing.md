# Tag parsing

## Scenario

`py-notes-cli` needs tags before it can support useful search and filtering. Users will type tags as loose text, but the application should store tags as a predictable list of normalized strings.

## Requirements

- Accept a comma-separated tag string.
- Trim whitespace around each tag.
- Lowercase every tag.
- Ignore empty chunks from repeated or trailing commas.
- Remove duplicate tags while preserving first-seen order.
- Reject any tag that contains internal spaces after trimming.
- Keep parsing pure: no printing, prompts, file reads, or global state.

## CLI/API contract

This milestone still avoids a real CLI. The contract is a Python function:

```python
def parse_tags(raw_tags: str) -> list[str]:
    ...
```

The function returns a list of normalized tags. It raises `ValueError` when a non-empty tag violates the project rule.

## Milestone task

Implement `parse_tags` so visible and hidden tests pass. Then explain why this function belongs in the reusable project core rather than inside command-line argument handling.

## Rubric

- Correctness: handles trimming, casing, empty chunks, duplicate removal, and invalid tags.
- Design: returns structured data instead of a reformatted string.
- Validation: rejects bad tag data explicitly with `ValueError`.
- Testing: implements the full written contract, not only the visible examples.
- Complexity: uses direct list/string operations instead of introducing a parser, regex-heavy solution, or framework dependency.
- Maintainability: uses direct list/string operations that a beginner can explain.

## Stretch goals

- Add a helper that parses tags from an existing `list[str]` as well as comma-separated text.
- Compare the list-only implementation with a list-plus-set implementation.
- Add tests for punctuation choices such as hyphens and underscores, then decide the project rule deliberately.
