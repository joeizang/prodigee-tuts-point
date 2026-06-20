# 0036 Add TypeScript Native Node Runtime Milestone

## Type

AFK

## Status

Completed

## Outcome

The TypeScript track gains a sixth `logprobe-typescript` milestone focused on native Node runtime wiring: request adaptation, response writing, dependency composition, request context, and a composed server handler.

## Acceptance Criteria

- [x] Add TypeScript concepts for Node runtime adapters and server composition.
- [x] Add lessons with required pedagogy structure, TypeScript code examples, key terms, check-yourself prompts, and source anchors.
- [x] Add a milestone markdown page with rubric coverage for correctness, design, testing, maintainability, and complexity.
- [x] Add four focused/project TypeScript exercises with visible tests, hidden tests, hints, model solutions, common wrong approaches, and expected solution characteristics.
- [x] Keep the implementation dependency-free and native Node-shaped so exercises run in the existing TypeScript/Vitest workspace.
- [x] Content indexing, quality validation, API tests, frontend tests, lint, and build pass.

## Verification

- `npm run lint`
- `npm run test`
- `npm run build`
- `dotnet test --no-restore`

## Full Feature Later

The full implementation should connect these adapters to a real `node:http` server, add integration tests over actual sockets or in-memory request injection, support streaming request bodies with size limits, handle backpressure, and compare the native approach with Fastify once framework integration begins.
