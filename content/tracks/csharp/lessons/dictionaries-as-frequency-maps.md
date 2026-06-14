# Dictionaries as Frequency Maps

## Learning objective

A frequency map is a dictionary where the key is the word and the value is the number of times it appears. `Dictionary<string, int>` is a direct model for this problem.

## Core update pattern

The important operation is an update: if the word exists, increment its count; otherwise, add it with a count of one. This makes lookup behavior and edge cases visible enough to test.

```csharp
if (counts.TryGetValue(word, out var count))
{
    counts[word] = count + 1;
}
else
{
    counts[word] = 1;
}
```

## Production caution

Dictionary lookup is usually the right first choice for frequency counting. It is not automatically the right choice for memory-constrained streaming systems, culture-sensitive comparison, or case-insensitive matching without an explicit comparer.

## Project connection

Project connection: the dictionary is the core data structure of the analyzer.

## Review prompts

- Why is `Dictionary<string, int>` a natural model for this milestone?
- What bug appears if you forget to initialize a missing word?
- When might a custom comparer become important?
