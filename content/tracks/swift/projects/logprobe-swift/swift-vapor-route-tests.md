# Swift Vapor Route Tests

This milestone replaces the earlier framework-free HTTP adapter with a real Vapor route tested through XCTVapor. The route accepts a JSON request body, validates the `limit` query parameter, summarizes supported log levels, and returns deterministic JSON. It proves that the SwiftPM exercise platform can now support a server-side Swift dependency workspace instead of only pretending Vapor exists.

The route is intentionally narrow. It does not introduce persistence, authentication, middleware, or streaming body reads yet. Those features belong in later milestones after the first route contract is stable. The purpose here is to make the server boundary real: HTTP method, path, body decoding, query parsing, response encoding, and error status all go through Vapor.

The project discipline stays the same as the command-line milestones. Raw input is not trusted. The route validates `limit` before running behavior, ignores unsupported log levels deliberately, normalizes supported levels case-insensitively, and returns sorted response values. A client should be able to depend on both the status code and the JSON shape.

This milestone also creates a platform standard for future server-side Swift work. Any later Vapor exercise should declare the `swiftpm-vapor` runtime when it genuinely needs Vapor and XCTVapor. Plain Swift language exercises should stay on the dependency-free `swiftpm` runtime so the curriculum remains fast and focused.

## Rubric

**Correctness**: `POST /log-levels` returns `200` with decoded JSON counts for supported levels, applies the default limit, and rejects non-positive limits with `400`.

**Design**: The route owns transport translation while the counting logic remains deterministic, small, and reviewable.

**Testing**: Route behavior is verified through XCTVapor rather than only direct function calls, covering request encoding, query parsing, status mapping, and response decoding.

**Maintainability**: The request and response contracts are named with `Content` structs, and the implementation avoids hidden global state or framework work inside unrelated helpers.

**Complexity**: Counting is linear in the number of log lines, sorting is bounded by the small supported level set, and the route avoids unnecessary async side effects.

## Full Feature Later

The full Vapor track should add dependency-injected services, route grouping, middleware-compatible error envelopes, structured logging, request IDs, body-size limits, streaming reads for large logs, cancellation-aware work, OpenAPI documentation, persistence-backed query history, and deployment diagnostics. This milestone is complete only when the first real Vapor dependency route can be generated, inspected, and tested.
