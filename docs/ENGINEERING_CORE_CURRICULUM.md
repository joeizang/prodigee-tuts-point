# Engineering Core Cross-Track Curriculum

## Purpose

Engineering Core is the cross-track curriculum that makes the platform useful beyond language syntax and framework APIs. It teaches the engineering judgment that should transfer across C#, TypeScript, Swift, Python, ASP.NET Core, Fastify, Vapor, and FastAPI.

Engineering Core should be a first-class track family in planning and review, but its concepts must also appear inside language and framework project milestones. The learner should not study "performance" or "debugging" as detached essays; they should use those skills while building, testing, reviewing, and hardening real projects.

## Curriculum Contract

Every Engineering Core module must define:

- Mental model: the durable idea the learner should carry between languages.
- Practice surface: focused exercises that force the idea into code.
- Project transfer: where the idea reappears in language/framework ladders.
- Evidence: what proves the learner can apply the idea.
- Source anchors: books or official docs used for depth and accuracy.
- Review prompts: questions that test judgment, not trivia.

## Module Map

| Module | Outcome | Primary project | Transfer surfaces |
| --- | --- | --- | --- |
| Algorithmic Thinking | Explain and choose algorithms using complexity, invariants, and edge-case reasoning | `algorithm-drills-lab` | C# collections, TypeScript streaming, Swift data structures, Python beginner data problems |
| Data Structures | Implement and use structures by tradeoff, not memorization | `data-structure-workbench` | Dictionaries/maps, trees, queues, heaps, graph traversals, route indexes |
| Testing Strategy | Design tests that reveal behavior, edge cases, and regressions | `test-harness-py`, all exercise runners | xUnit, Vitest, XCTest, pytest, HTTP integration tests |
| Debugging and Diagnostics | Move from symptom to reproduction to root cause to regression test | `debugging-casebook` | compiler diagnostics, failed tests, server logs, tracebacks, LSP feedback |
| Refactoring and Code Quality | Improve design while preserving behavior under tests | `refactoring-clinic` | C# APIs, TypeScript type refactors, Python modules, service boundaries |
| Architecture and Design | Choose boundaries that fit complexity, lifetime, and change pressure | `architecture-review-board` | ASP.NET Core vertical slices, Fastify plugins, Vapor services, FastAPI dependencies |
| Performance Engineering | Measure before optimizing and defend tradeoffs | `performance-lab` | streaming, allocation, async, DB queries, server latency |
| Security | Model trust boundaries and failure modes before shipping services | `production-readiness-review` | validation, auth, secrets, dependency risk, rate limits, data access |
| Production Readiness | Prepare software for operation, diagnosis, rollback, and maintenance | `production-readiness-review` | health checks, logs, metrics, deployment checks, graceful shutdown |

## Concept Map

### Algorithmic Thinking

Core concepts:

- Complexity as growth behavior, not stopwatch guessing.
- Invariants that explain why an algorithm stays correct.
- Edge cases as design inputs, not afterthoughts.
- Brute force as a baseline for correctness.
- Sorting, searching, hashing, recursion, dynamic programming basics.

Exercise families:

- Predict output and complexity.
- Fix a wrong algorithm with a failing edge-case test.
- Implement the same problem with brute force and improved approaches.
- Explain the invariant in a short review answer.
- Port one solution between C#, TypeScript, Swift, and Python.

Project transfer:

- `wordfreq-csharp`: dictionary counting and deterministic ordering.
- `logprobe-typescript`: bounded summaries over streams.
- `datastructures-swift`: trees, graphs, and traversals.
- `py-notes-cli`: beginner string/list/dictionary reasoning.

Mastery evidence:

- Can choose a data structure and explain the complexity tradeoff.
- Can write tests for empty, singleton, duplicate, sorted, unsorted, and malformed cases.
- Can identify when an algorithm is correct but operationally too expensive.

### Data Structures

Core concepts:

