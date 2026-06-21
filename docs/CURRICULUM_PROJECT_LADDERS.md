# Curriculum Project Ladders

## Purpose

This document defines the seven-project mastery ladders for the major curriculum tracks. It extends the coverage map by answering: "What must the learner build to prove useful, project-ready competence?"

The ladders are planning artifacts. A project appearing here does not mean content is already authored or implemented. It means future lessons, exercises, source anchors, milestones, tests, and review rubrics should converge on this sequence.

## Ladder Rules

Every project in a ladder must eventually define:

- Scenario and user-facing contract.
- Senior-engineer skill being proven.
- Theory clusters that prepare the learner for each milestone.
- Focused exercises before milestone implementation.
- Visible and hidden tests.
- Static analysis or language-service feedback where useful.
- Project rubric for correctness, design, testing, maintainability, complexity, and production readiness.
- Source anchors for major concepts.
- Full-feature-later notes for scoped-down slices.

The seventh project in each main track is a capstone. Capstones should require tests, static analysis, AI review, and a production-readiness review before the track can be considered complete.

## C# Language

Outcome: the learner can write modern C# libraries and tools that are typed, testable, maintainable, and performance-aware before moving into ASP.NET Core.

| # | Project | Mastery proof | Core concepts | Milestone arc |
| --- | --- | --- | --- | --- |
| 1 | `wordfreq-csharp` | Build a pure text analyzer with deterministic output | strings, collections, dictionaries, ordering, xUnit, small APIs | pure core -> CLI/file I/O -> streaming/scale |
| 2 | `logquery-csharp` | Build an operational log investigation tool | records, parsing, LINQ-style pipelines, grouping, errors | parse/filter/summarize -> time windows -> malformed-input reporting |
| 3 | `configsmith-csharp` | Build a typed configuration merger and validator | nullability, records, pattern matching, options, validation | parse sources -> merge precedence -> explain validation failures |
| 4 | `jobrunner-csharp` | Build a local background job runner | async/await, cancellation, channels, retries, logging | job contracts -> scheduler -> retries/cancellation -> observable runs |
| 5 | `datakit-csharp` | Build a small data access library | generics, interfaces, repositories, serialization, EF Core-ready boundaries | in-memory model -> persistence port -> query/specification layer |
| 6 | `perflab-csharp` | Build benchmarked text/data transformations | spans, allocation, BenchmarkDotNet-style reasoning, profiling | baseline -> measure -> optimize -> defend tradeoffs |
| 7 | `mini-ci-csharp` | Build a capstone developer tool that runs checks and reports results | process execution, filesystem, concurrency, diagnostics, packaging | command contract -> runners -> parallel execution -> report formats -> capstone review |

Source-anchor categories: C# language books, collections/LINQ chapters, async/concurrency references, .NET diagnostics/performance docs, code-quality books.

## TypeScript

Outcome: the learner can model data and behavior precisely with TypeScript, design package-quality modules, and use the type system to prevent real defects.

| # | Project | Mastery proof | Core concepts | Milestone arc |
| --- | --- | --- | --- | --- |
| 1 | `logprobe-typescript` | Build a typed command and server-core boundary | unions, narrowing, Node inputs, result types, async boundaries | CLI contracts -> file I/O -> streaming -> HTTP adapter -> runtime -> hardening |
| 2 | `schemaforge-ts` | Build a runtime validation and schema mapping library | unknown input, guards, discriminated unions, validation errors | primitive schemas -> objects/unions -> error paths -> JSON schema export |
| 3 | `package-workbench-ts` | Build a reusable package with tests and generated docs | modules, package exports, tsconfig, declaration files, API design | module structure -> public API -> docs -> release checks |
| 4 | `type-refactor-ts` | Refactor a stringly typed app into precise types | generics, mapped types, conditional types, utility types | baseline defects -> type modeling -> refactor -> regression tests |
| 5 | `stream-indexer-ts` | Build a streaming indexer over large text/data sources | async iterables, streams, backpressure, memory limits | line sources -> indexing -> bounded summaries -> cancellation |
| 6 | `worker-orchestrator-ts` | Build a typed worker task orchestrator | worker threads, queues, error envelopes, observability | task contracts -> worker pool -> retries/timeouts -> telemetry |
| 7 | `typed-platform-kit-ts` | Build a capstone TypeScript platform library | package design, validation, async, testing, documentation, strictness | library core -> adapters -> tests -> docs -> capstone review |

