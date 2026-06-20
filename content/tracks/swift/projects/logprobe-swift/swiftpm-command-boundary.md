# SwiftPM Command Boundary

This milestone starts the Swift/server-side Swift path with a real package, not a loose code cell. The project target is `logprobe-swift`, a log investigation tool that will later grow into CLI, async processing, native HTTP, Vapor, persistence, concurrency, and observability work.

The first milestone is deliberately narrow. You need a typed command request parser that accepts raw argument strings and returns a stable value:

- accepted levels: `debug`, `info`, `warn`, `error`
- default level: `info`
- default limit: `100`
- `--archived` as a boolean flag
- useful thrown errors for unknown options and invalid option values

The important professional habit is boundary discipline. Server-side Swift code receives untrusted input from command lines, HTTP routes, JSON bodies, headers, environment variables, and databases. Do not let those raw shapes spread. Convert them into explicit Swift values early, then test the conversion hard.

The generated workspace must remain a normal SwiftPM package:

```swift
// swift-tools-version: 6.0
import PackageDescription

let package = Package(
    name: "ProdigeeSwiftExercise",
    products: [
        .library(name: "Exercise", targets: ["Exercise"])
    ],
    targets: [
        .target(name: "Exercise"),
        .testTarget(name: "ExerciseVisibleTests", dependencies: ["Exercise"]),
        .testTarget(name: "ExerciseHiddenTests", dependencies: ["Exercise"])
    ]
)
```

That package shape is the bridge between learning and real work. SourceKit-LSP understands it. `swift test` understands it. Future Vapor projects will use the same package mental model.

## Rubric

### Correctness

- Parses `--level`, `--limit`, and `--archived` without relying on argument order beyond option/value pairs.
- Applies defaults when options are absent.
- Rejects unknown options, missing values, unsupported levels, and non-positive limits.

### Design

- Keeps raw `[String]` parsing in one function.
- Uses `enum` and `struct` values instead of passing string dictionaries through the system.
- Exposes a small public API that later CLI and Vapor handlers can call.

### Testing

- Visible tests cover ordinary successful parsing.
- Hidden tests cover invalid and missing input.
- Tests compare typed values instead of inspecting private implementation details.

### Maintainability

- Names errors and request fields clearly enough that future milestones can reuse them.
- Keeps parsing logic readable without clever index tricks.
- Avoids global mutable state and process access inside the parser.

### Complexity

- Runs in linear time over the argument list.
- Uses constant additional state.
- Does not allocate unnecessary intermediate dictionaries or arrays for every option.

The milestone is complete when the package structure is generated correctly and the parser is ready for the Swift runner that lands in task `0040`.
