# Streaming Large Text Input

## Learning objectives

- Explain why whole-file reads become a design problem before they become a syntax problem.
- Process text through `IEnumerable<string>` so tests can supply lines and production code can supply file streams.
- Keep ownership of file handles at the program edge instead of hiding it inside the analyzer.
- Describe the tradeoff between simple APIs and memory-aware APIs.

## Prerequisites

You should already understand the pure word-frequency analyzer, the CLI input modes, and the rule that file I/O belongs outside the core analyzer. This lesson keeps that rule but changes the shape of the input from one string to a sequence of lines.

## Mental model

**Term: streaming** means processing data as it arrives instead of first materializing the entire input. **Term: iterator boundary** means the method boundary where a caller hands you a sequence and you promise not to care whether it came from a file, memory, a socket, or a test fixture.

The core idea is not that streaming is always faster. The core idea is that streaming gives you a stable memory ceiling. A whole-file design says, "give me all the data, then I start." A streaming design says, "give me the next piece, and I will update the state I own."

## Production transfer

Large files, HTTP request bodies, message queues, and database cursors all punish code that assumes all input should be loaded at once. The same design used here transfers to ASP.NET Core endpoints that process upload streams, background services that consume events, and Node or Swift servers that transform data incrementally. Senior engineers are expected to recognize when the friendly API hides a scale boundary.

## Core idea

Start by lifting the analyzer from `string` to `IEnumerable<string>`:

```csharp
public static Dictionary<string, int> CountLines(IEnumerable<string> lines)
{
    var counts = new Dictionary<string, int>(StringComparer.Ordinal);

    foreach (var line in lines)
    {
        foreach (var word in Tokenize(line))
        {
            counts[word] = counts.TryGetValue(word, out var current) ? current + 1 : 1;
        }
    }

    return counts;
}
```

The method does not open a file. A real CLI can call `File.ReadLines(path)` and pass the resulting sequence. A test can pass `new[] { "alpha beta", "beta" }`.

## Worked example

```csharp
var lines = File.ReadLines("large-input.txt");
var counts = WordFrequencyStreaming.CountLines(lines);
var top = WordFrequencyStreaming.Top(counts, limit: 20);
```

This keeps resource ownership visible. The caller decides where the file sequence comes from; the analyzer decides how to count.

## Common mistakes

- Replacing `File.ReadAllText` with `File.ReadLines` but immediately calling `string.Join` on every line.
- Returning `IEnumerable<WordFrequency>` backed by a disposed reader.
- Hiding file access inside the counting method and making tests depend on the file system.
- Assuming streaming removes the need for deterministic ordering at the output boundary.

## Exercise connection

`StreamWordfreqLines` asks you to count tokens from a sequence of lines. It is deliberately smaller than the full project milestone so you can focus on state ownership and sequence processing.

## Project connection

The third wordfreq milestone moves the CLI toward production scale. The goal is not a benchmark contest; the goal is a design where memory behavior, counting state, and output limits are explicit enough to review.

## Check yourself

1. What changes when the analyzer receives `IEnumerable<string>` instead of `string`?
2. Why should the counting method not open the file itself?
3. What bug appears if a streamed sequence depends on a reader that has already been disposed?

## Source reference notes

Use the stream, iterator, and file chapters as API anchors. The curriculum text stays original: the lesson here is the architectural boundary around those APIs.
