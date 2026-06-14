# Technical Architecture

## Stack

- Frontend: React, TypeScript, Vite, Monaco Editor
- Backend: ASP.NET Core on .NET 10
- Persistence: EF Core with SQLite
- Runtime: local-first single-user app

Target .NET version: .NET 10.

## Runtime and Packaging

Resolved: use two dev servers during development and a single ASP.NET-hosted app for actual use.

Development:

- ASP.NET Core API server
- Vite frontend dev server
- Proxy frontend API calls to ASP.NET Core

Usable local build:

- Build React assets
- ASP.NET Core serves the built frontend
- SQLite local database
- Single local app URL

Full packaging vision:

- Desktop wrapper
- Auto-start backend
- Local file association/export
- Tray/menu integration
- Optional bundled runtime checks
- Better local update story

## Offline Capability

Resolved: version 1 is offline-capable except hosted AI review.

Offline-capable in version 1:

- Lessons
- Exercises
- Tests/static analysis
- Progress
- Notes
- Search
- Local Ollama-backed AI review when local model is available

Requires network:

- Hosted OpenAI review

Full offline implementation:

- Offline mode indicator
- Provider availability checks
- Downloadable/bundled content packs
- Local model health checks
- Cached docs/resources

## Frontend Routing

Resolved: use explicit React routes and deep-linkable pages.

Version 1 routes:

```text
/
/tracks
/tracks/:trackId
/projects/:projectId
/projects/:projectId/milestones/:milestoneId
/lessons/:lessonId
/exercises/:exerciseId
/review
/search
/sources
/settings
```

Full navigation vision:

- Deep links to lesson sections
- Deep links to source references
- Resume-last-session route
- Command palette navigation
- Saved workspaces

Version 1 command palette:

- Open tracks
- Open lessons
- Open exercises
- Open projects
- Open sources
- Open search
- Open review
- Open settings

Full command palette:

- Open anything
- Run app actions
- Switch theme
- Resume last workspace
- Start review
- Create note
- Jump to source reference
- Run exercise tests

## Keyboard Shortcuts

Resolved: include a small keyboard shortcut set in version 1.

Version 1:

- `Cmd/Ctrl+K`: command palette
- `Cmd/Ctrl+Enter`: run exercise/tests
- `Cmd/Ctrl+S`: save current files/notes
- `Esc`: close overlays

Full shortcut implementation:

- Configurable shortcuts
- Vim/Emacs editor modes if Monaco supports them cleanly
- Shortcut cheat sheet
- Command palette shows shortcuts
- Per-workspace shortcuts

## Runtime Checks

Full runtime-check vision:

- Required .NET SDK version check
- Swift SDK check
- Node.js check
- Ollama check
- Provider capability check
- Warnings for missing optional runtimes
- Setup guidance for missing tools

## Identity and Auth

Resolved: use local learner profiles with no real authentication.

Version 1:

- Create/use a default profile automatically.
- Do not show a login screen.
- Do not require a password.
- Attach progress, attempts, settings, and mastery state to a profile id.

Full profile implementation:

- Multiple local profiles
- Profile export/import
- Profile reset/archive
- Optional backup
- Optional local encryption

## Secrets and Provider Settings

Resolved: store non-secret provider settings in SQLite and keep secrets outside SQLite and outside the project directory for version 1.

SQLite provider settings:

- Provider name
- Base URL
- Model
- Enabled/disabled
- Strictness

Secrets:

- Hosted OpenAI API key comes from environment variables or an external local JSON config file outside the project directory.
- Local Ollama should not require a real API key for local default usage.
- Exports must exclude secrets by default.
- Do not use .NET user-secrets for this app.

Recommended local secret file:

```text
~/.prodigee-tuts-point/secrets.json
```

Example:

```json
{
  "providers": {
    "hosted-openai": {
      "apiKey": "..."
    }
  }
}
```

Full secrets implementation:

- Encrypted local secret store
- Per-provider secret management
- Key validation
- Rotate/delete secrets
- Explicit opt-in for exporting encrypted secrets

## Backup and Export

Resolved: version 1 should support manual export/import.

Export should include:

- Learner profile
- Progress
- Attempts and test results
- Mastery state
- Review cards
- Settings/theme preferences
- Source metadata
- Content version/index metadata
- Provider settings excluding secrets

Full backup implementation:

- Automatic local snapshots
- Retention policy
- Restore preview
- Profile-specific export
- Content compatibility checks
- Optional encrypted backup

