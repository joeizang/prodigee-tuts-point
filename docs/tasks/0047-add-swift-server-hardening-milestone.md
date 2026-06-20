# 0047 Add Swift Server Hardening Milestone

Status: Completed

## Goal

Add a `logprobe-swift` server hardening milestone that wraps the HTTP adapter with body-size validation, timeout mapping, telemetry, and stable outcome classification.

## Implemented Scope

- Added the `Swift server hardening` module, concept, lesson, exercise, and project milestone.
- Added `LogprobeTelemetry` and `HardenedLogprobeResponse` to the shared Swift starter.
- Added `handle-hardened-logprobe-request-swift` with visible and hidden SwiftPM tests for success telemetry, oversized body rejection, validation failure classification, and timeout mapping.
- Documented the full future Vapor feature: middleware, request id propagation, structured logging, metrics, body streaming limits, timeout scheduling, cancellation-aware command execution, and Vapor test-client integration tests.
- Added API coverage that verifies the milestone, theory cluster, source references, and exercise are returned.
- Added runner coverage that compiles and executes the hardening exercise through the Swift exercise runner.

## Acceptance Criteria

- `logprobe-swift` exposes the hardening milestone after the Vapor request adapter milestone.
- The exercise returns one response envelope for every path.
- Oversized request bodies are rejected before reaching command work.
- Timeout and validation failures map to stable status/body/outcome contracts.
- Content validation accepts the lesson, project milestone, and source references.

## Verification

- `dotnet run --project tools/ProdigeeTutsPoint.ContentValidation/ProdigeeTutsPoint.ContentValidation.csproj content`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~SwiftLogprobeMilestonesReturnLessonsExercisesAndSourceBackedTheory"`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~RunEndpointExecutesSwiftLogprobeMilestoneExercises"`