Source-anchor categories: Effective TypeScript, Essential TypeScript, Node.js Cookbook, official TypeScript handbook, package tooling docs.

## Swift Language

Outcome: the learner can write idiomatic Swift packages, command tools, and async cores that transfer cleanly into Vapor projects.

| # | Project | Mastery proof | Core concepts | Milestone arc |
| --- | --- | --- | --- | --- |
| 1 | `logprobe-swift` | Build a SwiftPM command core that can become a server service | value modeling, enums, errors, SwiftPM, XCTest, async streams | command boundaries -> file I/O -> streaming -> composition -> HTTP/Vapor adapter -> hardening |
| 2 | `textkit-swift` | Build a text normalization and parsing toolkit | strings, collections, Unicode caveats, protocols | text contracts -> parser combinators-lite -> deterministic output |
| 3 | `packagecraft-swift` | Build a reusable Swift package with public API discipline | access control, protocols, generics, extensions, package structure | module layout -> API review -> docs -> tests |
| 4 | `async-fetcher-swift` | Build an async client for paginated resources | async/await, cancellation, error envelopes, retries | request abstraction -> pagination -> cancellation -> retry policy |
| 5 | `datastructures-swift` | Build practical data structures and algorithms in Swift | arrays, dictionaries, stacks, queues, trees, graphs, complexity | linear structures -> trees -> graphs -> applied problems |
| 6 | `actor-state-swift` | Build a concurrency-safe state service | actors, tasks, isolation, Sendable reasoning | state model -> actor API -> concurrent tests -> cancellation |
| 7 | `swift-devtool-suite` | Build a capstone Swift developer tool suite | SwiftPM, CLI composition, async, package API design, diagnostics | tool core -> adapters -> concurrency -> distribution -> capstone review |

Source-anchor categories: The Swift Programming Language, Swift Apprentice, Advanced Swift-style references where available, SwiftPM and XCTest docs.

## Python

Outcome: the learner moves from absolute Python beginner to proficient Python developer before FastAPI becomes the primary vehicle.

| # | Project | Mastery proof | Core concepts | Milestone arc |
| --- | --- | --- | --- | --- |
| 1 | `py-notes-cli` | Build a notes CLI with files, JSON, and pytest | syntax, strings, lists/dicts, functions, files, JSON, pytest | title parsing -> tags -> command parsing -> storage -> command runner |
| 2 | `data-cleaner-py` | Build a CSV/JSON cleaning tool | iteration, comprehensions, standard library, error handling | read records -> normalize fields -> report errors -> write output |
| 3 | `package-lab-py` | Build a reusable Python package | modules, imports, pyproject, type hints, Ruff, Pyright/basedpyright | package layout -> public API -> lint/type checks -> docs |
| 4 | `test-harness-py` | Build a testing utility and fixture library | pytest fixtures, parametrization, temp files, monkeypatching | fixtures -> parametrized tests -> fake dependencies -> failure output |
| 5 | `async-fetcher-py` | Build an async HTTP data fetcher | asyncio, async context managers, httpx-style clients, retries | sync baseline -> async client -> cancellation -> retry/backoff |
| 6 | `sqlite-ledger-py` | Build a small local persistence layer | sqlite, transactions, migrations-lite, repositories | schema -> CRUD core -> transactions -> query/report layer |
| 7 | `python-devkit-capstone` | Build a capstone Python CLI/package with tests and type checks | packaging, typing, I/O, async, persistence, diagnostics | integrated CLI -> persistence -> async option -> quality gates -> capstone review |

Source-anchor categories: Python official tutorial, standard library docs, pytest docs, Ruff/Pyright docs, Python packaging docs.

## ASP.NET Core

Outcome: the learner can build production-shaped ASP.NET Core services with clear HTTP contracts, tests, persistence, auth, observability, and deployment readiness.

