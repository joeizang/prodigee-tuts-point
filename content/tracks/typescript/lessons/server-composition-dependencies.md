# Server Composition and Dependencies

## Learning objectives

- Compose a server handler from explicit dependencies instead of importing infrastructure everywhere.
- Build request context with time and request-id providers so behavior stays testable.
- Keep dependency objects small enough to reveal what a route actually needs.

## Prerequisites

You should understand injected file readers, HTTP handlers, and response contracts. This lesson extends the same idea to server runtime dependencies such as summary readers, clocks, and request-id providers.

## Mental model

**Term: composition root** means the place where infrastructure is assembled. In a larger Node service it might create loggers, repositories, config, clients, and route handlers. Inside exercises, the composition root is a small function returning dependencies.

**Term: request context** means per-request metadata that should travel with the operation: request id, start time, tenant, authenticated user, or trace id. Context is not business input; it is operational metadata.

```typescript
export type RuntimeDependencies = {
  readonly readSummary: LogSummaryReader
  readonly nowMs: () => number
  readonly newRequestId: () => string
}
```

This object is intentionally small. If a handler depends on twenty things, the design is probably hiding too much work in one place. If it depends on nothing and imports everything globally, tests become brittle.

## Production transfer

Server code needs controlled seams. A clock makes timeouts and telemetry deterministic in tests. A request-id provider makes logs and responses traceable. A summary reader lets HTTP routes call a core service without knowing whether data came from a file, memory, SQLite, or another API.

```typescript
export function createRouteDependencies(readSummary: LogSummaryReader): RuntimeDependencies {
  return {
    readSummary,
    nowMs: () => Date.now(),
    newRequestId: () => crypto.randomUUID(),
  }
}
```

The full implementation can wire real infrastructure. The exercise version keeps the pattern visible without requiring a framework container.

## Exercise connection

You will create route dependencies and compose a handler that builds context, calls the summary reader, and returns JSON. The tests verify that dependencies are used instead of hidden globals.

## Project connection

`logprobe-typescript` is moving toward server architecture. The same design will matter later for ASP.NET Core, Swift Vapor, and Node frameworks: dependencies should be explicit, testable, and assembled at the edge.

## Check yourself

- What belongs in request context but not in application input?
- Why is an injected clock better than direct `Date.now()` in handler logic?
- How can a dependency object become too broad?

## Source reference notes

Use server-side JavaScript references for handler composition and dependency wiring. Use Code That Fits in Your Head as a quality anchor for small interfaces and explicit seams.