## Personal Notes

Resolved: version 1 includes basic personal notes.

Notes should be SQLite learner state and attach to:

- Lesson
- Exercise
- Project
- Project milestone
- Concept
- Source reference

Full notes implementation:

- Markdown
- Tags
- Backlinks
- Note-to-review-card conversion
- Source/page references
- Full-text search
- Export

## Source Library

Resolved: source references are metadata pointers, not book contents.

Version 1:

- Source books are declared in content metadata.
- Source references are authored in YAML near lessons, exercises, projects, milestones, or concepts.
- The app indexes source metadata into SQLite for navigation and display.

Example:

```yaml
sourceReferences:
  - book: csharp-12-in-a-nutshell
    chapter: Collections
    pages: "320-345"
    topic: Dictionary<TKey,TValue>, hashing, lookup complexity
    usage: QualityAnchor
```

Full source-library implementation:

- Authoring-studio editing
- Source map import
- Reading plans
- Chapter progress
- Source coverage map
- Personal notes linked to source references
- Lesson-to-source and source-to-lesson navigation

## Backend Shape

Recommended solution structure:

```text
src/
  ProdigeeTutsPoint.Api/
  ProdigeeTutsPoint.Domain/
  ProdigeeTutsPoint.Infrastructure/
  ProdigeeTutsPoint.Web/
tests/
  ProdigeeTutsPoint.Api.Tests/
  ProdigeeTutsPoint.Domain.Tests/
```

`Api` should expose focused HTTP endpoints. `Domain` should hold curriculum/progress concepts. `Infrastructure` should hold EF Core, SQLite, migrations, seed data, and code runner integrations. `Web` should hold the React app.

## API Style

Resolved: use ASP.NET Core Minimal APIs with feature route groups unless a feature clearly benefits from controllers.

Suggested shape:

```text
Features/
  Curriculum/
    CurriculumEndpoints.cs
    CurriculumQueries.cs
  Exercises/
    ExerciseEndpoints.cs
    CSharpRunner.cs
  Projects/
    ProjectEndpoints.cs
  Progress/
    ProgressEndpoints.cs
  AiReview/
    AiReviewEndpoints.cs
```

Full API implementation:

- Route groups per feature
- Endpoint filters for validation where useful
- ProblemDetails responses
- OpenAPI
- Integration tests per feature
- Controllers only when a feature needs controller conventions

## Persistence

Use EF Core directly as the unit of work. Avoid a repository abstraction unless a concrete need appears.

Important persistence principles:

- Use explicit entity configuration.
- Project read models instead of over-fetching.
- Treat migrations as reviewed source code.
- Store attempts and progress durably.
- Keep content versioning in mind from the first schema.

## Curriculum Content Storage

Resolved: use a hybrid content model.

Authored curriculum should live in versioned files under `content/`. SQLite should store learner state and indexed metadata.

Recommended content shape:

```text
content/
  tracks/
    csharp/
      track.yml
      projects/
        wordfreq/
          project.yml
          milestones/
            01-count-words.md
      lessons/
        strings-and-text.md
        dictionaries.md
        cli-input.md
      exercises/
        normalize-words/
          exercise.yml
          starter.cs
          tests.cs
```

SQLite should store:

- Progress
- Attempts
- Test results
- Review cards
- Mastery scores
- Source metadata
- Content index records

## Content Validation

Resolved: version 1 includes content quality validation tooling.

Validator command should check:

- Lesson objectives
- Lesson prerequisites
- Examples
- Exercises
- Project connection
- Visible tests
- Hidden tests
- Hints
- Model solution
- Project milestone rubric
- Valid source reference ids
- Broken internal links

Full validation implementation:

- Quality score per lesson
- Coverage dashboard
- Missing concept map
- Stale source references
- Exercise difficulty calibration
- AI-assisted content review
- Authoring studio validation gates

## Future Authoring Studio

The first version should not include browser-based curriculum editing. Content should be authored in files and indexed by the app.

A later full authoring implementation should support:

- Track/module/project editing
- Theory cluster editing
- Lesson section editing
- Code example editing
- Exercise metadata, starter files, and tests
- Project milestone requirements and rubrics
- Source anchors and quality-review status
- Content validation before publishing
- File-backed persistence so authored content remains version-control friendly

## Versioning Principle

Every version 1 feature should name its full intended implementation. The architecture should avoid treating early simplifications as permanent limits unless a later ADR explicitly makes them permanent.

