# 0003 Implement hybrid content loading

## Status

Completed

## Type

AFK

## What to build

Implement the hybrid content model: authored curriculum files under `content/`, SQLite records for indexed metadata and learner state, and a repeatable indexing flow.

## Acceptance criteria

- [x] Content folder structure supports tracks, projects, milestones, lessons, exercises, and source metadata.
- [x] App can parse content metadata files.
- [x] App indexes tracks, lessons, concepts, exercises, projects, milestones, and source references into SQLite.
- [x] Indexing is repeatable without duplicating records.
- [x] Content records include stable ids and content version metadata.
- [x] Broken or invalid content produces useful diagnostics.

## Remediation notes

- Per-exercise `exercise.yml` files are parsed during indexing.
- `exercise.yml` `kind` is required, validated against the track exercise id, persisted on `Exercise.Kind`, and exposed through exercise detail responses.
- Indexing clears EF tracked entities after set-based deletes so repeated indexing in the same DbContext does not create duplicate tracking conflicts.
- Regression tests cover `kind` persistence, repeat indexing, and broken exercise metadata diagnostics.

## Blocked by

- 0001 Scaffold the .NET 10 and React app
