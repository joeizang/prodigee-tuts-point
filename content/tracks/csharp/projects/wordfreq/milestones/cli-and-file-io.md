# CLI and File I/O

## Goal

Turn the pure `wordfreq-csharp` analyzer into a small command-line tool with explicit input modes, useful errors, and deterministic text output. The core analyzer remains pure; the CLI layer handles arguments, files, stdin, output, and exit codes.

## Required behavior

- No arguments: read text from stdin.
- `--text VALUE`: analyze the provided text value.
- `--file PATH`: read all text from the provided file path.
- Unknown or ambiguous arguments: return a usage failure.
- Missing file: return a clear failure that names the missing path.
- Successful analysis: return one line per word frequency in the analyzer's deterministic order.
- Failures: return a non-zero exit code and do not pretend the command succeeded.

## Suggested API shape

```csharp
public enum CliInputMode
{
    Stdin,
    Text,
    File,
}

public sealed record CliOptions(CliInputMode Mode, string? Value);

public sealed record CliResult(int ExitCode, string Output, string Error);

public static class WordFrequencyCli
{
    public static CliOptions ParseOptions(string[] args);
    public static string ReadInputFile(string path);
    public static CliResult TryReadInputFile(string path);
    public static string FormatTable(IEnumerable<WordFrequency> frequencies);
    public static CliResult Run(string[] args, Func<string> readStdin, Func<string, string> readFile);
}
```

The delegates in `Run` keep stdin and file reading testable. A real executable can pass `Console.In.ReadToEnd` and `File.ReadAllText`; tests can pass lambdas.

## Theory cluster

- CLI Contracts and Exit Codes
- File Input Boundaries
- Formatting CLI Output

## Focused exercises

1. `ParseWordfreqCliOptions`
2. `ReadWordfreqInputFile`
3. `HandleMissingInputFile`
4. `FormatWordfreqTable`
5. `RunWordfreqCliRequest`

## Completion rule

Complete the focused exercises, then run the milestone integration exercise. The project is ready for review when the CLI layer can be tested without invoking a real process and the pure analyzer remains free of console and file-system dependencies.

## Common failure modes

- Reading files inside `Analyze`.
- Returning success while printing an error message.
- Parsing `--text` and `--file` at the same time without rejecting ambiguity.
- Formatting output before preserving deterministic analyzer order.
- Hiding normal user failures behind stack traces.

## Rubric

### Correctness

- Parses supported input modes exactly.
- Reads file input through a narrow boundary.
- Handles missing files and bad arguments with non-zero failures.
- Produces deterministic output for analyzer results.

### Design

- Keeps parser, reader, analyzer, formatter, and runner responsibilities separate.
- Keeps the pure analyzer independent of CLI and file-system concerns.
- Uses small records or result types to make command behavior explicit.

### Testing

- Tests no-argument, text, file, bad-argument, missing-file, and formatting paths.
- Uses delegates or small adapters so CLI behavior can be tested without launching a process.
- Tests both visible happy paths and hidden edge cases.

### Maintainability

- Avoids duplicated parsing branches and stringly-typed status handling.
- Makes usage and error messages clear enough to diagnose user mistakes.
- Names methods by responsibility rather than implementation detail.

### Complexity

- Uses explicit parsing for this milestone instead of prematurely adding a framework.
- Keeps file reading simple; streaming and large-file performance are later milestones.
- Does not add output formats beyond deterministic text table output.

## Source reference notes

The source references for this milestone point to relevant C# and maintainability material. They are anchors for deeper study and do not copy book content.
