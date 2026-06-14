# 0006 Add light diagnostic and soft locks

## Type

AFK

## Status

Completed

## What to build

Add a light C# diagnostic and soft-lock recommendations so the app can suggest a starting point without blocking progress.

## Acceptance criteria

- [x] C# diagnostic checks basic methods, strings, collections, dictionaries, xUnit assertions, and edge-case reasoning.
- [x] Diagnostic attempts are stored per local learner profile.
- [x] Diagnostic results recommend primers, drills, or `wordfreq-csharp` milestone 1.
- [x] Soft locks show prerequisite warnings on lessons, exercises, and milestones.
- [x] Learner can continue despite warnings.
- [x] Diagnostic results contribute evidence to concept mastery.

## Remediation notes

- Soft locks are now derived from indexed curriculum order instead of hardcoded lesson/exercise ids.
- Lesson soft locks point to earlier lessons in the same module.
- Exercise soft locks point to milestone lessons that precede or align with the exercise order.
- Milestone soft locks point to earlier milestones in the same project, so milestone 1 has no self-referential warning.
- API tests cover first/second lesson and first/second exercise prerequisite behavior.

## Blocked by

- 0004 Render the first curriculum path
