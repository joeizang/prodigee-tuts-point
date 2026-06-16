# Log Lines as Records

## Learning objectives

- Parse a log line into an explicit C# record.
- Decide what a malformed line means for a user-facing tool.
- Keep parsing separate from filtering and reporting.
- Preserve the original message text after structured fields are extracted.

## Prerequisites

You should understand strings, splitting, small API design, and user-facing errors. You should also be comfortable writing tests for edge cases before building larger command behavior.

## Mental model

**Term: parse boundary** means the place where untrusted text becomes a typed value or a deliberate failure. **Term: structured record** means a value whose fields have names and meaning instead of being passed around as raw substrings.

Log files are semi-structured. A line may look consistent until a service writes an unexpected message. The parser must be strict enough to protect downstream logic and clear enough to tell the learner or user what went wrong.

## Production transfer

Operational tools live or die on trustworthy parsing. Incident response, debugging, analytics, and support workflows all depend on turning messy text into reliable records. Senior engineers do not let filtering code guess where the timestamp ends or whether `"ERROR"` is a level or part of a message.

## Core idea

Use a narrow parse method:

```csharp
public sealed record LogEvent(DateTimeOffset Timestamp, string Level, string Message);

public static bool TryParseLine(string line, out LogEvent? logEvent)
{
    logEvent = null;
    var parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length < 3 || !DateTimeOffset.TryParse(parts[0], out var timestamp))
    {
        return false;
    }

    logEvent = new LogEvent(timestamp, parts[1].ToUpperInvariant(), parts[2]);
    return true;
}
```

The parser owns structure. Later query code receives `LogEvent` values, not raw lines.

## Worked example

```csharp
var ok = LogQuery.TryParseLine(
    "2026-06-16T09:15:00Z ERROR payment failed",
    out var logEvent);
```

The parsed level is `ERROR` and the message remains `payment failed`.

## Common mistakes

- Splitting on every space and losing the original message shape.
- Accepting malformed lines and creating records with empty timestamps.
- Mixing parsing, filtering, and formatting in one method.
- Comparing levels case-sensitively when logs often vary by source.

## Exercise connection

`ParseLogLine` is the first logquery exercise because every later filter and summary depends on trustworthy records.

## Project connection

`logquery-csharp` is the second C# project. It shifts from word counting into operational tooling, where parsing decisions directly affect investigation quality.

## Check yourself

1. Why should filtering code receive `LogEvent` instead of raw strings?
2. What should happen when a line does not contain a timestamp, level, and message?
3. Why should the parser preserve spaces inside the message text?

## Source reference notes

Use text parsing and record-type references for syntax. This project writes original parser rules instead of copying examples from any book.
