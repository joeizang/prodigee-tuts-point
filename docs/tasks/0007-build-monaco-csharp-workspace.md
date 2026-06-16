# 0007 Build the Monaco C# workspace

## Type

AFK

## Status

Completed.

## Quality bar

The C# editor must feel like the Microsoft C# VS Code extension editing experience, excluding code navigation for now. "Full IntelliSense" means semantic language service behavior, not generic Monaco callbacks or static helper text.

Do not mark a criterion complete unless the implementation uses Roslyn or a Roslyn-backed language-server bridge and has a regression test, endpoint check, or browser verification for the behavior.

## What to build

Build the multi-file Monaco exercise workspace with VS Code-grade C# editing connected to the generated .NET exercise workspace.

## Current implementation

- Multi-file Monaco workspace is visible in the exercise page.
- Editable, read-only visible, and hidden file roles are represented.
- Editable C# files use Monaco with a custom theme per app theme.
- C# language requests create a fast in-memory Roslyn project snapshot from the generated exercise workspace instead of reopening the project with `MSBuildWorkspace` on every request.
- C# language requests reuse cached immutable Roslyn base solutions and overlay the live Monaco buffer per request.
- C# completion is Roslyn-backed for the active editable file and uses prefix-aware filtering before the result cap.
- C# completion reads the current Monaco buffer at request time.
- C# completion filters keyword/snippet noise from member access.
- C# completion maps common Roslyn tags to Monaco item kinds.
- Live diagnostics use Roslyn compiler diagnostics plus an embedded Roslyn `DiagnosticAnalyzer` for exercise-specific lint feedback.
- Live Roslyn requests load the generated `.editorconfig` as an analyzer config document.
- Cached Roslyn snapshots include sibling C# source files and invalidate when project/config/sibling files change.
- Semantic hover returns symbol/type information for framework and user symbols.
- Signature help returns active method/constructor signatures and active parameter indexes.
- Document formatting is backed by Roslyn.
- Monaco surfaces language-service setup messages as warning markers.
- Monaco surfaces Roslyn LSP-backed code actions/refactorings as workspace edits, with the previous supported action set retained only as fallback.
- Exercise execution runs visible and hidden tests in generated .NET workspaces.

## Acceptance criteria

- [x] Exercise page shows a multi-file Monaco editor with file tree.
- [x] Editable, read-only visible, and hidden file roles are represented.
- [x] Monaco editor dimensions support serious coding work on large screens.
- [x] Editor theme changes immediately when the app theme changes.
- [x] Neutral app theme uses a distinct Nord Light-style editor theme.
- [x] C# completion requests use the live Monaco buffer, not stale React state.
- [x] C# completion recognizes in-scope method parameters and locals in the active buffer.
- [x] C# member-access completion excludes keyword/snippet noise such as `for`.
- [x] C# completion maps Roslyn semantic tags to appropriate Monaco item kinds.
- [x] C# compiler diagnostics produce Monaco markers/squiggles for active editable files.
- [x] C# document formatting is backed by Roslyn.
- [x] C# hover is semantic and Roslyn-backed, with symbol/type information instead of static workspace status text.
- [x] C# hover returns useful information for framework symbols, user-defined symbols, methods, parameters, variables, and exceptions.
- [x] C# signature help is implemented for method and constructor invocation.
- [x] C# diagnostics include Roslyn analyzer-backed static analysis/lint diagnostics, not only compiler diagnostics.
- [x] Live Roslyn diagnostics honor generated `.editorconfig` analyzer severity.
- [x] Diagnostics are project-aware and include symbols/settings from the generated `.csproj`.
- [x] Completion is project-aware and includes generated workspace references, package references, sibling source files where present, and project-level nullable/language settings.
- [x] Repeated language-service requests reuse cached Roslyn project snapshots.
- [x] Cached project snapshots invalidate when generated project/config/sibling source inputs change.
- [x] Formatting respects project/editor settings where available, including `.editorconfig`.
- [x] Code actions are surfaced in Monaco for currently supported fixable diagnostics.
- [x] Refactorings are surfaced in Monaco for currently supported valid selections/cursor positions.
- [x] Code actions/refactorings are backed by a Roslyn language-server bridge (`csharp-ls`) rather than static or hand-only provider logic.
- [x] Language-service failures produce actionable setup messages instead of silent empty results.
- [x] The editor never shows static placeholder language-service text in symbol hover.
- [x] Regression tests or explicit verification cover completion, hover, signature help, diagnostics, formatting, and the currently supported code actions/refactorings.

## Completion notes

- [x] Add persistent workspace/project snapshot caching beyond shared MEF host, metadata references, and startup warmup.
- [x] Feed `.editorconfig` as analyzer config into live Roslyn requests instead of only emitting it into generated workspaces.
- [x] Integrate the full Roslyn code-fix/refactoring provider catalog or a Roslyn-backed language-server bridge.

## Full implementation note

The full implementation should replace the current supported-action set with the complete Roslyn code-fix/refactoring catalog or a Roslyn-backed language-server bridge attached to each generated exercise workspace. It should load the generated `.csproj`, source files, package references, analyzer references, `.editorconfig`, nullable settings, and language version, then stream the same code fixes/refactorings available in the Microsoft C# VS Code extension, still excluding code navigation if desired.

## Later full-IDE upgrades

1. Expand LSP usage to completion/hover/formatting if direct Roslyn endpoints become harder to keep aligned across languages.
2. Add delegate-specific signature help tests once exercises contain delegate-heavy code.
3. Add package-reference exercises and regression tests proving completion/hover sees external package APIs.
4. Add multi-source-file exercises and regression tests proving sibling source symbols are present in completion/hover.
5. Expand cache invalidation if future exercises add package restore, analyzer package references, or generated source outside `src/Exercise/*.cs`.

## Blocked by

- 0002 Build the local app shell
- 0008 Generate real .NET exercise workspaces
