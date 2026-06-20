# HTTP Boundaries in Node

## Learning objectives

- Translate HTTP request details into application request objects without letting transport concerns leak everywhere.
- Validate method, path, and query string behavior as an explicit server contract.
- Reuse the same TypeScript core that a CLI uses instead of rebuilding logic for the web entry point.

## Prerequisites

You should understand the earlier command boundary and streaming milestones. You should also know that an HTTP request has a method, URL, headers, and sometimes a body, even when the exercise workspace uses a small plain object instead of Node's concrete server types.

## Mental model

**Term: transport boundary** means the protocol edge where external callers talk to your code. CLI arguments are one transport. HTTP is another. The business operation should not become different just because the caller used a URL instead of a terminal.

**Term: request adapter** means code that converts protocol details into an application request. The adapter understands HTTP methods, paths, status codes, and query strings. The core operation understands log levels, limits, and summaries.

```typescript
export type LogQueryHttpRequest = {
  readonly method: string
  readonly url: string
}

export type LogQueryRequest = {
  readonly level?: string
  readonly limit: number
}
```

The first type is transport-shaped. The second type is application-shaped. Keeping both lets a server handler reject `POST /logs` as an HTTP problem while still sending valid query requests to the same summarizer that a CLI can use.

## Production transfer

Most server bugs appear at boundaries: accepting the wrong method, silently defaulting invalid input, returning a `200` for a malformed request, or exposing internal exception messages. A useful TypeScript service has a boring, predictable edge.

```typescript
const parsed = new URL(url, 'http://localhost')
if (parsed.pathname !== '/logs') {
  return { ok: false, status: 404, message: 'Route not found' }
}
```

The base URL is only there because relative URLs need a host to parse. The application contract remains small: support `GET /logs`, optional `level`, and a positive integer `limit`.

## Exercise connection

The exercises ask you to parse a URL, reject unsupported methods, convert route results into HTTP responses, and compose those pieces into a handler. The hidden tests check bad routes, bad limits, and method errors because these are the places where server code becomes sloppy.

## Project connection

`logprobe-typescript` now starts to cross from command-line tool into reusable server-side service. The project should feel like one core with multiple adapters, not two unrelated applications that happen to count logs.

## Check yourself

- Which part of the code should know about HTTP status codes?
- Why should a malformed `limit` not reach the log summary core?
- What changes when the same core is called from a CLI and an HTTP handler?

## Source reference notes

Use Node.js server material to study request routing and responses. Use TypeScript sources to keep the adapter types precise and prevent broad stringly-typed APIs from spreading beyond the boundary.
