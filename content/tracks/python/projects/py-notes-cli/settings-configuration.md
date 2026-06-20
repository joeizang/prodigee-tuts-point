# Settings and Configuration

## Scenario

The notes API is no longer just local sample code. It needs a configuration boundary that works for development, tests, and production without scattering environment reads through the app.

## Requirements

- Parse environment-style values into typed settings.
- Support `dev`, `test`, and `prod` environments.
- Provide safe development defaults.
- Require explicit production API keys and database paths.
- Keep settings injectable through app factories and dependencies.

## CLI/API contract

Configuration changes how the app starts, not how clients use the API. Public route behavior should remain stable across environments.

## Milestone task

Build a validated settings object and a parser for environment-like dictionaries.

## Rubric

- Correctness: settings parse defaults and explicit values correctly.
- Testing: dev, test, prod, missing, and invalid values are covered.
- Maintainability: raw environment access is isolated.
- Design: route handlers receive configured services rather than reading process state.
- Production readiness: unsafe production defaults fail fast.

## Complexity

Configuration bugs are often invisible until deployment. Treat settings as input validation. A string such as `"true"` needs to become a boolean, a database path needs to become a `Path`, and a missing production key needs to stop startup. This is not glamorous code, but it protects the whole application.

The important habit is centralization. Once raw configuration becomes trusted settings, the rest of the app can depend on typed values instead of repeatedly interpreting process state.

## Stretch goals

- Add `.env` support later with a dedicated settings package.
- Add typed log levels.
- Add separate read and write database URLs.
