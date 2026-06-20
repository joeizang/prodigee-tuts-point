# SQLAlchemy and SQLModel Boundaries

## Scenario

The notes API has outgrown raw SQL exercises. Before adding a concrete ORM package to the runtime, the project needs to define where ORM models, sessions, repositories, services, and response models belong.

## Requirements

- Describe note table columns, constraints, and indexes.
- Define repository methods around use cases.
- Keep services independent from ORM sessions.
- Keep route responses independent from ORM row objects.
- Document where SQLAlchemy or SQLModel will be introduced.

## CLI/API contract

Clients should not know whether data comes from raw sqlite3, SQLAlchemy, SQLModel, or PostgreSQL. The API contract remains notes and response models.

## Milestone task

Produce and test an ORM mapping plan that can later become SQLAlchemy/SQLModel code.

## Rubric

- Correctness: table mapping includes required fields and constraints.
- Testing: repository contract shape and mapping decisions are covered.
- Maintainability: ORM concepts stay below service and route boundaries.
- Design: repository methods are use-case-oriented rather than generic table access.
- PostgreSQL readiness: constraints and indexes anticipate a real relational database.

## Complexity

The important decision is boundary placement. ORM packages can make code shorter, but they can also leak persistence concerns everywhere. The model is not the API response. The session is not the service. The repository is not a generic escape hatch for arbitrary queries. This milestone keeps those lines visible.

## Stretch goals

- Add real SQLAlchemy after dependency installation.
- Compare SQLModel table models with separate Pydantic response models.
- Add shared repository contract tests.