- Arrays/lists, maps/dictionaries, sets, stacks, queues, heaps, trees, graphs.
- Representation tradeoffs and operation costs.
- Mutability and ownership differences across languages.
- Iteration order, stable output, and deterministic tests.

Exercise families:

- Implement a structure from tests.
- Use a structure to simplify a real project milestone.
- Compare two representations for the same behavior.
- Debug mutation and aliasing bugs.
- Add deterministic output to an otherwise unordered structure.

Project transfer:

- C#: dictionaries, records, queues/channels.
- TypeScript: records/maps, discriminated unions over graph-like state.
- Swift: value semantics and collection mutation.
- Python: lists/dicts/sets and beginner mutability pitfalls.

Mastery evidence:

- Can explain which operations are cheap or expensive and why.
- Can preserve determinism in tests even when data structures do not guarantee useful order.
- Can avoid representation leaks across module boundaries.

### Testing Strategy

Core concepts:

- Tests as executable contracts.
- Visible vs hidden tests and what each teaches.
- Unit, integration, contract, property-style, and regression tests.
- Test data design and boundary cases.
- Fakes, fixtures, dependency injection, and deterministic time.

Exercise families:

- Write tests before implementation.
- Improve weak tests that pass broken code.
- Add a regression test for a bug.
- Convert an integration-only behavior into a unit-testable core.
- Compare visible and hidden test intent.

Project transfer:

- Every exercise runner.
- ASP.NET Core integration tests.
- Fastify route tests.
- Vapor route tests.
- FastAPI route and service tests.

Mastery evidence:

- Can design tests that fail for the right reason.
- Can name what a test does not prove.
- Can use tests to make refactoring safer.

### Debugging and Diagnostics

Core concepts:

- Reproduce, minimize, hypothesize, instrument, fix, regression-test.
- Reading compiler, type-checker, linter, and test output.
- Distinguishing symptoms from causes.
- Logging and diagnostic context.
- Failure classification: user error, dependency failure, code defect, environment issue.

Exercise families:

- Given a failure transcript, identify the next useful observation.
- Minimize a failing case.
- Add diagnostics without hiding the bug.
- Fix a defect and add a regression test.
- Compare language-service feedback across C#, TypeScript, Swift, and Python.

Project transfer:

- Monaco diagnostics and editor feedback.
- Failed xUnit/Vitest/XCTest/pytest runs.
- Server logs and request ids.
- AI review risk explanations.

Mastery evidence:

- Can explain the reproduction before proposing the fix.
- Can avoid speculative fixes without evidence.
- Can leave a regression test that would catch the failure again.

### Refactoring and Code Quality

Core concepts:

- Behavior-preserving change.
- Naming, cohesion, coupling, duplication, and accidental complexity.
- Extract function/type/module/service.
- Replace conditionals with polymorphism or explicit data where appropriate.
- Refactoring under tests.

Exercise families:

- Refactor a working but tangled function.
- Split adapters from pure core logic.
- Make dependencies explicit.
- Replace stringly typed data with precise types.
- Compare before/after design tradeoffs.

Project transfer:

- C# small API design and records.
- TypeScript discriminated unions and package APIs.
- Python module/package structure.
- ASP.NET Core feature boundaries.
- Fastify plugin boundaries.
- Vapor service boundaries.
- FastAPI routers/dependencies/services.

Mastery evidence:

- Can improve a design without changing observable behavior.
- Can justify why an abstraction is useful, not decorative.
- Can avoid refactors that make code clever but harder to operate.

### Architecture and Design

Core concepts:

- Boundary placement.
- Dependency direction.
- Vertical slices, modular monoliths, clean architecture, and DDD tradeoffs.
- Ports/adapters where useful.
- Domain model vs transport model vs persistence model.
- Change pressure and lifetime as architecture inputs.

Exercise families:

- Draw a boundary map for a small service.
- Refactor route handlers away from business logic.
- Compare two architecture options for the same project.
- Identify coupling that blocks testing.
- Write an architecture decision note for a real tradeoff.

