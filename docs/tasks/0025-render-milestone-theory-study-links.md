# 0025 Render milestone theory study links

## Type

AFK

## Status

Completed

## What to build

Move theory clusters from dashboard-only support into every milestone page so each project milestone shows direct links to the lessons and source anchors the learner should study.

## Acceptance criteria

- [x] Milestone pages render the milestone theory cluster from `/theory-cluster`.
- [x] Each theory item links to the lesson page.
- [x] Each source anchor links to the source library anchor.
- [x] The UI remains compact and appropriate for an IDE/book hybrid.
- [x] Add frontend regression coverage for lesson and source links.

## Verification

- `npm run lint && npm run test && npm run build`
- `dotnet test`
- Browser smoke: `streaming-and-scale` milestone renders Study Links with source anchors.

## Full implementation note

Later work should add read-status tracking, estimated reading time, per-source personal notes inline, source priority, and a "study before exercise" completion gate.
