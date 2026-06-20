# Typed Command Boundary

The first milestone of `logprobe-typescript` is not a flashy parser. It is the contract that decides whether every later feature receives reliable input or has to keep defending itself from vague strings. Build the boundary like a senior engineer would: small enough to test completely, explicit enough to explain to another maintainer, and strict enough that invalid requests cannot drift into the query engine.

The project will eventually read log lines, stream large files, filter events, group counts, and produce deterministic output. Before any of that matters, the tool needs a typed request object. The boundary accepts command-like arguments such as `--level warn`, `--limit 50`, and `--archived`, then returns a `CommandRequest` with a literal-union `level`, a positive integer `limit`, and a boolean archive flag. That object becomes the handoff between input parsing and useful work.

Good TypeScript here means the implementation does not hide behind `any`, does not return broad strings when the domain has only a few legal values, and does not let `NaN` or zero become a valid limit. The compiler should help you after parsing, not merely decorate JavaScript with annotations. Tests should cover normal input, defaults, unsupported levels, malformed limits, and repeated option behavior.

## Rubric

**Correctness**: The parser returns exactly the expected typed request for valid arguments, applies deterministic defaults, rejects unsupported levels, rejects non-positive or non-integer limits, and treats boolean flags consistently.

**Design**: Parsing is separate from future execution work. The returned object uses precise types, especially literal unions for levels and a number for limits. Helper functions are acceptable when they make individual parsing rules easier to test and reason about.

**Testing**: Visible and hidden tests should both pass. Your own reasoning should include default cases, error cases, and the case where arguments appear in ordinary command-line pairs.

**Maintainability**: Error messages should name the bad option or value. The implementation should be readable without clever parser tricks, because this boundary will expand in later milestones.

**Complexity**: A single linear pass over the arguments is enough. Avoid general-purpose parser abstractions until the project has real repeated complexity that justifies them.
