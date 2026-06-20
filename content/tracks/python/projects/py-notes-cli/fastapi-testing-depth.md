# FastAPI Testing Depth

## Scenario

The notes API now has meaningful status codes, Pydantic contracts, and dependency boundaries. The test suite must cover more than one happy path before persistence changes become safe.

## Requirements

- Test successful route behavior.
- Test duplicate, missing, semantic validation, and structural validation behavior.
- Test dependency override behavior.
- Test storage isolation with a fresh app per test.
- Assert status codes and response bodies together.

## CLI/API contract

API tests describe the public HTTP contract. Service and repository tests describe internal behavior. Keep those concerns separate enough that a persistence rewrite does not break API tests unless the public behavior changes.

## Milestone task

Build a small test harness around the FastAPI app and prove each important HTTP outcome.

## Rubric

- Correctness: the API returns expected status codes and bodies.
- Testing: dependency overrides, isolated storage, validation, not-found, and conflict cases are covered.
- Maintainability: tests explain the behavior category being protected.
- Design: fake dependencies are used where they clarify route behavior.
- Persistence readiness: tests can survive the JSON-to-SQLite storage swap.

## Complexity

The complexity is deciding what to test at which layer. Too little testing misses regressions. Too much end-to-end testing makes every storage change expensive.

Use test names as design documentation. A test named `test_invalid_tag_returns_400` tells future readers that semantic validation is intentionally different from Pydantic's structural `422`. That clarity matters when the implementation changes.

## Stretch goals

- Add pytest fixtures for app, client, and fake service.
- Add shared API contract tests that can run against JSON and SQLite app configurations.
- Add OpenAPI schema assertions for public models.
