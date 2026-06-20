# SQLAlchemy Repository Implementation

## Scenario

The notes app now has repository contracts, SQLite persistence experience, and an ORM boundary plan. This milestone turns those pieces into the production implementation shape for SQLAlchemy while preserving the service and route contracts already built.

## Requirements

- Declare the required ORM dependency set.
- Define the `NoteRow` table model shape.
- Keep SQLAlchemy sessions inside the repository layer.
- Map rows to plain service records before returning data.
- Preserve create, list, get, update, delete, and search behavior.

## CLI/API contract

Public behavior should not change. The CLI and FastAPI routes still deal in note commands, service records, Pydantic request models, and Pydantic response models. SQLAlchemy is a storage implementation detail.

## Milestone task

Produce a concrete SQLAlchemy repository implementation blueprint that can be converted into real modules as soon as the uv lock includes the ORM dependencies.

## Rubric

- Correctness: the model includes title, body, tags, archived, and timestamps.
- Testing: repository behavior remains contract-tested through service-facing methods.
- Maintainability: SQLAlchemy imports stay below route and service boundaries.
- Design: row-to-record mapping is explicit and reusable.
- Production readiness: session scope, commit, rollback, and close behavior are named.

## Complexity

The dangerous part of adding an ORM is not the first query. It is the slow spread of ORM details across the app. A high-quality implementation keeps SQLAlchemy powerful but contained: rows are rows, service records are service records, and response models are response models. That separation makes tests clearer, future migrations safer, and PostgreSQL adoption less disruptive.

This milestone is dependency-gated because the local environment cannot resolve new PyPI packages without network access. The content still records the exact package set and implementation shape so the next `uv sync` can activate real imports without redesigning the curriculum.

## Stretch goals

- Add real SQLAlchemy modules after the lockfile can be updated.
- Add repository integration tests against temporary SQLite databases.
- Add transaction failure tests for duplicate titles and rollback behavior.
