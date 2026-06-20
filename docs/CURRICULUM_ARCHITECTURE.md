# Curriculum Architecture

## Curriculum Principle

The curriculum must optimize for durable competence, not tutorial volume. Every track should integrate language mechanics, standard library fluency, testing, debugging, tooling, design judgment, performance, and applied projects.

The primary sequencing model is project-backed mastery with exercise-first pressure. Projects provide the spine, theory provides the depth, and exercises provide active coding repetition.

Progression should use soft locks. The curriculum recommends prerequisites and warns about gaps, but it does not hard-block the learner from jumping ahead.

Mastery should be evidence-based. Reading introduces a concept, but competence comes from exercises, project use, review recall, and repeated success.

Mastery statuses:

- Not Started
- Introduced
- Practiced
- Applied
- Reliable
- Needs Review

## Diagnostic Strategy

The first version should include light diagnostics, not a full adaptive engine.

Initial C# diagnostic should check:

- Basic method reading/writing
- String handling
- `List<T>` and `Dictionary<TKey, TValue>`
- Basic xUnit assertions
- Simple edge-case reasoning

Diagnostic results should recommend a starting point but must not hard-block progress.

## Tracks

### C# Language

Scope:

- Type system, value/reference semantics, nullability, records, pattern matching
- Generics, delegates, events, LINQ, async/await
- Exceptions, diagnostics, logging, configuration
- Collections, spans, memory-aware programming
- Testing, CLI apps, packaging, performance basics

### ASP.NET Core

Scope:

- HTTP and API fundamentals
- Minimal APIs and controller tradeoffs
- Middleware, dependency injection, options, validation
- ProblemDetails, auth, rate limiting, caching
- EF Core with SQLite
- Background services, integration tests, observability

### TypeScript

Scope:

- Structural typing, narrowing, unions, intersections
- Generics, conditional types, mapped types, utility types
- Runtime validation, module systems, package design
- API client/server modeling
- Testing and refactoring type-heavy code

### Node.js Servers

Scope:

- Event loop, async I/O, streams, workers
- HTTP APIs, routing, middleware, validation
- Fastify as the target production framework after native Node boundaries are understood
- Fastify schemas, plugins, hooks, typed route composition, and testable server modules
- Filesystem, child processes, process lifecycle
- Database access, error handling, logging
- Performance, security, deployment readiness

### Python

Scope:

- Absolute-beginner Python syntax, indentation, names, object references, truthiness, and `None`
- Strings, numbers, lists, dictionaries, tuples, sets, iteration, comprehensions, and standard library fluency
- Functions, modules, imports, virtual environments, packaging basics, and dependency management
- Type hints, dataclasses, Pydantic-style data modeling, exceptions, debugging, and tracebacks
- Testing with pytest, fixtures, parametrized tests, visible/hidden test design, and refactoring under tests
- CLI programs, file/JSON I/O, local persistence, HTTP clients, async fundamentals, performance boundaries, and production-ready Python style

The Python track has a different pacing rule from the existing C#, TypeScript, and Swift tracks: assume the learner starts with no Python. Fast syntax tours are not acceptable. Early milestones must be smaller, more repetitive, and more explicit about mental models before the learner is expected to use framework abstractions.

### FastAPI

Scope:

- HTTP fundamentals, request/response modeling, route handlers, status codes, and API contracts
- FastAPI routing, dependencies, validation, response models, exception handling, and OpenAPI docs
- Pydantic models, schema boundaries, typed request bodies, query/path parameters, and serialization
- Application structure with routers, services, repositories, settings, logging, and testable dependency overrides
- Persistence with SQLite/Postgres-ready patterns, migrations, transactions, pagination, and database-backed tests
- Authentication and authorization boundaries, background tasks, CORS, health checks, deployment readiness, and security review

FastAPI should arrive only after Python functions, modules, data structures, exceptions, type hints, and pytest are usable. The framework track must repeatedly connect FastAPI behavior back to plain Python rather than presenting decorator-driven behavior as magic.

### Swift Language

Scope:

- Optionals, value types, enums, protocols, extensions
- Generics, error handling, collections
- Swift concurrency, actors, tasks, cancellation
- Package manager, testing, CLI development
- Data structures and algorithmic fluency

