# File I/O Boundaries in Node

## Learning objectives

- Keep file-system access at the edge of a TypeScript program instead of mixing it into parsing and summarization logic.
- Represent expected read failures as explicit results that a CLI can convert into stderr and exit codes.
- Use injected file readers in exercises so the same logic can be tested without touching the real disk.

## Prerequisites

You should already understand the command request parser from the earlier TypeScript milestone. You should also be comfortable with `Promise`, `async` functions, and the difference between a value that is already available and a value that must arrive from an external system.

## Mental model

**Term: I/O boundary** means the point where your code talks to something outside the process: the file system, stdin, a socket, a database, or an HTTP request. The boundary is risky because it can be slow, missing, denied, malformed, or different on another machine.

**Term: injected dependency** means passing a function such as `readFile` into core logic instead of importing and calling it everywhere. This is not ceremony. It gives the code one place to touch the file system and many places to test the behavior.

```typescript
export type TextFileReader = (path: string) => Promise<string>

export async function readTextFileSafely(
  path: string,
  readFile: TextFileReader,
): Promise<FileReadResult> {
  try {
    return { ok: true, text: await readFile(path) }
  } catch (error) {
    return { ok: false, message: `Could not read input file: ${path}` }
  }
}
```

Notice the shape of the function. It does not parse logs. It does not print. It does not decide whether the command should exit with `1` or `2`. Its contract is smaller: convert one external read into either text or a controlled failure.

## Production transfer

Production Node services fail most often at boundaries. A path may not exist in a container. A mounted volume may be read-only. A deployment job may run from a different working directory. If file access is scattered through business logic, each caller invents a slightly different error policy.

For `logprobe-typescript`, the better design is: parse command arguments into an `InputSource`, resolve that source into text or a failure, then feed pure or streaming logic. This preserves a clean testing shape:

```typescript
const fakeReadFile: TextFileReader = async (path) => {
  if (path === 'app.log') return '2026-06-19 INFO started'
  throw new Error('ENOENT')
}
```

With an injected reader, tests can cover successful files, missing files, empty files, and permission-like failures without depending on the real machine.

## Exercise connection

The exercises ask you to write `readTextFileSafely`, split file text into stable log lines, and resolve either stdin or file input. These are deliberately small functions. They teach the habit of placing uncertainty at named edges before the parsing pipeline becomes more complex.

## Project connection

The `logprobe-typescript` project will eventually support real files. This milestone prepares that without forcing the browser exercise runner to create arbitrary files. In the full app, the same contracts can be backed by Node's `fs/promises.readFile`, but the learner-facing code remains testable.

## Check yourself

- Why is an injected `readFile` function easier to test than a direct `fs.readFile` call?
- Should a missing file be modeled the same way as a malformed log line?
- Where should a CLI decide whether a file error writes to stdout or stderr?

## Source reference notes

Use Node.js material to study `fs/promises`, path handling, and command input. Use Effective TypeScript and Essential TypeScript to connect the runtime uncertainty to precise result types, async function signatures, and narrow boundaries.
