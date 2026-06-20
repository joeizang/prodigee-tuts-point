# 0037 Add TypeScript Server Hardening Milestone

## Type

AFK

## Status

Completed

## Outcome

The TypeScript track gains a seventh `logprobe-typescript` milestone focused on server hardening: safe error boundaries, timeout fallbacks, request context, and structured request telemetry.

## Acceptance Criteria

- [x] Add TypeScript concepts for error boundaries, timeout policies, and request observability.
- [x] Add lessons with required pedagogy structure, TypeScript code examples, key terms, check-yourself prompts, and source anchors.
- [x] Add a milestone markdown page with rubric coverage for correctness, design, testing, maintainability, and complexity.
- [x] Add four focused TypeScript exercises with visible tests, hidden tests, hints, model solutions, common wrong approaches, and expected solution characteristics.
- [x] Keep expected client errors separate from unexpected server defects in the taught contracts.
- [x] Content indexing, quality validation, API tests, frontend tests, lint, and build pass.

## Verification

- `npm run lint`
- `npm run test`
- `npm run build`
- `dotnet test --no-restore`

## Full Feature Later

The full implementation should add abortable dependency calls, structured logger integration, OpenTelemetry tracing, request and response size limits, security headers, rate limiting, graceful shutdown, health checks, and load-oriented tests.
