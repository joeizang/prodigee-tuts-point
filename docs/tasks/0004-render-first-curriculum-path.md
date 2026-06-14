# 0004 Render the first curriculum path

## Status

Completed

## Type

AFK

## What to build

Render the first C# curriculum path end to end: C# track, `wordfreq-csharp` project, milestone 1, theory cluster, lesson reader, and project milestone page.

## Acceptance criteria

- [x] C# track page renders from indexed content.
- [x] `wordfreq-csharp` project page renders from indexed content.
- [x] Milestone 1 page shows project-backed mastery flow.
- [x] Lesson reader supports rich sections, code examples, review prompts, and project connection.
- [x] Milestone page lists required lessons and exercises.
- [x] Soft-lock prerequisite warnings can be displayed even if the learner can continue.

## Remediation notes

- Lesson/milestone markdown now renders through a real GFM parser instead of a hand-rolled paragraph/list parser.
- Markdown rendering supports ordered lists, unordered lists, blockquotes, links, tables, inline code, and syntax-highlighted C# fenced code.
- Dashboard metrics now read per-profile diagnostic and concept-mastery endpoints instead of displaying fixed mastery placeholders.

## Full implementation note

Structured review prompts and project-connection sections should become first-class lesson fields in authored content, rather than remaining only markdown headings. Full milestone mastery visualization remains part of the broader mastery/gamification work.

## Blocked by

- 0002 Build the local app shell
- 0003 Implement hybrid content loading
