# Pagination and Filtering

## Scenario

The notes API needs list behavior that a UI can depend on. Returning every note forever is not a production contract.

## Requirements

- Support optional `tag` and `q` filters.
- Support validated `limit` and `offset` query parameters for limit/offset pagination.
- Apply filtering before pagination.
- Use deterministic ordering.
- Return `items`, `total`, `limit`, and `offset`.

## CLI/API contract

The endpoint returns a response envelope instead of a bare list. This gives clients enough metadata to build paged views.

## Milestone task

Implement a filtered and paginated list endpoint.

## Rubric

- Correctness: filters and pagination produce expected slices.
- Testing: empty, filtered, invalid, and paged cases are covered.
- Maintainability: filtering logic is not duplicated in route handlers.
- Design: response metadata matches the selected slice.
- Client readiness: the endpoint can support a UI list view.

## Complexity

The order of operations matters. Load, filter, sort, count, then slice.

This is also the first point where response shape becomes more than "return the data." A bare list cannot tell a client whether there are more records, what slice it is seeing, or how a UI should build the next request. The envelope is part of the contract.

Keep the first implementation simple and deterministic. In a larger dataset the filtering should move closer to SQL, but the API contract should already describe what the client receives.

## Stretch goals

- Add cursor pagination.
- Move filtering into SQL once dataset size requires it.
- Add indexed tag search.
