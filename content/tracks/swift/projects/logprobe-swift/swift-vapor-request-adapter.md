# Swift Vapor Request Adapter

This milestone introduces the server-side Swift boundary without prematurely adding a heavyweight framework dependency to the exercise runner. The adapter receives an HTTP-shaped request value, validates query parameters, maps the body to command input, calls the reusable command core, and returns an HTTP-shaped response value.

The discipline is the same one a Vapor handler needs. Route code should translate transport details, not become the whole application. Query values are raw strings, so the adapter must narrow them into `OutputFormat` and a positive `limit`. The request body is optional, so the adapter must decide the empty-input behavior explicitly. The response must carry protocol decisions: status code, content type, and body.

This is a foundation milestone, not the final Vapor feature. The full implementation should later use Vapor's routing, request body APIs, dependency injection, structured logging, middleware-compatible error handling, cancellation-aware reads, and integration tests against Vapor's test client. Capturing that future feature now prevents this v1 adapter from becoming the product ceiling.

## Rubric

**Correctness**: Valid requests return `200` with the correct content type and rendered body. Unsupported formats and invalid limits return stable `400` responses.

**Design**: The adapter translates HTTP-shaped input into the existing command core instead of duplicating parsing, counting, or rendering logic.

**Testing**: Visible and hidden tests cover successful JSON, unsupported format, invalid limit, and empty body behavior.

**Maintainability**: HTTP status and content-type decisions are explicit and local to the adapter. Core command logic remains framework-independent.

**Complexity**: The adapter performs constant-size validation and delegates streaming work to the existing command pipeline.
