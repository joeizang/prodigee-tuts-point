# Pure Word Counting Core

## Goal

Build the reusable core of `wordfreq-csharp`.

## Required behavior

Required behavior:

- Normalize text with the milestone ASCII-first rule.
- Tokenize into words without empty tokens.
- Count words with `Dictionary<string, int>`.
- Sort by count descending, then word ascending.
- Return empty results for empty, whitespace, and separator-only input.

## Suggested core shape

```csharp
public sealed record WordFrequency(string Word, int Count);

public static class WordFrequencyAnalyzer
{
    public static IReadOnlyList<WordFrequency> Analyze(string text)
    {
        throw new NotImplementedException();
    }
}
```

## Soft-lock prerequisites

- Text as Data in C#
- Normalization and Tokenization
- Dictionaries as Frequency Maps
- Testing Pure Functions with xUnit

## Review rubric

- Correctness: handles casing, punctuation, empty input, and deterministic ordering.
- Design: keeps core logic pure and independent from CLI or file I/O.
- Testing: visible tests explain the contract and hidden tests cover edge cases.
- Maintainability: names and result shape are easy to explain.

## Completion rule

The milestone is complete when the analyzer passes visible and hidden tests and the implementation remains small enough to explain.
