# Health and Startup Readiness

## Learning objectives

- Separate liveness from readiness.
- Check database connectivity and schema version before reporting ready.
- Fail startup when required production prerequisites are missing.
- Document migration-before-startup deployment order.

## Prerequisites

You should already understand FastAPI app factories, settings, database engine configuration, Alembic migrations, and production startup checks.

## Mental model

Health is an operational contract. Liveness answers "is the process running?" Readiness answers "should traffic be routed here?" Startup checks answer "is it safe to start serving at all?"

**Term: liveness** means the process can respond at all.

**Term: readiness** means dependencies and schema are healthy enough to serve traffic.

## Core idea

Expose safe endpoints:

```text
GET /health/live
GET /health/ready
```

Liveness should be boring and not touch dependencies. Readiness can check database connectivity and expected schema version. Startup checks should fail fast in production if migrations have not run, the database URL is unsafe, or required settings are missing.

## Worked example

```python
def startup_check(checks: ReadinessChecks) -> None:
    result = checks.run()
    if result["status"] != "ready":
        raise RuntimeError("startup readiness checks failed")
```

The deployment command contract should run migrations before the app starts:

```text
uv run alembic upgrade head
uv run uvicorn notes_api.main:create_app --factory
```

## Production transfer

Load balancers and platforms can call readiness before sending traffic. Operators can distinguish process health from dependency readiness. Deployment scripts can make migration order explicit instead of relying on implicit table creation.

## Common mistakes

- Making `/health` run expensive dependency checks on every liveness probe.
- Returning secrets or raw database URLs from health responses.
- Reporting ready when schema version is old.
- Creating tables automatically in production startup after Alembic exists.

## Testing strategy

Test liveness success, readiness success, readiness failure, startup failure, and deployment command order. Readiness tests should verify safe output without secrets.

## Debugging strategy

When readiness fails, inspect the named checks first: database connectivity, migration version, settings, and dependency timeout behavior.

## Exercise connection

`VerifyHealthStartup` asks you to build health/readiness behavior and startup checks that enforce migration-before-startup discipline.

## Project connection

This milestone makes the notes API operationally honest: it only accepts traffic when the app, database, and schema are aligned.

## Check yourself

- What is the difference between liveness and readiness?
- What should startup reject?
- Where is migration-before-startup documented?

## Source reference notes

Use FastAPI lifespan and deployment guidance for lifecycle behavior, and pytest readiness tests to pin down operational contracts.
