# 0041 Build Swift SourceKit Language Service

## Type

Implementation

## Status

Completed

## Outcome

Editable Swift exercise files now use a real SourceKit-LSP-backed backend bridge instead of Monaco-only syntax highlighting. Monaco routes Swift diagnostics, completions, hover, signature help, formatting, and supported code actions through the same backend language-service API shape used by C#.

## Acceptance Criteria

- [x] Start and manage persistent `sourcekit-lsp --default-workspace-type swiftPM` sessions per generated SwiftPM workspace.
- [x] Use byte-accurate JSON-RPC framing over SourceKit-LSP stdio so responses are not lost when payloads contain non-ASCII content.
- [x] Sync editable Monaco content with SourceKit-LSP through `textDocument/didOpen` and `textDocument/didChange`.
- [x] Return real Swift completions from SourceKit-LSP, including local enum cases and framework symbols.
- [x] Return SourceKit hover and signature help through the existing exercise language-service endpoints.
- [x] Pull Swift diagnostics through `textDocument/diagnostic` and keep published diagnostics as a fallback.
- [x] Return SourceKit formatting edits and supported code-action edits through the shared Monaco adapter.
- [x] Register backend Monaco providers for both `csharp` and `swift`; Swift is no longer syntax-only in the editor.
- [x] Keep task messaging honest: Swift test execution is still tracked separately in 0040.

## Verification

- `dotnet build ProdigeeTutsPoint.slnx`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~Swift"`
- `dotnet test tests/ProdigeeTutsPoint.Api.Tests/ProdigeeTutsPoint.Api.Tests.csproj --filter "FullyQualifiedName~ExerciseEndpointTests"`
- `npm test -- --run src/features/exercises/typescriptLanguageService.test.ts`
- `npm run lint`
- `npm run build`

## Full Feature Later

0040 added Swift exercise execution with `swift test`, visible/hidden test privacy, timeout handling, output truncation, static-analysis parsing, and run-history persistence. Future Swift/Vapor tasks should reuse this SourceKit-LSP bridge rather than adding static or heuristic Monaco completions.
