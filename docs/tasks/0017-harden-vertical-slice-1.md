# 0017 Harden Vertical Slice 1

## Type

AFK

## Status

Completed for Vertical Slice 1 hardening.

## What to build

Harden Vertical Slice 1 with integration tests, runner cleanup safeguards, setup diagnostics, and an end-to-end acceptance pass.

## Acceptance criteria

- [x] Backend integration tests cover curriculum, exercise attempts, runner results, notes, search, progress, provider settings, export/import, and setup diagnostics.
- [x] Frontend smoke tests cover the main Vertical Slice 1 learning path surfaces.
- [x] Runner cleanup handles stale temp workspaces safely.
- [x] Missing .NET SDK, Ollama, provider config, or content/frontend setup produce actionable setup messages.
- [x] Content validator passes for seed content.
- [x] Manual export/import smoke test passes.
- [x] End-to-end path from diagnostic to milestone AI review can be completed through tested diagnostic, exercise, static analysis, provider, and advisory review paths.
- [x] The app can run as two dev servers and as a single ASP.NET-hosted built app.

## Implementation notes

- `/api/setup/diagnostics` reports `.NET SDK`, content validator, ASP.NET-hosted frontend assets, and optional Ollama health.
- Generated run workspaces are cleaned with a shorter temporary cutoff than durable learner workspaces.
- ASP.NET host fallback is tested against built frontend assets.
- Export/import has backend tests and live API smoke coverage.

## Full implementation note

This hardens the first C# vertical slice. Future hardening should add browser-level E2E tests with a dedicated runner, richer setup checks for Swift/TypeScript language servers, and packaging validation for a one-command local install.

## Blocked by

- 0001 Scaffold the .NET 10 and React app
- 0002 Build the local app shell
- 0003 Implement hybrid content loading
- 0004 Render the first curriculum path
- 0005 Add source references, notes, and search
- 0006 Add light diagnostic and soft locks
- 0007 Build the Monaco C# workspace
- 0008 Generate real .NET exercise workspaces
- 0009 Run C# exercises with visible and hidden tests
- 0010 Add progressive hints, solutions, and attempts
- 0011 Add static analysis feedback
- 0012 Add AI review providers
- 0013 Add mastery, review cards, gamification, and time tracking
- 0014 Author the wordfreq-csharp milestone content
- 0015 Validate content quality
- 0016 Add export and import
