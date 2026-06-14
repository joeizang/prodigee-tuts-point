# Product Spec

## Purpose

Prodigee Tuts Point is a private, local-first learning web app for becoming deeply useful in real projects across C#, ASP.NET Core, Swift, server-side Swift, TypeScript, Node.js, algorithms, and senior engineering practice.

The product should feel like a serious study operating system, not a blog, notes folder, or shallow tutorial site.

## Quality Bar

The app exists to support book-depth learning. A lesson is acceptable only when it can replace several hours of careful book study plus deliberate practice. Exercises must prove concrete capability. Projects must exercise production-relevant engineering judgment.

The first implementation must include real rich seed content rather than placeholders so the product is shaped around the intended density from day one.

## Primary User

The primary user is a single developer studying privately and using the app to build durable, project-ready competence.

## Identity

Version 1 should use a default local learner profile with no login screen and no password. All progress, attempts, preferences, mastery state, and gamification state should attach to a profile id.

Full profile vision:

- Multiple local profiles
- Profile export/import
- Profile reset
- Profile archive
- Optional profile backup
- Optional local encryption if sensitive notes or attempts need protection later

## Backup and Export

Version 1 should support manual export/import of learner state and relevant content version metadata.

Full backup vision:

- Manual export/import
- Automatic local snapshots
- Keep the last N backups
- Restore preview
- Profile-specific export
- Content version compatibility checks
- Optional encrypted backups

Exports should exclude provider secrets by default.

## Personal Notes

Version 1 should support basic personal notes attached to lessons, exercises, projects, milestones, concepts, or source references. Notes should be learner state stored in SQLite, not authored curriculum.

Full notes vision:

- Markdown notes
- Tags
- Backlinks between concepts
- Convert note into review card
- Attach book/source/page references
- Full-text search
- Export notes
- Surface notes during weak-area review

## Canonical Learning Loop

The canonical loop is a project-backed mastery loop with exercise-first pressure:

```text
Track goal or diagnostic
-> project milestone
-> theory cluster
-> focused exercises
-> milestone implementation
-> tests and rubric
-> weak-area review
-> next milestone
```

The app should still support lesson browsing and reference lookup, but those are secondary workflows. The primary question the product answers is: "Can I build this well?"

The first version should include a light diagnostic that recommends a starting point without blocking progress.

## Core Workflows

### Study a Lesson

The learner enters a theory cluster from a project milestone or track path, reads a lesson, studies worked examples, answers review prompts, and completes coding exercises linked to the lesson.

### Practice in Monaco

The learner opens frequent focused exercises, edits starter code in a multi-file Monaco workspace, runs tests, sees compiler/runtime/test feedback, iterates, and submits a passing attempt.

Exercises should offer progressive hints and model solutions. The app should nudge before revealing, but should allow an intentional solution unlock to avoid dead ends.

### Build a Project

The learner starts a project, completes milestones, writes code in the relevant language, runs project tests, receives static analysis feedback, receives AI-assisted review, and reviews rubric criteria for correctness, design, maintainability, and production readiness. Project milestones drive which theory and exercises appear next.

### Review Weak Areas

The learner sees concepts with failed attempts, stale review cards, or incomplete prerequisites and can drill those areas directly.

### Track Evidence-Based Mastery

The app infers mastery from passed exercises, failed attempts, project milestone rubrics, review recall, concept staleness, and repeated success over time.

### Track Study Time

Version 1 should track basic active time on lessons, exercises, projects, and review with idle timeout behavior.

Full analytics vision:

- Time by track/concept/project
- Coding vs reading split
- Focus sessions
- Weekly trends
- Time-to-pass exercise
- Time lost to failed attempts
- Mastery efficiency dashboard

### Use Basic Review Cards

Version 1 should include basic concept-linked review cards. Review responses should update mastery state and dashboard review counts.

Full review vision:

- Spaced repetition scheduling
- Concept-linked cards
- Code-output prediction cards
- Debugging cards
- Design judgment cards
- Note-to-card conversion
- Failed exercise auto-cards
- Review analytics

### Receive AI-Assisted Project Review

Version 1 project review should use an OpenAI-compatible AI review provider abstraction. Hosted OpenAI and local Ollama-style endpoints should both fit the same provider shape when the configured endpoint supports the needed capabilities. The review should evaluate against visible rubrics and produce structured feedback for design, maintainability, edge cases, and production judgment.

