# File I/O Boundaries

The third `logprobe-typescript` milestone adds file and stdin input contracts without letting I/O leak through the parser and summarizer. The project should now be able to say where input comes from, read that source through a controlled boundary, and report expected read failures without throwing random implementation details at the user.

This milestone deliberately uses injected readers in the exercise workspace. That is not a shortcut. It is the same design pressure you want in production: isolate `fs/promises`, stdin, and process wiring at the edge, then keep core functions ordinary and testable. The eventual full implementation can call Node's real file APIs, but the learner should first master the boundary shape.

By the end, the project should have an `InputSource` union, a safe file-read result, a line loader with explicit newline behavior, and a resolver that can choose stdin or file input without mixing parsing, reading, and reporting into one function.

## Rubric

**Correctness**: File reads return text on success and controlled failures on expected read errors. Line splitting handles empty input, trailing newlines, and mixed newline styles predictably. Stdin and file paths are resolved through the `InputSource` contract.

**Design**: Real I/O remains injectable. Parsing owns command shape, input resolution owns reading, and later log parsing owns malformed log lines. No function should both read files and format final CLI output unless it is explicitly an edge orchestrator.

**Testing**: Tests cover successful reads, missing-file-like failures, stdin input, file input, empty input, and newline edge cases without depending on the real local file system.

**Maintainability**: Error messages are useful enough for a command-line user. Function names reveal ownership, and async signatures make the external boundary visible.

**Complexity**: Prefer direct async functions and small unions over a generic I/O framework. Add abstraction only when repeated boundary cases make the simpler shape harder to maintain.
