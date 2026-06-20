# 0042 Add Swift CLI Contracts Milestone

Status: Completed

## Goal

Add the first post-foundation `logprobe-swift` milestone for command-line output contracts. The milestone must teach narrowing raw CLI strings into domain values and connect theory, source references, exercise work, and project rubric.

## Implemented Scope

- Added `Swift CLI contracts` to `content/tracks/swift/track.yml`.
- Added the `Swift CLI Output Contracts` lesson with learning objectives, prerequisites, mental model, production transfer, exercise connection, project connection, review prompts, key terms, code examples, and source reference notes.
- Added `parse-output-format-swift` as a SwiftPM exercise using the shared `LogprobeCore.swift` starter.
- Added visible and hidden tests for accepted formats, missing format defaulting, and unsupported format errors.
- Added a project milestone markdown file with a rubric for correctness, design, testing, maintainability, and complexity.
- Added API coverage that verifies the milestone, exercise, sources, and theory cluster are returned.
- Added runner coverage that verifies the exercise passes visible and hidden Swift tests with a real submitted solution.

## Acceptance Criteria

- The milestone appears under `logprobe-swift`.
- The theory cluster links to the lesson and includes source-backed study references.
- The exercise compiles and runs through the Swift exercise runner.
- Hidden tests are present and not exposed through the public exercise response.
- Content validation accepts the lesson and milestone depth.

## Verification

- `dotnet run --project tools/ProdigeeTutsPoint.ContentValidation/ProdigeeTutsPoint.ContentValidation.csproj content`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~SwiftLogprobeMilestonesReturnLessonsExercisesAndSourceBackedTheory"`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~RunEndpointExecutesSwiftLogprobeMilestoneExercises"`
