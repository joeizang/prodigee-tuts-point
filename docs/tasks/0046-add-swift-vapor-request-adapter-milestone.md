# 0046 Add Swift Vapor Request Adapter Milestone

Status: Completed

## Goal

Introduce the server-side Swift boundary by adapting HTTP-shaped request data onto the reusable `logprobe-swift` command core. The milestone should model real Vapor handler discipline without requiring a Vapor dependency in the fast SwiftPM exercise runner.

## Implemented Scope

- Added the `Swift Vapor request adapter` module, concept, lesson, exercise, and project milestone.
- Added `HttpLogprobeRequest` and `HttpLogprobeResponse` to the shared Swift starter.
- Added `handle-logprobe-request-swift` with visible and hidden SwiftPM tests for successful JSON output, unsupported formats, invalid limits, and empty bodies.
- Documented the full future Vapor feature: route module, request body streaming, typed query decoding, structured logging, cancellation-aware reads, middleware-friendly errors, dependency-injected services, and Vapor test-client integration tests.
- Added API coverage that verifies the milestone, theory cluster, source references, and exercise are returned.
- Added runner coverage that compiles and executes the exercise through the Swift exercise runner.

## Acceptance Criteria

- `logprobe-swift` exposes the milestone after CLI composition.
- The exercise maps HTTP-shaped input into the command core instead of duplicating parsing/counting/rendering logic.
- Success and client-error responses have stable status, content type, and body contracts.
- Content validation accepts the lesson, milestone, and source references.

## Verification

- `dotnet run --project tools/ProdigeeTutsPoint.ContentValidation/ProdigeeTutsPoint.ContentValidation.csproj content`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~SwiftLogprobeMilestonesReturnLessonsExercisesAndSourceBackedTheory"`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~RunEndpointExecutesSwiftLogprobeMilestoneExercises"`
