# 0013 Add mastery, review cards, gamification, and time tracking

## Type

AFK

## Status

Completed for the current C# vertical slice.

## What to build

Add evidence-based mastery, basic review cards, restrained private gamification, and active study-time tracking across lessons, exercises, projects, and review.

## Acceptance criteria

- [x] Concept mastery statuses are represented: Not Started, Introduced, Practiced, Applied, Reliable, Needs Review.
- [x] Exercise attempts, study/project usage, review recall, failures, and time decay contribute mastery evidence.
- [x] Basic review cards are concept-linked.
- [x] Dashboard shows review due count.
- [x] Study streak, milestone completion, mastery status, and track/project progress are visible.
- [x] Active time tracking records lessons, exercises, projects, milestones, and review with idle timeout.
- [x] Gamification remains private/personal with no social comparison.

## Implementation notes

- Review cards are seeded for the six C# concepts and are stored in SQLite.
- Review recall writes `ReviewCardAttempt` plus `ConceptMasteryEvidence`.
- Exercise runs now write positive or failure evidence for linked concepts.
- Active study time is posted from the frontend with an idle timeout and capped flush size.
- Dashboard metrics are personal-only: review due count, streak, exercise/milestone progress, mastery status, and a private gamification policy.

## Full implementation note

The current status engine is intentionally simple and transparent. Future tracks can add weighted concept maps, richer spacing intervals, and milestone-specific completion policies without changing the learner-state storage shape.

## Blocked by

- 0004 Render the first curriculum path
- 0009 Run C# exercises with visible and hidden tests
- 0010 Add progressive hints, solutions, and attempts
