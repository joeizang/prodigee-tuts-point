# Persistence Abstraction

## Scenario

The notes API still works with JSON storage, but the next step is a database. Before adding SQLite, the persistence details need to move behind a repository boundary.

## Requirements

- Define a repository contract for note persistence.
- Keep duplicate-note and validation rules in the service.
- Keep file path and JSON details in the repository implementation.
- Test service behavior with a fake repository.
- Keep FastAPI handlers dependent on the service, not storage.

## CLI/API contract

The CLI and API both call application services. Those services call repositories. Neither adapter should care whether the repository uses JSON today or SQLite tomorrow.

## Milestone task

Extract the repository boundary and move JSON load/save behavior behind it.

## Rubric

- Correctness: notes can be created, listed, found, updated, and deleted through the repository contract.
- Testing: service behavior is verified with fake storage and JSON repository behavior is verified with temporary files.
- Maintainability: service logic does not depend on paths, JSON, or SQLite-specific details.
- Design: the repository owns persistence mechanics and the service owns application rules.
- SQLite readiness: a future SQLite repository can implement the same contract.

## Complexity

The hard part is not writing another class. The hard part is drawing the boundary at the right place. If the repository starts raising HTTP exceptions, the boundary is too high. If the service starts opening files, the boundary is too low.

## Stretch goals

- Add a memory repository for fast tests.
- Add a shared repository test suite that every implementation must pass.
- Add typed note models once the dictionary shape becomes too loose.
