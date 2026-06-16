# File Input Boundaries

## Learning objectives

- Keep file reading outside the pure word-frequency analyzer.
- Read text through a narrow method that is easy to test.
- Convert missing-file conditions into useful CLI failures.
- Explain why side effects belong at the edge of the program.

## Prerequisites

You should understand the analyzer core, method extraction, exceptions, and basic file paths. You should also understand that the first milestone intentionally avoided file I/O.

## Mental model

**Term: side-effect boundary** means the line where code touches the outside world: files, console, environment variables, networks, clocks, or databases. **Term: adapter** means a small layer that translates outside-world input into values the core can use.

Your analyzer should not care whether text came from stdin, a file, an HTTP request, or a test string. The adapter gets text; the core analyzes text.

## Production transfer

Server and CLI systems are easier to test when I/O is isolated. File permissions, missing paths, encodings, and large inputs are real concerns, but they should not infect the counting algorithm. This pattern scales directly into ASP.NET Core handlers, background jobs, and Node or Swift server endpoints.

## Core idea

A narrow read method is enough for this milestone:

```csharp
public static string ReadInputFile(string path)
{
    return File.ReadAllText(path);
}
```

That method is intentionally small. Higher-level code decides whether to catch errors and how to report them. The analyzer remains pure:

```csharp
var text = WordFrequencyCli.ReadInputFile("input.txt");
var frequencies = WordFrequencyCli.Analyze(text);
```

## Worked example

```csharp
public static CliResult TryReadInputFile(string path)
{
    if (!File.Exists(path))
    {
        return new CliResult(2, string.Empty, $"Input file not found: {path}");
    }

    return new CliResult(0, File.ReadAllText(path), string.Empty);
}
```

This is not the only design, but it makes a normal user failure explicit and testable.

## Common mistakes

- Calling `File.ReadAllText` inside `Analyze`.
- Returning `null` for missing files and forcing every caller to guess what happened.
- Catching every exception and pretending the command succeeded.
- Letting file paths leak into tokenizer and counting methods.

## Exercise connection

`ReadWordfreqInputFile` keeps the first file boundary small. `HandleMissingInputFile` then turns one common I/O failure into a user-facing result.

## Project connection

Milestone 2 turns the pure core into a usable tool. The point is not just "read a file"; it is learning where file reading belongs so later streaming and server handlers remain clean.

## Check yourself

1. Why should `Analyze` not read a path directly?
2. What information should a missing-file error include?
3. Which parts of file I/O are normal user failures rather than programmer defects?

## Source reference notes

Use your C# file and stream references for API details, but keep the architecture rule separate: I/O belongs at the edge and the analyzer core stays pure.
