# Swift File Input Boundaries

This milestone adds input-source resolution to `logprobe-swift`. A useful log investigation tool must read from stdin in pipelines and from files in repeatable scripts. Those choices should not be represented as vague optional strings spread across the program. They should be explicit Swift values handled at the boundary.

The milestone uses injected async readers instead of real disk or stdin access. That keeps the exercise deterministic while preserving the architecture used in production: the outer shell knows how to talk to the operating system, and the core knows how to select the correct input source and map expected failures into a stable result.

This is also where the command begins to separate user errors from programmer errors. A missing file path or failed read is a normal operational case. The command should produce a controlled result that later code can turn into stderr and an exit code, not crash or leak low-level details.

## Rubric

**Correctness**: Stdin input calls only the stdin reader. File input calls only the file reader. File-read failures return a stable failure result that includes the requested path.

**Design**: `InputSource` and `FileReadResult` carry the policy. Real I/O remains outside the core. The resolver does not know about `FileManager`, process handles, or Vapor.

**Testing**: Visible and hidden tests cover stdin success, file success, and file failure without touching the actual filesystem.

**Maintainability**: The switch over input source should be direct enough that later sources, such as uploaded files or network streams, can be added deliberately.

**Complexity**: The resolver performs one branch and one read. It should not prefetch, retry, cache, or parse log content.
