# 0021 Render key-term bubbles

## Type

AFK

## Status

Completed.

## What to build

Turn authored `**Term: ...** means ...` lesson markers into visible, accessible key-term highlights with compact definition bubbles.

## Acceptance criteria

- [x] Markdown renderer detects authored key-term markers without requiring per-lesson custom code.
- [x] Key terms are visually emphasized in lesson and milestone markdown.
- [x] Definition bubbles are available on hover and keyboard focus.
- [x] Non-term bold text remains normal bold text.
- [x] Frontend tests, build, and browser smoke pass.

## Implementation notes

- Implemented in the shared `MarkdownText` renderer so all learning markdown benefits automatically.
- The first version extracts the definition from the sentence following `**Term: name** means ...`.
- Review remediation: bounded definition extraction at sentence terminators so later mentions do not bleed into the previous tooltip.
- Review remediation: added `aria-describedby` from focusable term buttons to their tooltip and covered multi-term boundary behavior in frontend tests.

## Full implementation note

The full version should move key terms into structured content metadata, support glossary pages, cross-lesson concept links, examples/misconceptions in the bubble, analytics for difficult terms, and review-card generation from marked terms. This task only renders the authored inline markers.

## Blocked by

- 0019 Deepen C# wordfreq milestone 1 pedagogy
