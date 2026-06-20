# Swift Server Hardening

This milestone adds operational discipline around the `logprobe-swift` server adapter. The previous milestone proved that HTTP-shaped input can call the command core. This milestone asks what happens when requests are too large, too slow, malformed, or simply need to be explained after the fact.

The hardened handler returns an envelope: the user-facing response plus telemetry. That is not decoration. Production systems need consistent evidence for every request path. A failed validation, rejected body, timeout, or internal failure should still produce request id, status, duration, and outcome classification.

The exercise keeps real Vapor runtime concerns out of the SwiftPM package for now. That is intentional. The contract is being developed in a fast test loop before wiring it into middleware, logging, metrics, cancellation, and streaming body readers.

The full future version should use Vapor middleware and dependency-injected services for request id propagation, body-size enforcement, structured logging, metrics, timeout scheduling, cancellation-aware command execution, and integration tests with Vapor's test client. This milestone is the hardening model that later framework code should preserve.

## Rubric

**Correctness**: The handler returns stable responses for success, validation failure, oversized bodies, and timeouts.

**Design**: Hardening wraps the existing request adapter instead of duplicating query parsing, counting, rendering, or command behavior.

**Testing**: Visible and hidden tests cover successful telemetry, body rejection, validation classification, and timeout mapping.

**Maintainability**: Outcome classification and response envelope construction are local and readable. The command core remains framework-independent.

**Complexity**: Boundary checks are constant-time except for measuring request body bytes. Streaming command work remains delegated to the existing pipeline.
