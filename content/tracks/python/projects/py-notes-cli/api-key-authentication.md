# API Key Authentication

## Scenario

The notes API has write operations. Before exposing it beyond local experiments, write routes need a first authentication boundary.

## Requirements

- Keep read routes public.
- Protect create, update, and delete routes.
- Read the expected API key from settings.
- Return `401` when the header is missing.
- Return `403` when the header is invalid.
- Never log or return the configured API key.

## CLI/API contract

Clients must send `X-API-Key` for protected writes. Read behavior remains unchanged.

## Milestone task

Implement API-key protection as a dependency and apply it consistently to write routes.

## Rubric

- Correctness: protected routes enforce credentials.
- Testing: public, missing, invalid, and valid credential paths are covered.
- Maintainability: auth checks live in dependency code.
- Design: settings provide secrets without hard-coding.
- Security readiness: failures use precise status codes and do not leak secrets.

## Complexity

API keys are simple but still security-sensitive. Avoid casual shortcuts. Do not check headers manually in every route. Do not use development defaults in production. Do not print secrets for debugging. The point of this milestone is disciplined route protection before richer auth arrives.

This is intentionally not a full identity system. It is a focused first boundary that teaches secrets, protected writes, and authorization tests without mixing in OAuth flows too early.

Later auth can be richer, but this milestone should already make unsafe writes impossible without an explicit key.

## Stretch goals

- Add key rotation.
- Add per-route scopes.
- Replace API keys with OAuth/JWT later.
