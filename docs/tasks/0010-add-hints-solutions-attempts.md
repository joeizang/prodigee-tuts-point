# 0010 Add progressive hints, solutions, and attempts

## Type

AFK

## Status

Completed for the current C# exercise slice.

## What to build

Add exercise attempt lifecycle, progressive hints, intentional solution unlock, and model solution display after passing or unlock.

## Acceptance criteria

- [x] Exercise attempts are tracked per learner profile.
- [x] Attempt history shows run status and result summary.
- [x] Current authored C# exercise supports conceptual, API/approach, and structural hints.
- [x] Learner can intentionally unlock a solution when the exercise has an authored solution.
- [x] Full model solution appears after passing or unlock for authored solutions.
- [x] Hint/solution usage is recorded as mastery evidence.
- [x] UI clearly distinguishes learner code from model solution.

## Implementation notes

- Run history is stored in `ExerciseRunHistory` and exposed through `/api/exercises/{exerciseId}/attempts`.
- Hints and model solutions are authored in `exercise.yml`.
- Hint usage and solution unlocks are stored separately from run history and emit `ConceptMasteryEvidence`.
- The first authored exercise with all three hint levels and a model solution is `normalize-to-lowercase`.
- Regression tests now cover the "solution visible after passing" branch and hint/unlock mastery evidence.
- Frontend tests cover hint reveal, model-solution rendering, static-analysis display, and attempt-history display.

## Full implementation note

Remaining exercises should receive their own authored hint ladders and model solutions as content quality work expands beyond the first slice.

## Blocked by

- 0007 Build the Monaco C# workspace
- 0009 Run C# exercises with visible and hidden tests
