# Swift CLI Contracts

This milestone turns the first typed request parser into the beginning of a reliable command-line tool. The goal is not a large argument-parsing framework. The goal is to establish small contracts that make later file I/O, streaming, HTTP, and Vapor work predictable.

The milestone focuses on output format selection. `logprobe-swift` will eventually speak to humans and automation, so the distinction between table output and JSON output cannot remain an unvalidated string. The supported values should be represented as Swift cases, missing values should receive a deliberate default, and unsupported values should fail loudly enough for a caller to fix the command.

The project should still keep process APIs at the edge. Core functions should not call `exit`, print directly, or read global state. They should accept plain values, return typed values, and throw typed expected errors. That design makes command behavior testable inside SwiftPM and reusable when server-side adapters arrive.

## Rubric

**Correctness**: Missing output format defaults to table. Supported table and JSON values are accepted exactly. Unsupported values preserve the original string in a typed error.

**Design**: Output choices are represented with a Swift enum. Raw strings stop at the boundary. The parsing function stays independent from process output and exit behavior.

**Testing**: Visible and hidden tests cover defaults, supported values, empty strings, and unsupported strings.

**Maintainability**: The function remains small, obvious, and easy to extend when future output modes are deliberately added.

**Complexity**: The parser is constant time and should not introduce a generic command framework for two supported cases.
