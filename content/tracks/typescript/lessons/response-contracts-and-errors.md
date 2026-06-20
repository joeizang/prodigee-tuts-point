# Response Contracts and Errors

## Learning objectives

- Convert successful and failed route results into deterministic HTTP responses.
- Use JSON response bodies and content-type headers consistently.
- Keep expected client errors separate from unexpected server defects.

## Prerequisites

You should understand discriminated unions, parse results, and CLI exit results from earlier milestones. HTTP response mapping uses the same mental model: turn an internal result into a protocol-specific response at the edge.

## Mental model

**Term: response contract** means the status code, headers, and body shape a caller can depend on. It is part of the API, not an implementation detail.

**Term: client error** means the caller sent something invalid: wrong method, wrong route, bad query string, or malformed body. That is different from a server defect such as a broken dependency or unhandled exception.

```typescript
export type HttpResponse = {
  readonly status: number
  readonly headers: Readonly<Record<string, string>>
  readonly body: string
}
```

This shape is deliberately framework-neutral. Fastify, ASP.NET Core, Vapor, and raw Node all eventually need status, headers, and a body. The adapter can convert this response to a real framework response later.

## Production transfer

Good server code is unsurprising. A `404` should mean the route was not found. A `405` should mean the route exists but the method is unsupported. A `400` should mean the request shape is wrong. A successful JSON response should say it is JSON.

```typescript
export function jsonResponse(status: number, value: unknown): HttpResponse {
  return {
    status,
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(value),
  }
}
```

The important move is centralizing response shape. If every branch manually builds slightly different headers and body shapes, the API becomes harder to test and harder for clients to consume.

## Exercise connection

You will map route results to HTTP responses and then write a handler that composes method validation, URL parsing, summary reading, and JSON output. The point is not to learn a specific framework yet. The point is to make protocol decisions explicit.

## Project connection

After this milestone, `logprobe-typescript` has a credible path into Node server work. Later tasks can introduce Fastify wiring while preserving the same handler contract and tests.

## Check yourself

- Why should expected client errors produce controlled JSON instead of thrown exceptions?
- Which status code belongs to an unsupported HTTP method?
- Why is a consistent `content-type` header part of correctness?

## Source reference notes

Use server-side JavaScript and Node.js references for request and response conventions. Use Effective TypeScript to keep result unions precise and to avoid leaking `any` through the server boundary.
