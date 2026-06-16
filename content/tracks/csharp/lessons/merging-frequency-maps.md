# Merging Frequency Maps

## Learning objectives

- Merge partial count maps without losing existing values.
- Explain why mutation is acceptable when ownership is clear.
- Distinguish aggregate state from input records.
- Preserve deterministic output after aggregation.

## Prerequisites

You should be comfortable with `Dictionary<string, int>`, `TryGetValue`, and the earlier frequency-map exercises. You should also understand that streaming often produces partial results that must be combined into a final result.

## Mental model

**Term: accumulator** means the mutable state that collects results across many input items. **Term: ownership** means the part of the program responsible for changing a piece of state.

A dictionary is not automatically bad because it is mutable. It becomes risky when many parts of the program mutate it without a clear rule. In this milestone, the counting function owns one accumulator and updates it in one obvious place.

## Production transfer

Merging maps appears in telemetry systems, search indexing, billing summaries, background jobs, and analytics endpoints. A senior engineer should be able to read a merge function and answer: who owns the destination, what happens on duplicate keys, and whether the result is deterministic enough to test.

## Core idea

The safest merge shape is explicit:

```csharp
public static void MergeInto(
    Dictionary<string, int> destination,
    IReadOnlyDictionary<string, int> source)
{
    foreach (var (word, count) in source)
    {
        destination[word] = destination.TryGetValue(word, out var current)
            ? current + count
            : count;
    }
}
```

The method mutates the destination and leaves the source alone. That is a readable contract.

## Worked example

```csharp
var total = new Dictionary<string, int>(StringComparer.Ordinal)
{
    ["alpha"] = 2
};

WordFrequencyStreaming.MergeInto(total, new Dictionary<string, int>
{
    ["alpha"] = 3,
    ["beta"] = 1
});
```

After the merge, `alpha` has count `5` and `beta` has count `1`.

## Common mistakes

- Replacing destination values instead of adding to them.
- Mutating the source dictionary and surprising the caller.
- Creating nested dictionaries when one flat aggregate is enough.
- Sorting inside the merge loop instead of sorting once at the output boundary.

## Exercise connection

`MergeFrequencyMaps` isolates the merge operation. It gives you a small place to practice precise mutation before the full streaming milestone composes line reading, counting, limiting, and formatting.

## Project connection

The streaming wordfreq project uses map merging as its central accumulation mechanic. Getting this small function right keeps the later CLI milestone readable.

## Check yourself

1. Why does the merge method mutate the destination but not the source?
2. What should happen if both dictionaries contain the same word?
3. Why is sorting inside `MergeInto` unnecessary work?

## Source reference notes

Use collection chapters for the dictionary API surface. The design principle is original to this project: mutation is acceptable when ownership and responsibilities are clear.
