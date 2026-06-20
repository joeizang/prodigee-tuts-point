# Swift CLI Composition

## Learning objectives

- Compose parser, input resolution, streaming, and rendering functions into one command contract.
- Keep command behavior independent from `CommandLine`, process exit codes, terminal output, and real file handles.
- Separate domain results from presentation text so table and JSON output remain deliberate edge decisions.

## Prerequisites

You should already understand `InputSource`, `OutputFormat`, `FileReadResult`, `AsyncSequence`, and `LevelCount`. This lesson assumes the previous `logprobe-swift` pieces exist and asks you to make them cooperate as a command pipeline.

## Mental model

**Term: command core** means the reusable function that performs the command's work without depending on process APIs. A thin executable can later read `CommandLine.arguments`, write to stdout, and choose exit codes, but the core should accept typed values and injected dependencies.

**Term: edge rendering** means turning trusted data into user-facing text at the boundary. Counting log levels should produce values; rendering should decide whether those values become a plain table or JSON.

```swift
public enum LogprobeCommandResult: Equatable {
    case rendered(String)
    case failed(String)
}
```

That result shape gives the command a stable contract. A CLI executable can map `.rendered` to stdout and exit code `0`, while `.failed` can become stderr and a non-zero exit. Tests do not need a real terminal to prove the behavior.

## Production transfer

Professional command-line tools are small systems. They parse raw inputs, resolve side effects, run core logic, render output, and report failures. If those responsibilities are mixed together, each new format, input source, or server adapter creates regressions. If they are composed behind typed contracts, the command can grow without becoming a script-shaped tangle.

```swift
let input = await resolveInputSource(request.source, readStdin: readStdin, readFile: readFile)
let counts = try await countLevels(from: lines, limit: request.limit)
let body = renderLevelCounts(counts, format: request.format)
```

The key habit is sequence. Resolve input first. Count values second. Render last. Do not render while counting, and do not read real files inside the counting loop.

## Exercise connection

The `RunLogprobeCommand` exercise asks you to compose previous functions into one command. Visible tests prove stdin plus table output. Hidden tests prove file input, JSON output, and controlled file failure behavior.

## Project connection

This milestone turns `logprobe-swift` from a set of useful functions into a command-shaped tool. The next milestone can adapt HTTP requests into the same command core instead of rebuilding parsing and counting for the server.

## Check yourself

- Which part of the command should know about table versus JSON text?
- Why should file read failures return a command failure instead of leaking low-level errors?
- What remains for a future executable target after this command core exists?

## Source reference notes

Use *The Swift Programming Language* for enums, async functions, closures, and error handling. Use *Server-Side Swift with Vapor* for the architectural habit of keeping adapters thin and reusable application logic framework-independent.
