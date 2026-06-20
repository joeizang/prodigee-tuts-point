# Node CLI Contracts

The second `logprobe-typescript` milestone turns the typed request parser into the beginning of a real command-line program. The goal is not to build a large parser library. The goal is to make the boundary explicit enough that every later feature can trust the shape of the command it receives.

This milestone covers the runtime edge around Node: the difference between `process.argv` and application arguments, output format choices, structured parse failures, and deterministic exit results. These are small pieces, but they control how the tool behaves inside scripts, CI jobs, and incident-response workflows. A senior engineer treats those behaviors as contracts, not incidental console output.

By the end, the project should have a clean path from raw runtime input to typed request or typed failure. The core logic should not call `process.exit`, should not write directly to stdout or stderr, and should not throw for ordinary user mistakes when a structured parse result would give better feedback. This keeps the CLI shell testable and makes future file I/O and streaming work easier to reason about.

## Rubric

**Correctness**: Node launcher arguments are stripped exactly once. Output format accepts only supported values. Parse failures preserve option and message. Exit results use success code `0`, parse-failure code `2`, stdout for successful output, and stderr for errors.

**Design**: Runtime concerns remain at the edge. Functions accept plain values and return typed objects. Discriminated unions are used where callers need controlled failure handling.

**Testing**: Visible and hidden tests cover normal cases, defaults, malformed values, short `argv` arrays, unknown options, and stream/exit-code expectations.

**Maintainability**: Error messages are direct enough for a user to fix a command. The parser is readable without depending on clever mutation or broad `any` types.

**Complexity**: Each function should be linear and focused. Do not introduce a generic command framework until repeated complexity appears in later milestones.