| # | Project | Mastery proof | Core concepts | Milestone arc |
| --- | --- | --- | --- | --- |
| 1 | `notes-api-aspnet` | Build a Minimal API with tested HTTP contracts | routing, request/response, validation, ProblemDetails, integration tests | endpoints -> validation -> error contracts -> tests |
| 2 | `catalog-service-aspnet` | Build a layered service with persistence | EF Core, SQLite, migrations, projections, pagination | model -> DbContext -> queries -> migrations -> tests |
| 3 | `identity-gate-aspnet` | Add authentication and authorization boundaries | JWT/cookies, policies, claims, endpoint protection | auth setup -> policies -> protected routes -> security tests |
| 4 | `background-hub-aspnet` | Build background processing inside ASP.NET Core | hosted services, queues, cancellation, logging | job enqueue -> worker -> retry -> status endpoints |
| 5 | `observability-api-aspnet` | Build a service with operational diagnostics | logging, metrics, health checks, tracing, correlation ids | structured logs -> health -> metrics -> incident drill |
| 6 | `modular-monolith-aspnet` | Build a maintainable multi-feature app | feature boundaries, DI, vertical slices, domain modeling | modules -> feature APIs -> transactions -> architecture review |
| 7 | `production-api-capstone-aspnet` | Build a capstone production-ready backend | HTTP, persistence, auth, background work, observability, deployment | end-to-end API -> hardening -> load/ops review -> capstone review |

Source-anchor categories: ASP.NET Core docs, ASP.NET Core in Action, Pro ASP.NET Core, EF Core docs, authentication/authorization docs.

## Node.js Servers With Fastify

Outcome: the learner can build Fastify services after understanding native Node boundaries, with strong schemas, plugins, tests, observability, and production hardening.

| # | Project | Mastery proof | Core concepts | Milestone arc |
| --- | --- | --- | --- | --- |
| 1 | `native-http-logprobe` | Understand the server boundary below Fastify | native HTTP, request/response adapters, streams, error contracts | native request -> response writer -> handler composition |
| 2 | `fastify-notes-api` | Build a small Fastify API with schemas | routes, JSON schemas, validation, replies, tests | route setup -> schemas -> validation errors -> route tests |
| 3 | `fastify-plugin-system` | Build a plugin-based service module | plugins, decorators, encapsulation, dependency registration | plugin contracts -> service plugin -> repository plugin -> tests |
| 4 | `fastify-data-service` | Add persistence and query contracts | database access, repositories, transactions, pagination | repository -> routes -> filters -> transactional writes |
| 5 | `fastify-auth-gateway` | Add auth and authorization boundaries | auth hooks, policies, user context, security errors | identity parsing -> route protection -> policy tests |
| 6 | `fastify-observability-lab` | Harden a service for operations | logging, metrics, request ids, health, graceful shutdown | structured logs -> metrics -> shutdown -> incident review |
| 7 | `fastify-production-capstone` | Build a capstone production Fastify service | schemas, plugins, persistence, auth, observability, deployment | full service -> hardening -> performance/security review -> capstone review |

Source-anchor categories: Node.js Cookbook, Fastify docs, TypeScript docs, Node runtime docs, security/observability docs.

## Server-Side Swift With Vapor

Outcome: the learner can build Vapor services using idiomatic Swift, async services, persistence boundaries, tests, and deployment-oriented hardening.

| # | Project | Mastery proof | Core concepts | Milestone arc |
| --- | --- | --- | --- | --- |
| 1 | `vapor-logprobe-adapter` | Adapt the Swift command core into HTTP handlers | routing, request/response, validation, command-core reuse | HTTP adapter -> Vapor route -> error mapping -> tests |
| 2 | `vapor-notes-api` | Build a small Vapor JSON API | routes, content decoding, response models, validation | routes -> DTOs -> validation -> route tests |
| 3 | `vapor-fluent-catalog` | Add persistence with Fluent-style boundaries | models, migrations, repositories, transactions | model -> migration -> repository -> query routes |
| 4 | `vapor-auth-gate` | Add authentication and authorization | auth middleware, user context, protected routes | login/token boundary -> middleware -> policy tests |
| 5 | `vapor-async-services` | Build async service composition | async/await, cancellation, services, external dependencies | service protocols -> async client -> retries -> cancellation |
| 6 | `vapor-ops-lab` | Harden a Vapor app for production | logging, metrics, health, graceful shutdown, config | config -> logging -> health -> ops review |
| 7 | `vapor-production-capstone` | Build a capstone server-side Swift service | Vapor, Fluent, auth, async services, tests, deployment | full service -> hardening -> performance/security review -> capstone review |

Source-anchor categories: Server-Side Swift with Vapor, Vapor docs, The Swift Programming Language, Swift concurrency docs, deployment docs.

## FastAPI

Outcome: the learner can build FastAPI services by connecting framework behavior back to plain Python functions, type hints, Pydantic models, tests, and production service design.

