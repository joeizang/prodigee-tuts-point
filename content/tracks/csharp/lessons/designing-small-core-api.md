# Designing a Small Core API

## Learning objective

The first milestone should produce a small reusable core, not a CLI-shaped tangle. A `WordFrequencyAnalyzer` can expose a method that accepts text and returns sorted frequency results.

## Candidate API

```csharp
public sealed record WordFrequency(string Word, int Count);

public static class WordFrequencyAnalyzer
{
    public static IReadOnlyList<WordFrequency> Analyze(string text)
    {
        // normalize, tokenize, count, sort
    }
}
```

## Design pressure

Keeping the core independent from files, console output, and command-line arguments makes it easier to test now and easier to reuse in later milestones.

## Project connection

Project connection: CLI, file input, and streaming will wrap this core instead of rewriting it.

## Review prompts

- Why should the core not read files directly?
- What result shape is easier to test than console output?
- Which later milestones reuse this API?
