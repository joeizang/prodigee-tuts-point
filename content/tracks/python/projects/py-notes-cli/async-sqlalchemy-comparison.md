# Async SQLAlchemy Comparison

## Scenario

The sync SQLAlchemy repository is the stable baseline. Before introducing async persistence, learners need a disciplined comparison that names what changes and why. Async is useful, but only when the app commits to the whole async boundary.

## Requirements

- Compare sync and async engine factories.
- Compare session factories and dependency cleanup.
- Compare repository method signatures.
- Name driver URL requirements.
- Define async pytest expectations.
- State when sync SQLAlchemy remains acceptable.

## CLI/API contract

The public API can remain the same, but implementation boundaries change. Async repository methods are awaited, async sessions are yielded differently, and tests need async support.

## Milestone task

Build a decision matrix for sync versus async SQLAlchemy in the notes API.

## Rubric

- Correctness: sync and async components are not mixed.
- Testing: async test marker and awaited methods are identified.
- Maintainability: route style matches repository style.
- Design: async is justified by operational needs.
- Production readiness: driver and pooling choices are explicit.

## Complexity

Async persistence adds more than syntax. It changes error surfaces, fixture setup, transaction handling, driver choice, and route dependencies. Introducing it after a working sync baseline lets learners compare from evidence instead of guessing.

The goal is not to declare async always better. The goal is to understand the tradeoff and choose deliberately.

That comparison keeps the curriculum honest. Async should arrive as a design decision with costs, not as a reflexive rewrite of code that already works.

## Stretch goals

- Add real async repository implementation later.
- Add async PostgreSQL integration tests.
- Add load-test comparison notes.
