# 0008 Generate real .NET exercise workspaces

## Type

AFK

## Status

Completed

## What to build

Generate real per-attempt .NET workspaces for C# exercises, including exercise project, test project, solution file, starter files, visible tests, hidden runner/test files, and cleanup safeguards.

## Acceptance criteria

- [x] Each attempt gets an isolated temp workspace.
- [x] Workspace contains real `.csproj` files and a solution.
- [x] Starter files are copied from content.
- [x] Visible tests are present in the learner-visible workspace.
- [x] Hidden tests are available to the runner but not exposed in the UI.
- [x] Workspace generation is deterministic and idempotent for an attempt.
- [x] Cleanup removes old temporary workspaces safely.

## Remediation notes

- The editable workspace remains stable per profile/exercise so learner files persist between visits.
- Each runner execution now copies that editable workspace into a separate timestamped run workspace under `App_Data/exercise-runs`.
- The generated solution now includes both project entries instead of being an empty `.sln`.
- Starter code is copied from content under `content/tracks/csharp/exercises/_shared/WordFrequencyAnalyzer.cs`.
- Visible and hidden exercise assertions are authored in each `exercise.yml` under `workspace.visibleTest` and `workspace.hiddenTest`.
- Cleanup now only considers top-level editable workspaces, avoiding recursive deletion of nested project directories.
- Run workspace copies are deleted immediately after each run and old run roots are included in best-effort cleanup.
- Generated support files such as solution, projects, visible tests, hidden tests, and `.editorconfig` refresh when authored content changes; learner-editable source files remain write-if-missing so personal work is not overwritten.

## Full implementation note

The editable workspace is intentionally stable per profile/exercise in this version. A future multi-attempt model should introduce explicit attempt ids so parallel attempts can be isolated without losing the simple "resume my current work" behavior.

## Blocked by

- 0003 Implement hybrid content loading
