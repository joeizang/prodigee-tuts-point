# String Contracts and Tokenization

The first `textkit-swift` milestone builds a small but reusable text-normalization primitive. The output is simple: optional user text becomes an ordered array of lowercase ASCII tokens. The engineering habit is deeper than the function. You must define the contract, preserve an invariant while scanning input, and test the edge cases that usually make text code unreliable.

This milestone supports later server-side Swift work. Vapor query strings, route parameters, CLI arguments, JSON fields, and log filters all arrive as strings. If every adapter invents its own normalization rule, the service becomes hard to debug. `textkit-swift` starts with a tested core that later route handlers can call.

## Requirements

- `nil` and whitespace-only input return an empty array.
- ASCII letters and digits are kept.
- ASCII letters are lowercased.
- Any non-letter and non-digit character is a separator.
- Repeated separators do not produce empty tokens.
- Token order follows input order.
- The implementation stays independent from Vapor, Foundation file APIs, and process state.

```swift
XCTAssertEqual(normalizeSearchTokens("Swift, Vapor-API 101"), ["swift", "vapor", "api", "101"])
```

## Engineering Core transfer

The core invariant is that every emitted token contains only normalized token characters. The current token under construction must also satisfy that rule. When the implementation fails, the bug should fall into one of four places: character classification, lowercasing, flushing on separators, or final-token flushing.

This is algorithmic thinking in a small Swift package. You are not expected to memorize a tokenizer recipe. You are expected to explain why the loop is correct and which tests prove the important boundaries.

## Rubric

**Correctness**: Handles nil, empty text, whitespace, punctuation, repeated separators, mixed case, digits, and final-token flushing according to the stated contract.

**Design**: Keeps the tokenizer as a pure Swift function with no Vapor, file-system, process, or global state dependency. The name communicates that this is search-token normalization, not universal language parsing.

**Testing**: Visible and hidden tests cover happy path, missing input, punctuation, repeated separators, digit preservation, and edge-case flushing.

**Maintainability**: The loop is readable enough to debug. Character classification and token flushing are explicit rather than hidden in a dense regular expression.

**Complexity**: Performs a single pass over the input and keeps only the current token plus the result array. Unicode and locale-aware tokenization are deliberately deferred and named as future work.

## Future work

The full `textkit-swift` project should later add Unicode-aware token categories, quoted phrase parsing, parser composition, package API review, documentation examples, and a Vapor query adapter that reuses this tokenizer instead of duplicating string logic in route handlers.
