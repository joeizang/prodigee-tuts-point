# Text as Data in C#

## Learning objectives

- Explain the practical difference between `string`, `char`, UTF-16 code units, and user-perceived characters.
- Treat the milestone tokenizer as an explicit engineering contract instead of a vague "word" detector.
- Choose simple ASCII-first logic deliberately, while naming where that logic is not production-complete.
- Read and write small string code without mixing parsing, counting, and output concerns.

## Prerequisites

You should be comfortable with C# methods, local variables, `if` statements, `foreach`, and returning values. You do not need advanced Unicode knowledge yet. The important professional habit is to state the text contract clearly before writing code that depends on it.

## Mental model

**Term: string contract** means the exact promise your code makes about nulls, casing, separators, and supported characters. **Term: code unit** means the storage-sized `char` value C# exposes when you index a string; it is useful but not identical to a complete written character in every language.

Treat the milestone text rule like an API boundary. Once the rule is explicit, every helper method can be judged against that boundary instead of against a vague idea of what "word" might mean.

## Production transfer

Real text systems fail when teams hide text assumptions. Search, compliance exports, data importers, and logs all need clear rules for casing, separators, encoding, and unsupported input. This milestone practices the habit on a small ASCII-first surface so later server and CLI projects can use the same discipline with broader Unicode and localization requirements.

## Core idea

The first `wordfreq-csharp` milestone treats text as input data. A C# `string` is immutable: methods such as `ToLowerInvariant` return a new string rather than changing the existing one. It is indexable, and indexing returns a `char`. That is convenient for ASCII text, but it is not a complete model of human writing systems.

```csharp
var text = "HELLO";

Console.WriteLine(text.Length); // 5
Console.WriteLine(text[0]);     // H

var lower = text.ToLowerInvariant();
Console.WriteLine(text);  // HELLO
Console.WriteLine(lower); // hello
```

Senior engineers keep the useful simplification and the limitation separate. For this milestone, we intentionally use an ASCII-first contract: ASCII letters and digits may become word characters; punctuation and whitespace separate words; broader Unicode segmentation is named as future work. This would be too weak for a compliance-sensitive text pipeline, but it is appropriate for a deliberately scoped first milestone.

## Worked example

```csharp
static bool IsAsciiLetterOrDigit(char value)
{
    return value is >= 'a' and <= 'z'
        or >= 'A' and <= 'Z'
        or >= '0' and <= '9';
}
```

This is not "the correct definition of a word." It is the project rule. That distinction matters. A search engine, legal document processor, or multilingual analytics system would need a richer approach. The milestone is narrower because the first engineering target is clean decomposition: normalize, tokenize, count, sort.

## Common mistakes

- Pretending `char` equals a full human character in every language.
- Lowercasing with culture-sensitive APIs when the exercise expects deterministic behavior.
- Mixing parsing and console output in the same method, which makes tests indirect and brittle.
- Leaving the tokenizer contract implicit, then debugging disagreements later in tests or CLI output.

## Exercise connection

This lesson supports `NormalizeToLowercase`, `KeepAsciiLettersAndDigits`, and the later tokenizer drills. When those exercises ask for ASCII behavior, they are asking you to implement a named project boundary, not a universal natural-language parser.

## Project connection

Every later `wordfreq-csharp` milestone depends on this first decision. CLI flags, file input, streaming, and formatted output should wrap a stable core instead of redefining what counts as a word.

## Check yourself

1. Why does `ToLowerInvariant` return a new string?
2. Why is an ASCII-first tokenizer acceptable for this milestone but not for every product?
3. What should a test prove about punctuation?

## Source reference notes

Use your C# language references for `string`, `char`, immutability, and indexing. The platform docs and books are anchors for API behavior; the milestone contract decides which subset we apply here.
