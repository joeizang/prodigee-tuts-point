# 0016 Add export and import

## Type

AFK

## Status

Completed for manual profile-scoped learner state portability.

## What to build

Add manual export/import for learner state and content version metadata while excluding provider secrets by default.

## Acceptance criteria

- [x] Export includes learner profile id, progress, attempts, test results, mastery state, review cards, study time, source metadata, content index metadata, and provider settings excluding secrets.
- [x] Export excludes provider secrets by default.
- [x] Import restores learner state into a compatible app/database.
- [x] Import checks content version compatibility.
- [x] Import reports conflicts or incompatible content clearly.
- [x] Export/import works offline.

## Implementation notes

- API endpoints live under `/api/portable-state`.
- Export is JSON and profile-scoped by `profileId`.
- Provider settings export `SecretName` only; no secret values are represented.
- Import rejects incompatible export format, app id, content versions, missing content ids, and payloads that claim secret values are present.
- Settings UI exposes manual export and paste-based import for offline use.

## Full implementation note

This is manual portability, not automatic backup. Future backup work can add encrypted local snapshots, backup rotation, and explicit opt-in secret export without changing the v1 JSON compatibility checks.

## Blocked by

- 0002 Build the local app shell
- 0013 Add mastery, review cards, gamification, and time tracking
