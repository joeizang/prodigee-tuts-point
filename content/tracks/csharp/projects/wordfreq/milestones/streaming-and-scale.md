# Streaming and Scale

## Goal

Make `wordfreq-csharp` handle larger input responsibly. The first two milestones proved correctness and CLI boundaries. This milestone adds line streaming, explicit aggregation, deterministic top-N output, and a performance conversation that is grounded in code rather than slogans.

## Required behavior

- Accept a sequence of input lines and count words without requiring one whole input string.
- Merge partial frequency maps without replacing existing counts.
- Return top-N results after deterministic ordering by count descending and word ascending.
- Keep file access outside the analyzer and pass streamed lines into the core.
- Explain which memory costs are bounded by streaming and which still grow with unique vocabulary.

## Suggested API shape

```csharp
public static class WordFrequencyStreaming
{
    public static Dictionary<string, int> CountLines(IEnumerable<string> lines);
    public static void MergeInto(Dictionary<string, int> destination, IReadOnlyDictionary<string, int> source);
    public static IReadOnlyList<WordFrequency> Top(IReadOnlyDictionary<string, int> counts, int limit);
    public static string Run(IEnumerable<string> lines, int limit);
}
```

The API still returns values instead of printing directly. That keeps tests fast and keeps the CLI shell thin.

## Theory cluster

- Streaming Large Text Input
- Merging Frequency Maps
- Performance Bounds and Top-N Output

## Focused exercises

1. `StreamWordfreqLines`
2. `MergeFrequencyMaps`
3. `LimitTopResults`
4. `RunStreamingWordfreq`

## Completion rule

Complete the focused exercises, then run the integration exercise. The milestone is ready for review when you can explain the memory behavior, the output limit, and the exact ordering rule without reading the implementation.

## Common failure modes

- Calling `File.ReadAllText` inside the streaming analyzer.
- Materializing every line into one giant string before counting.
- Replacing existing counts during a merge.
- Applying `Take` before sorting.
- Treating "streaming" as a reason to ignore output size.

## Rubric

### Correctness

- Counts words across multiple input lines.
- Merges duplicate words by adding counts.
- Applies top-N after deterministic ordering.
- Handles empty input and zero limits predictably.

### Design

- Keeps input sequence ownership outside the analyzer.
- Makes aggregation state explicit and easy to inspect.
- Keeps formatting at the boundary after values are produced.

### Testing

- Tests multi-line input, repeated words across lines, map merges, tie ordering, and zero-limit behavior.
- Includes visible tests for the happy path and hidden tests for edge cases.
- Avoids real file-system dependencies in focused exercises.

### Maintainability

- Keeps the streaming API small enough to extend later.
- Names methods around behavior rather than implementation tricks.
- Preserves the earlier analyzer rules instead of forking inconsistent tokenization.

### Complexity

- Uses simple dictionary aggregation before introducing advanced data structures.
- Documents the remaining memory cost of high-cardinality vocabularies.
- Leaves benchmarking, heap profiling, and heap-based top-K algorithms for a later full implementation milestone.

## Source reference notes

The source references point to streams, iterators, collections, and maintainability material. They are study anchors; the milestone text and exercises are original.
