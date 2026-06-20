# Swift File Input Boundaries

## Learning objectives

- Represent stdin and file input as explicit Swift values instead of loosely coupled flags.
- Use async dependency injection so input resolution can be tested without real stdin or disk access.
- Convert expected file-read failures into controlled results at the boundary.

## Prerequisites

You should understand Swift enums with associated values, async functions, closures, and basic error handling. You do not need Vapor, Foundation file APIs, or real process streams for this lesson. Those details belong at the outer adapter after the core behavior is proven.

## Mental model

**Term: input source** means the user-visible place a command should read from: stdin, a path, or a later network source. Encoding that choice as an enum is stronger than passing around optional paths and booleans.

**Term: injected reader** means a closure supplied by the caller to perform an effect. The core function decides *which* reader to call, but the shell decides *how* a file or stdin is actually read.

```swift
public enum InputSource: Equatable {
    case stdin
    case file(String)
}

public enum FileReadResult: Equatable {
    case success(String)
    case failure(String)
}
```

The resolver should call exactly one dependency. If the source is `.stdin`, it should not inspect a file path. If the source is `.file("app.log")`, it should not read stdin first. This precision keeps slow, flaky, or privileged side effects out of the core.

## Production transfer

Server-side Swift code uses the same design move when reading request bodies, uploaded files, environment variables, and configuration files. The adapter talks to Foundation, Vapor, or the operating system. The core receives typed values and explicit result shapes.

```swift
public func resolveInputSource(
    _ source: InputSource,
    readStdin: () async -> String,
    readFile: (String) async throws -> String
) async -> FileReadResult
```

This signature makes failure policy visible. A missing file is expected user input failure, not a crash. Unexpected infrastructure errors may still be logged later, but the learner-facing command can return a stable message.

## Exercise connection

The `ResolveInputSource` exercise asks you to switch over `InputSource`, await one injected reader, and return `FileReadResult`. Visible tests prove stdin behavior. Hidden tests prove file success and controlled missing-file failure.

## Project connection

`logprobe-swift` needs file input before streaming and HTTP work can be meaningful. This milestone keeps input resolution separate from log parsing so future streaming code can consume text or lines without knowing where they came from.

## Check yourself

- Why should this function not call real `FileManager` or stdin directly?
- What bug appears if both readers are called before checking the source case?
- Why is a controlled failure result better than a raw thrown error for a missing user-supplied file?

## Source reference notes

Use Swift async function material for `async` closures and `await`. Use Swift error-handling material for the distinction between thrown infrastructure errors and controlled command failures. Use server-side Swift material to connect this to adapters around request bodies and uploaded files.
