# Observability and Request Context

## Learning objectives

- Create request context from headers and clocks so each request can be traced.
- Record handler telemetry with status and duration without polluting business logic.
- Keep logs structured enough for later filtering and summaries.

## Prerequisites

You should understand request context and response contracts. You should also know that observability is not just printing text; it is producing structured facts that help you answer operational questions.

## Mental model

**Term: observability event** means a structured record about something the system did. For a server request, useful fields include request id, method, URL, status, and duration.

**Term: correlation id** means an identifier used to connect logs and responses from the same request. If a caller provides `x-request-id`, the server can preserve it. Otherwise, the server should create one.

```typescript
export type TelemetryEvent = {
  readonly requestId: string
  readonly method: string
  readonly url: string
  readonly status: number
  readonly durationMs: number
}
```

This is intentionally small and structured. A string like `"GET /logs took 4ms"` is readable but harder to query later. A structured event can power logs, metrics, tests, and review tools.

## Production transfer

When a server misbehaves, engineers need facts. Which route is slow? Which status codes spike? Which request id maps to a customer complaint? Context and telemetry let you answer those questions without scattering `console.log` across the codebase.

```typescript
export async function recordHandlerTelemetry(request, context, handle, nowMs, sink) {
  const response = await handle()
  sink({
    requestId: context.requestId,
    method: request.method,
    url: request.url,
    status: response.status,
    durationMs: nowMs() - context.startedAtMs,
  })
  return response
}
```

The handler still returns the same response. Telemetry is a side effect at the boundary, not a reason to reshape application logic.

## Exercise connection

You will create request context and record telemetry around a handler. Hidden tests verify header precedence, fallback request ids, duration calculation, and preservation of the original response.

## Project connection

`logprobe-typescript` is itself a log investigation tool. Teaching observability inside the project is important: the server should produce the kind of structured evidence it later helps analyze.

## Check yourself

- Why is `x-request-id` useful in both responses and logs?
- What telemetry fields should every HTTP request record?
- Why should telemetry wrapping preserve the handler response exactly?

## Source reference notes

Use Node.js and server-side JavaScript references for request headers and operational logging. Use Code That Fits in Your Head for the discipline of small, observable, maintainable behavior.
