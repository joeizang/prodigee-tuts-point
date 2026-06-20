# 0054 Plan Python/FastAPI Language-Service And Runner Architecture

## Type

Architecture

## Status

In Progress

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
- Python 3.14 is available locally.
- `pytest`, `ruff`, `basedpyright-langserver`, `pyright-langserver`, FastAPI, and Pydantic are managed through the repository-level uv project rather than installed globally or inside generated exercise workspaces.
- `uv` is installed locally and `uv.lock` captures the repository-level Python tool environment.
- Added `python-pytest` workspace generation, Python runner hooks, Ruff/Pyright-oriented project files, and backend Monaco routing for editable `.py` files.
- Added `PythonLspBridge` using `uv run --project <repo> basedpyright-langserver --stdio`, with Ruff formatting through `uv run --project <repo> ruff format` and explicit setup warnings when uv/tools are missing.
- Verified Python visible and hidden pytest execution through uv for the first Python foundation exercise.

## Remaining Work

- Run a live Monaco smoke proving completions, diagnostics, hover, signature help, formatting, and package-aware imports.
- Add FastAPI dependency-profile workspaces after the Python foundations runner is verified.

## Full Feature Later

Later implementation tasks should add the Python workspace generator, pytest runner, Pyright/basedpyright LSP bridge, Ruff diagnostics/formatting integration, FastAPI dependency profile, and browser/API tests proving Python and FastAPI editor parity.
