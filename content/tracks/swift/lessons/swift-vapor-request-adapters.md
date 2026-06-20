# Swift Vapor Request Adapters

## Learning objectives

- Translate HTTP-shaped data into trusted command values before calling reusable core logic.
- Map validation failures, success bodies, and content types into explicit response values.
- Keep Vapor-specific types outside the core exercise package while preserving the design used by real Vapor handlers.

## Prerequisites

You should understand the `RunLogprobeCommand` pipeline, Swift error handling, dictionaries, optional values, and async functions. You do not need a real Vapor dependency for this exercise. The test uses small request and response structs so the adapter design can be practiced quickly and deterministically.

## Mental model

**Term: request adapter** means the boundary code that reads framework data and converts it into application input. In Vapor, that boundary might inspect query parameters, headers, route parameters, request body bytes, authentication state, and services from `Application` or `Request`.

**Term: response mapping** means converting application outcomes into protocol-level decisions: status code, content type, and body. A successful JSON result should not accidentally return `text/plain`; a malformed query should not look like a server crash.

```swift
public struct HttpLogprobeResponse: Equatable {
    public let status: Int
    public let contentType: String
    public let body: String
}
```

The exercise uses this struct as a stand-in for Vapor's `Response`. The design pressure is the same: convert the web request at the edge, call the command core, then map the result back to HTTP.

## Production transfer

Real Vapor code should be thin at the route handler. A handler that parses every option, counts every line, formats output, logs telemetry, and decides status codes directly is difficult to test and easy to break. A better handler turns Vapor's request into a command request, calls a reusable service, then translates the result.

```swift
let command = LogprobeCommandRequest(source: .stdin, format: format, limit: limit)
let result = try await runLogprobeCommand(command, readStdin: bodyReader, readFile: fileReader)
```

That shape keeps the future Vapor version honest. The framework owns transport details. The command core owns behavior. The adapter owns translation.

## Full future feature

The full server-side Swift implementation should use a real Vapor route module with request body streaming, typed query decoding, structured logging, cancellation-aware reads, middleware-friendly errors, dependency-injected services, and integration tests against Vapor's test client. This v1 milestone intentionally models the adapter without a Vapor dependency so the core boundary can be tested with the existing SwiftPM runner first.

## Exercise connection

The `HandleLogprobeRequest` exercise asks you to validate query values, map the body to stdin, call the command core, and return stable HTTP-shaped responses. Visible tests prove successful JSON output. Hidden tests prove unsupported formats, invalid limits, and empty request bodies.

## Project connection

This milestone is the bridge from Swift command-line work to server-side Swift. It proves that the command core is reusable enough to sit behind an HTTP boundary, which is the discipline needed before adding real Vapor routing, middleware, persistence, and observability.

## Check yourself

- Why should unsupported query values return `400` instead of silently defaulting?
- What code belongs in a Vapor route handler and what code belongs in the reusable command core?
- How does this adapter make future Vapor integration smaller?

## Source reference notes

Use *Server-Side Swift with Vapor* for route handler structure, request/response thinking, and dependency boundaries. Use *The Swift Programming Language* for optionals, dictionaries, async functions, and error handling.
