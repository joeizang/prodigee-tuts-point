# Parse, Filter, Summarize

## Goal

Start `logquery-csharp`, the second C# project. Build a small operational CLI core that turns timestamped log lines into records, filters them by level and message text, counts matching events by level, and produces deterministic summary output.

## Required behavior

- Parse lines shaped like `2026-06-16T09:15:00Z ERROR payment failed`.
- Treat malformed lines as deliberate parse failures.
- Filter parsed events by optional level and optional message substring.
- Count matching events by level.
- Format summaries in deterministic level order.
- Keep parse, filter, group, and format stages separately testable.

## Suggested API shape

```csharp
public sealed record LogEvent(DateTimeOffset Timestamp, string Level, string Message);
public sealed record LogQueryOptions(string? Level, string? Contains);
public sealed record LogQueryResult(int ExitCode, string Output, string Error);

public static class LogQuery
{
    public static bool TryParseLine(string line, out LogEvent? logEvent);
    public static IReadOnlyList<LogEvent> ParseMany(IEnumerable<string> lines, out int malformedCount);
    public static IEnumerable<LogEvent> Filter(IEnumerable<LogEvent> events, LogQueryOptions options);
    public static IReadOnlyList<KeyValuePair<string, int>> CountByLevel(IEnumerable<LogEvent> events);
    public static LogQueryResult Run(IEnumerable<string> lines, LogQueryOptions options);
}
```

The API makes operational behavior explicit: malformed input is not the same as a successful query with no matches.

## Theory cluster

- Log Lines as Records
- Filtering and Grouping Log Events

## Focused exercises

1. `ParseLogLine`
2. `FilterLogEvents`
3. `ParseManyLogLines`
4. `CountLogLevels`
5. `RunLogquerySummary`

## Completion rule

Complete the parser exercises before the query exercises. The milestone is ready for review when one integration run can parse several lines, reject malformed input, filter by level or text, and produce a stable summary.

## Common failure modes

- Splitting the log message on every space and losing part of the message.
- Letting malformed lines silently disappear.
- Comparing log levels case-sensitively.
- Returning dictionary iteration order from a summary.
- Building one large command method that cannot be tested in pieces.

## Rubric

### Correctness

- Parses valid timestamp, level, and message fields.
- Rejects malformed lines without fabricating records.
- Filters by normalized level and case-insensitive message substring.
- Counts and formats matching levels deterministically.

### Design

- Separates parsing from querying and formatting.
- Uses records to name the domain values.
- Exposes result types for success and user-facing failure.

### Testing

- Tests valid and invalid lines, level normalization, message-preserving parsing, filtered queries, empty matches, and malformed input.
- Uses focused unit tests before integration behavior.
- Keeps tests independent of process launching or real log files.

### Maintainability

- Keeps each pipeline stage short enough to reason about.
- Preserves the original log message content after parsing.
- Makes future options such as time ranges or JSON output easy to add.

### Complexity

- Uses simple parsing rules for the first milestone.
- Avoids regex-heavy cleverness unless later requirements justify it.
- Leaves JSON logs, multiline stack traces, streaming files, and indexed search for the full implementation.

## Source reference notes

The source anchors point to C# text processing and LINQ material. This project uses those ideas to build an original operational tool rather than copying book examples.
