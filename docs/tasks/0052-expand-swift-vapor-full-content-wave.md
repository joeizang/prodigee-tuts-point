# 0052 Expand Swift Language and Server-Side Swift with Vapor Full-Content Wave

## Type

Content

## Status

Completed

## Outcome

Wave 1 now has its first concrete expansion beyond `logprobe-swift`: the new `textkit-swift` project starts Swift language deepening with string contracts, optionals, deterministic tokenization, engineering invariants, and future Vapor query-adapter transfer.

## Acceptance Criteria

- [x] Add a Swift language project from the seven-project ladder.
- [x] Add a project milestone with a visible rubric and future Vapor transfer.
- [x] Add a lesson satisfying the full pedagogy contract with Swift code examples, key terms, production transfer, debugging strategy, testing strategy, check-yourself prompts, and source anchors.
- [x] Add a SwiftPM exercise with visible tests, hidden tests, progressive hints, model solution, common wrong approaches, and expected solution characteristics.
- [x] Embed Engineering Core concepts rather than treating Swift as syntax-only content.
- [x] Keep the implementation framework-independent while documenting the later Vapor query-adapter direction.
- [x] Add API and runner regression tests for the new project/milestone/exercise.
- [x] Content validation and targeted API tests pass.

## Verification

- Added `textkit-swift` to `content/tracks/swift/track.yml`.
- Added `Swift String Contracts and Tokenization`.
- Added `normalize-search-tokens-swift`.
- Added `String contracts and tokenization` milestone.
- Added API coverage for the `textkit-swift` project and theory cluster.
- Added runner coverage proving the SwiftPM exercise passes visible and hidden tests.

## Full Feature Later

The full Swift/Vapor wave should expand `textkit-swift` with Unicode-aware token categories, parser composition, public package API review, and a Vapor query adapter. It should also add a real Vapor dependency workspace for route tests once the framework-specific runner profile is introduced.
