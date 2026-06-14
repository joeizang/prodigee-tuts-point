# Remediation: 0001-0009 Review

Date: 2026-06-14

This records the fixes made after `docs/review-notes/0001-0009-review.md`.

## Fixed

- Implemented missing `Cmd/Ctrl+S` and `Cmd/Ctrl+Enter` app-level shortcuts.
- Made theme persistence local-profile keyed, with the original global key retained as a migration fallback.
- Added explicit light theme token rules instead of relying on `:root` fallback variables.
- Added content-backed `/api/curriculum/navigation` and wired the command palette to indexed tracks, projects, milestones, lessons, exercises, and concepts.
- Parsed per-exercise `exercise.yml` metadata during indexing and persisted `Exercise.Kind`.
- Added content-indexing regression tests for exercise kind, repeat indexing, and broken exercise metadata diagnostics.
- Cleared EF tracked indexed entities after set-based reindex deletes to make repeat indexing safe in the same DbContext.
- Replaced the hand-rolled lesson markdown renderer with a marked-backed GFM renderer.
- Changed Dashboard mastery/diagnostic metrics to read learner endpoints instead of fixed placeholder values.
- Added lesson markdown-body search in addition to SQL-backed metadata search.
- Added concept detail UI route with notes support.
- Changed curriculum search to database-backed `LIKE` filtering.
- Added focused concept and source-reference search deep links.
- Added source-reference anchors on the sources page.
- Replaced hardcoded soft-lock heuristics with data-driven curriculum-order logic.
- Added regression tests for concept search, concept notes, source-reference notes, and soft-lock behavior.
- Added a runner abstraction for C# exercise execution.
- Changed exercise execution to use isolated per-run workspace copies.
- Replaced the empty generated solution with real project entries.
- Moved starter code and visible/hidden exercise checks into content-authored files/metadata.
- Suppressed hidden-test implementation details from learner-visible failure output and diagnostics.
- Made skipped hidden tests report as not passed when visible tests fail.
- Added regression tests for hidden-test privacy, skipped hidden status, and real generated solution contents.
- Deleted per-run copied workspaces after execution and added run-root cleanup.
- Refreshed generated workspace support files when authored content changes while preserving learner-editable source files.
- Moved exercise-specific lint feedback into a real Roslyn `DiagnosticAnalyzer` execution path.
- Kept Monaco shortcut handlers and app command wiring maintainable with stable React callbacks.

## Intentional v1 Scope

- The durable editable workspace remains keyed by profile and exercise so learner edits survive navigation.
- The isolated workspace is created for execution, not for every page visit.
- Full Roslyn code-fix/refactoring provider parity and persistent workspace snapshot caching remain open for 0007.
- Structured lesson fields for review prompts/project connections, full mastery visualization, and full AI/static-review loops remain later tasks beyond 0001-0009.

## Verification

- `dotnet test ProdigeeTutsPoint.slnx`
- `npm run lint`
- `npm run test`
- `npm run build`
