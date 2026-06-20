# Typed Command Boundaries

## Learning objectives

- Model raw command input as an untrusted boundary instead of letting loose strings spread through the program.
- Use literal unions and narrowing to convert user-facing options into reliable TypeScript values.
- Keep parsing separate from execution so later Node.js CLI, API, and worker code can reuse the same core contract.

## Prerequisites

You should already be comfortable reading function signatures, arrays, objects, and basic `if` statements in TypeScript. This lesson assumes you know that Node.js programs receive command-line arguments as strings, but it does not assume that you have built a polished command-line tool before.

## Mental model

**Term: boundary** means the point where less-trusted outside data enters your code. At a command-line boundary, every argument starts life as text, even when it represents a number, boolean, file path, or mode. Good server-side TypeScript code does not pretend those strings are already safe domain values.

**Term: literal union** means a type such as `"info" | "warn" | "error"` where only specific string values are accepted. Literal unions are valuable at boundaries because they let the parser reject unknown options before deeper code has to defend itself repeatedly.

The central habit is simple: parse once, trust later. A raw request can be modeled as `readonly string[]`, but the rest of the program should receive a smaller object with names and types that explain intent.

```typescript
export type LogLevel = "debug" | "info" | "warn" | "error"

export type CommandRequest = {
  readonly level: LogLevel
  readonly limit: number
  readonly includeArchived: boolean
}
```

This is not just style. Once `level` is a `LogLevel`, filtering code can switch over known cases, tests become sharper, and future refactors have compiler support. What should happen when the user passes `--level trace`? What should happen when `--limit` is missing? What should happen when the value is present but not a positive integer?

## Production transfer

The same shape appears in HTTP handlers, queue consumers, scheduled jobs, and configuration loaders. Server projects often fail because raw request values leak into service code and every layer invents slightly different validation rules. A small parser gives one place for defaults, coercion, and error messages. It also gives observability a cleaner story because failures can be attached to a named boundary instead of thrown from arbitrary business logic.

```typescript
export function parseLimit(value: string | undefined): number {
  if (value === undefined) return 100

  const parsed = Number(value)
  if (!Number.isInteger(parsed) || parsed < 1) {
    throw new Error("limit must be a positive integer")
  }

  return parsed
}
```

Notice the deliberate narrowness: the parser converts one input into one reliable value. Larger functions are built by composing these focused pieces, not by mixing validation, file reads, filtering, and formatting in one block.

## Exercise connection

The exercise asks you to implement `parseCommandRequest`. You will receive an array of arguments and return a typed `CommandRequest`. Write the parser as if it will become the front door for a real tool: clear defaults, exact accepted values, and meaningful failures.

## Project connection

The `logprobe-typescript` project will become a Node.js log investigation tool. This first milestone gives the project a contract that later milestones can use for file input, streaming, filtering, and summary output. A weak parser would force every later feature to ask the same questions again.

## Check yourself

- Why is `string` too broad for a log level after parsing?
- Where should a default limit be applied: in parsing, filtering, or output formatting?
- What failure message would help you fix a bad command quickly when using the tool under pressure?

## Source reference notes

Use your TypeScript books to review type design, literal unions, narrowing, and precise object types. Use your Node.js material to connect that type model to command-line arguments and process boundaries. The platform stores these references as pointers for study; it does not copy the books' prose.