## Monaco and IntelliSense

TypeScript can use Monaco's built-in TypeScript language service.

Resolved: C# must have full IntelliSense in the first vertical slice.

C# should use a Roslyn-based language service or language server bridge connected to Monaco. The quality target is the Microsoft C# VS Code extension editing experience, except code navigation can be deferred. The first slice must include:

- Semantic completion with correct item kinds and member-access filtering
- Current-buffer accurate locals, parameters, and project symbols
- Live diagnostics, linting, and squiggles
- Formatting
- Code actions and refactoring
- Hover information
- Signature help
- Project-aware references from the generated exercise workspace
- Compiler/test diagnostics after execution

Swift should eventually use SourceKit-LSP.

The first slice should support one language first, recommended C#, while preserving language-aware schema and route design for TypeScript and Swift.

## Code Execution

Exercise execution should:

- Create isolated temp workspaces.
- Apply timeouts.
- Capture stdout/stderr.
- Capture compiler diagnostics.
- Run tests.
- Clean up after execution.
- Store attempt results.

Resolved: use host process execution first, behind a container-ready runner abstraction.

Runner design:

```text
ICodeRunner
CSharpHostProcessRunner
future: CSharpContainerRunner
future: TypeScriptContainerRunner
future: SwiftContainerRunner
```

Minimum safeguards:

- Temp workspace per attempt
- Timeout
- Output size limit
- No arbitrary shell strings
- Known commands only
- Workspace cleanup
- Diagnostic capture
- Trusted-local execution warning

## C# Exercise Workspace

Resolved: each C# exercise attempt should use a real generated .NET workspace.

Recommended shape:

```text
attempt-workspaces/
  {attemptId}/
    Exercise/
      Exercise.csproj
      Program.cs
      WordCounter.cs
    Exercise.Tests/
      Exercise.Tests.csproj
      WordCounterTests.cs
    Exercise.slnx
```

The frontend may show a simplified file tree, but the backend and language service should see real projects.

Exercises should support multiple files from version 1.

File roles:

- Editable learner files
- Read-only visible files
- Hidden runner/test files

Full multi-file implementation:

- File tree
- Read-only files
- Hidden files
- Reset file
- Compare with solution
- Diff against starter
- Multi-file project tasks

## Exercise Tests

Resolved: exercises use both visible and hidden tests.

Version 1:

- Visible tests are shown in the workspace or lesson UI.
- Hidden tests are added by the runner during validation.
- Results should distinguish visible and hidden failures without exposing hidden test source.

Full testing implementation:

- Property-style tests where useful
- Mutation-style challenge mode
- Performance tests for advanced milestones
- Test explanation after passing

## Exercise Hints and Solutions

Resolved: exercises support progressive hints and model solutions.

Version 1:

- Conceptual hint
- API/approach hint
- Structural hint
- Intentional solution unlock
- Full solution visible after passing

Full implementation:

- Multiple solution styles
- Compare learner solution to model
- Rubric-based solution review
- "Why this solution" commentary
- Common wrong solution gallery
- Refactoring challenge after passing

## Project Validation

Resolved: project validation uses tests, static analysis, and AI-assisted review.

Version 1:

- Run automated tests in the project workspace.
- Run static analysis appropriate to the language.
- Run AI-assisted review against the project rubric.
- Store validation results with the attempt/milestone state.
- Keep AI review behind a provider abstraction.
- Treat AI review as advisory for normal milestones and required for capstones.

C# first-slice static analysis should use .NET compiler diagnostics and analyzers where practical.

AI review provider:

- Use an AI review provider abstraction.
- Implement an OpenAI-compatible provider first.
- Support hosted OpenAI and local OpenAI-compatible endpoints such as Ollama.
- Do not hard-code review logic directly to one provider's SDK surface or one endpoint shape.
- Store provider, model, prompt version, rubric version, and structured review output with the review result.
- Choose the exact OpenAI model/API shape during implementation using current official OpenAI documentation.

Version 1 provider configuration:

- Provider kind: `OpenAICompatible`
- Base URL, for example hosted OpenAI or local Ollama
- API key, required for hosted OpenAI and optional/dummy for local providers depending on runtime
- Model id
- Capability check before enabling review

Version 1 provider presets:

```text
Hosted OpenAI
- Base URL: https://api.openai.com/v1
- API key: required
- Model: user-configured

Local Ollama
- Base URL: http://localhost:11434/v1
- API key: optional/dummy
- Model: user-configured from local model names
```

