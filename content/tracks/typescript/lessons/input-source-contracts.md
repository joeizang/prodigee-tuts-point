# Input Source Contracts

## Learning objectives

- Model stdin and file input as a discriminated union instead of ad hoc boolean flags.
- Keep command parsing separate from input resolution so each failure has the right owner.
- Preserve empty input and whitespace behavior deliberately instead of letting `split` defaults decide.

## Prerequisites

You should know the earlier `ParseResult` pattern and be able to narrow a union with a discriminant. You should also understand that command-line programs often read from stdin when no file is provided.

## Mental model

**Term: input source** means the place a command reads from after parsing options. For this project, the two important sources are stdin and a file path. Later, the same idea can extend to URLs, sockets, or saved query profiles.

**Term: ownership boundary** means deciding which function is responsible for which problem. Argument parsing owns `--file` versus default stdin. File resolution owns missing files. Log parsing owns malformed lines. Keeping those boundaries separate makes failures clearer.

```typescript
export type InputSource =
  | { readonly kind: 'stdin' }
  | { readonly kind: 'file'; readonly path: string }
```

This type is small, but it removes a large class of ambiguous states. There is no `{ useStdin: true, filePath: 'app.log' }` contradiction. There is no empty string pretending to be a meaningful path. The shape tells the rest of the program what it can safely assume.

## Production transfer

Server-side TypeScript often accepts input from more than one protocol: CLI flags, HTTP bodies, queue messages, config files, and environment variables. The same discipline applies everywhere. Convert protocol-specific input into an application type, then make the application type impossible or at least difficult to misuse.

```typescript
export async function resolveInputSource(
  source: InputSource,
  readStdin: () => Promise<string>,
  readFile: TextFileReader,
): Promise<FileReadResult> {
  return source.kind === 'stdin'
    ? { ok: true, text: await readStdin() }
    : readTextFileSafely(source.path, readFile)
}
```

The example is intentionally direct. A senior design is not always a bigger abstraction. Often it is the smallest function that names the boundary and makes the next layer boring.

## Exercise connection

You will split log text into lines and resolve stdin/file input through injected functions. The hidden tests check empty input, trailing newlines, and missing files because those are the cases that make command-line tools irritating when the contract is implicit.

## Project connection

`logprobe-typescript` should eventually run in scripts and CI jobs. A predictable input source contract lets later milestones focus on parsing, filtering, and summaries instead of revisiting how the command decides where input comes from.

## Check yourself

- What invalid states does the `InputSource` union remove?
- Why should command parsing not read from the file system while it parses flags?
- Should an empty stdin input be an I/O failure, a parse failure, or valid empty data?

## Source reference notes

Use TypeScript books for discriminated unions and readonly object design. Use Node.js references for stdin and file reading behavior, but keep framework-specific details out of the core contract.
