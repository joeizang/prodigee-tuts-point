# 0049 Define Seven-Project Mastery Ladders

## Type

Planning

## Status

Completed

## Outcome

The project now has a seven-project mastery ladder blueprint for each major language and server framework track, plus a cross-track Engineering Core ladder. The ladders define what the learner must build to prove practical competence instead of merely reading lessons.

## Acceptance Criteria

- [x] Define seven-project ladders for C#, TypeScript, Swift, Python, ASP.NET Core, Node.js Servers with Fastify, Server-Side Swift with Vapor, and FastAPI.
- [x] Define a separate Engineering Core cross-track ladder rather than forcing engineering practice into a single language/framework shape.
- [x] For every project, name the mastery proof, core concepts, and milestone arc.
- [x] Preserve already-seeded projects such as `wordfreq-csharp`, `logquery-csharp`, `logprobe-typescript`, `logprobe-swift`, and `py-notes-cli`.
- [x] Keep Fastify as the Node.js server framework target and avoid Express.
- [x] Keep FastAPI dependent on Python foundations rather than treating framework decorators as the starting point.
- [x] Identify source-anchor categories and capstone expectations.
- [x] Link future work to `0050` Engineering Core and later track-slice implementation tasks.

## Verification

- Added [Curriculum Project Ladders](../CURRICULUM_PROJECT_LADDERS.md).
- Updated [task index](./README.md) so `0049` points to this task and is marked completed.

## Full Feature Later

The full implementation should move these ladders into structured curriculum metadata so the app can render project-roadmap views, prerequisite graphs, source-anchor coverage, maturity status, estimated study time, and missing-content reports. For now, the ladders are a human-readable planning artifact.
