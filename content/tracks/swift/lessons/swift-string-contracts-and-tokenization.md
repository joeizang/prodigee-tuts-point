# Swift String Contracts and Tokenization

## Learning objectives

- Define a precise string-normalization contract before writing Swift text code.
- Use optionals, `String`, `Character`, and arrays to produce deterministic tokens.
- Explain the invariant that makes a tokenizer correct across nil, empty, punctuation, and mixed-case input.
- Connect small text transformations to future Vapor query parsing and search endpoints.

## Prerequisites

You should understand Swift functions, `String`, `Character`, arrays, optionals, `for` loops, and XCTest assertions. You do not need advanced Unicode expertise yet, but you should be ready to state what this first tokenizer does and does not promise.

## Mental model

**Term: string contract** means the exact promise a text function makes about nil input, whitespace, casing, separators, supported characters, and result order. **Term: tokenizer invariant** means the rule that remains true after every character is processed; here, completed tokens contain only lowercase ASCII letters or digits, and the current token is either empty or still valid.

Swift makes text pleasant to read, but production text is rarely pleasant. Search boxes, route parameters, CLI flags, JSON fields, and log lines all arrive as untrusted strings. A senior engineer does not let every caller guess how those strings are normalized. The function owns the contract.

```swift
public func normalizeSearchTokens(_ text: String?) -> [String] {
    // Contract first: nil and whitespace produce no tokens.
    guard let text else { return [] }
    // Implementation comes after the contract is clear.
    return text.split(separator: " ").map { $0.lowercased() }
}
```

That example is intentionally incomplete. It handles spaces, but not punctuation. The lesson is not "use split and move on." The lesson is that every shortcut becomes part of the contract whether you write it down or not.

## Concept explanation

The `textkit-swift` project starts with a small tokenizer because tokenizers expose several Swift fundamentals at once: optionals, character iteration, mutable arrays, string building, and deterministic output. The first milestone chooses an ASCII-first rule:

- `nil` becomes `[]`.
- Whitespace-only input becomes `[]`.
- ASCII letters and digits are kept.
- Letters are lowercased.
- Any other character is a separator.
- Empty tokens are not emitted.
- Token order follows input order.

This is not a universal human-language tokenizer. It is a deliberate engineering contract for a first reusable search and command-input primitive. Later milestones can add Unicode categories, locale-aware casing, quoted phrases, or parser combinators. The important habit is to name the boundary before adding sophistication.

```swift
let tokens = normalizeSearchTokens("  Swift, Vapor-API 101!  ")
// ["swift", "vapor", "api", "101"]
```

The invariant is simple: while scanning characters, `current` only contains normalized ASCII token characters. When a separator appears, `current` is flushed if it is not empty. That invariant makes edge cases easier to debug because each failure points to either character classification, flushing, or final-token handling.

## Syntax/API reference

Use optional binding to handle `nil` directly:

```swift
guard let text else {
    return []
}
```

Use `Character` comparisons for the first ASCII-focused pass:

```swift
if character >= "a" && character <= "z" {
    current.append(character)
}
```

Use `lowercased()` when normalizing uppercase letters. It returns a `String`, so append its contents deliberately:

```swift
current.append(contentsOf: String(character).lowercased())
```

This is slightly verbose, but it avoids pretending that a `Character` and a normalized string are always the same shape in every language.

## Production transfer

The same pattern appears in server-side Swift. A Vapor route might receive a query string such as `?q=Swift,Vapor`. If route code, service code, and repository code each normalize the query differently, search behavior becomes impossible to reason about. A small tested function gives the route a stable adapter boundary.

The Engineering Core transfer is the invariant. This tokenizer is not just a Swift syntax exercise. You should be able to explain why the algorithm emits no empty tokens, why order is deterministic, and which inputs reveal bugs.

## Common mistakes

- Treating `nil` as `[""]`, which forces every caller to remember that an empty string means no query.
- Splitting only on spaces, then accidentally keeping punctuation inside tokens.
- Sorting tokens alphabetically even though the contract says preserve input order.
- Lowercasing without naming the locale and Unicode caveat.
- Forgetting to flush the final token after the loop ends.

## Testing strategy

Good tests should cover behavior classes, not only one happy path:

```swift
XCTAssertEqual(normalizeSearchTokens(nil), [])
XCTAssertEqual(normalizeSearchTokens("Swift, Vapor!"), ["swift", "vapor"])
XCTAssertEqual(normalizeSearchTokens("API 101"), ["api", "101"])
```

Hidden tests should press on repeated separators, leading/trailing separators, mixed case, and digit preservation. These are not trivia. They prove the contract is stable enough for future CLI and Vapor adapters.

## Debugging strategy

When tokenization fails, reduce the case to one character class. If `"A"` fails, casing is wrong. If `","` emits a token, separator handling is wrong. If `"a,"` fails, flushing is wrong. If `"a"` works but `"a b"` fails, state reset is wrong.

The fastest useful debug print is not the entire result. Print the character, the current token, and whether the branch kept or flushed it. Remove the print after the regression test passes.

## Exercise connection

You will implement `normalizeSearchTokens(_:)`. The visible tests show the main contract. The hidden tests check repeated separators, punctuation, final-token flushing, and digit preservation.

## Project connection

This is the first milestone of `textkit-swift`. Later milestones can use the same contract discipline for query parsing, command parsing, and Vapor request adapters. The goal is a reusable package, not one hardcoded helper.

## Check yourself

- Why is `nil` better represented as `[]` than as `[""]` for a search-token function?
- What invariant should be true about `current` after every character is processed?
- Which test would catch forgetting to flush the final token?
- Why should a future Vapor route call this function instead of reimplementing normalization?

## Source reference notes

Use The Swift Programming Language for `String`, `Character`, optionals, and control flow. Use Swift Apprentice-style collections and algorithm references for deterministic transformations, arrays, and edge-case tests. This lesson intentionally names the ASCII-first boundary so later Unicode work can be added without rewriting the contract history.
