# 0044 Add Swift Streaming and Scale Milestone

Status: Completed

## Goal

Add a `logprobe-swift` milestone for async streaming and bounded summaries. The milestone must move beyond array-backed toy examples into `AsyncSequence`-based processing that can transfer to CLI pipes, file readers, and server request bodies.

## Implemented Scope

- Added `Swift streaming and scale` to `content/tracks/swift/track.yml`.
- Added the `Swift Async Line Streams` lesson with source-backed theory, key terms, code examples, production transfer, project connection, and self-check prompts.
- Added `count-levels-swift` as a SwiftPM exercise over generic `AsyncSequence` input.
- Added visible and hidden tests for supported level counting, unsupported level filtering, empty streams, limits, and deterministic ordering.
- Added a project milestone markdown file with a scale-focused rubric.
- Added API coverage that verifies the milestone, lesson, exercise, theory cluster, and source references.
- Added runner coverage that verifies the exercise passes through the Swift test runner.

## Acceptance Criteria

- The milestone appears under `logprobe-swift`.
- The exercise consumes lines incrementally with `for await` rather than collecting all input first.
- Results are bounded by `limit` and sorted by count descending, then level ascending.
- Theory explains why the same core should work behind test streams, files, process pipes, and future Vapor adapters.
- Content validation accepts all lesson, project, and source reference metadata.

## Verification

- `dotnet run --project tools/ProdigeeTutsPoint.ContentValidation/ProdigeeTutsPoint.ContentValidation.csproj content`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~SwiftLogprobeMilestonesReturnLessonsExercisesAndSourceBackedTheory"`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~RunEndpointExecutesSwiftLogprobeMilestoneExercises"`
