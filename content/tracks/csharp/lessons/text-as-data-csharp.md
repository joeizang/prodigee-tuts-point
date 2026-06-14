# Text as Data in C#

## Learning objective

The first `wordfreq-csharp` milestone treats text as input data, not as decoration. A C# `string` is immutable, indexable, and made of `char` values, but `char` is not the same thing as a human-perceived character in every writing system.

## Mental model

A string is a stable sequence of UTF-16 code units. For ordinary ASCII text, indexing and character checks feel direct. For broader human language, that model is not enough. Senior engineers keep those two facts separate: use the simple model when the project contract allows it, and name the limitation when it does not.

```csharp
var text = "HELLO";

Console.WriteLine(text.Length); // 5
Console.WriteLine(text[0]);     // H
```

## Milestone constraint

For milestone 1, the project deliberately uses an ASCII-first rule. That keeps the first analyzer focused on clean control flow, deterministic tests, and dictionary-based counting. Unicode segmentation becomes an advanced extension after the core is proven.

## Production caution

The ASCII-first rule would be unacceptable for a search engine, multilingual analytics system, or compliance-sensitive text pipeline. It is acceptable here because the milestone states the input contract explicitly.

## Project connection

Project connection: every later part of `wordfreq-csharp` depends on having a clear rule for what counts as a word.

## Review prompts

- What is the difference between a `char` and a user-perceived character?
- Why is it better to document an ASCII-first tokenizer than to pretend it is universal?
- What later milestone should revisit Unicode handling?
