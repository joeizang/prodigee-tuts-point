# 0040 Add Swift Exercise Runner

## Type

Implementation

## Status

Completed

## Outcome

Swift exercises now execute as real Swift Package Manager packages with `swift build --build-tests` and `swift test`. The runner follows the same visible-before-hidden policy used by the C# and TypeScript runners: visible tests must pass before hidden tests run, hidden failure details stay masked, and compiler diagnostics are stored as static-analysis feedback.

## Acceptance Criteria

- [x] Add a dedicated Swift exercise runner using the host Swift toolchain.
- [x] Run Swift static analysis with `swift build --build-tests` before test execution.
- [x] Run visible XCTest targets through `swift test --filter ExerciseVisibleTests`.
- [x] Run hidden XCTest targets only after visible tests pass.
- [x] Preserve hidden-test privacy by masking hidden output and diagnostics on hidden failure.
- [x] Parse Swift compiler diagnostics into static-analysis records with file, line, column, severity, and message.
- [x] Keep command timeout and output truncation aligned with existing exercise runners.
- [x] Replace the previous “planned for 0040” runner placeholder and update workspace messaging.

## Verification

- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~Swift"`

## Full Feature Later

Future Swift runner hardening should add cancellation telemetry, richer Swift warning categorization, and platform setup diagnostics that explain when `swift` or XCTest is missing. Vapor/server-side Swift exercises should reuse this runner shape for package-level tests before adding service-specific harnesses.
