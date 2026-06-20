# Logging and Observability

## Scenario

The API can now run, but production work requires visibility. When a request fails, the app should emit enough structured information to diagnose the request without exposing secrets.

## Requirements

- Generate or accept request IDs.
- Return request IDs in response headers.
- Emit structured events with method, path, status, and duration.
- Record safe error information.
- Avoid logging API keys or request bodies by default.

## CLI/API contract

Observability should not change successful route bodies. It adds operational signals around the API contract.

## Milestone task

Add middleware that records request telemetry and exposes a test sink.

## Rubric

- Correctness: every request produces a structured event.
- Testing: request IDs, status codes, durations, and error paths are asserted.
- Maintainability: middleware owns request logging instead of each route duplicating it.
- Design: sensitive values are excluded from telemetry.
- Production readiness: emitted fields support debugging and tracing.

## Complexity

The hard part is restraint. More logs are not automatically better. A good event tells you what happened, where it happened, and how to correlate it. It should not dump secrets, raw bodies, or implementation noise. Request IDs become especially important when multiple services or clients interact.

Observability also needs tests because middleware can silently disappear during refactors. A small test sink proves that success and error paths both emit useful telemetry.

## Stretch goals

- Add JSON log formatting.
- Add correlation ID propagation.
- Add metrics counters after logs are stable.
