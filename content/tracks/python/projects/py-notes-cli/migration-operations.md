# Migration-Before-Startup Operations

## Scenario

The notes API now has Alembic configuration, readiness checks, and SQLAlchemy persistence. The remaining production discipline is operational: migrations must run before the app starts, and startup should verify schema readiness instead of changing schema.

## Requirements

- Define deployment command order.
- Keep Alembic as the production schema owner.
- Make startup verify schema and database readiness.
- Document rollback and manual-review policy.
- Test the operation contract.

## CLI/API contract

Clients should never see half-migrated behavior. The deployment pipeline runs migrations first, then starts the ASGI server only when schema checks pass.

## Milestone task

Build a migration operation policy that can be used by deployment scripts, startup checks, and tests.

## Rubric

- Correctness: migration commands precede server startup.
- Testing: command order and startup gate are asserted.
- Maintainability: schema ownership is explicit.
- Design: rollback policy is documented separately from code startup.
- Production readiness: old schema blocks traffic.

## Complexity

Migration operations are where good code meets deployment reality. The app should not paper over schema drift by creating tables opportunistically. If code expects a column that is not deployed, startup should fail quickly and loudly before users hit broken routes.

This milestone also makes rollback honest. Some migrations can be downgraded; others need forward fixes or data-specific playbooks. The operation plan should state the policy instead of assuming every downgrade is safe.

## Stretch goals

- Add CI checks for migration command order.
- Add Alembic head verification.
- Add deployment runbook documentation.
