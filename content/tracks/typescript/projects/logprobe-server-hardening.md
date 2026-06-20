# Server Hardening

The seventh `logprobe-typescript` milestone hardens the server boundary. The project already knows how to parse requests and return responses. Now it must behave predictably when handlers throw, dependencies are slow, request ids need to be preserved, and telemetry must be recorded for operations.

Hardening is not polish. It is part of senior engineering. The same code path that works in a test can fail under load, during a deployment, or when a dependency stalls. A useful server narrows that damage and leaves evidence behind.

By the end, the project should have an error boundary, timeout helper, request-context builder, and telemetry wrapper. These are still small framework-neutral functions, but they express the policies that production frameworks later enforce at scale.

## Rubric

**Correctness**: Unexpected exceptions become safe `500` JSON responses. Slow operations return a deterministic timeout value. Request context preserves incoming request ids and records start time. Telemetry records method, URL, status, request id, and duration while preserving the original response.

**Design**: Hardening logic wraps handlers instead of contaminating core business functions. Context and telemetry are explicit boundary concerns. Expected client errors remain separate from unexpected defects.

**Testing**: Tests cover successful handlers, thrown handlers, timeout fallback, fast operation pass-through, request-id header precedence, fallback ids, duration calculation, and telemetry side effects.

**Maintainability**: Error responses avoid leaking stack traces. Telemetry events are structured and stable. The timeout helper documents its limitation as a fallback pattern until abortable dependencies are added.

**Complexity**: Use small wrappers and dependency injection. Do not add a logging framework, metrics backend, tracing SDK, or cancellation framework until later integration milestones require them.
