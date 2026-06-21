# Swift Vapor Routing and Route Tests

## Learning objectives

- Register a real Vapor route that keeps request transport work at the edge.
- Decode JSON content and validate query parameters before running application behavior.
- Return deterministic response values that are easy to assert in integration tests.
- Use XCTVapor to test status codes, decoded response bodies, and failure behavior through the framework.

## Prerequisites

You should already understand the framework-free request adapter milestone, Swift dictionaries, arrays, `throw`, closures, value structs, and SwiftPM packages. This lesson is the first point where the exercise workspace depends on Vapor itself, so it also assumes you can distinguish reusable package code from framework integration code.

## Mental model

**Term: route registration** means binding an HTTP method and path to a handler on `Application`. The handler is not the whole program; it is the adapter that receives a `Request`, narrows raw input, calls useful behavior, and returns a response value.

**Term: route test** means a test that sends an HTTP request through Vapor's testing client instead of calling your function directly. Direct function tests still matter, but route tests catch wiring mistakes: wrong method, wrong path, missing content decoding, bad status mapping, and accidental response-shape drift.

```swift
public func routes(_ app: Application) throws {
    app.post("log-levels") { request async throws -> [LevelCountResponse] in
        let body = try request.content.decode(LogBody.self)
        let limit = request.query[Int.self, at: "limit"] ?? 10
        return summarizeLogLevels(body.text, limit: limit)
    }
}
```

That shape should feel familiar from the previous adapter lesson. The difference is that the request and response now move through Vapor's real content, query, status, and testing pipeline. You are no longer proving a hand-written stand-in; you are proving the route contract the server will expose.

The important discipline is ownership. Vapor owns routing, decoding, query access, and HTTP errors. Your application owns the log-level counting rule. The handler owns translation between them. When those responsibilities blur, route code grows into a hard-to-test script.

## Production transfer

Real server-side Swift services need more than a route that happens to work once. Production routes must state their input contract, reject invalid input deliberately, produce stable output, and keep domain behavior small enough to review. A route that silently treats `limit=0` as the default teaches clients the wrong contract; a route that returns unsorted dictionary output creates flaky clients and tests.

```swift
guard limit > 0 else {
    throw Abort(.badRequest, reason: "limit must be a positive integer")
}
```

This explicit failure is not ceremony. It is the server boundary doing its job. The same thinking later applies to authentication, body-size limits, streaming bodies, request IDs, structured logging, persistence failures, and cancellation. The route should expose a clear contract even when the internals become more capable.

## Exercise connection

The `BuildVaporLogLevelRoute` exercise asks you to register `POST /log-levels`. The request body is JSON with a `text` field. The optional `limit` query parameter defaults to `10`, must be positive, and controls the number of returned levels. The response is JSON containing supported log levels sorted by count descending and then by level name ascending.

Visible tests prove the happy path through `XCTVapor`: route registration, JSON body encoding, query parsing, status code, and decoded response. Hidden tests prove invalid limits, default limits, unsupported lines, case-insensitive level detection, and deterministic tie ordering.

## Project connection

This milestone moves `logprobe-swift` from a framework-shaped adapter to a real Vapor route. It is still intentionally small, because the goal is to make the first Vapor dependency workspace trustworthy before adding controllers, services, middleware, persistence, authentication, or deployment concerns. The full project ladder can now build on a route that is genuinely exercised through Vapor.

## Check yourself

- What bug can a route test catch that a direct function test would miss?
- Why should `limit=0` return a client error instead of falling back to `10`?
- Where should log-level counting live once the route grows more production features?
- What response fields must remain deterministic so clients and tests do not become flaky?

## Source reference notes

Use *Server-Side Swift with Vapor* for route registration, `Request` content decoding, `Abort` errors, and XCTVapor testing style. Use *The Swift Programming Language* for throwing validation, closures, arrays, dictionaries, and deterministic value modeling. This lesson does not copy those books; it turns their ideas into a project-specific server-side exercise.
