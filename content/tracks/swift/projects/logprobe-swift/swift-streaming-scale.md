# Swift Streaming and Scale

This milestone gives `logprobe-swift` its first scale-aware core. Reading a few strings in a unit test is easy; processing a real log file, request body, or background event stream requires a different habit. The function should consume lines incrementally, update bounded state, and return deterministic summaries.

The exercise uses `AsyncSequence` because it is broad enough to represent test streams now and real asynchronous input later. A file reader, process output reader, HTTP body adapter, or Vapor route can all eventually feed the same core if the function depends on the sequence abstraction instead of a concrete storage type.

The milestone also introduces deterministic top-N behavior. Production summaries must not change order because a dictionary happened to enumerate keys differently. Sorting by count descending and level ascending makes output stable for tests, scripts, and incident reviews.

This is also the first milestone where the command starts to behave like a server-side component instead of a toy script. The same loop that handles a fixture-backed `AsyncStream` should be able to sit behind a Vapor request body reader, a file tailer, or a process pipe without changing the counting rules. That pressure is intentional: reusable Swift backend code should keep parsing, accumulation, and presentation separate enough that each part can be tested under load and replaced at the boundary.

## Rubric

**Correctness**: Supported levels are counted from the async line source. Unsupported levels are ignored. The result contains at most the requested limit.

**Design**: The core accepts an `AsyncSequence` and avoids concrete file, stdin, or framework dependencies. The count model is explicit through `LevelCount`.

**Testing**: Visible and hidden tests cover normal counts, unsupported levels, empty streams, limits, and tie ordering.

**Maintainability**: The loop should stay readable: parse level, update dictionary, sort once at the end. Avoid clever buffering or premature streaming frameworks.

**Complexity**: The function consumes lines once with memory proportional to supported level categories, not total input size.
