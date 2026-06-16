# Dictionaries as Frequency Maps

## Learning objectives

- Use `Dictionary<string, int>` as a direct model for word counts.
- Choose an update pattern that handles missing and existing keys correctly.
- Keep counting independent from tokenization and sorting.
- Recognize when dictionary behavior needs an explicit comparer or a different storage strategy.

## Prerequisites

You should know generic collections, `foreach`, `TryGetValue`, and basic value updates. You should have already tokenized text into clean words.

## Mental model

**Term: frequency map** means a dictionary where each key is an observed value and each value is the number of observations. **Term: lookup/update path** means the small branch that decides whether the current token starts a count or increments an existing count.

The dictionary is not just a container here; it is the central model. If the update path is wrong, every later stage receives believable but false data.

## Production transfer

Frequency maps show up in telemetry aggregation, rate limits, cache hit counters, recommendation systems, and duplicate detection. Senior engineers watch the update path carefully because off-by-one and missing-key bugs often look like valid business numbers.

## Core idea

A frequency map has one job: record how many times each token appears. The key is the word; the value is the count. That model is simple, but the update operation must be precise.

```csharp
var counts = new Dictionary<string, int>();

foreach (var word in words)
{
    if (counts.TryGetValue(word, out var count))
    {
        counts[word] = count + 1;
    }
    else
    {
        counts[word] = 1;
    }
}
```

This pattern makes the two cases explicit. A missing word starts at one. An existing word increments. Do not write code that assumes the key is already present.

## Worked example

```csharp
var counts = WordFrequencyAnalyzer.CountWords(["go", "go", "stop"]);

Assert.Equal(2, counts["go"]);
Assert.Equal(1, counts["stop"]);
```

The dictionary is not the final result shape. It is an internal model optimized for lookup and update. The analyzer later converts it into sorted `WordFrequency` records.

## Common mistakes

- Accessing `counts[word]++` before the key exists.
- Counting empty tokens because tokenization failed earlier.
- Sorting inside the counting method, which mixes two separate responsibilities.
- Assuming dictionary iteration order is the project output order.

## Exercise connection

This lesson supports `CountWordsWithDictionary` and `UpdateFrequencyMapSafely`. One drill builds the whole map; the other focuses on the single update operation so the hidden tests can pressure existing-key behavior.

## Project connection

The dictionary is the core data structure of the analyzer. Later milestones may stream file input, but they will still need a clear count-update boundary.

## Check yourself

1. What should happen when a word appears for the first time?
2. Why should sorting not happen inside `CountWords`?
3. When might `StringComparer.OrdinalIgnoreCase` become useful, and why is it not needed here?

## Source reference notes

Use your C# collections references for `Dictionary<TKey,TValue>`, lookup, mutation, and comparer behavior. This milestone uses normalized lowercase tokens, so the default comparer is sufficient.
