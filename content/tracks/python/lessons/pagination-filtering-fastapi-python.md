# Pagination and Filtering in FastAPI

## Learning objectives

- Add validated query parameters to a list endpoint.
- Implement deterministic tag and text filtering.
- Return pagination metadata with items.
- Explain why ordering must be stable before limit and offset.
- Test empty, filtered, and paginated results.

## Prerequisites

You should understand FastAPI query parameters, Pydantic response models, repository boundaries, and API tests.

## Mental model

**Term: filter** means reducing the matching set before pagination.

**Term: pagination** means returning a slice of a larger result set.

**Term: deterministic ordering** means the same data always appears in the same order before limit and offset are applied.

**Term: response envelope** means a JSON object that contains `items` plus metadata such as `total`, `limit`, and `offset`.

## Core idea

The endpoint contract should be explicit:

```python
GET /notes?tag=python&q=api&limit=20&offset=0
```

The response should explain the slice:

```python
{
    "items": [...],
    "total": 42,
    "limit": 20,
    "offset": 0
}
```

## Worked example

Filter before slicing:

```python
matches = repository.list_notes(tag=tag, q=q)
return {
    "items": matches[offset:offset + limit],
    "total": len(matches),
    "limit": limit,
    "offset": offset,
}
```

If you slice first, a matching item on the next page can disappear.

## Production transfer

Pagination and filtering turn a toy list endpoint into an API clients can use repeatedly. This is where backend behavior affects UI performance, scroll behavior, search screens, and operational reliability.

## Common mistakes

- Applying limit before filters.
- Returning lists without total metadata.
- Leaving ordering to database accident.
- Accepting negative limits or offsets.
- Returning `404` when a filter has no matches.

## Testing strategy

Test the contract matrix:

- no filters
- tag filter
- text filter
- combined filters
- limit and offset
- invalid query values
- empty result set

## Debugging strategy

If pagination looks wrong, print the count after each step: loaded, filtered, sorted, sliced. Most bugs come from applying those steps in the wrong order.

## Exercise connection

`PaginateFilterNotes` asks you to implement a FastAPI list endpoint with `tag`, `q`, `limit`, and `offset`.

## Project connection

This milestone gives the notes API a real list contract suitable for a UI.

## Check yourself

- Why filter before slicing?
- Why return total?
- Why should empty matches return `200`?
- What range should `limit` allow?

## Source reference notes

- FastAPI query docs anchor query parameter validation.
- FastAPI response model docs anchor envelopes.
- pytest assertion docs anchor list contract tests.
