# Packaging and Running

## Scenario

The API needs a clear way to run. Developers and deployment tools should not guess the ASGI import target, runtime settings, or startup checks.

## Requirements

- Define a stable app factory target.
- Build a uvicorn command for local running.
- Expose runtime metadata without secrets.
- Provide a health endpoint.
- Validate startup requirements before serving.

## CLI/API contract

Runtime packaging should make the API easier to start without changing the public route contract.

## Milestone task

Implement startup metadata, health checks, and run-command construction.

## Rubric

- Correctness: generated run command targets the app factory.
- Testing: metadata, health, and invalid startup paths are covered.
- Maintainability: runtime concerns are not mixed into route business rules.
- Design: health output avoids secrets and includes useful deployment facts.
- Production readiness: startup checks catch missing configuration early.

## Complexity

Running is part of the product. A backend that only works from a particular shell directory is fragile. A clear import target and command make local development, CI, and deployment more repeatable. Health output gives operators a safe way to verify that the process is alive and configured.

The health route should be useful but boring. It should show safe runtime facts, not secrets, and startup checks should catch missing production requirements before health ever returns `ok`.

This closes the loop from code to a repeatable running process.

## Stretch goals

- Add a console script entry point.
- Add Docker-specific run commands.
- Add readiness versus liveness checks.