### Server-Side Swift

Scope:

- Vapor routing, request/response handling, validation
- Fluent-style persistence concepts
- Async services, auth, testing
- Deployment and operational concerns

### Algorithms and Engineering Practice

Scope:

- Complexity analysis, arrays, strings, maps, recursion
- Trees, graphs, sorting, searching
- Debugging, profiling, refactoring
- API design, maintainability, architecture tradeoffs

## Lesson Contract

Every serious lesson should include:

- Learning objective
- Prerequisites
- Mental model
- Concept explanation
- Syntax/API reference
- Multiple focused examples
- One worked example
- Production use section
- Failure modes and common bugs
- Testing strategy
- Debugging strategy
- Performance notes where relevant
- Exercises
- Project connection
- Review prompts
- Further reading/source anchors

Lessons should normally belong to a theory cluster that supports a project milestone. Standalone reference lessons are allowed, but they are not the dominant curriculum shape.

Python beginner lessons have additional acceptance criteria:

- Name the beginner mental model explicitly before introducing syntax.
- Show at least one common wrong beginner interpretation and how to debug it.
- Require recall, code writing, test running, and refactoring before the concept is considered practiced.
- Avoid moving into FastAPI until the same concept has been used in plain Python.
- Explain editor diagnostics, hover text, formatting, and type feedback as part of the learning loop.

## Exercise Contract

Every coding exercise should include:

- Concrete capability being tested
- Starter code
- Hidden or visible tests
- Constraints
- Hints in progressive layers
- Expected solution characteristics
- Common incorrect approaches
- Review explanation after completion

Exercises should be frequent enough that writing code is the default way concepts become durable. They should be smaller than project milestones but more realistic than trivia.

Exercises should support progressive reveal. Hints come before solution access, and full model solutions are visible after passing or after deliberate unlock.

Version 1 reveal sequence:

1. Conceptual nudge
2. API/approach hint
3. Structural hint
4. Intentional solution unlock
5. Full solution visible after passing

Full solution vision:

- Multiple solution styles
- Compare learner solution to model
- Rubric-based solution review
- "Why this solution" commentary
- Common wrong solution gallery
- Refactoring challenge after passing

Exercise tests should include both visible and hidden tests. Visible tests teach expectations and testing style; hidden tests check edge cases, robustness, and anti-hardcoding.

Full exercise testing vision:

- Visible tests
- Hidden tests
- Property-style tests where useful
- Mutation-style challenge mode
- Performance tests for advanced milestones
- Test explanation after passing

## Project Contract

Every project should include:

- Scenario
- Requirements
- CLI/API contract
- Milestones
- Test suite
- Rubric
- Stretch goals
- Design review prompts
- Language-specific idioms
- Production concerns

Projects are the curriculum spine. A project milestone should declare the theory cluster and exercises required to complete it well.

Project validation should include tests, static analysis, and AI-assisted review.

Version 1 project validation:

- Automated tests for correctness
- Static analysis for code quality and common defects
- AI review for design, maintainability, edge cases, and production judgment
- Rubric criteria used by the AI review and visible to the learner
- AI review is advisory for normal milestones and required for capstones.

Full project validation vision:

- Test history
- Static analysis trend
- AI review history
- Compare against model solution
- Design smell detection
- Refactoring challenge after passing
- Performance checks for advanced milestones
- Security checks for server/API projects

## Milestone Cluster Sizing

All tracks should use the same project-backed milestone cluster pattern:

```text
Project milestone
-> theory cluster
-> focused exercises
-> milestone implementation
-> tests/rubric
-> review cards
```

Cluster size scales with topic difficulty.

Normal milestone:

- 4-8 lessons
- 6-12 focused exercises
- 1 milestone task
- Review cards
- Rubric

Hard milestone:

- 6-12 lessons
- 10-20 focused exercises
- 1-3 milestone tasks
- Debugging/refactoring drills
- Design questions
- Rubric

Small concept:

- 1-3 lessons
- 3-6 exercises
- Small applied task

## Project Ladder

Initial seven-project ladder:

1. `wordfreq` - text parsing, file/stdin I/O, maps, sorting, tests
2. `todo` - local persistence, validation, schema evolution
3. `httpie-lite` - HTTP clients, headers, JSON, async, errors
4. `logq` - streaming, parsing, filtering, memory discipline
5. `servebox` - small file/API server, routing, middleware, security basics
6. `jobrunner` - queues, retries, cancellation, concurrency
7. `pkg-index` - external APIs, caching, data modeling

Resolved: `pkg-index` is project 7. `mini-ci` remains the first advanced capstone after the core ladder.

Python and FastAPI should use a beginner-to-proficient ladder that starts below the normal language-track floor:

1. `py-notes-cli` - Python files, strings, functions, collections, JSON, CLI output, pytest
2. `task-tracker-core` - data modeling, type hints, dataclasses/Pydantic-style validation, exceptions, module boundaries
3. `task-api` - FastAPI routing, request bodies, response models, validation, status codes, route tests
4. `task-api-v2` - routers, dependencies, settings, service boundaries, error responses, logging
5. `task-api-sqlite` - persistence, migrations, repository boundaries, pagination, database-backed tests
6. `team-tasks-api` - auth boundaries, ownership checks, background tasks, health checks, CORS, deployment configuration
7. `learning-journal-api` - capstone service tied to Prodigee Tuts Point concepts, notes, review cards, study sessions, search, and production review

### First C# Project

The first C# project is `wordfreq-csharp`.

It proves the loop with:

- CLI input contract
- File and stdin handling
- String normalization and tokenization
- Dictionaries and sorting
- Focused unit tests
- Edge-case handling
- Readable implementation structure

Resolved milestone ramp:

1. Pure word counting functions
2. CLI, stdin, and file input
3. Output formats, validation, errors, and edge cases
4. Performance, streaming, and larger inputs

This hybrid ramp gives immediate coding practice without hiding the core algorithm behind CLI plumbing too early.

Milestone 1 minimum:

- Implement a reusable word-frequency engine.
- Normalize text consistently.
- Tokenize words with an ASCII-first rule.
- Ignore punctuation by documented rule.
- Handle casing.
- Count frequencies with `Dictionary<string, int>`.
- Return deterministic sorted results.
- Pass xUnit tests.
- Explain complexity tradeoffs.

Unicode word segmentation is out of scope for milestone 1 and should appear later as an advanced extension.

Milestone 1 theory cluster:

1. Text as Data in C#
   - `string`, `char`, immutability, indexing, basic Unicode caveat
2. Normalization and Tokenization
   - casing, punctuation, ASCII-first rule, invariants
3. Dictionaries as Frequency Maps
   - `Dictionary<TKey, TValue>`, lookup/update patterns, complexity
4. Deterministic Ordering
   - sort by count descending, then word ascending, stable output expectations
5. Testing Pure Functions with xUnit
   - test cases, edge cases, assertions, Arrange/Act/Assert
6. Designing a Small Core API
   - `WordFrequencyAnalyzer`, pure functions, result shape, naming

Milestone 1 focused exercises:

1. `NormalizeToLowercase`
2. `KeepAsciiLettersAndDigits`
3. `SplitWordsOnSeparators`
4. `TokenizeSimpleSentence`
5. `TokenizeWithPunctuation`
6. `CountWordsWithDictionary`
7. `UpdateFrequencyMapSafely`
8. `SortFrequenciesDeterministically`
9. `HandleEmptyAndWhitespaceInput`
10. `BuildWordFrequencyAnalyzer`

Milestone 1 project task:

- Implement the `WordFrequencyAnalyzer` core so all project tests pass.

## First Slice Language Strategy

The first implementation slice should use one language first, while keeping tracks, exercises, projects, and attempts language-aware from the start.

Resolved first language: C#.

Reasons:

- The backend is ASP.NET Core, so the app's implementation stack reinforces the curriculum.
- C# runner support can use the installed .NET SDK and test tooling.
- C# lets the first project cover CLI, tests, collections, file I/O, and production-style error handling without waiting for Swift or TypeScript infrastructure.
- The same project/exercise abstractions can later support TypeScript and Swift.

## Open Decisions

- Whether each project should be implemented in all three language families or selected per track.
- Whether algorithms should be its own track or interwoven through language tracks.
- How much theory must precede project work.
- Whether project validation should happen through tests only or also through rubric-based review questions.
