# API Key Authentication

## Learning objectives

- Protect write routes with an API key.
- Keep read routes public when the product requires it.
- Distinguish missing credentials from invalid credentials.
- Use dependencies for authorization.
- Test `401`, `403`, and success paths.

## Prerequisites

You should understand FastAPI dependencies, settings, HTTP semantics, and route tests.

## Mental model

**Term: authentication** means proving who or what is calling.

**Term: authorization** means deciding what the caller can do.

**Term: API key** means a shared secret sent with requests, often in a header.

## Core idea

The dependency owns the check:

```python
def require_api_key(x_api_key: str | None = Header(default=None)) -> None:
    ...
```

Routes declare the dependency instead of checking headers manually.

## Worked example

Missing and invalid are different:

```python
if x_api_key is None:
    raise HTTPException(status_code=401)
if x_api_key != settings.api_key:
    raise HTTPException(status_code=403)
```

## Production transfer

API keys are not the final security model for every system, but they are a useful first boundary. They teach protected routes, secret configuration, and auth tests before OAuth/JWT complexity arrives.

## Common mistakes

- Hard-coding secrets in code.
- Protecting only one write route.
- Returning `200` with an error body.
- Treating missing and invalid credentials the same.
- Logging API keys.

## Testing strategy

Test public reads, missing write credentials, invalid write credentials, and valid write credentials.

## Debugging strategy

If all requests fail, verify the settings value and the header name.

## Exercise connection

`ProtectNotesApi` asks you to protect note write routes with `X-API-Key`.

## Project connection

This milestone is the first auth boundary before richer identity and authorization models.

## Check yourself

- Which routes should be public?
- Why use dependencies for auth?
- Why should missing be `401` and invalid be `403`?
- Where should the API key come from?

## Source reference notes

- FastAPI security docs anchor API key patterns.
- Dependency docs anchor route protection.
- Testing docs anchor protected route verification.
