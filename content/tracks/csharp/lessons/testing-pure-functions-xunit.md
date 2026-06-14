# Testing Pure Functions with xUnit

## Learning objective

Pure functions are ideal early targets for tests because the same input should always produce the same output. The first analyzer tests should cover normal sentences, punctuation, casing, empty input, whitespace, repeated words, and tie ordering.

## Example test shape

```csharp
[Fact]
public void CountWordsCombinesDifferentCasing()
{
    var result = WordFrequencyAnalyzer.Analyze("Hello hello HELLO");

    Assert.Equal(3, result.Single(item => item.Word == "hello").Count);
}
```

## Testing strategy

Good tests are not only correctness checks. They document the parsing contract and protect the project from accidental rule changes.

## Project connection

Project connection: visible tests teach the expected behavior, while hidden tests pressure edge cases.

## Review prompts

- What edge case would you test before sorting?
- Why are pure functions easier to test than console programs?
- What should hidden tests protect against?
