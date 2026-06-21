# Health and Startup Readiness

## Scenario

The notes API has database configuration, migrations, and production packaging. It now needs operational truthfulness. A running process is not necessarily ready to receive traffic, and a healthy database connection is not enough if migrations have not been applied.

## Requirements

- Expose a liveness endpoint that avoids dependency checks.
- Expose a readiness endpoint that checks database and schema readiness.
- Return safe health payloads with no secrets.
- Fail startup when production prerequisites are missing.
- Document migration-before-startup command order.
- Test both ready and not-ready states.

## CLI/API contract

`GET /health/live` reports process liveness. `GET /health/ready` reports dependency and schema readiness. Deployment commands must run migrations before starting the ASGI server.

## Milestone task

Build health/readiness behavior and startup checks that enforce database and migration readiness.

## Rubric

- Correctness: liveness and readiness are separate.
- Testing: ready, not-ready, and startup-failure paths are covered.
- Maintainability: checks are named and composable.
- Design: health output is safe for operators.
- Production readiness: migration-before-startup ordering is explicit.

## Complexity

Health endpoints are operational APIs. They influence load balancers, deploy scripts, and incident diagnosis. If readiness lies, traffic reaches broken instances. If liveness is too expensive, probes can create failure pressure. Splitting the two keeps the system honest and predictable.

The startup check should not replace migrations. It should verify that migrations already happened. That distinction matters because production schema changes need review, ordering, and rollback plans.

## Stretch goals

- Add dependency timeout budgets.
- Add detailed but safe readiness components.
- Add deployment smoke tests after migration commands.
