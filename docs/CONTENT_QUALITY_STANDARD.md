# Content Quality Standard

## Standard

Content must be above ordinary tutorial quality. It should be deep enough that a serious learner can use it as a primary study path, not merely a refresher.

The first implementation must include real rich seed content, not placeholders. Building against placeholder lessons would let the UI, schema, and workflow optimize for shallow content by accident.

## Acceptance Criteria for Lessons

A lesson is accepted only if it:

- Teaches why the concept matters in real projects.
- Builds from fundamentals to production use.
- Includes multiple examples with increasing realism.
- Shows common mistakes and how to diagnose them.
- Includes at least one testable exercise.
- Connects the topic to a larger project.
- Makes tradeoffs explicit instead of presenting rules as magic.
- Avoids shallow "what is X" filler.

## Acceptance Criteria for Exercises

An exercise is accepted only if it:

- Tests one or more named capabilities.
- Can be checked automatically.
- Has meaningful failure feedback.
- Includes realistic constraints.
- Forces the learner to think, not just copy syntax.

## Acceptance Criteria for Projects

A project is accepted only if it:

- Produces a real executable or service.
- Has a clear user-facing contract.
- Requires tests.
- Includes error handling.
- Includes maintainability concerns.
- Exercises at least one senior-engineering judgment point.
- Has stretch goals that deepen the design rather than adding decoration.

## Anti-Slop Rules

- Do not create generic introductory pages with no exercises.
- Do not stop at syntax when the concept has production consequences.
- Do not use one toy example as the whole lesson.
- Do not generate projects that are CRUD wrappers with no engineering pressure.
- Do not mark a lesson complete without examples, exercises, and review prompts.
- Do not hide tradeoffs.
- Do not let gamification replace evidence of competence.

## Review Rubric

Score each lesson from 0-4 on:

- Conceptual clarity
- Practical usefulness
- Example quality
- Exercise quality
- Production realism
- Debugging value
- Project connection

Any lesson below 3 in any category needs revision before being promoted.

## Content Validation Tooling

Version 1 should include a content validator command.

Validator checks:

- Lesson has objectives
- Lesson has prerequisites
- Lesson has examples
- Lesson has exercises
- Lesson has project connection
- Exercise has visible tests
- Exercise has hidden tests
- Exercise has hints
- Exercise has model solution
- Project milestone has rubric
- Source references use valid ids
- Internal links are not broken

Full validation vision:

- Quality score per lesson
- Coverage dashboard
- Missing concept map
- Stale source references
- Exercise difficulty calibration
- AI-assisted content review
- Authoring studio validation gates