Project transfer:

- ASP.NET Core DI and feature route groups.
- Fastify plugins and service decorators.
- Vapor route/service/persistence split.
- FastAPI dependencies and repository boundaries.
- Language package API design.

Mastery evidence:

- Can explain why a boundary exists.
- Can choose a simpler architecture when complexity is low.
- Can spot when persistence or framework concerns are leaking into core logic.

### Performance Engineering

Core concepts:

- Measurement before optimization.
- Big-O vs constant factors.
- Allocation, memory pressure, CPU, I/O, latency, throughput.
- Streaming and backpressure.
- Benchmark design and profiling pitfalls.
- Performance budgets.

Exercise families:

- Predict a bottleneck, then measure it.
- Replace whole-input processing with streaming.
- Compare allocation-heavy and allocation-aware implementations.
- Add a performance guard to an advanced milestone.
- Write a short performance review explaining tradeoffs.

Project transfer:

- C# spans and allocation-aware text processing.
- TypeScript streams and worker orchestration.
- Swift async streams and actors.
- Python I/O and async clients.
- Server framework latency, DB queries, and request-size limits.

Mastery evidence:

- Can distinguish performance guesses from measured facts.
- Can avoid optimizing code that is not on the hot path.
- Can state what got faster and what got more complex.

### Security

Core concepts:

- Trust boundaries.
- Input validation and output encoding.
- Authentication vs authorization.
- Secrets handling.
- Dependency and supply-chain risk.
- Rate limits, request size limits, and denial-of-service concerns.
- Safe error messages.

Exercise families:

- Classify trusted and untrusted inputs.
- Fix an endpoint that leaks internal errors.
- Add validation and explain the failure contract.
- Protect a route with an authorization rule.
- Review a dependency or configuration risk.

Project transfer:

- ASP.NET Core auth policies.
- Fastify schemas and hooks.
- Vapor middleware.
- FastAPI dependencies and exception handlers.
- AI provider secret handling.

Mastery evidence:

- Can identify which data crosses a trust boundary.
- Can separate validation failures from server defects.
- Can avoid putting secrets into project files or exports.

### Production Readiness

Core concepts:

- Operational contracts: logs, metrics, health, tracing, request ids.
- Configuration and environment boundaries.
- Graceful shutdown and cancellation.
- Deployment checks.
- Incident review and rollback thinking.
- Documentation that helps future maintainers.

Exercise families:

- Add health checks and request ids.
- Convert ad hoc logs to structured facts.
- Write a readiness checklist for a service.
- Simulate a dependency outage.
- Produce an incident-style post-fix review.

Project transfer:

- ASP.NET Core health checks and hosted services.
- Fastify lifecycle hooks and graceful shutdown.
- Vapor logging and configuration.
- FastAPI settings and health routes.
- CLI tools with predictable exit codes and diagnostics.

Mastery evidence:

- Can say how a future operator knows the system is healthy.
- Can preserve useful diagnostics without leaking sensitive details.
- Can make failure states visible and actionable.

## Cross-Track Transfer Matrix

| Engineering Core area | C# | TypeScript | Swift | Python | ASP.NET Core | Fastify | Vapor | FastAPI |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Algorithms/data structures | dictionaries, LINQ, spans | maps, unions, async iterables | collections, value semantics | lists, dicts, sets | query/pagination choices | route indexes, stream handling | Fluent queries, async collections | Pydantic/query transformations |
| Testing strategy | xUnit | Vitest | XCTest | pytest | WebApplicationFactory-style tests | route/plugin tests | Vapor app tests | TestClient/httpx route tests |
| Debugging | compiler/analyzer diagnostics | TS worker diagnostics | SourceKit/XCTest failures | tracebacks/Pyright/Ruff | logs/problem details | request logs/hooks | Vapor logs/errors | validation traces/logs |
| Refactoring | small APIs, records | precise types/modules | protocols/extensions | modules/functions | vertical slices | plugins/services | routes/services | routers/dependencies |
| Architecture | libraries and adapters | package boundaries | packages/services | packages/repositories | DI, middleware, EF Core | plugins, decorators | middleware, services, Fluent | dependencies, services, repositories |
| Performance | allocation, streaming | streams, workers | async streams, actors | I/O, async | DB queries, request pipeline | backpressure, hooks | async services | async endpoints, DB access |
| Security | validation, secrets | schema validation | validation, config | input handling | auth policies | hooks/auth/plugins | middleware/auth | dependencies/auth |
| Production readiness | CLI diagnostics | server telemetry | logging/config | settings/logging | health, metrics, hosted services | health, shutdown, logs | config, logs, deploy | health, settings, deployment |

