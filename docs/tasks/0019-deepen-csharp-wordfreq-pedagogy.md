# 0019 Deepen C# wordfreq milestone 1 pedagogy

## Type

HITL

## Status

Completed for the milestone 1 pedagogy metadata pass.

## What to build

Deepen the `wordfreq-csharp` Pure Word Counting Core content from strong seed material into a richer study unit that better matches the project standard: theory clarity, examples, misconceptions, transfer to real projects, and active recall.

## Acceptance criteria

- [x] Each milestone 1 lesson has a sharper mental model section and an explicit production-transfer section.
- [x] Key terms are marked in content with enough metadata for the UI to render emphasis and explanatory bubbles later.
- [x] Each lesson has at least three check-yourself prompts that are suitable for review-card generation later.
- [x] Each lesson explicitly links theory to the focused exercises and final milestone rubric through existing exercise/project connection sections.
- [x] Exercise descriptions include common wrong approaches and expected solution characteristics.
- [x] Source references remain metadata pointers and do not copy book text.
- [x] Content validator enforces the added pedagogy fields so future tracks meet the same bar.

## Implementation notes

- Keep the curriculum original. The owned books are quality anchors for depth and topic coverage, not text sources to copy.
- Prefer small, compilable examples over abstract prose where the idea is language-specific.
- Preserve syntax-highlighted code fences for all code examples.
- The validator now requires `## Mental model`, `## Production transfer`, key-term markers, sufficient check-yourself prompts, common wrong approaches, and expected solution characteristics.
- Existing milestone 1 exercises now carry root-level `commonWrongApproaches` and `expectedSolutionCharacteristics` metadata.

## Full implementation note

The full pedagogy feature should later include rendered key-term bubbles, generated review cards from lesson prompts, concept maps, worked-example stepping, "predict before running" prompts, and post-exercise explanations comparing common wrong approaches with robust solutions.

## Blocked by

- 0014 Author the wordfreq-csharp milestone content
- 0015 Validate content quality
