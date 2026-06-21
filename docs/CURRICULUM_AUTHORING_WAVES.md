# Curriculum Authoring Waves

## Purpose

The platform has moved from proving the learning engine into authoring the full curriculum in waves. This document fixes the build-out order so content expansion does not drift into random lesson growth.

Each wave should flesh out one track family deeply enough to become a stronger usable slice while embedding Engineering Core concepts from the start.

## Wave Order

| Wave | Track family | Reason | Engineering Core expectation |
| --- | --- | --- | --- |
| 1 | Swift Language and Server-Side Swift with Vapor | Swift has the smallest original seeded content volume, but the platform already has SwiftPM, SourceKit-LSP, runner support, and Vapor-shaped milestones. | Include testing, debugging, async reasoning, package design, server boundaries, validation, performance notes, and production hardening prompts inside the Swift/Vapor lessons and exercises. |
| 2 | TypeScript and Node.js Servers with Fastify | TypeScript/Node already has strong framework-neutral server-boundary work. Fastify should extend that foundation without replacing it. | Include schema validation, plugin boundaries, async I/O, streaming, observability, security, and production readiness. |
| 3 | C# and ASP.NET Core | C# has the strongest original slice, so ASP.NET Core can build on a more mature language foundation after Swift and Fastify catch up. | Include DI boundaries, EF Core query discipline, auth, background services, diagnostics, performance, and production readiness. |
| 4 | Dedicated Engineering Core | Engineering Core should become its own explicit track after its concepts have already appeared inside the prior waves. | Consolidate algorithms, data structures, testing, debugging, refactoring, architecture, performance, security, and production readiness with transfer evidence across languages/frameworks. |

Python and FastAPI remain active but separate from this original Swift/TypeScript/C# server-family wave order. Python foundations should continue when already in progress; FastAPI should still wait until the Python foundation path is stable.

## Wave Completion Standard

A wave is not complete merely because new lessons exist. At minimum, a wave needs:

- Updated track/module/project metadata.
- At least one coherent project-backed path improved or added.
- Theory clusters satisfying the lesson contract.
- Focused exercises with visible tests, hidden tests, hints, model solutions, common wrong approaches, and expected solution characteristics.
- Source anchors for all serious concepts.
- Syntax-highlighted code examples.
- Key terms marked where term bubbles add value.
- Engineering Core transfer prompts embedded in lessons and milestones.
- Content validation passing.
- API/runner tests proving the new path is reachable and executable.

## Wave 1: Swift And Vapor

Primary goal: move Swift/server-side Swift from a seeded slice into a stronger usable track family.

Recommended scope:

- Expand Swift language foundations around value modeling, optionals, errors, protocols, generics, package design, and concurrency.
- Expand `logprobe-swift` from Vapor-shaped adapter lessons toward real Vapor learning milestones.
- Add at least one new Swift language project milestone from the ladder, likely `textkit-swift` or `packagecraft-swift`.
- Add real Vapor project planning around routing, content decoding, validation, middleware, testing, and persistence boundaries.
- Keep SourceKit-LSP, SwiftPM, and `swift test` as non-negotiable editor/runner quality gates.

First implementation candidates:

1. `0052` Add `textkit-swift` string contracts and tokenization as the first Swift language expansion milestone. Completed.
2. Continue Wave 1 with `packagecraft-swift` or a real Vapor dependency workspace and route-test milestone.
3. Later task: Add real Vapor dependency workspaces and the first true Vapor route-test milestone.

## Wave 2: TypeScript And Fastify

Primary goal: move from framework-neutral Node server contracts into Fastify without losing TypeScript rigor.

Recommended scope:

- Add Fastify as the production framework target after native Node boundaries.
- Add route schemas, typed request/reply contracts, plugins, hooks, validation errors, and route tests.
- Preserve TypeScript strictness and avoid weakening types with framework convenience.
- Connect Fastify concepts back to existing `logprobe-typescript` native HTTP/runtime milestones.

## Wave 3: C# And ASP.NET Core

Primary goal: build the ASP.NET Core track on top of the already strong C# slice.

Recommended scope:

- Add Minimal API foundations, ProblemDetails, validation, integration tests, DI, options, EF Core, auth, background services, and observability.
- Link back to C# language concepts such as records, nullability, async, LINQ, and small API design.
- Keep EF Core read queries non-tracking by default unless mutation/tracking is explicitly required.

## Wave 4: Dedicated Engineering Core

Primary goal: consolidate the cross-track skills after they have appeared in multiple language/framework waves.

Recommended scope:

- Start with `algorithm-drills-lab` and `debugging-casebook`.
- Require transfer evidence across at least two languages/frameworks.
- Add review cards for invariants, complexity, reproduction, refactoring judgment, and production readiness.

## Operating Rules

- Do not treat wave order as permission to ignore Engineering Core until wave 4.
- Do not mark a wave complete if only theory was added.
- Do not add framework content before the underlying language concepts can carry it.
- Do not rename Node.js Servers to Express; Fastify remains the target framework.
- Do not use scoped-down implementation notes as the final product ceiling.
