# 0030 Add TypeScript Starter Exercise Content

## Type

AFK

## Status

Completed

## Outcome

The content catalog includes a first TypeScript/server-side TypeScript exercise that proves the Monaco language service and Node runner through the normal curriculum path.

## Acceptance Criteria

- [x] A TypeScript track exists with book-depth starter lesson, concepts, module, project, milestone, and exercise metadata.
- [x] The starter exercise focuses on a useful server-side TypeScript skill: parsing a CLI-style request into a typed command object.
- [x] The exercise includes progressive hints, model solution, common wrong approaches, expected solution characteristics, visible tests, and hidden tests.
- [x] The lesson has required pedagogy structure, highlighted key terms, syntax-highlighted TypeScript examples, check-yourself prompts, and source reference notes.
- [x] The milestone links to the lesson, exercise, and source references.
- [x] Content indexing and content quality validation pass with both C# and TypeScript tracks present.

## Verification

- `dotnet test --no-restore`
- Live smoke: `/api/curriculum/tracks` returns both `csharp:CSharp` and `typescript:TypeScript`.
- Live smoke: Vite route `/exercises/parse-command-request-ts` returns 200.

## Full Feature Later

- Expand this into a seven-project TypeScript/Node path with CLI tools, streaming data, HTTP APIs, persistence, concurrency, package design, and observability.
- Add source-reference drafting workflows specifically for TypeScript and Node books.
- Add project-backed mastery loops that compare C#, TypeScript, and Swift implementations of similar server-side tools.
