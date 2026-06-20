# PostgreSQL Engine Configuration

## Learning objectives

- Build database settings that distinguish SQLite development from PostgreSQL production.
- Name the SQLAlchemy engine and session factory decisions.
- Explain when sync versus async database access is acceptable.
- Define production guards for unsafe database URLs.

## Prerequisites

You should already understand settings loading, FastAPI dependencies, async boundaries, and why PostgreSQL behavior cannot be inferred fully from SQLite tests.

## Mental model

The database URL is an operational contract. It chooses the provider, driver, host, database name, pool behavior, and production safety. The application should parse that contract once, create an engine and session factory, and inject repository instances through FastAPI dependencies.

**Term: engine** means the SQLAlchemy object that owns database connectivity and pool behavior.

**Term: session factory** means the callable that creates request-scoped database sessions for repositories.

## Core idea

For this app, the default path is a synchronous SQLAlchemy repository used behind thin FastAPI routes. That is acceptable while requests are simple and database calls are short, but the decision must be named. If the app grows into high-concurrency database-heavy work, the track can later introduce async SQLAlchemy with async drivers and async sessions.

The production URL should look like:

```text
postgresql+psycopg://user:password@host:5432/notes
```

SQLite remains a development and learning option:

```text
sqlite:///tmp/notes.db
```

## Worked example

The engine configuration should distinguish providers:

```python
if provider == "postgresql":
    options = {"pool_pre_ping": True, "pool_size": 5, "max_overflow": 10}
else:
    options = {"connect_args": {"check_same_thread": False}}
```

The FastAPI dependency owns cleanup:

```python
def get_session() -> Iterator[Session]:
    with SessionLocal() as session:
        yield session
```

## Production transfer

Production should fail fast if `APP_ENV=production` uses SQLite. Test environments may use SQLite for quick unit-style integration tests, but PostgreSQL behavior needs its own integration profile before deployment confidence is real.

## Common mistakes

- Letting production silently use a local SQLite file.
- Creating one global session and sharing it across requests.
- Mixing async route handlers with unbounded blocking database work.
- Hiding pool settings in scattered route code.

## Testing strategy

Settings tests should cover SQLite development URLs, PostgreSQL production URLs, invalid providers, missing database names, and production guard failures. Dependency tests should prove sessions are yielded and closed by the dependency layer, not by route handlers.

## Debugging strategy

When database configuration fails, inspect the parsed URL first. Then inspect whether provider-specific engine options are selected, whether production guard rules are active, and whether dependency cleanup happens for success and error paths.

## Exercise connection

`ConfigurePostgresEngine` asks you to produce provider-aware SQLAlchemy engine settings and dependency decisions for the notes API.

## Project connection

This milestone prepares the app for a real PostgreSQL integration profile without breaking the SQLite learning path.

## Check yourself

- Why should production reject SQLite URLs?
- Which layer creates sessions?
- When would you switch from sync SQLAlchemy to async SQLAlchemy?

## Source reference notes

Use FastAPI session dependency guidance, URL parsing from the Python standard library, and pytest settings matrices to keep configuration behavior explicit.
