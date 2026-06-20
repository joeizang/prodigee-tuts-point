# Settings and Configuration

## Learning objectives

- Separate configuration from code.
- Load settings from environment-style values.
- Validate dev, test, and production settings differently.
- Avoid unsafe production defaults.
- Inject settings through FastAPI dependencies.

## Prerequisites

You should understand app factories, dependencies, Pydantic models, SQLite paths, and package structure.

## Mental model

**Term: settings** means runtime values such as environment name, database path, log level, and API key.

**Term: configuration boundary** means the place where raw strings become trusted application values.

**Term: fail fast** means refusing to start when required production settings are missing.

## Core idea

Raw environment data should be parsed once:

```python
settings = AppSettings.from_env(os.environ)
```

Route handlers should not read environment variables.

## Worked example

Development can have safe defaults. Production cannot:

```python
if environment == "prod" and not api_key:
    raise ValueError("API_KEY is required in prod")
```

That rule prevents accidental public write access.

## Production transfer

Configuration becomes more important during deployment. Local tests can use temporary paths. Production needs explicit database paths, secrets, host settings, and logging levels. A validated settings object gives those values a single owner.

## Common mistakes

- Reading environment variables inside route handlers.
- Using dev API keys in production.
- Treating all values as strings forever.
- Hiding missing production config until first request.
- Letting tests mutate real process environment.

## Testing strategy

Pass dictionaries into `from_env` so tests do not leak environment state.

```python
settings = AppSettings.from_env({"APP_ENV": "test", "DATABASE_PATH": "test.db"})
```

## Debugging strategy

If config behaves strangely, inspect the raw input dictionary and the parsed model separately.

## Exercise connection

`LoadAppSettings` asks you to parse environment-style dictionaries into validated settings.

## Project connection

This milestone prepares the notes API for local, test, and production startup paths.

## Check yourself

- Which layer should read environment variables?
- Why should production reject a missing API key?
- Why should tests avoid global environment mutation?
- Which values should become typed fields?

## Source reference notes

- Python `os` docs anchor environment access.
- FastAPI settings guidance anchors app configuration.
- pytest monkeypatch docs anchor isolated config tests.