Version 1 provider presets:

- Hosted OpenAI
- Local Ollama

Version 1 secrets policy:

- Store non-secret provider settings in SQLite.
- Read hosted provider API keys from environment variables or an external local JSON config file outside the project directory.
- Do not require a real API key for local Ollama default usage.
- Exclude secrets from exports by default.
- Do not use .NET user-secrets for this app.

Full AI review vision:

- Hosted OpenAI provider
- Local Ollama/OpenAI-compatible provider
- Local model provider
- Provider comparison
- Multiple saved providers
- Provider priority/fallback
- Per-project provider override
- Configurable review strictness
- Review prompt versioning
- Offline mode
- Hosted provider token/cost tracking

### Navigate With Soft Locks

The learner can jump ahead, but the app shows prerequisite warnings and recommended preparation before advanced exercises or milestones.

### Take a Light Diagnostic

The learner can take a short baseline diagnostic that checks prerequisite skill and recommends whether to start with primers, focused drills, or the first project milestone.

### Manage Curriculum Sources

The learner may record source books and topic references as quality anchors for curriculum planning. Source references are metadata pointers to books, chapters, page ranges, or topics; they are not copied book contents. The app does not need book ingestion to satisfy the core product goal.

### Use the Source Library

The learner can see which books, chapters, page ranges, or topics support a lesson, concept, exercise, project, or milestone. Version 1 source references enter the platform through authored YAML metadata. Full source-library support should add reading plans, chapter progress, source coverage maps, authoring-studio editing, and optional source map import.

### Search the Curriculum

Version 1 should include basic local search across tracks, lessons, concepts, projects, exercises, and source references.

Full search vision:

- Full-text search across curriculum and notes
- Filters by language, track, concept, project, and mastery status
- Search code examples
- Search failed attempts
- Semantic search
- "Show me where this concept appears in projects"

### Future Curriculum Authoring

The first version does not need in-app authoring. Authored curriculum lives in files. A later full implementation should include a curriculum authoring studio for managing tracks, theory clusters, lessons, exercises, project milestones, rubrics, source anchors, and validation status.

## Main Screens

- Home dashboard
- Track catalog
- Track detail
- Module detail
- Lesson reader
- Exercise workspace
- Project workspace
- Progress dashboard
- Review queue
- Source library
- Settings and theme switcher

Pages should be deep-linkable through explicit frontend routes.

Full navigation vision:

- Deep links to lesson sections
- Deep links to source references
- Resume-last-session route
- Command palette navigation
- Saved workspaces

Version 1 should include a basic command palette for opening tracks, lessons, exercises, projects, sources, search, review, and settings.

Full command palette vision:

- Open anything
- Run app actions
- Switch theme
- Resume last workspace
- Start review
- Create note
- Jump to source reference
- Run exercise tests

Version 1 keyboard shortcuts:

- `Cmd/Ctrl+K`: command palette
- `Cmd/Ctrl+Enter`: run exercise/tests
- `Cmd/Ctrl+S`: save current files/notes
- `Esc`: close overlays

Full shortcut vision:

- Configurable shortcuts
- Vim/Emacs editor modes if Monaco supports them cleanly
- Shortcut cheat sheet
- Command palette shows shortcuts
- Per-workspace shortcuts

## Experience Direction

The UI should be a serious IDE/book hybrid:

- Learning dashboard as the first screen, not a marketing landing page.
- Dense but calm navigation for tracks, projects, exercises, review, and source references.
- Lesson pages should feel like technical book chapters with executable examples.
- Exercise pages should feel closer to an IDE than a quiz site.
- Project pages should feel like engineering specs with milestones and tests.
- Progress should be visible without making the app feel childish.

Gamification should be restrained and mastery-oriented:

- Streaks for consistent study.
- Milestone completion.
- Mastery levels/statuses.
- Review pressure for stale concepts.
- Project completion markers.
- No badge clutter or points-driven behavior as the main goal.

Version 1 gamification:

- Study streak
- Project milestone completion
- Concept mastery statuses
- Review due count
- Track/project progress

Full gamification vision:

