# Swift Package API and Protocol Boundaries

## Learning objectives

- Design a small Swift package API with deliberate `public` and internal boundaries.
- Use a protocol when behavior genuinely needs to vary.
- Compose filters without losing deterministic order or testability.
- Connect package API design to future Vapor service boundaries.

## Prerequisites

You should understand Swift structs, functions, arrays, loops, access modifiers, and the SwiftPM package shape used in earlier exercises. You should also understand the `textkit-swift` tokenizer contract, because this lesson builds the next package layer over token arrays.

## Mental model

**Term: public API** means the surface another module, CLI adapter, Vapor route, or test can rely on. **Term: protocol boundary** means a small behavior contract that lets one part vary without making every caller know the concrete type.

A package API is a promise. If everything is public, every helper becomes part of that promise. If nothing is public, no adapter can reuse the package. Senior Swift code sits between those extremes: expose the concepts the outside world needs, keep incidental implementation private, and test the public behavior hard.

```swift
public protocol TokenFilter {
    func include(_ token: String) -> Bool
}
```

This protocol is small enough to justify itself. A filter answers one question: should this token stay? It does not own tokenization, routing, persistence, or output formatting.

## Concept explanation

The `packagecraft-swift` project teaches package design through a token-filtering pipeline. The pipeline accepts an ordered list of tokens and applies a list of filters. Order matters. A search endpoint, CLI command, or Vapor service may care that `"swift"` appears before `"vapor"` because it reflects user input.

```swift
let pipeline = TokenFilterPipeline(filters: [
    MinimumLengthFilter(minimumLength: 4),
    PrefixFilter(prefix: "s")
])

let result = pipeline.run(["go", "swift", "server", "vapor"])
// ["swift", "server"]
```

The design pressure is not the algorithm; it is the boundary. `MinimumLengthFilter`, `PrefixFilter`, and `TokenFilterPipeline` are reasonable public types because a caller might compose them. A private helper used to loop over filters should stay internal. In a later Vapor route, the route should depend on the package API, not copy filtering logic into the handler.

## Syntax/API reference

Declare the protocol as public when external modules need to provide their own filters:

```swift
public protocol TokenFilter {
    func include(_ token: String) -> Bool
}
```

Store protocol values with `any` when the pipeline should accept mixed filter types:

```swift
public struct TokenFilterPipeline {
    public let filters: [any TokenFilter]
}
```

Use an initializer to keep construction explicit:

```swift
public init(filters: [any TokenFilter]) {
    self.filters = filters
}
```

This is not abstraction for decoration. It is a package seam: future tests can provide a custom filter, and future Vapor code can compose request-specific filters without changing the pipeline.

## Production transfer

Server-side Swift services often need small reusable policy objects: validation rules, query filters, authorization checks, response mappers, and persistence adapters. Vapor route handlers should stay thin. A route can decode input, choose filters, call a package service, and return a response. The package owns behavior.

Engineering Core shows up as boundary design. The protocol is valuable only if it reduces coupling or improves testability. If the app has one filter forever, a closure or direct function may be simpler. The lesson is to choose the seam deliberately.

## Common mistakes

- Making every helper `public`, which freezes implementation details as API.
- Creating broad protocols with many unrelated methods.
- Sorting filtered tokens accidentally and destroying input order.
- Hiding all behavior behind protocols before the concrete behavior is understood.
- Putting Vapor request types into the package core, which makes the package harder to test and reuse.

## Testing strategy

Test the public behavior, not private implementation details:

```swift
let pipeline = TokenFilterPipeline(filters: [MinimumLengthFilter(minimumLength: 5)])
XCTAssertEqual(pipeline.run(["api", "swift", "vapor"]), ["swift", "vapor"])
```

Hidden tests should verify multiple filters, empty filter lists, empty token input, prefix filtering, and order preservation. Those tests protect the package contract for future CLI and Vapor adapters.

## Debugging strategy

If the output contains too many tokens, inspect which filter returned `true` unexpectedly. If the output is empty, start with one filter and one token. If order changes, search for sorting or set conversion. If a future route fails, first prove the package pipeline works outside Vapor, then inspect the adapter.

## Exercise connection

You will implement `TokenFilter`, `MinimumLengthFilter`, `PrefixFilter`, and `TokenFilterPipeline`. The exercise forces access control and protocol composition into working Swift code instead of leaving them as theory.

## Project connection

This is the first milestone of `packagecraft-swift`. Later milestones should add API documentation examples, package review, custom filters, and Vapor service adapters that call the package without leaking framework types into the core.

## Check yourself

- Why should `TokenFilter` be small instead of owning the whole pipeline?
- Which types deserve to be public in this package, and which helpers should stay internal?
- What test catches accidental sorting?
- When would a closure be simpler than a protocol?

## Source reference notes

Use The Swift Programming Language for access control, protocols, structs, and existential values. Use Swift Apprentice-style protocol-oriented programming references for composition and test seams. Use Vapor source references later to compare package services with framework route handlers.
