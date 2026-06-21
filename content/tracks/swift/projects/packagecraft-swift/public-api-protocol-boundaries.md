# Public API and Protocol Boundaries

The first `packagecraft-swift` milestone turns a simple filtering problem into a package API design exercise. The learner builds a small token-filtering pipeline that can be reused by CLI commands, future Vapor routes, tests, and other package modules.

The point is not to add protocols everywhere. The point is to expose a small behavior seam only where variation is useful. A `TokenFilter` can represent minimum length, prefix matching, future stop-word filtering, or request-specific policy. The pipeline should compose those filters while preserving token order.

## Requirements

- Expose a public `TokenFilter` protocol with one behavior: decide whether a token should be included.
- Expose public `MinimumLengthFilter` and `PrefixFilter` structs.
- Expose a public `TokenFilterPipeline` that accepts `[any TokenFilter]`.
- Apply all filters to each token.
- Preserve input order.
- Return the original tokens when no filters are configured.
- Keep the package independent from Vapor, Foundation file APIs, process state, and global configuration.

```swift
let pipeline = TokenFilterPipeline(filters: [MinimumLengthFilter(minimumLength: 4)])
XCTAssertEqual(pipeline.run(["api", "swift", "vapor"]), ["swift", "vapor"])
```

## Engineering Core transfer

This milestone is an architecture exercise in small form. The key question is not "can I write a protocol?" The key question is "does this protocol create a useful boundary?" A useful boundary is small, testable, and easier to adapt than duplicate.

Future Vapor routes should be able to decode a query, choose filters, call the package, and map the result. The route should not know how every filter works. The package should not know about Vapor request types.

## Rubric

**Correctness**: Minimum-length filtering, prefix filtering, multiple filters, no-filter behavior, empty input, and order preservation all behave exactly as specified.

**Design**: Public types are deliberate. The protocol is small. The pipeline composes behavior without importing framework concerns or hiding simple logic behind unnecessary layers.

**Testing**: Visible and hidden tests cover individual filters, composed filters, empty filter lists, and order preservation.

**Maintainability**: The API is easy to read from another module. Names reveal behavior. Future filters can be added without changing the pipeline contract.

**Complexity**: The implementation uses straightforward iteration. It avoids generic cleverness, set conversion, sorting, reflection, and framework dependencies.

## Future work

The full `packagecraft-swift` project should add package documentation examples, API review checklists, custom filter composition, semantic versioning discussion, and a Vapor service adapter that depends on the package API rather than duplicating filtering logic in route handlers.
