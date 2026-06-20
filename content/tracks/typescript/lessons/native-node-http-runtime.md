# Native Node HTTP Runtime

## Learning objectives

- Adapt Node-shaped request data into a plain application request before routing.
- Write response objects back to a writable HTTP response without spreading server APIs through the core.
- Keep byte and header normalization explicit at the edge of the server.

## Prerequisites

You should understand the framework-neutral HTTP adapter from the previous milestone. You should also know that native Node HTTP servers expose request and response objects that are stream-like, mutable, and transport-specific.

## Mental model

**Term: runtime adapter** means a thin layer that translates concrete platform objects into the plain contracts your application already understands. For Node HTTP, that means method, URL, headers, and body bytes become a `RuntimeRequest`.

**Term: response writer** means the opposite adapter: it takes a plain response and applies status, headers, and body to the mutable runtime response object. The application should not need to call `setHeader` directly.

```typescript
export type RuntimeRequest = {
  readonly method: string
  readonly url: string
  readonly headers: Readonly<Record<string, string>>
  readonly body: string
}
```

The adapter is not glamorous, but it is where server code becomes trustworthy. It decides how missing methods default, how duplicate headers are represented, how body chunks are decoded, and where the mutable Node API stops.

## Production transfer

Native Node request objects are powerful but easy to misuse. If every handler reads streams, parses headers, and writes response headers directly, each route can invent a different policy. Serious server code narrows the runtime surface early.

```typescript
export function writeNodeResponse(response: RuntimeResponse, writable: WritableResponse): void {
  writable.statusCode = response.status
  for (const [name, value] of Object.entries(response.headers)) {
    writable.setHeader(name, value)
  }
  writable.end(response.body)
}
```

This lets tests verify response behavior with a fake writable object, and later framework integrations can reuse the same response contract.

## Exercise connection

You will adapt a request from body chunks, write a response into a fake writable object, and compose a server handler from dependencies. Hidden tests check byte chunks, repeated headers, missing method defaults, and whether response writing preserves all headers.

## Project connection

`logprobe-typescript` is now leaving the purely framework-neutral layer. This milestone shows how to connect the core to something that looks like native Node without sacrificing the clean contracts you already built.

## Check yourself

- Why should route code not read raw body chunks directly?
- What policy should normalize an array-valued header into a string?
- Which layer should mutate the real response object?

## Source reference notes

Use Node.js server material to study request streams, headers, and response writing. Use TypeScript references to keep runtime adapters typed narrowly instead of leaking broad `any` values.