| # | Project | Mastery proof | Core concepts | Milestone arc |
| --- | --- | --- | --- | --- |
| 1 | `notes-api-fastapi` | Build a small FastAPI API after Python foundations | routes, Pydantic models, status codes, TestClient/httpx tests | route basics -> request bodies -> response models -> tests |
| 2 | `validation-contracts-fastapi` | Build precise request/response validation | Pydantic fields, query/path params, error shapes, OpenAPI | schemas -> validation errors -> docs -> contract tests |
| 3 | `service-boundaries-fastapi` | Structure a testable FastAPI app | routers, dependencies, services, repositories, settings | router split -> dependency injection -> fake services -> tests |
| 4 | `persistence-fastapi` | Add database-backed behavior | SQLite/Postgres-ready patterns, transactions, migrations | repository -> DB tests -> pagination -> transactions |
| 5 | `auth-fastapi` | Add auth and authorization boundaries | OAuth2/JWT-style flows, dependencies, policies, security errors | identity dependency -> protected routes -> policy tests |
| 6 | `ops-fastapi` | Harden a FastAPI app for operations | logging, health, CORS, background tasks, settings, deployment checks | settings -> logging -> health -> background work -> ops review |
| 7 | `fastapi-production-capstone` | Build a capstone production FastAPI service | validation, persistence, auth, observability, tests, deployment | full service -> hardening -> performance/security review -> capstone review |

Source-anchor categories: FastAPI docs, Pydantic docs, Python docs, pytest docs, database/migration docs.

## Engineering Core Cross-Track Ladder

Engineering Core is not a normal language track. Its ladder should prove transfer across languages and frameworks. Each project can be implemented in one language first, then revisited in another language to expose tradeoffs. The detailed module and concept plan is defined in [Engineering Core Cross-Track Curriculum](./ENGINEERING_CORE_CURRICULUM.md).

| # | Project | Mastery proof | Core concepts | Transfer target |
| --- | --- | --- | --- | --- |
| 1 | `algorithm-drills-lab` | Solve and explain core array/string/map problems | complexity, invariants, tests, edge cases | C#, TypeScript, Swift, Python |
| 2 | `data-structure-workbench` | Implement and test practical data structures | stacks, queues, trees, graphs, heaps, traversal | language rotation |
| 3 | `debugging-casebook` | Diagnose failing programs from symptoms to root cause | reproduction, minimization, instrumentation, regression tests | all exercise runners |
| 4 | `refactoring-clinic` | Improve design without changing behavior | seams, naming, decomposition, coupling, tests | C#, TypeScript, Python |
| 5 | `performance-lab` | Measure and improve a real bottleneck | profiling, allocation, CPU, I/O, benchmarks, tradeoffs | C#, Node, Swift, Python |
| 6 | `architecture-review-board` | Compare architectures for realistic services | boundaries, modularity, DDD, vertical slices, tradeoffs | ASP.NET Core, Fastify, Vapor, FastAPI |
| 7 | `production-readiness-review` | Harden a service before release | security, observability, deployment checks, incident drills | all server framework tracks |

Engineering Core source-anchor categories: Algorithmic Thinking, Grokking Algorithms, Code That Fits in Your Head, language performance docs, framework production docs.

## Milestone Pattern For Each Project

Every project should normally use this milestone progression:

1. Contract and domain model.
2. Pure core behavior.
3. Boundary adapter.
4. Error and edge-case hardening.
5. Testing and review.
6. Performance, security, or operational pass where relevant.
7. Capstone integration for final projects.

Small beginner projects may combine milestones, but the missing milestones must be named as future work rather than silently omitted.

## First Slice Selection Guidance

When choosing what to implement next:

1. Prefer a project that unlocks an entire track family.
2. Prefer projects that reuse existing runner/editor capability.
3. Do not start a framework project before its language foundations can support the lesson.
4. Do not mark a ladder project complete until at least one milestone has full theory, exercises, tests, source anchors, and rubric.
5. Keep current in-progress Python/FastAPI work honest: Python foundations can proceed now; FastAPI should wait until the Python foundation path is stable.

## Open Decisions For Later Tasks

- Which framework ladder gets the first full seven-project implementation: ASP.NET Core, Fastify, Vapor, or FastAPI.
- Whether capstone AI review should be hard-gated for all seventh projects.
- How much repetition is desirable when the same project pattern appears across languages.
- How to represent estimated study time, difficulty, and prerequisite soft locks in authored YAML.
