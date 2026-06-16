# 0023 Add wordfreq-csharp streaming and scale milestone

## Type

AFK

## Status

Completed

## What to build

Add the third `wordfreq-csharp` milestone so the project moves beyond small in-memory strings into line streaming, explicit aggregation, top-N limiting, and performance-boundary reasoning.

## Acceptance criteria

- [x] Add a new project milestone for streaming and scale with theory cluster, focused exercises, source anchors, and rubric.
- [x] Cover streamed line input, map merging, deterministic top-N output, and memory/performance tradeoffs.
- [x] Add focused exercises for streamed counting, map merging, top-N limiting, and an integration pipeline.
- [x] Generate real .NET exercise workspaces for the new exercises with visible and hidden tests.
- [x] Add API regression tests proving the milestone, theory cluster, and generated workspaces are reachable.
- [x] Source references remain metadata pointers and do not copy book text.
- [x] Existing content quality validation passes with the new material.

## Verification

- `dotnet test`
- `npm run lint && npm run test && npm run build`
- Live API smoke: `wordfreq-csharp/streaming-and-scale` returns 3 lessons, 4 exercises, and source anchors.
- Browser smoke: expanded curriculum remains reachable through the running app.

## Implementation notes

- This milestone must not pretend streaming solves all scale concerns. It should explicitly teach that unique vocabulary can still grow the count map.
- Keep file ownership outside the analyzer. The exercise input is `IEnumerable<string>` so tests stay file-system independent.
- Keep syntax-highlighted code fences in all lesson and milestone examples.

## Full implementation note

Later work should add benchmarking, memory profiling, heap-based top-K selection, very-large-file fixtures, output formats, cancellation, progress reporting, and true file-stream integration in a packaged CLI. Those are full-version features and are not complete in this milestone.

## Blocked by

- 0020 Add wordfreq-csharp CLI and file I/O milestone
