# Deterministic Ordering

## Learning objectives

- Sort word frequencies by count descending and word ascending.
- Explain why dictionary iteration order is not a user-facing contract.
- Write tests that prove both the primary order and the tie-breaker.
- Keep sorting separate from counting so each stage remains easy to inspect.

## Prerequisites

You should understand `Dictionary<string, int>`, records, and basic LINQ ordering. You should already have a frequency map from clean tokens.

## Mental model

**Term: deterministic output** means the same input always produces the same observable order. **Term: tie-breaker** means the secondary rule used when the primary sort key is equal.

Counting creates facts; ordering creates a contract for readers and tests. A result that is mathematically correct but randomly ordered is still a poor interface when humans, snapshots, or downstream tools compare it.

## Production transfer

Stable ordering matters in APIs, CLIs, generated reports, tests, and data migrations. If two equal-count words swap positions across runs, users lose trust and automated comparisons become noisy. Tie-breakers are a small design choice that prevents a surprising amount of operational friction.

## Core idea

Counting tells you the data. Ordering tells you how to present it. For `wordfreq-csharp`, the output contract is stable:

1. Higher counts first.
2. If counts are equal, lower alphabetical word first.

```csharp
var frequencies = new Dictionary<string, int>
{
    ["beta"] = 1,
    ["alpha"] = 1,
};

var sorted = WordFrequencyAnalyzer.SortFrequencies(frequencies);

Assert.Equal(["alpha", "beta"], sorted.Select(item => item.Word));
```

The tie-breaker is not cosmetic. Without it, tests become unreliable and CLI output becomes hard to compare.

## Worked implementation shape

```csharp
return frequencies
    .Select(pair => new WordFrequency(pair.Key, pair.Value))
    .OrderByDescending(item => item.Count)
    .ThenBy(item => item.Word, StringComparer.Ordinal)
    .ToList();
```

`StringComparer.Ordinal` matches the ASCII-first, deterministic nature of the milestone. Culture-sensitive ordering can be valid in user-facing applications, but that is not the current contract.

## Common mistakes

- Sorting words ascending before sorting counts descending.
- Forgetting the tie-breaker.
- Returning dictionary entries directly and assuming iteration order is stable.
- Mutating shared collections while trying to sort them.

## Exercise connection

This lesson supports `SortFrequenciesDeterministically` and the final analyzer integration drill. Hidden tests should include equal counts and count differences so both ordering levels are proven.

## Project connection

Later CLI output will be judged by exact line order. A deterministic core makes that output predictable before file I/O and formatting add more moving parts.

## Check yourself

1. What is the primary sort key?
2. What is the tie-breaker?
3. Why is dictionary iteration not enough for project output?

## Source reference notes

Use your LINQ and collections references for `OrderByDescending`, `ThenBy`, projections, and comparer choices. The project chooses ordinal ordering for repeatable engineering feedback.
