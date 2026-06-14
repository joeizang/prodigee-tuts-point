# Implementation Phases

## Phase 0 - Planning and Decisions

- Create planning docs.
- Resolve core domain terms.
- Resolved primary learning loop: project-backed mastery with exercise-first pressure and theory for mastery.
- Resolved first slice language strategy: one language first, cross-language-ready model.
- Resolved first slice language: C#.
- Resolved content storage model: hybrid authored files plus SQLite learner state/indexes.
- Resolved runner isolation model: host process first, container-ready abstraction.
- Resolved first slice editor requirement: full C# IntelliSense up front.
- Resolved C# exercise workspace model: real generated .NET project per attempt.
- Resolved progression model: soft locks.
- Resolved authoring model: no in-app authoring initially; future file-backed authoring studio.
- Resolved mastery model: evidence-based mastery statuses.
- Resolved diagnostics scope: light diagnostics in first version.
- Resolved experience direction: serious IDE/book hybrid with restrained gamification.
- Resolved planning rule: every v1 feature should document the intended full feature.
- Resolved identity model: local learner profile, no real authentication.
- Resolved backup model: manual export/import in v1, automatic snapshots later.
- Resolved notes model: basic personal notes in v1, full knowledge-base later.
- Resolved source model: source references are YAML-authored metadata pointers, not copied book content.
- Resolved review model: basic review cards in v1, full spaced repetition later.
- Resolved search model: basic local search in v1, deep/semantic search later.
- Resolved exercise test model: visible plus hidden tests.
- Resolved exercise workspace UI: multi-file exercises from v1.
- Resolved exercise support model: progressive hints and model solutions.
- Resolved project validation model: tests, static analysis, and AI review.
- Resolved AI review gating: advisory for normal milestones, required for capstones.
- Resolved AI review provider: OpenAI-compatible abstraction supporting hosted OpenAI and local Ollama-style endpoints.
- Resolved AI review presets: Hosted OpenAI and Local Ollama in v1.
- Resolved secrets model: provider settings in SQLite; secrets in environment variables or external local JSON outside the project directory.
- Resolved runtime model: two dev servers during development, single ASP.NET-hosted app for actual use.
- Resolved implementation target: .NET 10.
- Resolved backend API style: Minimal APIs with feature route groups.
- Resolved frontend routing: explicit React routes and deep-linkable pages.
- Resolved navigation feature: basic command palette in v1.
- Resolved shortcut feature: small keyboard shortcut set in v1.
- Resolved offline model: v1 is offline-capable except hosted AI review.
- Resolved analytics model: basic active time tracking in v1.
- Resolved content quality tooling: validator command in v1.
- Resolved seed content requirement: first slice uses real rich content, not placeholders.
- Decide first vertical slice.

## Phase 1 - App Foundation

- Initialize solution and frontend.
- Add ASP.NET Core API.
- Add React app.
- Add EF Core SQLite.
- Add initial migrations.
- Add default local learner profile.
- Add theme system.
- Add app shell and navigation.
- Add basic command palette.
- Add v1 keyboard shortcuts.

## Phase 2 - Curriculum and Reading

- Add tracks, modules, lessons, sections, code examples.
- Add lesson reader.
- Seed first track/module/lesson.
- Add progress tracking.
- Add source library metadata and YAML indexing.
- Add basic personal notes.
- Add basic local search.
- Add content quality validator command.

## Future Phase - Full Content Quality System

- Add quality score per lesson.
- Add coverage dashboard.
- Add missing concept map.
- Add stale source reference detection.
- Add exercise difficulty calibration.
- Add AI-assisted content review.
- Add authoring studio validation gates.

## Future Phase - Full Search

- Add full-text search across curriculum and notes.
- Add filters by language, track, concept, project, and mastery status.
- Add code example search.
- Add failed attempt search.
- Add semantic search.
- Add concept-to-project occurrence search.

## Phase 3 - Exercises

- Add Monaco editor.
- Add multi-file exercise workspace.
- Add full C# IntelliSense.
- Add visible and hidden test support.
- Add progressive hints and model solution unlock.
- Store attempts and results.
- Add test result display.

## Phase 4 - C# and Swift Execution

- Add C# runner if not completed in the first slice.
- Add Swift runner.
- Add language-specific starter templates.
- Add richer diagnostics.

## Phase 5 - IntelliSense

- Harden C# language server bridge.
- Add Swift SourceKit-LSP bridge.
- Keep TypeScript Monaco language service.

