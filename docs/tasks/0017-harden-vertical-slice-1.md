# 0017 Harden Vertical Slice 1

## Type

AFK

## What to build

Harden Vertical Slice 1 with integration tests, runner cleanup safeguards, setup diagnostics, and an end-to-end acceptance pass.

## Acceptance criteria

- [ ] Backend integration tests cover curriculum, exercise attempts, runner results, notes, search, progress, and provider settings.
- [ ] Frontend smoke tests cover the main Vertical Slice 1 learning path.
- [ ] Runner cleanup handles stale temp workspaces safely.
- [ ] Missing .NET SDK, Ollama, provider config, or language service dependencies produce actionable setup messages.
- [ ] Content validator passes for seed content.
- [ ] Manual export/import smoke test passes.
- [ ] End-to-end path from diagnostic to milestone AI review can be completed.
- [ ] The app can run as two dev servers and as a single ASP.NET-hosted built app.

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
