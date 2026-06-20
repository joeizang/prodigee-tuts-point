# Node Process Arguments and Exit Contracts

## Learning objectives

- Separate Node runtime arguments from the application arguments your parser should own.
- Design stdout, stderr, and exit-code behavior as part of the command contract instead of an afterthought.
- Keep process interaction thin so the useful logic remains testable without launching a real process.

## Prerequisites

You should understand the first TypeScript boundary lesson, especially literal unions and converting raw strings into explicit request objects. You should also know that Node exposes process information through globals such as `process.argv`, even though the exercise code in this platform works with plain arrays for testability.

## Mental model

**Term: process boundary** means the edge where the operating system and Node runtime hand control to your program. At this boundary, the program receives strings, environment variables, streams, and an eventual exit code. Server-side TypeScript code should isolate that edge so most logic can be tested as ordinary functions.

**Term: application arguments** means the slice of `process.argv` that belongs to your tool, not to Node and not to the script launcher. In a normal Node process, `process.argv[0]` is the Node executable and `process.argv[1]` is the script path. Your parser usually wants everything after those two entries.

```typescript
export function parseProcessArgv(argv: readonly string[]): readonly string[] {
  return argv.slice(2)
}
```

That function looks small because the policy is small. The important move is naming the policy instead of scattering `slice(2)` across the program. What happens when a test passes only application arguments? What happens when a package runner adds a wrapper argument? Where should that variation be handled?

## Production transfer

Production command-line tools are integration surfaces. Shell scripts, CI jobs, deployment hooks, and incident-response workflows may call them under pressure. A tool that writes errors to stdout, returns success after a bad request, or parses the wrong argument offset can break automation silently.

```typescript
export type ExitResult = {
  readonly exitCode: number
  readonly stdout: string
  readonly stderr: string
}
```

Returning this object from core logic keeps `process.exitCode`, `console.log`, and `console.error` at the edge. The same idea transfers to HTTP handlers: convert framework-specific request objects into domain input, run core logic, then convert the result back into the protocol response.

## Exercise connection

This milestone starts by making you write tiny functions that look almost too small. That is deliberate. Small boundary functions are where senior engineers prevent later code from depending on accidental runtime details. You will strip Node arguments, parse output format, represent failures, and map parse results to exit behavior.

## Project connection

`logprobe-typescript` will eventually read logs and produce investigation summaries. Before it does that, it must behave like a reliable command. This milestone gives the project a stable command-line shell around the typed parser from milestone 1.

## Check yourself

- Why should `process.argv` not be passed directly into domain logic?
- Which stream should receive a validation error: stdout or stderr?
- What exit code should a command return when parsing fails, and why?

## Source reference notes

Use your Node.js material to review `process.argv`, stdout, stderr, and command-line behavior. Use your TypeScript books to connect those runtime strings to typed boundary functions and narrow return values.