## Mastery Evidence Levels

Engineering Core concepts should use the same evidence-based mastery statuses as the rest of the app:

| Status | Evidence |
| --- | --- |
| Introduced | Learner has read the mental model and answered at least one review prompt |
| Practiced | Learner has completed focused exercises for the concept |
| Applied | Learner has used the concept inside a project milestone |
| Reliable | Learner has applied the concept in at least two contexts or languages and passed review |
| Needs Review | Recent failed attempts, stale review cards, or weak rubric feedback indicate decay |

For Engineering Core, `Reliable` should normally require transfer. Example: performance is not reliable after one C# streaming exercise; it becomes reliable after the learner can recognize and measure similar pressure in Node, Swift, Python, or a server framework.

## First Usable Slice

The first Engineering Core slice should be `algorithm-drills-lab` plus a small `debugging-casebook` companion.

Rationale:

- Algorithms and debugging support every other track.
- Existing runners can host focused exercises in C#, TypeScript, Swift, and Python.
- The slice can start small without needing a new framework runtime.
- It creates reusable review cards for complexity, invariants, edge cases, and reproduction discipline.

Minimum first slice:

- One theory cluster: complexity, invariants, edge cases, and regression tests.
- Four focused exercises: array scan, frequency map, stable ordering, failing-case minimization.
- At least two languages, preferably C# and TypeScript first because their runners are mature.
- One debugging exercise where the learner receives a failing test transcript and must produce a minimal fix plus regression test.
- Source anchors to Algorithmic Thinking, Grokking Algorithms, and Code That Fits in Your Head.

Full first slice later:

- Repeat the same exercise family in Swift and Python.
- Add review cards for complexity and invariant explanations.
- Add a transfer prompt comparing language-specific data structures.
- Add a small performance note showing why complexity and allocation are different concerns.

## Source-Anchor Backlog

The current source library should eventually include these Engineering Core anchors:

- Algorithmic Thinking.
- Grokking Algorithms.
- Data Structures and Algorithms in Swift.
- Code That Fits in Your Head.
- Learning JavaScript Design Patterns.
- The Rust Programming Language for ownership, error handling, and systems-level contrast.
- Efficient Go and Go Crazy as contrast anchors for concurrency, CLI tools, and production tooling.
- Official framework docs for production/security/observability topics.

Source anchors remain metadata pointers. The app must not copy book content.

## Content Authoring Rules

- Do not author Engineering Core as generic essays.
- Every module needs at least one code exercise and one project transfer.
- Every architecture lesson needs a concrete scenario and tradeoff.
- Every performance lesson needs measurement or a reason measurement is deferred.
- Every security lesson needs a trust-boundary example.
- Every debugging lesson needs an actual failure artifact.
- Every refactoring lesson needs before/after behavior-preserving tests.
- Every production-readiness lesson needs an operational question: "How would I know this is failing in use?"

## Relationship To Other Tasks

- `0048` defines the overall coverage map.
- `0049` defines the project ladders.
- `0050` defines this Engineering Core curriculum.
- Later track-slice tasks should reference this document when adding lessons, exercises, rubrics, and review cards.
- Capstone tasks should use this document as part of their production-readiness review checklist.
