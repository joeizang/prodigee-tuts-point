# Formatting CLI Output

## Learning objectives

- Format analyzer results without changing the analyzer itself.
- Produce deterministic output that is friendly to humans and tests.
- Keep output formatting separate from parsing and file reading.
- Explain how formatting choices become part of a CLI contract.

## Prerequisites

You should understand records, `IEnumerable<T>`, string interpolation, `string.Join`, and deterministic ordering from milestone 1.

## Mental model

**Term: presentation boundary** means the layer that turns structured data into text for a user or another process. **Term: stable format** means output that does not change accidentally between runs.

The analyzer returns records. The formatter turns those records into lines. If you merge those responsibilities, every future output format risks changing the core behavior.

## Production transfer

Command output is often consumed by people first and scripts later. When output is stable, tests can assert it, users can compare it, and future JSON or CSV modes can be added without breaking the table mode. The same boundary appears in APIs when domain objects become response DTOs.

## Core idea

Format already-sorted records into one line per result:

```csharp
var rows = new[]
{
    new WordFrequency("go", 2),
    new WordFrequency("stop", 1),
};

var output = string.Join(Environment.NewLine, rows.Select(row => $"{row.Word}\t{row.Count}"));
```

The formatter should not sort unless sorting is explicitly its contract. In this milestone, the analyzer returns sorted results, and the formatter preserves that order.

## Worked example

```csharp
public static string FormatTable(IEnumerable<WordFrequency> frequencies)
{
    return string.Join(
        Environment.NewLine,
        frequencies.Select(item => $"{item.Word}\t{item.Count}"));
}
```

The tab delimiter is simple, deterministic, and testable. Later milestones can add aligned tables, JSON, CSV, or quiet modes.

## Common mistakes

- Formatting inside `Analyze` and losing structured results.
- Adding extra trailing whitespace that makes snapshot tests noisy.
- Re-sorting in the formatter and hiding upstream ordering bugs.
- Printing directly to `Console` from formatting code.

## Exercise connection

`FormatWordfreqTable` asks you to convert records to deterministic text. `RunWordfreqCliRequest` later uses the formatter as the final step of the command.

## Project connection

The CLI milestone needs useful output before it can be used as a real tool. Stable formatting is also the bridge to later output-format milestones.

## Check yourself

1. Why should formatting return a string instead of writing directly to the console?
2. What makes table output deterministic?
3. How would a future JSON mode benefit from keeping formatting separate?

## Source reference notes

Use your C# references for string interpolation, `string.Join`, and sequence projection. Treat formatting as a contract, not decoration.
