# FastAPI Dependency Boundaries

## Scenario

The notes API currently works, but service construction is still too close to the route handlers. This milestone moves service resolution behind FastAPI dependencies so handlers are easier to test and later replace with database-backed services.

## Requirements

- Build the app with an app factory.
- Store runtime configuration on the app rather than in global variables.
- Resolve `NotesService` through `Depends`.
- Allow tests to override the notes-service dependency.
- Keep route handlers focused on HTTP translation and service calls.

## CLI/API contract

The CLI and API should both depend on the same service contract. FastAPI dependencies decide how the API receives the service; they do not change the service behavior.

## Milestone task

Refactor the API shape so the route handler declares `service: NotesService = Depends(...)` and tests can replace that dependency with a fake service.

## Rubric

- Correctness: default app behavior still reads and writes notes through the configured path.
- Testing: dependency overrides prove handlers can run against a fake service.
- Maintainability: service construction is centralized in a provider function.
- Design: route handlers do not directly instantiate concrete storage services.
- Production readiness: the structure can later support database sessions and authenticated user context.

## Complexity

The new idea is inversion of control. The handler asks for a service; FastAPI decides how to provide it.

## Stretch goals

- Add separate providers for configuration and service construction.
- Add a fake service class used by multiple route tests.
- Document which dependencies are safe to override in tests.
