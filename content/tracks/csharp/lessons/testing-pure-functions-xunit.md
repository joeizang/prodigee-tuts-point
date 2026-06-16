# Testing Pure Functions with xUnit

## Learning objectives

- Explain why pure functions are the safest first test target in a CLI project.
- Write xUnit assertions that document parsing, counting, and ordering behavior.
- Separate visible teaching tests from hidden edge-case tests.
- Use tests to protect the project contract instead of implementation details.

## Prerequisites

You should know C# methods, return values, arrays or lists, and basic xUnit `[Fact]` tests. You should also understand the analyzer pipeline: normalize, tokenize, count, sort.

## Mental model

**Term: pure function** means a function whose result depends only on the input you pass it. **Term: edge case** means an input near a boundary of the contract: null, empty, repeated separators, ties, casing, or punctuation.

Good tests do not merely prove happy paths. They make the contract executable, especially around the awkward inputs that humans forget and production eventually finds.

## Production transfer

Backend services and CLI tools become easier to change when core behavior is protected by pure-function tests. You can refactor file handling, streaming, or HTTP layers later while still trusting the analyzer core, because the tests pin the important behavior below those integration concerns.

## Core idea

A pure function returns a value based only on its inputs. It does not read files, write console output, mutate global state, or depend on time. That makes it ideal for early project work because every behavior can be checked with direct assertions.

```csharp
[Fact]
public void AnalyzeCombinesDifferentCasing()
{
    var result = WordFrequencyAnalyzer.Analyze("Hello hello HELLO");

    Assert.Equal(new WordFrequency("hello", 3), Assert.Single(result));
}
```

This test does more than check correctness. It documents the contract: casing is normalized, repeated words are counted, and the result shape is a `WordFrequency` record.

## Visible and hidden tests

Visible tests should teach the learner what the method is supposed to do. Hidden tests should protect against shallow solutions that pass only the obvious case. For example, a visible test might check `"Hello world"`, while a hidden test checks null, repeated punctuation, whitespace-only input, or tie ordering.

## Common mistakes

- Testing console output before the pure core exists.
- Asserting implementation details such as "uses a loop" instead of behavior.
- Writing only happy-path tests.
- Combining many behaviors in one failing assertion, which makes feedback unclear.

## Exercise connection

Every focused exercise in milestone 1 has visible and hidden tests. Read visible tests as contract examples. Expect hidden tests to check nulls, empty inputs, punctuation, existing dictionary keys, and deterministic ties.

## Project connection

Later CLI milestones can be tested at the boundary, but the core should already be trusted. Strong pure-function tests keep command-line parsing bugs separate from analyzer bugs.

## Check yourself

1. Why is `Analyze("Hello hello")` easier to test than a console program?
2. What hidden test would catch a tokenizer that emits empty strings?
3. Why should tests prefer returned values over private implementation details?

## Source reference notes

Use your xUnit and maintainability references for arranging small tests, naming scenarios, and keeping assertions readable. The platform does not decide your edge cases; the project rubric does.
