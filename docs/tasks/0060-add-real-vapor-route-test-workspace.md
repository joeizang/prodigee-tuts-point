# 0060 Add Real Vapor Route-Test Workspace

## Type

Platform + Content

## Status

Completed

## Outcome

The first `swiftpm-vapor` exercise runtime, real Vapor route-test milestone, and curriculum content are implemented. The generated workspace emits a real `Package.swift` with `Vapor` and `XCTVapor`, and regression tests now cover curriculum indexing, workspace generation, and the full app-runner path with visible and hidden Vapor route tests.

## Acceptance Criteria

- [x] Add an explicit Vapor-capable SwiftPM runtime without slowing dependency-free Swift exercises.
- [x] Generate `Package.swift` with `Vapor` and `XCTVapor` dependencies for Vapor exercises.
- [x] Add a real Vapor lesson with syntax-highlighted Swift examples, key terms, production transfer, exercise connection, project connection, check-yourself prompts, and source anchors.
- [x] Add a `logprobe-swift` milestone that explains the route-test standard and the full future Vapor feature.
- [x] Add a Vapor exercise with visible tests, hidden tests, progressive hints, model solution, wrong approaches, and expected characteristics.
- [x] Add API regression coverage for the new curriculum and workspace runtime.
- [x] Verify whether the route exercise can restore/build/run in the current local environment; only mark completed when the real Vapor dependency route actually passes.

## Verification

- `dotnet run --project tools/ProdigeeTutsPoint.ContentValidation/ProdigeeTutsPoint.ContentValidation.csproj -- content`
- `dotnet test --no-restore tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~ExerciseEndpointTests.RunEndpointExecutesSwiftVaporRouteExercise"`
- `dotnet test --no-restore tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~ContentQualityValidatorTests|FullyQualifiedName~CurriculumEndpointTests.SwiftFoundationMilestoneReturnsLessonsExercisesAndSources|FullyQualifiedName~CurriculumEndpointTests.SwiftLogprobeMilestonesReturnLessonsExercisesAndSourceBackedTheory|FullyQualifiedName~ExerciseEndpointTests.WorkspaceEndpointGeneratesVaporSwiftPackageWorkspace|FullyQualifiedName~ExerciseEndpointTests.RunEndpointExecutesSwiftVaporRouteExercise|FullyQualifiedName~ExerciseEndpointTests.RunEndpointExecutesSwiftVisibleAndHiddenTests"`
- Manual generated-workspace verification: `swift build --scratch-path "$PWD/.build" --disable-sandbox --build-tests` and `swift test --scratch-path "$PWD/.build" --disable-sandbox --filter ExerciseHiddenTests` passed after the Vapor dependency was locally resolved.

## Full Feature Later

Future Vapor milestones should add route groups, services, middleware, structured logging, request IDs, body-size limits, streaming request reads, persistence, OpenAPI documentation, auth, deployment diagnostics, and production readiness checks.
