# HTTP Adapter

The fifth `logprobe-typescript` milestone turns the project from a CLI-shaped tool into a reusable server-side core with an HTTP adapter. It does not introduce a framework yet. That is deliberate. First, the project needs a clean boundary: method, URL, query parameters, status codes, headers, and JSON response bodies.

The milestone should reuse the mental model from CLI work. A CLI adapter converts `process.argv` and streams into application requests and exit results. An HTTP adapter converts request methods and URLs into application requests and HTTP responses. The core operation should remain testable without a running web server.

By the end, the project should parse `GET /logs` query strings, reject bad routes and methods, convert route results into stable JSON responses, and compose a small handler that calls an injected summary reader.

## Rubric

**Correctness**: Only supported routes and methods reach the summary reader. Invalid limits return client errors. Success responses use status `200`, JSON content type, and deterministic body shapes. Unsupported methods and routes use appropriate non-2xx statuses.

**Design**: Transport-specific HTTP details stay in the adapter. The application request type stays small and reusable. The handler composes parser, validator, response mapper, and injected summary dependency without becoming a framework-specific monolith.

**Testing**: Visible and hidden tests cover success, bad method, bad route, default limit, explicit limit, bad limit, and summary-reader composition.

**Maintainability**: Error messages are clear enough for API clients. Response shapes are centralized so future framework wiring does not duplicate status/header/body decisions.

**Complexity**: Use small functions and plain types. Do not add Fastify, decorators, dependency injection containers, or OpenAPI generation until the framework integration milestone needs them.
