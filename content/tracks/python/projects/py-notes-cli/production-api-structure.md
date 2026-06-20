# Production API Structure

## Scenario

The notes API has enough behavior that a single-file mental model is no longer enough. This milestone organizes the application around module ownership.

## Requirements

- Identify model, service, repository, dependency, and app responsibilities.
- Keep SQL out of route handlers.
- Keep FastAPI out of service code.
- Use the app factory as the composition root.
- Preserve dependency override seams for tests.

## CLI/API contract

The public API behavior remains stable. The change is internal structure: each boundary gets a clear owner.

## Milestone task

Assemble a production-shaped app package in exercise form and prove the routes still work.

## Rubric

- Correctness: the assembled app supports expected notes operations.
- Testing: app factory and route behavior are covered.
- Maintainability: responsibilities are separated by boundary.
- Design: imports point inward toward services and repositories.
- Production readiness: future settings, routers, and migrations have a place to live.

## Complexity

The risk is creating folders without architecture. This milestone focuses on ownership and dependency direction, not file count.

The learner should be able to explain why each piece exists before splitting a real project. If a module boundary does not reduce confusion or protect a dependency direction, it is just decoration. The important outcome is that route handlers stay small, service code remains framework-free, and persistence details cannot leak upward into request handling.

## Stretch goals

- Split the exercise into real files after the workspace supports multi-file edits.
- Add settings and router modules.
- Add package-level docs for import direction.
