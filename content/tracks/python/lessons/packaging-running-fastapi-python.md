# Packaging and Running FastAPI

## Learning objectives

- Define a stable ASGI import target.
- Generate a local uvicorn command.
- Expose runtime metadata.
- Run startup checks before serving requests.
- Explain package import paths.

## Prerequisites

You should understand app factories, settings, SQLite startup, and FastAPI route tests.

## Mental model

**Term: ASGI target** means the import path an ASGI server uses to find the app.

**Term: startup check** means validation that must pass before the app serves traffic.

**Term: runtime metadata** means values such as app name, environment, version, and database path.

## Core idea

The run command should be explicit:

```text
uv run uvicorn notes_api.main:create_app --factory --host 127.0.0.1 --port 8000
```

Developers should not guess the import path.

## Worked example

Runtime metadata can be returned by a health route:

```python
GET /health
```

The response should not leak secrets.

## Production transfer

Packaging and running is where local code becomes an operational program. Import targets, startup checks, and commands become part of the developer and deployment contract.

## Common mistakes

- Relying on current working directory tricks.
- Hiding startup errors until first request.
- Returning secrets from health endpoints.
- Having no documented run command.
- Confusing an app object target with an app factory target.

## Testing strategy

Test the generated command, metadata, health route, and startup failure behavior.

## Debugging strategy

If uvicorn cannot import the app, check module path, factory flag, and package root.

## Exercise connection

`PackageRunNotesApi` asks you to expose runtime metadata, health checks, startup validation, and a uvicorn command builder.

## Project connection

This milestone closes the first production-readiness loop before moving into SQLAlchemy/PostgreSQL.

## Check yourself

- What is the ASGI target?
- Why does `--factory` matter?
- What should health expose?
- What must startup verify?

## Source reference notes

- FastAPI first steps anchor app targets.
- Deployment docs anchor ASGI server startup.
- Python module docs anchor import paths.
