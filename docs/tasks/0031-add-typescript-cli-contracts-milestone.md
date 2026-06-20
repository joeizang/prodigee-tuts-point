# 0031 Add TypeScript CLI Contracts Milestone

## Type

AFK

## Status

Completed

## Outcome

The TypeScript track gains a second `logprobe-typescript` milestone focused on Node command-line contracts: `process.argv`, typed options, structured parse failures, stdout/stderr, and exit-code behavior.

## Acceptance Criteria

- [x] Add TypeScript concepts for Node process arguments, typed parse failures, output format contracts, and exit results.
- [x] Add lessons with required pedagogy structure, TypeScript code examples, key terms, check-yourself prompts, and source anchors.
- [x] Add a milestone markdown page with rubric coverage for correctness, design, testing, maintainability, and complexity.
- [x] Add at least four focused TypeScript exercises with visible tests, hidden tests, hints, model solutions, common wrong approaches, and expected solution characteristics.
- [x] Generalize TypeScript exercise generation so exercises can specify their exported entrypoint instead of always importing `parseCommandRequest`.
- [x] Keep the existing `parse-command-request-ts` exercise compatible by default.
- [x] Content indexing, quality validation, API tests, frontend tests, lint, and build pass.

## Verification

- `npm run lint`
- `npm run test`
- `npm run build`
- `dotnet test --no-restore`

## Full Feature Later

Later TypeScript milestones should add file input, streaming line readers, log parsing, query composition, deterministic output rendering, package boundaries, HTTP APIs, persistence, concurrency, and observability.
