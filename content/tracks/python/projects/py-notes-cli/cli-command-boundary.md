# CLI command boundary

## Scenario

`py-notes-cli` has pure note helpers and storage helpers. Now it needs a command boundary: raw command-line text should become request data, and trusted note records should become deterministic user-facing output.

## Requirements

- Parse `add` command options from a list of argument strings.
- Support `--title`, `--body`, and optional `--tags`.
- Reject unknown options.
- Reject missing option values.
- Reject missing required `--title` or `--body`.
- Render note records as stable numbered lines.
- Keep parser and renderer pure: no `sys.argv`, `print`, file reads, or file writes.

## CLI/API contract

The milestone contracts are Python functions:

```python
def parse_add_command(args: list[str]) -> dict[str, object]:
    ...

def render_note_list(notes: list[dict[str, object]]) -> str:
    ...
```

These functions are not the full executable yet. They are the testable boundary that a future executable can call.

## Milestone task

Implement both exercises. Then explain how the same request/rendering style can later map to FastAPI request models and response bodies.

## Rubric

- Correctness: parses supported options, rejects invalid command data, and renders stable output.
- Design: keeps command parsing and rendering separate from file I/O and core note validation.
- Validation: surfaces missing and unknown options with clear `ValueError` messages.
- Testing: uses exact list and string assertions for command behavior.
- Complexity: uses straightforward list walking and string formatting before introducing a full CLI framework.
- Maintainability: produces functions that a future executable, CLI command runner, or FastAPI adapter can call.

## Stretch goals

- Replace the manual parser with `argparse` while keeping the same function contract.
- Add a `search` command request parser.
- Add a command runner that composes parsing, storage, note creation, and rendering.
