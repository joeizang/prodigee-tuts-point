# 0011 Add static analysis feedback

## Type

AFK

## Status

Completed for exercise validation.

## What to build

Add C# static analysis feedback to exercise and project validation using compiler diagnostics and .NET analyzers where practical.

## Acceptance criteria

- [x] Static analysis runs against generated C# workspaces.
- [x] Analyzer/compiler diagnostics are normalized for UI display.
- [x] Diagnostics include severity, message, rule id, and location when the compiler/analyzer supplies one.
- [x] Static analysis results are stored with attempts or milestone validations.
- [x] UI displays analysis feedback separately from test failures.
- [x] Static analysis participates in project validation evidence.

## Implementation notes

- `IExerciseRunner.RunStaticAnalysisAsync` runs `dotnet build` against the copied run workspace before visible/hidden tests.
- Diagnostics are parsed into `StaticAnalysisDiagnosticRecord` rows and returned as `staticAnalysis` on the run response.
- Build-output parsing handles file-location diagnostics and project-level/locationless diagnostics, and the runner forces English `dotnet` CLI output for stable parsing.
- Attempt history stores static-analysis error and warning counts in `ExerciseRunHistory`.
- Persisted diagnostic details are exposed through `/api/exercises/{exerciseId}/static-analysis`.
- The exercise UI renders static-analysis feedback in its own panel, separate from runner output.
- Milestone AI review consumes recent run history, including static-analysis counts, as project validation evidence.
- Regression tests cover persisted diagnostic retrieval and locationless diagnostics.

## Full implementation note

Project-level milestone validation can later aggregate all required exercise diagnostics into a dedicated validation summary. Current implementation stores and surfaces exercise-level analysis and passes that evidence into advisory AI review.

## Blocked by

- 0008 Generate real .NET exercise workspaces
- 0009 Run C# exercises with visible and hidden tests
