# 0027 Add source reference draft management

## Type

AFK

## Status

Completed

## What to build

Add a local source-reference drafting aid so new study anchors can be prepared consistently without ingesting book text or storing secrets in the project.

## Acceptance criteria

- [x] Sources page includes a local source-reference draft form.
- [x] Drafts select from indexed owned source books.
- [x] Drafts produce YAML snippets matching the existing `sourceReferences` schema.
- [x] Drafts are stored locally in browser storage only.
- [x] The feature does not ingest, OCR, upload, or copy book content.
- [x] Add frontend regression coverage for YAML generation and local draft storage.

## Verification

- `npm run lint && npm run test && npm run build`
- `dotnet test`
- Browser smoke: Sources page renders Source Reference Drafts with YAML output.
- Review remediation verification: `/api/curriculum/sources` now filters to owned source books.
- Review remediation verification: the draft YAML test now asserts the list-item shape and `usage: QualityAnchor`.

## Full implementation note

Later work should add a proper local admin/editor mode that writes structured content files after explicit confirmation, validates draft references against lesson and milestone ids, supports batch review, and shows unused or stale source anchors.
