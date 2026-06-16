# 0020 Add wordfreq-csharp CLI and file I/O milestone

## Type

AFK

## Status

Completed for the first CLI/file-I/O milestone.

## What to build

Add the next `wordfreq-csharp` project milestone: turn the pure analyzer core into a command-line tool that accepts text from arguments, stdin, and files, reports useful errors, and produces deterministic output.

## Acceptance criteria

- [x] Add a new project milestone for CLI and file I/O with theory cluster, focused exercises, source anchors, and rubric.
- [x] Cover command-line argument parsing, stdin, file reads, file-not-found handling, exit codes, and output formatting.
- [x] Add focused exercises for parsing options, reading files, validating inputs, formatting results, and returning meaningful errors.
- [x] Generate real .NET exercise workspaces for the new exercises with visible and hidden tests.
- [x] Extend project validation so the milestone can run an end-to-end CLI-style test.
- [x] Add content validation rules for milestone ordering, source anchors, and exercise coverage.
- [x] Update dashboard/project navigation so the learner can continue from milestone 1 into milestone 2.

## Implementation notes

- The milestone should follow the spirit of practical CLI projects: small executable, clear contract, deterministic output, useful errors, and tests that protect the behavior.
- Avoid introducing a third-party CLI framework in the first CLI milestone unless the complexity justifies it. Start with explicit parsing so the learner understands the mechanics.
- Added `cli-and-file-io` with three lessons and five exercises: option parsing, file reading, missing-file handling, table formatting, and an integration runner.
- Added a shared `WordFrequencyCli.cs` starter surface with explicit CLI models and testable I/O delegates.
- Added backend tests for the new milestone endpoint, theory cluster endpoint, and project milestone list.
- Review remediation: added real content-quality validation for contiguous milestone ordering, milestone source anchors, and milestone exercise coverage.
- Review remediation: added `RunEndpointExecutesWordfreqCliCapstoneVisibleAndHiddenTests` so the CLI integration exercise is executed through the real runner path.

## Full implementation note

Later CLI milestones should add output formats, large-file streaming, benchmarking, packaging, shell completion, configuration files, logging/tracing, and distribution. Those are not part of this first CLI/file-I/O milestone unless explicitly promoted into scope.

## Blocked by

- 0018 Polish and audit Slice 1
- 0019 Deepen C# wordfreq milestone 1 pedagogy
