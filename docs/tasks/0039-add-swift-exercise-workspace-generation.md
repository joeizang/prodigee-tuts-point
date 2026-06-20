# 0039 Add Swift Exercise Workspace Generation

## Type

Implementation

## Status

Completed

## Outcome

The backend can generate real SwiftPM exercise workspaces for Swift exercises. The first Swift foundation exercise proves the content-to-workspace path with an editable Swift library file, visible tests, hidden tests, `Package.swift`, and SourceKit-LSP-ready package structure.

## Acceptance Criteria

- [x] Add a Swift track scaffold with a real exercise definition, lesson, project milestone, source references, hints, solution, wrong approaches, and expected solution characteristics.
- [x] Generate `Package.swift`, `Sources/Exercise/Exercise.swift`, `Tests/ExerciseVisibleTests/VisibleTests.swift`, and `Tests/ExerciseHiddenTests/HiddenTests.swift` for `swiftpm` exercises.
- [x] Return Swift workspace files through the existing exercise workspace API with the hidden test body withheld.
- [x] Preserve learner edits by writing starter source only when missing while regenerating package/test files.
- [x] Map Swift files to Monaco's `swift` language id and register an interim Swift tokenizer for syntax highlighting only.
- [x] Add automated tests for Swift curriculum indexing, workspace shape, hidden-test privacy, and frontend language mapping.

## Verification

- `dotnet test --no-restore --filter "FullyQualifiedName~ContentQualityValidatorTests|FullyQualifiedName~CurriculumEndpointTests.TracksEndpointReturnsIndexedTracks|FullyQualifiedName~CurriculumEndpointTests.SwiftFoundationMilestoneReturnsLessonsExercisesAndSources|FullyQualifiedName~ExerciseEndpointTests.WorkspaceEndpointGeneratesSwiftPackageWorkspace"`
- `npm run test -- --run src/features/exercises/typescriptLanguageService.test.ts`
- Full backend and frontend checks were run after implementation.

## Full Feature Later

0040 now runs Swift workspaces with `swift test`, parses Swift compiler diagnostics into static-analysis records, preserves hidden-test privacy, and cleans run workspaces. 0041 replaced syntax-only editing with SourceKit-LSP-backed diagnostics, completion, hover, signature help, formatting, and supported code actions.
