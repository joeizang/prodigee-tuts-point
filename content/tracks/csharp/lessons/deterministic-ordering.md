# Deterministic Ordering

## Learning objective

Frequency results need stable ordering. Milestone 1 sorts by count descending and then word ascending. The tie-breaker matters because dictionaries do not express a user-facing order.

## Expected ordering

```csharp
// Input: "beta alpha beta alpha beta"
// Output:
// beta  3
// alpha 2
```

For ties, alphabetical order makes the output stable:

```csharp
// Input: "beta alpha"
// Output:
// alpha 1
// beta  1
```

## Testing strategy

Deterministic output makes tests reliable and makes command-line output easier to compare in later milestones.

## Project connection

Project connection: the analyzer should return a stable result shape before the CLI prints anything.

## Review prompts

- Why is dictionary iteration order not the output contract?
- What is the primary sort key?
- What is the tie-breaker?
