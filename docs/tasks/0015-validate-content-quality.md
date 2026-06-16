# 0015 Validate content quality

## Type

AFK

## Status

Completed for current content format.

## What to build

Add a content validator command that protects the book-depth quality bar and rejects incomplete or shallow authored content.

## Acceptance criteria

- [x] Validator checks lesson objectives, prerequisites, examples, exercises, and project connection.
- [x] Validator checks exercise visible tests, hidden tests, hints, and model solution.
- [x] Validator checks project milestone rubric.
- [x] Validator checks source reference ids.
- [x] Validator checks broken internal links.
- [x] Validator output is actionable for authors.
- [x] Validator runs against the `wordfreq-csharp` seed content.
- [x] Build or test workflow can run the validator.

## Implementation notes

- `ContentQualityValidator` validates the content root and returns structured diagnostics with code, scope, and message.
- `tools/ProdigeeTutsPoint.ContentValidation` runs the validator from the command line.
- API tests include a validator regression test against the live seed content.
- The solution includes the validator tool so `dotnet build ProdigeeTutsPoint.slnx` compiles it.

## Command

```bash
dotnet run --project tools/ProdigeeTutsPoint.ContentValidation -- content
```

## Full implementation note

The validator is strict for the current markdown/YAML format. Future content formats should extend this validator rather than bypass it, especially for richer project rubrics, source metadata, and internal cross-links.

## Blocked by

- 0003 Implement hybrid content loading
- 0014 Author the wordfreq-csharp milestone content