## Phase 6 - Projects

- Add project workspace.
- Add milestones.
- Add project validation with tests, static analysis, and AI review.
- Seed `wordfreq`.
- Add project rubrics.
- Build toward the seven-project ladder.
- Treat `mini-ci` as the first advanced capstone after the seven-project ladder.

## Phase 7 - Mastery System

- Add review cards.
- Add weak-area dashboard.
- Add prerequisites and mastery scoring.
- Add light diagnostic tests and recommendations.
- Add v1 gamification: streak, milestone completion, mastery statuses, review due count, and track/project progress.
- Add basic active time tracking.

## Future Phase - Full Learning Analytics

- Add time by track/concept/project.
- Add coding vs reading split.
- Add focus sessions.
- Add weekly trends.
- Add time-to-pass exercise.
- Add failed-attempt time analysis.
- Add mastery efficiency dashboard.

## Future Phase - Full Review System

- Add spaced repetition scheduling.
- Add code-output prediction cards.
- Add debugging cards.
- Add design judgment cards.
- Add note-to-card conversion.
- Add failed exercise auto-cards.
- Add review analytics.

## Future Phase - Full Gamification

- Add mastery timeline.
- Add streak recovery rules.
- Add cross-language project portfolio progression.
- Add personal challenge modes.
- Add attempt history views tied to rubric quality.
- Keep gamification mastery-oriented, private, and avoid social comparison by default.

## Phase 8 - Hardening

- Add integration tests.
- Add runner cleanup safeguards.
- Add export/backup.
- Improve seed/versioning workflow.

## Future Phase - Full Backup System

- Add automatic local snapshots.
- Add retention policy.
- Add restore preview.
- Add profile-specific export.
- Add content compatibility checks.
- Add optional encrypted backups.

## Future Phase - Secret Management

- Add encrypted local secret store.
- Add per-provider secret management.
- Add key validation.
- Add rotate/delete secrets.
- Add explicit opt-in for encrypted secret export.

## Future Phase - Desktop Packaging

- Add desktop wrapper.
- Add auto-start backend.
- Add local file association/export.
- Add tray/menu integration.
- Add bundled runtime checks.
- Add better local update story.

## Future Phase - Runtime Checks

- Add .NET SDK check.
- Add Swift SDK check.
- Add Node.js check.
- Add Ollama check.
- Add provider capability check.
- Add setup guidance for missing tools.

## Future Phase - Full Knowledge Base

- Add note tags.
- Add backlinks.
- Add note-to-review-card conversion.
- Add source/page references on notes.
- Add full-text search.
- Add notes export.
- Surface notes during weak-area review.

## Future Phase - Authoring Studio

- Add in-app editing for tracks, modules, theory clusters, lessons, exercises, projects, and rubrics.
- Keep authored content file-backed and version-control friendly.
- Add validation checks for lesson quality, broken links, missing exercises, missing rubrics, and invalid exercise workspaces.
- Add content review status: draft, needs review, approved, published.

## First Proposed Vertical Slice

Resolved: Vertical Slice 1 is locked. See [VERTICAL_SLICE_1.md](./VERTICAL_SLICE_1.md).

Build:

- App shell with three themes.
- SQLite-backed tracks/modules/lessons.
- First `wordfreq-csharp` project milestone.
- Six real lessons supporting that milestone, not placeholders.
- 10 focused Monaco exercises supporting that milestone.
- Visible tests, hidden tests, hints, and model solutions.
- Full C# IntelliSense in Monaco.
- Backend C# test runner.
- C# static analysis feedback.
- AI-assisted review for the first milestone rubric.
- OpenAI-compatible AI review provider abstraction.
- Hosted OpenAI and local Ollama-style provider configuration.
- Provider test action before enabling AI review.
- Attempt storage and progress display.

The first `wordfreq-csharp` milestone should focus on pure word counting functions before expanding into CLI and file I/O in later milestones.

Milestone 1 uses ASCII-first tokenization with an explicit Unicode caveat.

Milestone 1 theory cluster:

- Text as Data in C#
- Normalization and Tokenization
- Dictionaries as Frequency Maps
- Deterministic Ordering
- Testing Pure Functions with xUnit
- Designing a Small Core API

Milestone 1 includes 10 focused exercises plus the project milestone task.

This proves the learning loop before spending effort on harder C# and Swift IntelliSense integration.
