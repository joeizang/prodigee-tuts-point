# Production API Package Structure

## Learning objectives

- Split a FastAPI app by responsibility instead of by accident.
- Name model, service, repository, dependency, and app boundaries clearly.
- Explain what each module should import and what it should not import.
- Keep the app factory as the composition root.
- Preserve testability while the project grows.

## Prerequisites

You should understand FastAPI dependencies, Pydantic models, services, repositories, SQLite persistence, and API testing. This lesson is about arranging those pieces into a maintainable shape.

## Mental model

**Term: composition root** means the place where concrete implementations are assembled. In this project, `create_app` is the composition root.

**Term: module ownership** means each file owns one kind of decision. Models own public data shape. Services own rules. Repositories own persistence. Dependencies wire concrete objects into route handlers.

**Term: import direction** means dependencies should flow inward. Routes may import services and models. Services should not import FastAPI.

## Core idea

A production-shaped notes API can start with this package structure:

```python
notes_api/
    main.py
    models.py
    services.py
    repositories.py
    dependencies.py
```

The goal is not ceremony. The goal is to prevent every future feature from landing in `main.py`.

## Worked example

The app factory should compose the app:

```python
def create_app(settings: Settings) -> FastAPI:
    app = FastAPI()
    app.state.database_path = settings.database_path
    include_routes(app)
    return app
```

The dependency provider should create the service:

```python
def get_notes_service(request: Request) -> NotesService:
    repository = SqliteNoteRepository(request.app.state.database_path)
    repository.initialize()
    return NotesService(repository)
```

## Production transfer

This is the point where the learner stops thinking of FastAPI as a single file and starts thinking in deployable application boundaries. A larger app will add routers, settings, authentication, migrations, and background jobs. Clear module ownership makes those additions survivable.

## Common mistakes

- Putting repository SQL into route handlers.
- Importing FastAPI types into service code.
- Creating global services at import time.
- Splitting files by framework examples instead of project responsibilities.
- Moving code into files without improving dependency direction.

## Testing strategy

Test package-level composition by exercising `create_app`, dependency overrides, and the public routes. Also test services and repositories directly. If a route test fails because repository SQL changed, the abstraction is leaking.

## Debugging strategy

When a structured app fails, ask which boundary owns the failure:

- Wrong JSON shape: models or route return value.
- Wrong business behavior: service.
- Wrong persistence behavior: repository.
- Wrong concrete implementation: dependency provider.
- App does not start: composition root.

## Exercise connection

`AssembleNotesApiPackage` asks you to assemble those responsibilities in one exercise file. Even though the exercise is one file, the exports and tests enforce the production boundaries.

## Project connection

This milestone turns the previous snippets into a shape that can grow into a real backend project.

## Check yourself

- Which layer should import FastAPI?
- Which layer should know SQL?
- Where should app settings be read?
- What should stay unchanged if SQLite is replaced later?

## Source reference notes

- FastAPI bigger-applications guidance anchors application layout.
- Python module guidance anchors import boundaries.
- pytest fixture guidance anchors app-factory test setup.
