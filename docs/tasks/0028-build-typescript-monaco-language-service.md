# 0028 Build TypeScript Monaco Language Service

## Type

AFK

## Status

Completed

## Outcome

TypeScript exercises use Monaco's real TypeScript language service worker with strict compiler settings, multi-file workspace awareness, Node.js type declarations, diagnostics, hover, signature help, formatting, and supported code actions.

## Acceptance Criteria

- [x] TypeScript editor tabs use `typescript`, `javascript`, or `json` Monaco languages based on file extension.
- [x] Every visible workspace file is synchronized into a stable Monaco model URI under `/workspace`, so imports and tests participate in semantic analysis.
- [x] Monaco TypeScript defaults use strict settings: `strict`, `noImplicitAny`, `strictNullChecks`, `noUncheckedIndexedAccess`, `noImplicitReturns`, `moduleResolution: NodeJs`, and modern ECMAScript targets.
- [x] Node.js ambient declarations from local dependencies are registered as lazy Monaco extra libs, including `node:` imports through local `@types/node`.
- [x] TypeScript exercises use Monaco's semantic TypeScript worker rather than backend fake suggestions.
- [x] Diagnostics and red squiggles come from the Monaco TypeScript worker.
- [x] Formatting and supported quick fixes/refactors are available through Monaco/TypeScript.
- [x] C# continues using the existing Roslyn-backed language-service path.
- [x] Automated frontend tests cover language mapping, workspace model synchronization intent, TypeScript default configuration, and Node type-lib registration.

## Verification

- `npm run lint`
- `npm run test`
- `npm run build`
- `dotnet test --no-restore`
- Live smoke: `/api/exercises/parse-command-request-ts/workspace?profileId=live-ts-smoke` returns `TypeScript` and `node-typescript`.

## Full Feature Later

- Add per-exercise dependency manifests with curated package type declarations.
- Add dependency-aware virtual file systems for package-specific `.d.ts` files.
- Add a latency instrumentation panel for cold/warm completions and diagnostics.
- Add a language-service health badge that distinguishes Monaco worker health from backend runner health.
