# Error Boundaries and Timeouts

## Learning objectives

- Wrap handlers so unexpected defects become controlled server responses.
- Apply request timeouts without changing the handler's core contract.
- Keep expected client errors separate from server faults.

## Prerequisites

You should understand route result mapping and async functions. You should also know that an awaited operation can hang or reject, and a production server must decide what response contract applies when that happens.

## Mental model

**Term: error boundary** means a wrapper that catches unexpected exceptions at the server edge and converts them into a controlled response. It is not a replacement for validation; it is a last line of defense.

**Term: timeout policy** means a deliberate limit on how long an operation may run before the server returns a fallback response or cancellation signal. Without timeouts, one slow dependency can consume server capacity indefinitely.

```typescript
export function wrapHandlerErrors(handler: Handler): Handler {
  return async (request, context) => {
    try {
      return await handler(request, context)
    } catch {
      return jsonResponse(500, { error: 'Internal server error', requestId: context.requestId })
    }
  }
}
```

The response should not leak stack traces. The request id gives the operator a path to logs without exposing internal details to the caller.

## Production transfer

Operational failures are normal: a downstream service fails, a file handle stalls, a query is slow, or a programmer defect escapes a route. Good server code narrows the damage. Expected client errors return `400`, `404`, or `405`; unexpected defects return `500`; slow operations return a deterministic timeout result.

```typescript
export async function applyRequestTimeout<T>(
  operation: Promise<T>,
  timeoutMs: number,
  timeoutValue: T,
): Promise<T> {
  return await Promise.race([
    operation,
    new Promise<T>((resolve) => setTimeout(() => resolve(timeoutValue), timeoutMs)),
  ])
}
```

This exercise-level helper teaches the idea. A full server later should use abort signals and dependency-level cancellation when the underlying operation supports it.

## Exercise connection

You will implement an error boundary and a timeout helper. Hidden tests verify that successful handlers are not altered, thrown errors become safe responses, and slow promises return the timeout value.

## Project connection

`logprobe-typescript` is becoming a server. Reliability now matters as much as parsing correctness. The server must fail in ways that clients and operators can understand.

## Check yourself

- Why should a server not return raw exception messages to clients?
- What is the difference between a validation failure and an unexpected defect?
- What should happen to a dependency call that never resolves?

## Source reference notes

Use Node.js server references for async errors and server lifecycle behavior. Use production engineering books as anchors for safe error messages, explicit timeouts, and operational debugging.
