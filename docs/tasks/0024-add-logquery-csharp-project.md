# 0024 Add logquery-csharp project

## Type

AFK

## Status

Completed

## What to build

Add the second C# project, `logquery-csharp`, as a project-backed mastery loop focused on operational tooling: parse log lines into records, filter events, group summaries, and report malformed input deliberately.

## Acceptance criteria

- [x] Add a new project with at least one milestone, theory cluster, source anchors, focused exercises, and rubric.
- [x] Cover structured parsing, parse failures, optional filtering, grouped summaries, deterministic output, and operational diagnostics.
- [x] Add focused exercises for parsing, filtering, level counting, and an integration query runner.
- [x] Generate real .NET exercise workspaces for the new exercises with visible and hidden tests.
- [x] Add API regression tests proving the project, milestone, theory cluster, navigation, and generated workspaces are reachable.
- [x] The project must deepen senior-engineer skills beyond word frequency by introducing an operational investigation tool.
- [x] Existing content quality validation passes with the new material.

## Verification

- `dotnet test`
- `npm run lint && npm run test && npm run build`
- Live API smoke: `logquery-csharp` returns the `parse-filter-summarize` milestone and generated exercise workspace.
- Browser smoke: `logquery-csharp` project and milestone pages render in the running app with `ParseLogLine` and `RunLogquerySummary`.
- Review remediation verification: `RunEndpointExecutesLogqueryCapstoneVisibleAndHiddenTests` proves the repaired capstone passes visible and hidden tests through the real runner.

## Implementation notes

- This first project milestone is intentionally not a full log platform. It teaches disciplined parsing and query composition before adding files, time ranges, JSON, or multiline stack traces.
- Keep parsing, filtering, grouping, and formatting separately testable.
- Malformed input must be visibly different from a successful query with zero matches.
- Review remediation: added the missing `ParseManyLogLines` focused exercise.
- Review remediation: expanded the `RunLogquerySummary` model solution so it replaces every method it depends on instead of calling untouched stubs.

## Full implementation note

Later milestones should add CLI option parsing, file streaming, time-window filters, JSON log support, multiline stack traces, structured output formats, saved queries, benchmark fixtures, and integration with the review/gamification loop.

## Blocked by

- 0022 Harden C# language-service parity and caching
