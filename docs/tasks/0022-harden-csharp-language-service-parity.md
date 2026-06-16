# 0022 Harden C# language-service parity and caching

## Type

AFK

## Status

Completed.

## What to build

Close the remaining `0007` quality gaps that keep the Monaco C# workspace below the Microsoft C# VS Code extension standard, excluding code navigation.

## Acceptance criteria

- [x] Repeated language-service requests reuse a persistent Roslyn project snapshot instead of rebuilding the project graph each time.
- [x] Snapshot invalidation responds to generated project/config/source changes that affect semantic results.
- [x] Cached snapshots include sibling C# source files so completion/hover/diagnostics can see multi-file exercise symbols.
- [x] Snapshot behavior is covered by deterministic endpoint regression tests rather than flaky timing assertions.
- [x] Live diagnostics still honor generated `.editorconfig` analyzer severity after caching.
- [x] Code actions are backed by Roslyn's full exported code-fix/refactoring provider catalog or a Roslyn-backed language-server bridge.
- [x] Unsupported provider-catalog work is isolated behind an honest fallback; no fake full-parity claims.
- [x] Browser/API verification confirms completion, diagnostics, hover, formatting, and supported actions still work after cache changes.

## Implementation notes

- The first implementation caches immutable Roslyn base solutions keyed by workspace path and editable document path.
- The live Monaco buffer is applied with `Solution.WithDocumentText` for each request, so completions and diagnostics still use current unsaved editor content.
- The cache stamp includes generated `.csproj`, `.editorconfig`, and sibling source files. The active editable file is deliberately excluded from the stamp because its live content is supplied by Monaco.
- Sibling files are loaded from `src/Exercise/*.cs` to support future multi-file exercises.
- Live API smoke after caching verified completion, diagnostics, hover, formatting, and the currently supported expression-bodied refactoring. A repeated completion smoke measured warm requests at roughly 214-230ms on the current local machine.
- Code actions now use `csharp-ls` through a stdio LSP bridge. The bridge keeps one LSP session per generated exercise workspace, sends current Monaco content with `didOpen`/`didChange`, requests `textDocument/codeAction`, and translates LSP `WorkspaceEdit` objects back to Monaco edits.
- The previous supported action set remains as a fallback only when the LSP bridge is unavailable or returns no usable current-document edits.
- A regression test proves an LSP-only provider refactoring (`Extract method` / `Extract local function`) is returned; the fallback cannot satisfy that test.

## Full implementation note

The full implementation should expose the same practical code-fix/refactoring catalog a learner expects from the Microsoft C# VS Code extension for the generated exercise workspace, while keeping code navigation out of scope for now. Completion latency should remain suitable for keystroke-speed use after Swift and TypeScript language services are added.

## Blocked by

- 0007 Build the Monaco C# workspace
