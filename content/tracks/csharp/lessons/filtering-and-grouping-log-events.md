# Filtering and Grouping Log Events

## Learning objectives

- Filter records by level and message text without reparsing raw lines.
- Count events by level with deterministic output.
- Compose query stages so each stage can be tested alone.
- Explain how a small CLI query mirrors production observability work.

## Prerequisites

You should understand `LogEvent`, LINQ filtering, dictionaries, deterministic ordering, and small core APIs. You should also understand that parsing failures are handled before query logic runs.

## Mental model

**Term: query pipeline** means a sequence of transformations where each stage has one job: parse, filter, group, sort, or format. **Term: operational summary** means a compact result that helps a developer answer a production question quickly.

Good tooling does not only print data. It reduces noise while preserving enough detail to trust the result. A query pipeline makes the reduction steps visible.

## Production transfer

The same shape appears in log aggregators, metrics dashboards, search endpoints, data exports, and background reports. Senior engineers are expected to keep these pipelines readable because operational code is often modified during pressure.

## Core idea

Keep filtering and grouping separate:

```csharp
public static IEnumerable<LogEvent> Filter(
    IEnumerable<LogEvent> events,
    string? level,
    string? contains)
{
    var query = events;

    if (!string.IsNullOrWhiteSpace(level))
    {
        query = query.Where(item => item.Level == level.ToUpperInvariant());
    }

    if (!string.IsNullOrWhiteSpace(contains))
    {
        query = query.Where(item => item.Message.Contains(contains, StringComparison.OrdinalIgnoreCase));
    }

    return query;
}
```

Grouping then becomes another stage, not a hidden side effect.

## Worked example

```csharp
var errors = LogQuery.Filter(events, "error", "payment");
var summary = LogQuery.CountByLevel(errors);
```

This asks a narrow operational question: how many matching payment events occurred at each level?

## Common mistakes

- Reparsing lines every time a new filter is added.
- Returning dictionary iteration order and calling it deterministic.
- Combining parse errors with zero-result queries so the user cannot tell the difference.
- Building one giant method that cannot be tested in stages.

## Exercise connection

`FilterLogEvents`, `CountLogLevels`, and `RunLogquerySummary` move from individual transformations to a composed query.

## Project connection

This project is a bridge from language mechanics to systems thinking. It teaches the kind of command-line tool a senior engineer might build to inspect logs before deciding whether a production service is healthy.

## Check yourself

1. Why is filtering easier after parsing into records?
2. What ordering rule should a level summary use?
3. How should a query distinguish malformed input from a valid query with no matches?

## Source reference notes

Use LINQ references for API syntax. The project-specific value is the disciplined separation between parse, filter, group, and format.
