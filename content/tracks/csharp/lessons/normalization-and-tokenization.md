# Normalization and Tokenization

## Learning objectives

- Separate normalization from tokenization so each rule can be tested directly.
- Implement the milestone rule: lowercase first, keep ASCII letters and digits, and treat all other characters as separators.
- Avoid empty tokens when punctuation or whitespace appears repeatedly.
- Explain why parsing order changes correctness.

## Prerequisites

You should understand C# strings, characters, loops, `List<T>`, and small helper methods. You should also understand the ASCII-first constraint from the previous lesson.

## Mental model

**Term: normalization** means converting equivalent-looking input into one comparable representation. **Term: tokenization** means turning a stream of characters into a sequence of words or symbols that later code can count.

The professional move is to keep those responsibilities separate. Normalization answers "what shape should this text have?" Tokenization answers "where are the units?" Counting should not answer either question.

## Production transfer

Server-side parsers, CLI tools, log processors, and API importers all need predictable boundaries between cleanup and interpretation. When those boundaries blur, teams get hard-to-reproduce bugs: two services count differently, retries produce different keys, or punctuation slips into analytics as fake words.

## Core idea

Normalization changes text into a comparable form. Tokenization decides where words begin and end. If you count before normalizing, `Hello`, `HELLO`, and `hello` become separate keys. If you split without handling punctuation, `hello` and `hello,` become separate keys. If you create empty strings, separators become fake data.

```csharp
var input = "Hello, HELLO. C# 12";
var tokens = WordFrequencyAnalyzer.Tokenize(input);

Assert.Equal(["hello", "hello", "c", "12"], tokens);
```

The example is a contract, not a universal grammar. `#` is a separator in this milestone, so `C#` becomes `c`. A different product could choose another rule, but it must say so and test it.

## Worked approach

Build tokenization as a pipeline:

```csharp
var lower = NormalizeToLowercase(text);
var separated = KeepAsciiLettersAndDigits(lower);
var words = SplitWordsOnSeparators(separated);
```

Each step should be small enough to test alone. That is not busywork. It makes the final analyzer easy to debug because a failing token list tells you which stage is wrong.

## Common mistakes

- Lowercasing after counting, which creates duplicate keys.
- Using `Split(' ')` without removing empty entries.
- Keeping punctuation inside tokens.
- Writing one large method that hides normalization, separation, splitting, and filtering.

## Exercise connection

This lesson supports `KeepAsciiLettersAndDigits`, `SplitWordsOnSeparators`, `TokenizeSimpleSentence`, and `TokenizeWithPunctuation`. The drills intentionally move from one rule to the whole tokenization pipeline.

## Project connection

The tokenizer feeds the frequency map. Bad tokenization corrupts every count, every sort, every CLI output line, and every later file-processing milestone.

## Check yourself

1. What should happen to repeated punctuation such as `"...!!!"`?
2. Why should tokenization happen before dictionary counting?
3. Which test proves uppercase variants collapse into one word?

## Source reference notes

Use your C# text-processing references for character checks and string splitting APIs. The project contract chooses deterministic ASCII behavior so the first analyzer remains focused and testable.
