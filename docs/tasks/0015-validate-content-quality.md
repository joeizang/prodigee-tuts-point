# 0015 Validate content quality

## Type

AFK

## What to build

Add a content validator command that protects the book-depth quality bar and rejects incomplete or shallow authored content.

## Acceptance criteria

- [ ] Validator checks lesson objectives, prerequisites, examples, exercises, and project connection.
- [ ] Validator checks exercise visible tests, hidden tests, hints, and model solution.
- [ ] Validator checks project milestone rubric.
- [ ] Validator checks source reference ids.
- [ ] Validator checks broken internal links.
- [ ] Validator output is actionable for authors.
- [ ] Validator runs against the `wordfreq-csharp` seed content.
- [ ] Build or test workflow can run the validator.

## Blocked by

- 0003 Implement hybrid content loading
- 0014 Author the wordfreq-csharp milestone content
