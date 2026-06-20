# 0055 Seed Python Foundations Track Slice

## Type

Content

## Status

In Progress

## Outcome

The content catalog gains the first serious Python beginner slice: a Python track, beginner concepts, one project-backed module, rich lesson content, focused exercises, pytest validation, and review prompts designed for a learner with no previous Python experience.

## Acceptance Criteria

- Add a `python` track with beginner-friendly concepts, module metadata, lesson metadata, project metadata, milestone metadata, and exercise metadata.
- Seed the first project as `py-notes-cli`, focused on Python files, strings, functions, collections, JSON, deterministic CLI-style output, and pytest.
- Add the first lesson with the full pedagogy contract: objectives, prerequisites, mental model, concept explanation, syntax/API reference, examples, worked example, production transfer, common beginner mistakes, testing strategy, debugging strategy, exercises, project connection, review prompts, and source anchors.
- Add focused exercises with visible tests, hidden tests, progressive hints, model solutions, common wrong approaches, expected solution characteristics, and review explanations.
- Treat editor feedback as pedagogy: lesson and exercise copy should explain how diagnostics, hover, formatting, and type feedback help the learner reason about their code.
- Keep FastAPI out of this first slice except as a named future destination.
- Content indexing and content quality validation pass with Python present.

## Verification

- Added `content/tracks/python/track.yml` with the first Python module, concept map, lesson, exercise, project, and milestone.
- Added `Text as Data in Python` with beginner mental models, production transfer, testing/debugging strategy, common mistakes, exercise connection, project connection, review prompts, and source anchors.
- Added `normalize-note-title-py` with starter code, visible tests, hidden tests, hints, model solution, common wrong approaches, and expected solution characteristics.
- Added API tests for Python track/milestone visibility and Python pytest workspace generation.
- Content validator and API tests are currently blocked by the existing .NET NuGet audit failure for `SQLitePCLRaw.lib.e_sqlite3` 2.1.11.
- Python exercise execution through uv is verified for the first foundation exercise.

## Remaining Work

- Run content quality validation after the NuGet audit blocker is resolved.
- Expand the Python foundations slice into the next small beginner milestone.

## Full Feature Later

Expand Python foundations into the full beginner-to-proficient ladder: `py-notes-cli`, `task-tracker-core`, testing/tooling hardening, HTTP client work, async fundamentals, and production Python style before FastAPI becomes the main vehicle.
