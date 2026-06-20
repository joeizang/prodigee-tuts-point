# 0043 Add Swift File I/O Milestone

Status: Completed

## Goal

Add a `logprobe-swift` milestone for file and stdin boundaries. The milestone must teach how side effects enter a Swift command safely while keeping core behavior deterministic and testable.

## Implemented Scope

- Added `Swift file input boundaries` to `content/tracks/swift/track.yml`.
- Added the `Swift File Input Boundaries` lesson with source-backed theory, key terms, code examples, project transfer, and check-yourself prompts.
- Added `resolve-input-source-swift` as a SwiftPM exercise over injected async stdin and file reader closures.
- Added visible and hidden tests for stdin, file success, and controlled file failure results.
- Added a project milestone markdown file with a rubric that emphasizes side-effect isolation and adapter discipline.
- Added API coverage that verifies the milestone, lesson, exercise, theory cluster, and source references.
- Added runner coverage that verifies the exercise passes through the Swift test runner.

## Acceptance Criteria

- The milestone appears under `logprobe-swift`.
- The lesson and project milestone explain the boundary between reusable core logic and file-system adapters.
- The exercise never requires real disk or stdin access during tests.
- File read failures return a stable `FileReadResult.failure` message rather than leaking low-level errors.
- Content validation accepts all lesson, project, and source reference metadata.

## Verification

- `dotnet run --project tools/ProdigeeTutsPoint.ContentValidation/ProdigeeTutsPoint.ContentValidation.csproj content`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~SwiftLogprobeMilestonesReturnLessonsExercisesAndSourceBackedTheory"`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~RunEndpointExecutesSwiftLogprobeMilestoneExercises"`