The settings UI should include a "Test provider" action before enabling AI review.

Ollama note:

- Local Ollama commonly runs at `http://localhost:11434`.
- Ollama documents OpenAI compatibility, including `/v1/responses` support in current docs.
- Ollama local API access does not require authentication by default.
- The app should verify the configured endpoint at runtime instead of assuming every local model supports the same structured-output behavior.

Full validation implementation:

- Test history
- Static analysis trend
- AI review history
- Model-solution comparison
- Design smell detection
- Refactoring challenges
- Performance checks
- Security checks for server/API projects

Full AI review provider vision:

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

## Data Model Draft

Core entities:

- LearnerProfile
- Track
- Module
- Lesson
- LessonSection
- CodeExample
- Exercise
- ExerciseTest
- Project
- ProjectMilestone
- Attempt
- ProgressRecord
- StudySession
- ReviewCard
- PersonalNote
- Concept
- ConceptMastery
- Diagnostic
- DiagnosticAttempt
- DiagnosticRecommendation
- SourceBook
- SourceReference

## Mastery Model

Resolved: use evidence-based mastery.

Signals:

- Exercise success
- Failed attempts
- Project milestone application
- Review recall
- Time decay
- Repeated success

Statuses:

- Not Started
- Introduced
- Practiced
- Applied
- Reliable
- Needs Review

## Study Time Tracking

Resolved: version 1 includes basic active time tracking.

Version 1:

- Track active time on lessons
- Track active time on exercises
- Track active time on projects
- Track active time on review
- Use idle timeout behavior

Full analytics implementation:

- Time by track/concept/project
- Coding vs reading split
- Focus sessions
- Weekly trends
- Time-to-pass exercise
- Time lost to failed attempts
- Mastery efficiency dashboard

## Review Cards

Resolved: version 1 includes basic review cards.

Version 1:

- Concept-linked review prompts
- Due count on dashboard
- Review response updates mastery state
- Manual or seeded cards from authored content

Full review implementation:

- Spaced repetition scheduling
- Code-output prediction cards
- Debugging cards
- Design judgment cards
- Note-to-card conversion
- Failed exercise auto-cards
- Review analytics

## Search

Resolved: version 1 includes basic local search.

Version 1:

- Search tracks
- Search lessons
- Search concepts
- Search projects
- Search exercises
- Search source references

Use SQLite FTS if it fits the schema cleanly.

Full search implementation:

- Full-text search across curriculum and notes
- Filters by language, track, concept, project, and mastery status
- Code example search
- Failed attempt search
- Semantic search
- Concept-to-project occurrence search

## Diagnostics

Resolved: include light diagnostics in the first version.

Diagnostics should produce recommendations and mastery signals. They should not become strict gates.

## Open Decisions

- Resolved: database/content storage uses a hybrid model.
- Resolved: code runner starts as host process execution with a container-ready abstraction.
- Resolved: C# exercises use real generated .NET project workspaces.
- Resolved: no in-app authoring initially; future authoring studio should remain file-backed.
- Resolved: local learner profile only; no real auth for the single-user app.
- Resolved: manual export/import in v1; automatic snapshots later.
- Resolved: basic personal notes in v1; full knowledge-base later.
- Resolved: source references are YAML-authored metadata pointers, not copied book content.
- Resolved: project validation uses tests, static analysis, and AI review.
- Resolved: AI review is advisory for normal milestones and required for capstones.
- Resolved: AI review uses an OpenAI-compatible provider abstraction supporting hosted OpenAI and local Ollama-style endpoints.
- Resolved: v1 includes Hosted OpenAI and Local Ollama provider presets.
- Resolved: provider settings live in SQLite; secrets stay outside SQLite and outside the project directory in v1.
- Resolved: two dev servers during development; single ASP.NET-hosted app for actual use.
- Resolved: implementation targets .NET 10.
- Resolved: backend API style is Minimal APIs with feature route groups.
- Resolved: frontend uses explicit React routes and deep-linkable pages.
- Resolved: v1 includes a basic command palette.
- Resolved: v1 includes a small keyboard shortcut set.
- Resolved: v1 is offline-capable except hosted AI review.
- Resolved: v1 includes basic active time tracking.
- Resolved: v1 includes content quality validation tooling.
- Whether to use containers for Swift/C#/TypeScript execution.
- Whether to require installed Swift and .NET SDKs on the host.
