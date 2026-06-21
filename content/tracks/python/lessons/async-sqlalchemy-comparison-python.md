# Async SQLAlchemy Comparison

## Learning objectives

- Compare sync and async SQLAlchemy without mixing the two accidentally.
- Name the async engine, async session, and async driver requirements.
- Decide when the sync repository remains acceptable.
- Define async test expectations.

## Prerequisites

You should understand sync SQLAlchemy repositories, FastAPI async route behavior, session factories, and async testing.

## Mental model

Async SQLAlchemy is not "add `async` everywhere." It changes engine type, session type, driver, dependency cleanup, repository method signatures, and test patterns.

**Term: async engine** means SQLAlchemy's asynchronous database engine created with an async driver URL.

**Term: async session** means the session object whose operations are awaited.

## Core idea

Sync baseline:

```python
engine = create_engine(url)
SessionLocal = sessionmaker(bind=engine)
```

Async variant:

```python
engine = create_async_engine(url)
SessionLocal = async_sessionmaker(engine, expire_on_commit=False)
```

The route and repository contracts must agree. An async route should await an async repository; it should not hide blocking sync database calls in the event loop unless the boundary and tradeoff are deliberate.

## Production transfer

Many FastAPI apps start with sync SQLAlchemy and remain healthy. Async becomes valuable when database concurrency, latency, and operational evidence justify the added complexity.

## Common mistakes

- Using sync `Session` inside async routes without naming the blocking boundary.
- Using a sync PostgreSQL driver with async SQLAlchemy.
- Forgetting to await session operations.
- Mixing sync and async repository interfaces.

## Testing strategy

Test the comparison matrix: engine factory, session factory, driver URL, route style, repository methods, and pytest marker.

## Debugging strategy

When async persistence fails, inspect driver URL, session type, missing `await`, and fixture event loop configuration first.

## Exercise connection

`CompareAsyncSqlAlchemy` asks you to produce a clear decision matrix between sync and async SQLAlchemy.

## Project connection

This keeps async as an advanced choice grounded in a working sync baseline.

## Check yourself

- Which driver does async PostgreSQL need?
- Which methods become awaited?
- When is sync SQLAlchemy still acceptable?

## Source reference notes

Use FastAPI async guidance and pytest async testing patterns as anchors.
