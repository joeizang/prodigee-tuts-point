# Swift CLI Output Contracts

## Learning objectives

- Convert raw CLI option strings into small Swift enums before formatting logic runs.
- Treat defaults, rejected values, stdout, stderr, and exit behavior as contracts.
- Keep process-facing behavior outside the core so command behavior is easy to test in a SwiftPM package.

## Prerequisites

You should understand Swift enums, optionals, throwing functions, and the first command-boundary lesson. You do not need to build a real command executable yet. The exercise works at the package-core layer because a senior Swift CLI keeps most command behavior testable before it touches `CommandLine.arguments` or process output.

## Mental model

**Term: output contract** means the promise a command makes about supported formats, default behavior, success output, failure output, and exit codes. Even when this lesson focuses on `OutputFormat`, the larger idea is that command-line behavior is an API. Scripts and operators depend on it.

**Term: boundary default** means a default applied at the edge where raw input enters the program. If no format is supplied, choosing `.table` at the parser boundary lets deeper formatting code receive a concrete enum instead of repeatedly handling `nil`.

```swift
public enum OutputFormat: String, Equatable {
    case table
    case json
}
```

The enum is small, but it changes the design pressure. Formatting code no longer accepts any string. It accepts exactly the cases the command supports. What should happen to `"yaml"`? Should an empty string become table, or should it be rejected because the caller supplied a bad value?

## Production transfer

Server-side Swift teams often focus on Vapor routes first, but command contracts show up everywhere: migration tools, queue replayers, one-off maintenance commands, and incident-response scripts. A command that silently treats `"yaml"` as table can break automation just as surely as an HTTP handler returning the wrong status code.

```swift
public func parseOutputFormat(_ value: String?) throws -> OutputFormat {
    switch value {
    case nil: return .table
    case "table": return .table
    case "json": return .json
    case let unsupported?: throw CliParseError.unsupportedFormat(unsupported)
    }
}
```

## Exercise connection

The `ParseOutputFormat` exercise asks you to narrow an optional raw string into an `OutputFormat`. Visible tests cover the default and a supported explicit format. Hidden tests cover supported table output and rejected malformed values. The point is not the amount of code; the point is making the command contract impossible to ignore.

## Project connection

`logprobe-swift` will later produce summaries for humans and machines. This milestone gives that output path a typed switch instead of a stringly-typed formatting flag. The same habit will carry into HTTP response content types and Vapor route behavior.

## Check yourself

- Why is `.table` a reasonable missing-value default but not a safe fallback for `"yaml"`?
- Where should raw command strings stop traveling through the program?
- How would this enum help when a later function writes stdout or JSON?

## Source reference notes

Use *The Swift Programming Language* for enum, optional, and error-handling mechanics. Use *Server-Side Swift with Vapor* as a reminder that framework entry points still benefit from small typed adapters. Use command-line application material for the idea that output formats and exit behavior are public contracts.
