# 0014 Author the wordfreq-csharp milestone content

## Type

HITL

## Status

Completed for milestone 1 seed content.

## What to build

Author real rich seed content for `wordfreq-csharp` milestone 1: Pure Word Counting Core. This must be production-quality curriculum content, not placeholder text.

## Acceptance criteria

- [x] `wordfreq-csharp` project metadata is authored.
- [x] Milestone 1 spec is authored.
- [x] Six rich lessons are authored:
  - Text as Data in C#
  - Normalization and Tokenization
  - Dictionaries as Frequency Maps
  - Deterministic Ordering
  - Testing Pure Functions with xUnit
  - Designing a Small Core API
- [x] Ten focused exercises are authored:
  - `NormalizeToLowercase`
  - `KeepAsciiLettersAndDigits`
  - `SplitWordsOnSeparators`
  - `TokenizeSimpleSentence`
  - `TokenizeWithPunctuation`
  - `CountWordsWithDictionary`
  - `UpdateFrequencyMapSafely`
  - `SortFrequenciesDeterministically`
  - `HandleEmptyAndWhitespaceInput`
  - `BuildWordFrequencyAnalyzer`
- [x] Each exercise has starter files, visible tests, hidden tests, hints, and model solution.
- [x] Source references are metadata pointers, not copied book content.
- [x] Milestone rubric covers correctness, design, testing, maintainability, edge cases, and complexity.

## Implementation notes

- Lessons use a consistent structure: learning objectives, prerequisites, core idea, worked examples, common mistakes, exercise connection, project connection, check-yourself prompts, and source-reference notes.
- The milestone spec now includes required behavior, suggested API shape, soft locks, rubric, completion rule, and source-reference notes.
- All ten exercises include visible tests, hidden tests, three-level hints, and model solutions.

## Full implementation note

This completes the first C# milestone seed. Later milestones should meet the same validator bar and can deepen the project into CLI parsing, file input, output formats, performance, streaming, and packaging.

## Blocked by

- 0003 Implement hybrid content loading
