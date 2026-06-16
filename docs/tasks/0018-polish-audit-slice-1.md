# 0018 Polish and audit Slice 1

## Type

AFK

## Status

Completed for the first Slice 1 polish pass.

## What to build

Run a focused polish and audit pass over Vertical Slice 1 so the completed foundation is quiet, coherent, and ready for deeper C# project work.

## Acceptance criteria

- [x] API startup does not log expected duplicate-column failures on existing local SQLite databases.
- [x] Built app and two-server dev mode both pass a browser smoke test for dashboard, milestone, lesson, exercise, review, sources, and settings.
- [x] Desktop and mobile viewports are checked for sidebar, theory cluster, editor workspace, notes, settings, and review cards.
- [x] Any remaining Slice 1 rough edges are captured as explicit tasks or review notes, not left as vague follow-up.
- [x] Full backend, frontend, content validator, and live smoke verification passes after the polish changes.

## Implementation notes

- Replaced the blind `ALTER TABLE Exercises ADD COLUMN Kind` startup operation with a metadata check, avoiding the benign-but-noisy SQLite failure log on already-upgraded local databases.
- Fixed mobile horizontal overflow on the milestone page by containing syntax-highlighted code blocks and constraining the mobile nav/shell width.
- Added explicit follow-up tasks for deeper C# pedagogy and the next `wordfreq-csharp` CLI/file-I/O milestone.

## Full implementation note

This task is not meant to add major product scope. The full hardening vision should eventually include Playwright E2E tests with screenshots, accessibility checks, performance budgets for Monaco/Roslyn, one-command local startup validation, and repeatable packaging checks.

## Blocked by

- 0017 Harden Vertical Slice 1
