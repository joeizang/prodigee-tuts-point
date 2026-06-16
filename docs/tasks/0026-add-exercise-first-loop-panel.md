# 0026 Add exercise-first loop panel

## Type

AFK

## Status

Completed

## What to build

Add an exercise-first sequencing panel to milestone pages so the learner sees code tasks as the active learning spine, with relevant theory links attached to each exercise.

## Acceptance criteria

- [x] Milestone pages render an exercise-first loop ahead of the legacy lessons/exercises lists.
- [x] Each exercise links directly to its coding workspace.
- [x] Each exercise row includes nearby study links to relevant milestone lessons.
- [x] The feature complements, rather than replaces, project-backed mastery.
- [x] Add frontend regression coverage for exercise and lesson links.

## Verification

- `npm run lint && npm run test && npm run build`
- `dotnet test`
- Browser smoke: `streaming-and-scale` milestone renders Exercise-First Loop with coding task links.
- Review remediation verification: frontend tests now render the full milestone page and assert Study Links / Exercise-First Loop ordering before the legacy lists.
- Review remediation verification: the loop test now asserts both the exercise href and the lesson href within the loop panel.

## Full implementation note

Later work should map exercises to concepts and lessons precisely from content metadata, show current attempt status, recommend the next exercise from learner state, and support "predict, code, run, review" micro-loops.
