# Query and Mutation Commands

## Scenario

`py-notes-cli` can add notes. A useful tool also needs to inspect and maintain them. This milestone adds `list`, `search`, `update`, and `delete` while preserving the boundary discipline from the earlier slices.

## Requirements

- `list` reads notes and renders deterministic numbered output.
- `search --tag <tag>` reads notes and renders only matching notes.
- `update --title <title> --body <body>` changes one existing note body.
- `delete --title <title>` removes one existing note.
- Query commands must not rewrite the storage file.
- Mutation commands must save deterministic JSON and fail when the target note does not exist.

## CLI/API contract

The CLI command runner receives raw command tokens and a storage path. It returns display text or raises `ValueError` for invalid command data. The service helpers underneath should not depend on CLI strings so FastAPI can reuse them.

## Milestone task

Implement a command dispatcher and service helpers for list, search, update, and delete. Keep parsing, storage, mutation, and rendering as separate concepts even if they live in one exercise file for now.

## Rubric

- Correctness: every command produces the expected output and persisted state.
- Safety: failed updates and deletes do not corrupt the notes file.
- Design: query and mutation operations are clearly separated.
- Testing: behavior is covered through `tmp_path` workflow tests.
- Maintainability: command dispatch, parsing, storage, mutation, and rendering remain separate enough to change independently.
- FastAPI readiness: core operations can be called without command-line argument strings.

## Complexity

This is the first Python slice where the number of branches matters. Keep helpers small and name them after the decision they own.

## Stretch goals

- Add `search --text <query>` across title and body.
- Return richer service results that a CLI can render and FastAPI can serialize.
- Add stable note IDs before allowing title changes.
