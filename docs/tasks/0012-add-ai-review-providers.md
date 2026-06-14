# 0012 Add AI review providers

## Type

AFK

## Status

Completed for advisory milestone review.

## What to build

Add AI-assisted project review using an OpenAI-compatible provider abstraction with Hosted OpenAI and Local Ollama presets, provider testing, external secret loading, and structured review storage.

## Acceptance criteria

- [x] Provider settings are stored in SQLite without secrets.
- [x] Hosted OpenAI preset exists.
- [x] Local Ollama preset exists.
- [x] Secrets are read from environment variables or `~/.prodigee-tuts-point/secrets.json`.
- [x] .NET user-secrets are not used.
- [x] Provider test action checks endpoint/model capability before enabling review.
- [x] AI review runs against visible milestone markdown as the current rubric source.
- [x] Review result stores provider, model, prompt version, rubric version, and structured output.
- [x] Normal milestone AI review is advisory.
- [x] Capstone AI review can be required later by validation policy.

## Implementation notes

- Provider presets are seeded as `hosted-openai` and `local-ollama` in `AiReviewProviderSettings`.
- SQLite stores endpoint, model, preset, enabled state, and secret name only. Secret values are read from environment variables first, then `~/.prodigee-tuts-point/secrets.json`.
- Provider testing calls an OpenAI-compatible `/chat/completions` endpoint and only enables the provider after successful normal chat and JSON-object capability responses.
- Provider endpoints are constrained to Hosted OpenAI or local Ollama-compatible hosts before outbound calls.
- AI review HTTP calls use an explicit 30 second timeout.
- Advisory milestone review stores `ProviderId`, `ProviderPreset`, `Model`, `PromptVersion`, content-derived `RubricVersion`, `Policy`, score, summary, strengths, risks, next steps, and raw output.
- Missing or empty milestone markdown fails closed instead of storing a review with an empty rubric.
- The milestone UI exposes provider testing, advisory review execution, and stored review history.
- Regression tests cover successful provider capability testing, empty-rubric failure, and frontend provider/review display.

## Full implementation note

The persisted `Policy` field currently stores `Advisory` for normal milestones. A future capstone validation policy can require AI review by writing `Required` policy decisions around capstone milestones without changing the stored review shape.

## Blocked by

- 0011 Add static analysis feedback
