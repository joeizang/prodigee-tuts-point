# Swift Server Hardening

## Learning objectives

- Add validation, timeout mapping, body-size limits, and telemetry around a server-side Swift request adapter.
- Distinguish client failures from server failures with stable status codes and operational outcomes.
- Keep hardening behavior at the boundary so command logic remains reusable and testable.

## Prerequisites

You should understand the `HandleLogprobeRequest` adapter, HTTP status codes, Swift optionals, async functions, and value modeling. This lesson builds on the previous adapter rather than replacing it.

## Mental model

**Term: hardened boundary** means the request edge protects the core from unsafe or unbounded input. It checks size limits, time budgets, malformed options, and failure classification before or around the reusable command.

**Term: telemetry envelope** means the response carries operational facts as well as user-facing output. A server needs to know whether a request was `ok`, `validation-failed`, `rejected`, `timed-out`, or `failed`.

```swift
public struct LogprobeTelemetry: Equatable {
    public let requestId: String
    public let status: Int
    public let durationMs: Int
    public let outcome: String
}
```

The important shift is that failure paths are first-class. A handler that records telemetry only after success is blind exactly when production support needs evidence.

## Production transfer

Server-side Swift code runs under pressure: concurrent requests, slow clients, malformed query strings, oversized bodies, and operations that may outlive the user's patience. A route handler should not merely call the happy path. It should enforce boundaries and make every failure leg observable.

```swift
if duration > deadlineMs {
    return timeoutEnvelope
}

if body.utf8.count > maxBodyBytes {
    return rejectedEnvelope
}
```

In a full Vapor application, deadline and cancellation would connect to request cancellation, event-loop scheduling, middleware, and structured logging. In this milestone, the testable shape is smaller: inject time, apply limits, delegate to the existing adapter, and classify the result.

## Full future feature

The full implementation should use Vapor middleware and services: request id propagation, structured logging, metrics counters and histograms, request body streaming with byte limits, cancellation-aware async work, timeout enforcement using Vapor or Swift concurrency primitives, domain-specific error types, and integration tests through Vapor's test client. This v1 milestone deliberately captures the hardening contract without pretending those runtime integrations are complete.

## Exercise connection

The `HandleHardenedLogprobeRequest` exercise asks you to return a response envelope containing both the HTTP response and telemetry. Visible tests prove the successful path. Hidden tests prove oversized body rejection, validation failure classification, and timeout mapping.

## Project connection

This milestone makes `logprobe-swift` operationally credible. The app can now express not only what response it would send but also how the server would describe the request afterward. That is the foundation for production logging, dashboards, incident review, and later Vapor middleware.

## Check yourself

- Why should oversized input be rejected before reaching the command core?
- Which failures should be `400`, `413`, `504`, and `500`?
- Why is telemetry more useful when it is produced for every path, not just success?

## Source reference notes

Use *Server-Side Swift with Vapor* for request lifecycle, middleware, logging, and route hardening concepts. Use *The Swift Programming Language* for value modeling, async functions, closures, and explicit error/failure contracts.
