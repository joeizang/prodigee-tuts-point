# Performance Bounds and Top-N Output

## Learning objectives

- Explain why output limits are product behavior, not only performance behavior.
- Sort frequency results deterministically before applying a limit.
- Reason about time, memory, and readability tradeoffs without premature optimization.
- Add a top-N boundary that keeps large results useful.

## Prerequisites

You should understand deterministic ordering from milestone 1 and formatted CLI output from milestone 2. You should also understand that a streaming counter can still produce a very large dictionary if the input has many unique words.

## Mental model

**Term: performance boundary** means an explicit decision that limits how much work or output a feature will produce. **Term: top-N query** means returning only the highest-ranked items after applying a stable ordering rule.

Streaming protects input memory, but it does not magically protect output size. If a file contains one million unique tokens, the frequency map can still grow. A top-N option turns "dump everything" into a deliberate contract.

## Production transfer

APIs, CLIs, dashboards, and background jobs need limits. A search endpoint without pagination, a log query without a result cap, and a report that emits millions of rows all create operational risk. Senior-level design names the limits, tests them, and documents their effect on correctness.

## Core idea

Limit only after deterministic ordering:

```csharp
public static IReadOnlyList<WordFrequency> Top(
    IReadOnlyDictionary<string, int> counts,
    int limit)
{
    return counts
        .Select(item => new WordFrequency(item.Key, item.Value))
        .OrderByDescending(item => item.Count)
        .ThenBy(item => item.Word, StringComparer.Ordinal)
        .Take(Math.Max(0, limit))
        .ToList();
}
```

This makes ties stable and makes zero or negative limits predictable.

## Worked example

```csharp
var topTwo = WordFrequencyStreaming.Top(
    new Dictionary<string, int>
    {
        ["beta"] = 2,
        ["alpha"] = 2,
        ["gamma"] = 1
    },
    2);
```

The result is `alpha`, then `beta`; the tie is broken by word order.

## Common mistakes

- Applying `Take` before sorting and returning arbitrary dictionary iteration order.
- Treating `limit <= 0` as an exception when the command can return an empty result.
- Designing the API around console output instead of returning values that tests can inspect.
- Optimizing with complex data structures before measuring the simple design.

## Exercise connection

`LimitTopResults` asks you to preserve ranking rules under a result cap. `RunStreamingWordfreq` then uses the cap as part of the full command behavior.

## Project connection

The milestone review asks you to explain memory and output behavior. You should be able to say what still scales linearly, what is bounded, and which future improvement would require measurement.

## Check yourself

1. Why must sorting happen before `Take`?
2. What should top-N return when the limit is zero?
3. Which part of the streaming design can still grow with unique vocabulary size?

## Source reference notes

Use LINQ and collections references for syntax and API details. The milestone focuses on the engineering judgment behind limits and ranking.
