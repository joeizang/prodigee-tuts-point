# 0009 Run C# exercises with visible and hidden tests

## Type

AFK

## Status

Completed

## What to build

Implement the trusted-local C# runner that runs visible and hidden tests in generated .NET workspaces with timeouts, output limits, diagnostics, and stored results.

## Acceptance criteria

- [x] Runner executes known `dotnet` commands without arbitrary shell strings.
- [x] Runner applies timeout and output-size limits.
- [x] Runner captures compiler diagnostics, stdout, stderr, and test results.
- [x] Results distinguish visible test failures from hidden test failures without exposing hidden source.
- [x] Attempt results are stored in SQLite.
- [x] Failed, passed, timed out, and runner-error states are represented.
- [x] Runner is behind an abstraction that can support container execution later.

## Remediation notes

- `IExerciseRunner`/`DotnetExerciseRunner` isolates the current local `dotnet test` execution strategy.
- Hidden tests no longer expose source, method names, exception details, or runner output when they fail.
- Hidden tests skipped after visible-test failure are represented as not passed, not silently passed.
- Runner errors now map to a distinct `RunnerError` state.
- API tests cover hidden-test privacy and skipped-hidden-test status behavior.

## Blocked by

- 0008 Generate real .NET exercise workspaces