- Mastery timeline that shows how concepts move from introduced to reliable.
- Streaks with recovery rules that reward consistency without encouraging shallow study.
- Project portfolio progression across languages and skill areas.
- Review pressure that highlights stale or fragile concepts.
- Milestone history with attempt evidence and rubric quality.
- Personal challenge modes for speed, refactoring, debugging, and edge-case hardening.
- No leaderboards or social comparison unless the product later becomes intentionally multi-user.
- Gamification remains private/personal by default.

## Planning Rule

Every feature scoped for version 1 should also document the intended full feature. MVP decisions are implementation steps, not the final product ceiling.

## Themes

The app must support:

- Light
- Dark
- Neutral

Neutral means a middle-contrast theme suitable for long reading sessions when light is too bright and dark is too heavy.

## Runtime Experience

During development, the app may run as separate ASP.NET Core and Vite servers. For actual use, it should run as a single local ASP.NET Core-hosted web app that serves the built React frontend and uses a local SQLite database.

Full runtime vision:

- Desktop wrapper
- Auto-start backend
- Local file association/export
- Tray/menu integration
- Optional bundled runtime checks

Implementation target: .NET 10.

## Offline Capability

Version 1 should be offline-capable for lessons, exercises, tests/static analysis, progress, notes, search, and local Ollama-backed AI review when a local model is available. Hosted OpenAI review requires network access and a configured key.

Full offline vision:

- Explicit offline mode indicator
- Provider availability checks
- Downloadable/bundled content packs
- Local model health checks
- Cached docs/resources

## Non-Goals

- Multi-user SaaS
- Social learning
- Payments
- Public content marketplace
- Commercial publishing workflow
- Book scanning or automatic ingestion

## Open Decisions

- Resolved: the first-class learner workflow is project-backed mastery with exercise-first pressure and serious theory.
- Resolved: the first vertical slice starts with one language while preserving a cross-language curriculum model.
- Resolved: C# is the first slice language.
- Resolved: authored curriculum lives in versioned files; SQLite stores progress, attempts, mastery state, source metadata, and indexed content metadata.
- Resolved: code execution starts as trusted-local host process execution with a container-ready runner abstraction.
- Resolved: full C# IntelliSense is required in the first vertical slice.
- Resolved: C# exercises use real generated .NET projects, not single-file snippets.
- Resolved: progression uses soft locks instead of strict gates.
- Resolved: no in-app authoring in the first version; document a full authoring studio as a future capability.
- Resolved: mastery uses evidence-based status rather than page completion.
- Resolved: first version includes light diagnostics that recommend, not block.
- Resolved: UI direction is serious IDE/book hybrid with restrained gamification.
- Resolved: version 1 feature scope must document the intended full feature for later.
- Resolved: gamification is private/personal by default, with no social comparison architecture planned.
- Resolved: use local learner profiles with no real authentication.
- Resolved: v1 includes manual export/import; automatic snapshots are a later full feature.
- Resolved: v1 includes basic personal notes; full knowledge-base features come later.
- Resolved: source references are YAML-authored metadata pointers, not copied book content.
- Resolved: v1 includes basic review cards; full spaced repetition comes later.
- Resolved: v1 includes basic local search; deep/semantic search comes later.
- Resolved: exercises support multiple files from v1.
- Resolved: exercises support progressive hints and model solutions.
- Resolved: project validation uses tests, static analysis, and AI review.
- Resolved: first slice uses real rich seed content, not placeholders.
- Resolved: Vertical Slice 1 is locked in `docs/VERTICAL_SLICE_1.md`.
- Resolved: AI review uses an OpenAI-compatible provider abstraction supporting hosted OpenAI and local Ollama-style endpoints.
- Resolved: v1 includes Hosted OpenAI and Local Ollama provider presets.
- Resolved: provider settings live in SQLite; secrets stay outside SQLite and outside the project directory for v1.
- Resolved: two dev servers during development; single ASP.NET-hosted app for actual use.
- Resolved: implementation targets .NET 10.
- Resolved: frontend uses explicit React routes and deep-linkable pages.
- Resolved: v1 includes a basic command palette.
- Resolved: v1 includes a small keyboard shortcut set.
- Resolved: v1 is offline-capable except hosted AI review.
- Resolved: v1 includes basic active time tracking.
