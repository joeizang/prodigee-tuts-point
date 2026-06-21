# Curriculum Coverage Map

## Purpose

This document is the curriculum blueprint for Prodigee Tuts Point. It prevents the app from growing through disconnected lessons and keeps every future content slice aligned with the original goal: book-depth, project-backed, senior-engineer-useful learning across languages, server frameworks, and engineering practice.

The existing platform has proven the learning machinery across C#, TypeScript, Swift, and early Python workspaces. The next phase is curriculum expansion against this map, not random content growth.

## Canonical Learning Model

Every serious topic should flow through the same model:

```text
track outcome
-> project milestone
-> theory cluster
-> focused exercises
-> milestone implementation
-> tests, static analysis, and review
-> weak-area review
-> next milestone
```

Theory is not optional. Exercises are not optional. Projects are the proof that the learner can use the knowledge under realistic constraints.

## Track Families

### Language Tracks

Language tracks build fluency in the language itself before framework abstractions dominate.

| Track | Current status | Full target |
| --- | --- | --- |
| C# Language | Seeded through `wordfreq-csharp` and `logquery-csharp` slices | Modern C# fluency: type system, nullability, generics, LINQ, async, diagnostics, memory, testing, CLI tools, packaging, performance basics |
| TypeScript | Seeded through `logprobe-typescript` Node/server slices | TypeScript fluency: structural typing, narrowing, advanced generics, conditional/mapped types, module/package design, validation boundaries, refactoring type-heavy code |
| Swift Language | Seeded through `logprobe-swift` server-shaped slices | Swift fluency: optionals, value/reference semantics, protocols, generics, error handling, Swift concurrency, package manager, testing, CLI work |
| Python | In progress through `py-notes-cli` beginner foundations | Beginner-to-proficient Python: syntax, data structures, functions, modules, type hints, exceptions, pytest, file/JSON I/O, async fundamentals, production style |

### Server Framework Tracks

Server tracks are first-class because frameworks have their own lifecycle, composition model, testing style, deployment concerns, and production risks.

| Track | Current status | Full target |
| --- | --- | --- |
| ASP.NET Core | Planned | HTTP fundamentals, Minimal APIs, controllers, middleware, DI, options, validation, ProblemDetails, auth, EF Core, background services, caching, integration tests, observability, deployment |
| Node.js Servers with Fastify | Planned after framework-neutral Node foundations | Native Node boundaries first, then Fastify routing, schemas, validation, plugins, hooks, typed route composition, testing, security, observability, persistence, deployment |
| Server-Side Swift with Vapor | Seeded through Vapor-shaped adapter and hardening milestones | Vapor routing, request/response handling, middleware, validation, Fluent-style persistence, auth, async services, testing, deployment, logging, production hardening |
| FastAPI | Planned after Python foundations | FastAPI routing, dependencies, Pydantic models, validation, exception handling, OpenAPI, routers/services/repositories, persistence, auth, testing, deployment readiness |

### Engineering Core Tracks

Engineering Core is the spine that keeps the product from becoming syntax tutorials. The detailed cross-track curriculum is defined in [Engineering Core Cross-Track Curriculum](./ENGINEERING_CORE_CURRICULUM.md).

| Track | Current status | Full target |
| --- | --- | --- |
| Algorithms and Data Structures | Planned | Complexity, arrays, strings, maps, stacks, queues, recursion, trees, graphs, sorting, searching, practical problem solving in supported languages |
| Architecture and Design | Planned | API design, modularity, dependency boundaries, clean architecture tradeoffs, DDD where appropriate, event boundaries, maintainability reviews |
| Testing Strategy | Seeded through exercise runners and visible/hidden tests | Unit, integration, contract, property-style, mutation-style challenge mode, test data design, testability refactoring, CI-minded feedback |
| Debugging and Diagnostics | Planned | Reproduction, minimization, tracebacks, logs, debuggers, structured diagnostics, failure classification, incident-style reasoning |
| Refactoring and Code Quality | Planned | Naming, decomposition, duplication, coupling, code smells, safe refactors under tests, language-specific refactoring tools |
| Performance Engineering | Seeded through streaming/scale milestones | Measurement, allocation, CPU, memory, I/O, latency, streaming, profiling, benchmarking, performance budgets |
| Security and Production Readiness | Seeded lightly through server hardening milestones | Input validation, auth boundaries, secrets, rate limiting, observability, health checks, graceful shutdown, deployment checks, operational playbooks |

## Required Coverage Per Track

Each full track must eventually define:

