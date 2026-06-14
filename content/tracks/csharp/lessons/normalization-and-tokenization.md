# Normalization and Tokenization

## Learning objective

Normalization changes input into a comparable form. Tokenization decides where words begin and end. In milestone 1, uppercase ASCII letters become lowercase, ASCII letters and digits are kept, and other characters behave as separators.

## Worked example

```csharp
var input = "Hello, HELLO. C# 12";
var tokens = new[] { "hello", "hello", "c", "12" };
```

The example is not a universal text-processing law. It is the milestone contract: punctuation separates words, ASCII casing is normalized, and digits are allowed.

## Failure modes

- Keeping punctuation inside tokens makes `hello` and `hello,` count as different words.
- Lowercasing after counting creates duplicate keys.
- Producing empty tokens makes counts noisy and tests brittle.

## Project connection

This is not the only valid tokenizer. It is a documented project contract. Senior engineers make parsing rules explicit because invisible parsing assumptions become bugs at integration boundaries.

Project connection: the tokenizer feeds the frequency map, so bad tokenization corrupts every later result.

## Review prompts

- Why should tokenization happen before counting?
- Which characters are separators in milestone 1?
- What test would prove repeated punctuation does not create empty words?
