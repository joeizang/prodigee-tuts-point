# Python IntelliSense Dependency Verification

## Scenario

The Python track now depends on FastAPI, Pydantic, SQLAlchemy, SQLModel, Alembic, psycopg, pytest, Pyright, basedpyright, and Ruff. The editor must resolve those packages in generated workspaces, or the learning experience degrades even when tests pass.

## Requirements

- List required import probes.
- Use the root uv project for dependency resolution.
- Verify Pyright or basedpyright missing-import behavior.
- Verify Ruff lint/format behavior.
- Keep generated workspace `PYTHONPATH` aligned with the runner.

## CLI/API contract

The verification contract should prove that editor diagnostics and test execution agree about installed packages.

## Milestone task

Define the dependency resolution probes and tooling commands required for world-class Monaco Python IntelliSense.

## Rubric

- Correctness: all framework and database packages are included.
- Testing: Pyright and Ruff command expectations are explicit.
- Maintainability: generated workspaces use the root dependency environment.
- Design: editor tooling is treated as a feature.
- Production readiness: dependency drift is caught before learners see false diagnostics.

## Complexity

IntelliSense failures are not cosmetic in a learning app. False missing-import diagnostics teach the wrong lesson and reduce trust in the workspace. Verifying dependencies makes the editor part of the tested surface.

The check should focus on import resolution and command shape first. Later integration work can assert completions, hover, and signature help at specific source positions.

This milestone also protects future package additions. Every major dependency introduced into the curriculum should join the probe list so editor quality does not regress silently.

## Stretch goals

- Add automated Pyright JSON diagnostic parsing.
- Add Monaco completion smoke tests.
- Add hover/signature verification for FastAPI and SQLAlchemy symbols.
