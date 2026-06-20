# 0054 Plan Python/FastAPI Language-Service And Runner Architecture

## Type

Architecture

## Status

Planned

## Outcome

Python and FastAPI exercises will use real generated Python workspaces, pytest execution, Pyright or basedpyright semantic analysis, and Ruff formatting/linting. Monaco must provide serious Python/FastAPI IntelliSense through a backend language-service bridge rather than syntax-only highlighting or handcrafted suggestions.

## Acceptance Criteria

- Decide the Python workspace shape for beginner exercises: `pyproject.toml`, `src/exercise.py`, `tests/test_visible.py`, and `tests/test_hidden.py`.
- Decide the FastAPI workspace shape: `pyproject.toml`, `src/app.py`, optional `src/schemas.py`, `src/routes.py`, `src/services.py`, visible route tests, and hidden route tests.
- Decide the runner path: host-process pytest with strict timeout, output truncation, generated run workspace cleanup, hidden-test privacy, and static-analysis capture.
- Decide the semantic editor backend: Pyright or basedpyright for completions, diagnostics, hover, and signature help; Ruff for formatting, lint diagnostics, organize imports, and code actions where supported.
- Ensure the runner and language service resolve the same dependency profile, including FastAPI, Pydantic, pytest, httpx, and Ruff for framework exercises.
- Capture the decision in an ADR so Python/FastAPI work does not drift into fake Monaco IntelliSense or script-only exercises.

## Verification

- ADR added: `docs/adr/0007-python-fastapi-pyright-ruff-workspaces.md`.
- Local or repo-managed Python tooling path identified.
- Spike proves a generated workspace can run pytest and expose package-aware Pyright/basedpyright completions for an unsaved Monaco buffer.

## Full Feature Later

Later implementation tasks should add the Python workspace generator, pytest runner, Pyright/basedpyright LSP bridge, Ruff diagnostics/formatting integration, FastAPI dependency profile, and browser/API tests proving Python and FastAPI editor parity.
