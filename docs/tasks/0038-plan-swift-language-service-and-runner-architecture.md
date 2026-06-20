# 0038 Plan Swift Language-Service and Runner Architecture

## Type

Architecture

## Status

Completed

## Outcome

Swift exercises will be built around real Swift Package Manager workspaces and the Apple Swift toolchain. The language-service path is SourceKit-LSP or a SourceKit-backed backend bridge, not Monaco-only heuristics. Exercise execution will use `swift test` in generated run workspaces with the same timeout, output limit, cleanup, and hidden-test privacy model used by C# and TypeScript.

## Acceptance Criteria

- [x] Decide the Swift workspace shape: `Package.swift`, `Sources/Exercise/Exercise.swift`, `Tests/ExerciseVisibleTests/VisibleTests.swift`, and `Tests/ExerciseHiddenTests/HiddenTests.swift`.
- [x] Decide the semantic editor backend: SourceKit-LSP or SourceKit-backed bridge, with Monaco-only syntax highlighting allowed only as an interim display layer.
- [x] Decide the runner path for 0040: host-process `swift test`, strict timeout, output truncation, generated run workspace cleanup, hidden-test privacy, and static-analysis diagnostics parsed from Swift compiler output.
- [x] Capture the decision in an ADR so later Swift/Vapor work does not accidentally drift into fake IntelliSense or script-only exercises.

## Verification

- ADR added: `docs/adr/0006-swiftpm-and-sourcekit-lsp-for-swift-exercises.md`.
- Local toolchain checked: `swift` is available and reports Apple Swift 6.3.2; `sourcekit-lsp` is present on PATH.
- 0039 tests prove generated SwiftPM workspace shape.

## Full Feature Later

0040 implemented `swift build --build-tests` static analysis plus visible/hidden XCTest execution. 0041 connected Monaco to SourceKit-LSP for semantic diagnostics, completions, hover, signature help, formatting, and supported code actions, so Swift IntelliSense is no longer syntax-only.
