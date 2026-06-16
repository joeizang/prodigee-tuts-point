# CLI Contracts and Exit Codes

## Learning objectives

- Design a command-line contract before writing parser code.
- Represent stdin, literal text, and file input as explicit modes.
- Use exit codes as part of the user-facing API.
- Keep validation errors clear enough for a human to act on.

## Prerequisites

You should understand the pure analyzer core from milestone 1, records, arrays, and simple branching. You do not need a CLI framework yet; this milestone keeps parsing explicit so the contract remains visible.

## Mental model

**Term: CLI contract** means the exact set of arguments, input modes, output rules, and errors your executable promises. **Term: exit code** means the integer status a shell or automation tool reads after the process finishes.

Think of the command line as an API where the caller is a human, shell script, CI job, or another process. The string arguments are your request body. The output and exit code are your response.

## Production transfer

Production command-line tools often run inside scripts. A vague failure message or inconsistent exit code becomes an automation bug. A senior engineer designs the command boundary so users can recover: bad arguments produce a usage message, missing files name the path, and successful output is deterministic enough for tests and pipes.

## Core idea

Start by modeling the supported modes instead of scattering string comparisons across the program.

```csharp
public enum CliInputMode
{
    Stdin,
    Text,
    File,
}

public sealed record CliOptions(CliInputMode Mode, string? Value);
```

A small parser can then map arguments into that model:

```csharp
CliOptions options = args switch
{
    [] => new CliOptions(CliInputMode.Stdin, null),
    ["--text", var value] => new CliOptions(CliInputMode.Text, value),
    ["--file", var path] => new CliOptions(CliInputMode.File, path),
    _ => throw new ArgumentException("Usage: wordfreq [--text VALUE|--file PATH]")
};
```

The parser should not read files, analyze text, or format output. It should only decide what the user asked for.

## Worked example

```csharp
static int ExitCodeFor(Exception exception)
{
    return exception is ArgumentException ? 2 : 1;
}
```

This tiny function shows the mindset: failures are not all the same. Bad user input is different from an unexpected defect.

## Common mistakes

- Treating every failure as exit code `0` because the program printed an error.
- Parsing and executing in one method, which makes tests hard to aim.
- Accepting ambiguous arguments such as `--text hello --file input.txt`.
- Printing stack traces for normal user mistakes.

## Exercise connection

`ParseWordfreqCliOptions` asks you to turn argument arrays into a small model. `RunWordfreqCliRequest` later composes that parser with input reading and formatting.

## Project connection

The CLI milestone wraps the pure analyzer core. If the CLI contract is clean, the core can stay stable while the executable grows new input modes and output formats.

## Check yourself

1. What should `wordfreq` do when called with no arguments?
2. Why should bad arguments use a non-zero exit code?
3. Which method should know about `--file`: the parser, the analyzer, or the formatter?

## Source reference notes

Use your maintainability and C# references to compare this explicit parser with later framework-based parsing. The point here is to understand the contract before hiding it behind a library.
