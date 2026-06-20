# 0045 Add Swift CLI Composition Milestone

Status: Completed

## Goal

Turn the existing `logprobe-swift` parsing, input, streaming, and rendering pieces into a process-independent command core. The milestone must prove that the Swift exercises are becoming a cohesive project rather than disconnected drills.

## Implemented Scope

- Added the `Swift CLI composition` module, concept, lesson, exercise, and project milestone.
- Added `LogprobeCommandRequest` and `LogprobeCommandResult` to the shared Swift starter.
- Added `run-logprobe-command-swift` with visible and hidden SwiftPM tests for stdin, file input, table output, JSON output, and controlled file failures.
- Added source-backed theory that explains command cores, edge rendering, injected dependencies, and future executable boundaries.
- Added API coverage that verifies the milestone, theory cluster, source references, and exercise are returned.
- Added runner coverage that compiles and executes the exercise through the Swift exercise runner.

## Acceptance Criteria

- `logprobe-swift` exposes the milestone after the streaming milestone.
- The exercise composes input resolution, async line counting, and rendering without touching real process APIs.
- Visible and hidden tests pass through SwiftPM.
- Content validation accepts the lesson, milestone, and source references.

## Verification

- `dotnet run --project tools/ProdigeeTutsPoint.ContentValidation/ProdigeeTutsPoint.ContentValidation.csproj content`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~SwiftLogprobeMilestonesReturnLessonsExercisesAndSourceBackedTheory"`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~RunEndpointExecutesSwiftLogprobeMilestoneExercises"`
