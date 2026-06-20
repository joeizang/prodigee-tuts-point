# 0029 Add TypeScript Node Exercise Runner

## Type

AFK

## Status

Completed

## Outcome

TypeScript exercises generate runnable Node/Vitest workspaces and execute visible and hidden tests through local pinned tooling without per-exercise package installs.

## Acceptance Criteria

- [x] Exercise YAML can identify a workspace runtime as `dotnet` or `node-typescript`; existing C# exercise definitions continue working through the default `dotnet` runtime.
- [x] TypeScript workspaces generate `package.json`, `tsconfig.json`, `src/exercise.ts`, `tests/visible.test.ts`, and `tests/hidden.test.ts`.
- [x] Visible tests return full learner-facing output.
- [x] Hidden tests only reveal pass/fail guidance, not hidden assertions.
- [x] Static analysis runs `tsc --noEmit` and returns parsed TypeScript diagnostics as `StaticAnalysisDiagnosticResponse` records.
- [x] The runner invokes local repository tooling from `src/ProdigeeTutsPoint.Web/node_modules` and does not run `npm install` inside exercise workspaces.
- [x] Timeouts kill the whole process tree and report a deterministic timeout result.
- [x] Automated API tests cover generated TypeScript workspace shape, passing TypeScript visible/hidden tests, and TypeScript static-analysis diagnostics.

## Verification

- `dotnet test --no-restore`
- Targeted API tests:
  - `WorkspaceEndpointGeneratesTypeScriptNodeWorkspace`
  - `RunEndpointExecutesTypeScriptVisibleAndHiddenTests`
  - `RunEndpointReturnsTypeScriptStaticAnalysisDiagnostics`
- Live smoke: `/api/exercises/parse-command-request-ts/run` passes visible and hidden tests with the model solution.

## Full Feature Later

- Add sandboxed process limits for CPU, memory, and file writes.
- Add package allowlists and deterministic dependency restoration for advanced Node exercises.
- Add ESLint-based static analysis beside `tsc`.
- Add richer problem matchers for Vitest assertion locations.
