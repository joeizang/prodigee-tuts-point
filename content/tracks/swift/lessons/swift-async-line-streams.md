# Swift Async Line Streams

## Learning objectives

- Consume `AsyncSequence` values incrementally instead of collecting all input upfront.
- Count supported log levels with explicit filtering and stable ordering.
- Produce bounded summaries that remain deterministic as input grows.

## Prerequisites

You should understand dictionaries, arrays, sorting closures, async functions, and `for try await`. You do not need to work with a real file handle in the exercise. The test harness supplies an `AsyncStream<String>` so the core streaming logic stays independent from Foundation.

## Mental model

**Term: async line stream** means a source that produces lines over time. It might be a file, network stream, process output, or test stream. The core should not care. It should consume the sequence, update a small amount of state, and move on.

**Term: bounded summary** means the result is capped by a limit and sorted by an explicit rule. Without a bound, a command can produce huge output. Without a stable tie-breaker, equal counts may appear in dictionary iteration order.

```swift
public struct LevelCount: Equatable {
    public let level: String
    public let count: Int
}
```

The function should count only supported levels: `DEBUG`, `INFO`, `WARN`, and `ERROR`. A line such as `TRACE ignored` should not create a new output category just because it appears in the stream.

## Production transfer

Server-side Swift often processes data that is too large or too slow to treat as a single value: request bodies, event streams, logs, database cursors, and queue messages. Async sequences are a Swift-native way to model that flow. The professional habit is to keep the loop simple and make ordering, limits, and error behavior explicit.

```swift
for try await line in lines {
    // update counts and discard the line
}
```

That shape is the difference between streaming and pretending. If you first convert the sequence to an array, memory grows with input size and the function no longer teaches the right operational habit.

## Exercise connection

The `CountLevels` exercise asks you to consume an async line source, count supported levels, and return at most `limit` results sorted by count descending and level ascending. Visible tests prove a small happy path. Hidden tests prove unsupported levels, empty streams, and deterministic tie behavior.

## Project connection

This milestone gives `logprobe-swift` its first scale-aware core. Later HTTP and Vapor milestones can feed the same logic from request bodies, uploaded files, or background jobs without rewriting the counting algorithm.

## Check yourself

- Why is collecting the entire stream into an array a different algorithm?
- What tie-breaker makes equal-count summaries deterministic?
- How does this function stay independent from files, stdin, and Vapor request bodies?

## Source reference notes

Use *The Swift Programming Language* for async sequences, dictionaries, and sorting closures. Use data-structure and algorithm references for counting maps, top-N summaries, and complexity reasoning. Use server-side Swift material to connect async sequences to request and stream processing.
