# 0016 Add export and import

## Type

AFK

## What to build

Add manual export/import for learner state and content version metadata while excluding provider secrets by default.

## Acceptance criteria

- [ ] Export includes learner profile, progress, attempts, test results, mastery state, review cards, settings, source metadata, content index metadata, and provider settings excluding secrets.
- [ ] Export excludes provider secrets by default.
- [ ] Import restores learner state into a compatible app/database.
- [ ] Import checks content version compatibility.
- [ ] Import reports conflicts or incompatible content clearly.
- [ ] Export/import works offline.

## Blocked by

- 0002 Build the local app shell
- 0013 Add mastery, review cards, gamification, and time tracking
