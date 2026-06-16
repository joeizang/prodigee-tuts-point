# Pure Word Counting Core

## Goal

Build the reusable, pure-function core of `wordfreq-csharp`. This milestone deliberately avoids command-line parsing and file I/O. The deliverable is a small analyzer API that later project milestones can wrap without rewriting parsing or counting behavior.

## Required behavior

- Normalize text with the milestone ASCII-first rule.
- Treat ASCII letters and digits as word characters.
- Treat punctuation, symbols, and whitespace as separators.
- Tokenize without producing empty words.
- Count words with `Dictionary<string, int>`.
- Sort output by count descending, then word ascending with ordinal comparison.
- Return empty results for null, empty, whitespace-only, and separator-only input.

## Suggested core shape

```csharp
public sealed record WordFrequency(string Word, int Count);

public static class WordFrequencyAnalyzer
{
    public static IReadOnlyList<WordFrequency> Analyze(string? text)
    {
        var tokens = Tokenize(text);
        var counts = CountWords(tokens);
        return SortFrequencies(counts);
    }
}
```

The helper methods may remain public during the learning milestone because the exercises target each one directly. In a production library, you would revisit which helpers are part of the public contract.

## Soft-lock prerequisites

- Text as Data in C#
- Normalization and Tokenization
- Dictionaries as Frequency Maps
- Deterministic Ordering
- Testing Pure Functions with xUnit
- Designing a Small Core API

## Rubric

### Correctness

- Handles casing, punctuation, repeated separators, null, empty input, and whitespace.
- Counts repeated words accurately.
- Does not emit empty tokens.
- Applies the exact deterministic ordering rule.

### Design

- Keeps the analyzer core pure.
- Separates normalization, tokenization, counting, and sorting into explainable steps.
- Uses `WordFrequency` as a structured result instead of formatted strings.
- Avoids file, console, and command-line concerns.

### Testing

- Visible tests demonstrate the contract.
- Hidden tests protect edge cases.
- Pure functions are tested through return values.
- Tests do not depend on dictionary iteration order.

### Maintainability

- Names describe behavior rather than implementation trivia.
- Methods remain short enough to reason about.
- The ASCII-first limitation is documented.
- Future Unicode or CLI work can be added around the core.

### Complexity

- Counting should be linear in the number of tokens.
- Sorting should be proportional to the number of distinct words.
- The implementation should avoid repeated full rescans when a single pass is enough.

## Completion rule

The milestone is complete when the analyzer passes visible tests, hidden tests, and static analysis; the implementation can be explained from the method names; and the AI review is advisory rather than a substitute for tests.

## Source reference notes

The source references for this milestone point to relevant book chapters and topics. They are anchors for deeper study, not copied source material.
