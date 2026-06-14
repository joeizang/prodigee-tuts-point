# 0005 Add source references, notes, and search

## Type

AFK

## Status

Completed

## What to build

Add the v1 source library, personal notes, and basic local search so the learner can connect curriculum to source anchors, capture private notes, and find content quickly.

## Acceptance criteria

- [x] Source books and source references are indexed from YAML metadata.
- [x] Source references are displayed as metadata pointers, not book content.
- [x] Basic personal notes can be attached to lessons, exercises, projects, milestones, concepts, and source references.
- [x] Notes are stored as learner state in SQLite.
- [x] Basic search covers tracks, lessons, concepts, projects, exercises, and source references.
- [x] Search results deep-link to the relevant page.

## Remediation notes

- Added `/concepts/:conceptId` UI route with personal notes, closing the concept-notes UI gap.
- Search now filters in SQLite with `LIKE` predicates instead of loading every content table into memory first.
- Search also scans lesson markdown bodies for terms not present in title/summary metadata and marks those matches as `Markdown body`.
- Concept results deep-link to `/concepts/{id}` and source-reference results deep-link to `/sources#{referenceId}` anchors.
- API tests cover concept detail, concept search links, anchored source links, and notes on concept/source-reference targets.

## Blocked by

- 0003 Implement hybrid content loading
- 0004 Render the first curriculum path
