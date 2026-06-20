# Typed CLI Failures

## Learning objectives

- Model parse failures as data when callers need controlled output instead of thrown exceptions.
- Preserve the exact option and message that explain why parsing failed.
- Convert success and failure results into deterministic exit behavior.

## Prerequisites

You should be comfortable with TypeScript discriminated unions and with the idea that command-line tools communicate through exit codes and streams. You do not need a full error-handling framework; this lesson focuses on small, explicit objects that are easy to test.

## Mental model

**Term: discriminated result** means a union where one property tells you which branch you have. In this milestone the discriminator is `ok`, so callers can narrow the result before touching either `value` or `error`.

**Term: operational failure** means a user-facing problem the program expects and can report cleanly, such as an unknown option or malformed limit. That is different from a programmer defect, such as a broken invariant inside the implementation.

```typescript
export type ParseResult =
  | { readonly ok: true; readonly value: CommandRequest }
  | { readonly ok: false; readonly error: ParseError }
```

The value of this shape is that parsing can fail without forcing every caller into `try`/`catch`. A command runner can inspect the result and decide exactly what goes to stdout, what goes to stderr, and which exit code should be returned. What information does the user need to fix their command? What information would be noise? Which failures should still throw because they indicate a bug?

## Production transfer

Typed failures are common in serious server code. Validation results, authorization decisions, queue-message decoding, and configuration loading all benefit from returning structured failure data when the failure is expected. Exceptions still matter, but they should not become the only language the system speaks.

```typescript
export function toExitResult(result: ParseResult): ExitResult {
  if (!result.ok) {
    return { exitCode: 2, stdout: '', stderr: result.error.message }
  }

  return { exitCode: 0, stdout: JSON.stringify(result.value), stderr: '' }
}
```

The exact output format will evolve later, but the contract is already visible: success writes useful output and exits `0`; parse failure writes a useful error and exits non-zero.

## Exercise connection

The exercises force you to use types as design pressure. A loose `string` error or a thrown exception may pass a tiny happy-path test, but it will not give the runner enough information to produce reliable CLI behavior. Keep the failure shape small and exact.

## Project connection

`logprobe-typescript` will later combine parsing, file reads, log filtering, and summaries. Typed failures keep those stages honest: malformed input belongs to parsing, missing files belong to file I/O, and empty query results belong to reporting.

## Check yourself

- When should parsing return `{ ok: false }` instead of throwing?
- Why should the failure object include the option that caused the failure?
- How does a discriminated result help TypeScript narrow the successful request?

## Source reference notes

Use Effective TypeScript for union design and narrowing. Use Node.js command-line references for exit behavior and stream conventions. Keep the book references as study anchors, not copied text.
