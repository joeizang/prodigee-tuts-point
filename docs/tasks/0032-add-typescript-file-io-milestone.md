# 0032 Add TypeScript File I/O Milestone

## Type

AFK

## Status

Completed

## Outcome

The TypeScript track gains a `logprobe-typescript` milestone focused on file and stdin input boundaries: injected file readers, safe read results, line loading, and input-source resolution.

## Acceptance Criteria

- [x] Add TypeScript concepts for Node file I/O, async boundaries, and input-source contracts.
- [x] Add original lessons with required pedagogy structure, TypeScript code examples, key terms, check-yourself prompts, and source anchors.
- [x] Add a milestone markdown page with rubric coverage for correctness, design, testing, maintainability, and complexity.
- [x] Add at least three focused TypeScript exercises with visible tests, hidden tests, hints, model solutions, common wrong approaches, and expected solution characteristics.
- [x] Support async TypeScript exercise tests in generated Vitest workspaces.
- [x] Content indexing, quality validation, API tests, frontend tests, lint, and build pass.

## Verification

- `npm run lint`
- `npm run test`
- `npm run build`
- `dotnet test --no-restore`

## Full Feature Later

The full implementation should connect these contracts to real Node `fs/promises`, stdin readers, path validation, allowed working-directory policies, file encoding choices, user-facing permission errors, and an integrated CLI runner that writes stdout/stderr at the process edge.