- Track outcome: what the learner can build and reason about after completing it.
- Prerequisites: prior concepts or tracks recommended by soft locks.
- Concept map: named concepts with source anchors and mastery evidence.
- Module map: ordered modules that group related concepts.
- Project ladder: seven projects or an explicit reason the track uses fewer.
- Milestone map: each project broken into milestone loops.
- Exercise families: focused drills tied to concepts and project milestones.
- Source strategy: books/docs used as quality anchors without copying source content.
- Review strategy: cards, prompts, debugging questions, and production-transfer checks.
- Full-feature notes: what is intentionally deferred beyond the current slice.

## Coverage Depth Levels

Use these levels when describing track maturity:

| Level | Meaning |
| --- | --- |
| Planned | Track exists in the roadmap but has no usable learner path yet |
| Seeded | Track has at least one real theory cluster, exercise set, project milestone, and runner/editor path |
| Usable Slice | Track can teach one coherent project-backed path end to end |
| Project Ladder | Track has multiple connected projects with increasing complexity |
| Full Track | Track covers the target concept map, project ladder, review path, and production readiness |

Current maturity:

| Track | Level |
| --- | --- |
| C# Language | Usable Slice |
| TypeScript | Usable Slice |
| Swift Language | Seeded |
| Python | Seeded/In Progress |
| ASP.NET Core | Planned |
| Node.js Servers with Fastify | Planned |
| Server-Side Swift with Vapor | Seeded |
| FastAPI | Planned |
| Engineering Core | Planned with detailed curriculum blueprint |

## Seven-Project Ladder Requirement

The project ladders are defined in [Curriculum Project Ladders](./CURRICULUM_PROJECT_LADDERS.md). A project ladder should move from fundamentals to production judgment without repeating the same problem shape.

For language tracks, projects should include CLI/tooling, data modeling, file/stream processing, package/module design, testing/refactoring, performance, and a capstone.

For server tracks, projects should include HTTP fundamentals, validation, persistence, auth, background work, observability, hardening, deployment, and a capstone service.

For Engineering Core, projects can be cross-language or repeated across supported languages to prove transfer.

## Source-Anchor Strategy

Source anchors must remain metadata pointers. They should identify the quality reference used for a lesson or milestone without copying book content.

Use source anchors in four ways:

- QualityAnchor: the source that keeps the lesson deep and accurate.
- FurtherReading: optional study path after the lesson.
- ContrastAnchor: a source used to compare language/framework tradeoffs.
- OfficialReference: official docs used for APIs, standards, or framework behavior.

The source library should expand before large curriculum authoring waves so every major concept has at least one quality anchor.

## Cross-Track Integration Rules

Engineering Core concepts must reappear inside language and server tracks. They should not live as isolated theory.

Examples:

- Complexity appears in C# dictionaries, TypeScript streaming summaries, Swift async streams, Python collections, and algorithm projects.
- Debugging appears in compiler diagnostics, pytest/xUnit/Vitest/XCTest failures, server logs, and incident-style project reviews.
- Architecture appears in ASP.NET Core DI, Fastify plugin boundaries, Vapor services, FastAPI dependencies, and plain language module design.
- Security appears in validation, auth, secrets, HTTP hardening, and database access.

## Anti-Slop Coverage Gates

No future track slice should be considered complete unless it has:

- At least one project milestone with a visible rubric.
- A theory cluster that satisfies the lesson contract.
- Focused exercises with visible tests, hidden tests, hints, model solutions, common wrong approaches, and expected solution characteristics.
- Source anchors for every serious lesson.
- Syntax-highlighted code examples.
- Key terms marked for term bubbles where the concept matters.
- A full-feature-later section for any intentionally scoped-down capability.
- Automated content validation.
- At least one API or runner test proving the slice is reachable and executable where applicable.

## Recommended Authoring Order

The full content build-out order is defined in [Curriculum Authoring Waves](./CURRICULUM_AUTHORING_WAVES.md).

1. Expand Swift Language and Server-Side Swift with Vapor.
2. Expand TypeScript and Node.js Servers with Fastify.
3. Expand C# and ASP.NET Core.
4. Build the dedicated Engineering Core track after Engineering Core concepts have already appeared in the earlier waves.
5. Continue Python foundations when that work is active; seed FastAPI only after plain Python functions, modules, type hints, exceptions, pytest, and file/JSON I/O are usable.

Engineering Core must be embedded from wave 1 onward, even though the dedicated Engineering Core track is the fourth wave.

## Open Decisions For Later Tasks

- How much source-library metadata should be added before authoring each large track wave.
- Whether capstone reviews become hard-gated before a track can be called complete.
- How to represent difficulty calibration and estimated study time in authored content.
