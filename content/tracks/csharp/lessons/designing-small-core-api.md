# Designing a Small Core API

## Learning objectives

- Design a small analyzer API that later CLI, file, and streaming layers can reuse.
- Choose names and return types that make behavior easy to test.
- Keep I/O outside the pure core.
- Explain how a small API reduces project risk as features accumulate.

## Prerequisites

You should understand records, static methods, lists, and the previous pipeline stages. You should also have seen how the focused exercises isolate individual behaviors.

## Mental model

**Term: core API** means the smallest useful programming surface that represents the domain behavior without UI, CLI, or storage concerns. **Term: boundary** means the line between pure logic and outside-world details such as files, console arguments, clocks, or networks.

A good core API is boring in the best way: it is easy to call, easy to test, and hard to misuse. The CLI should wrap it; the CLI should not become it.

## Production transfer

Senior engineers often rescue projects by finding the small stable core hidden inside a noisy application. Once the core is isolated, tests become cheaper, bugs become smaller, and later features can compose instead of rewriting the same logic behind every interface.

## Core idea

The first milestone should produce a reusable core, not a CLI-shaped tangle. The core should accept input text and return structured results. It should not read files, inspect command-line arguments, or print output.

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

This surface is deliberately boring. That is a strength. A boring core is easy to call from a CLI command, a test, a benchmark, or a future web endpoint.

## Design pressure

Good API design is mostly about pressure management. Put file reading in the core and every test needs files. Put console output in the core and every caller has to parse strings. Return a dictionary and every caller has to know the sorting rule. Return `IReadOnlyList<WordFrequency>` and the analyzer communicates exactly what later layers need.

## Common mistakes

- Returning formatted strings before the result model is stable.
- Reading files inside `Analyze`, which couples parsing to I/O.
- Exposing mutable internal collections.
- Hiding too much in one large method, making focused tests impossible.

## Exercise connection

`BuildWordFrequencyAnalyzer` is the integration exercise. It should feel like assembling proven helpers, not inventing the whole project at once.

## Project connection

CLI, file input, output formats, and streaming will wrap this core. Senior engineers protect a small reliable center so outer layers can change without destabilizing core behavior.

## Check yourself

1. Why is `IReadOnlyList<WordFrequency>` a better return type than console output?
2. Which method should own file reading in a later milestone?
3. What behavior belongs in `Analyze`, and what behavior belongs in the CLI wrapper?

## Source reference notes

Use your API design and maintainability books for naming, boundaries, and small-surface design. The project milestone uses those principles in a concrete command-line application path.
