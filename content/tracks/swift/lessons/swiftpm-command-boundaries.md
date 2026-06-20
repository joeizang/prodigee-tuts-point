# SwiftPM Command Boundaries

## Learning objectives

- Build the first Swift exercise as a real Swift Package Manager library instead of a loose snippet.
- Convert raw command-style strings into a typed `CommandRequest`.
- Use Swift optionals, enums, and throwing validation to make invalid input explicit.
- Keep the parsing boundary reusable for later CLI, HTTP, and Vapor handlers.

## Prerequisites

You should be comfortable with functions, arrays, structs, enums, and `throws`. You do not need Vapor yet. This lesson is deliberately before framework work because server-side Swift quality depends on small packages whose behavior can be tested without booting a server.

## Mental model

**Term: SwiftPM package** means the unit of code that the Swift toolchain can build, test, and expose to SourceKit-LSP. In this app, a Swift exercise is not just `Exercise.swift`; it is a package with a library target and two XCTest targets. That shape matters because server-side Swift projects use the same package discipline when they later add Vapor, async services, database adapters, and routing.

**Term: command boundary** means the narrow function that accepts raw input and returns typed application data. A boundary is not the whole program. It is a defensive door. Everything outside it may be missing, misspelled, duplicated, or malformed. Everything after it should receive a small value that is safe to trust.

For `logprobe-swift`, the first value is intentionally tiny:

```swift
public enum LogLevel: String, Equatable {
    case debug
    case info
    case warn
    case error
}

public struct CommandRequest: Equatable {
    public let level: LogLevel
    public let limit: Int
    public let includeArchived: Bool
}
```

The point is not that every future command uses exactly these fields. The point is that raw strings should stop at the boundary. Swift gives you strong tools for this: `enum` cases for allowed values, `Int` parsing for numeric input, `Bool` fields for flags, and `throw` for expected validation failures.

## Production transfer

Server-side Swift work frequently starts with HTTP rather than command-line arguments, but the design pressure is the same. A Vapor route receives path parameters, query parameters, JSON, headers, and authentication state. Those values are external data. A senior implementation converts them into typed request values before domain code runs.

```swift
public func parseCommandRequest(_ args: [String]) throws -> CommandRequest {
    var level = LogLevel.info
    var limit = 100
    var includeArchived = false

    var index = 0
    while index < args.count {
        let option = args[index]
        switch option {
        case "--archived":
            includeArchived = true
            index += 1
        case "--level":
            guard index + 1 < args.count, let parsed = LogLevel(rawValue: args[index + 1]) else {
                throw CommandRequestError.invalidLevel
            }
            level = parsed
            index += 2
        case "--limit":
            guard index + 1 < args.count, let parsed = Int(args[index + 1]), parsed > 0 else {
                throw CommandRequestError.invalidLimit
            }
            limit = parsed
            index += 2
        default:
            throw CommandRequestError.unknownOption(option)
        }
    }

    return CommandRequest(level: level, limit: limit, includeArchived: includeArchived)
}
```

Later Vapor handlers should follow the same habit: parse and validate at the route edge, return typed request models to the core, and keep side effects outside the smallest useful functions. That is how Swift's type system becomes a production tool instead of decoration.

## Exercise connection

The `ParseCommandRequest` exercise asks you to implement this boundary inside a real SwiftPM package. Visible tests cover the normal path: default values, accepted levels, limits, and the archived flag. Hidden tests cover missing values, unknown options, and invalid limits. If the function is too permissive, the hidden tests should catch it.

## Project connection

This is the first milestone of `logprobe-swift`. The milestone rubric grades correctness, design, testing, maintainability, and complexity. Later CLI, async streaming, native HTTP, and Vapor milestones should reuse the same package habit: a small library target with focused tests before adding framework glue.

## Check yourself

1. Why should raw strings stop at `parseCommandRequest` instead of being passed through the whole application?
2. What makes an enum better than a broad `String` for log levels?
3. When should a parser throw instead of silently choosing a default?
4. How does a SwiftPM package help SourceKit-LSP and `swift test` give better feedback?

## Source reference notes

Use *The Swift Programming Language* for Swift's value-type, enum, optional, and error-handling mechanics. Use *Swift Apprentice* as a package/testing anchor. The lesson applies those ideas to server-side design without copying book text.
