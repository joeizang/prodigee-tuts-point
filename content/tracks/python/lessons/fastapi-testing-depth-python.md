# FastAPI Testing Depth

## Learning objectives

- Test route behavior across success and failure paths.
- Use dependency overrides deliberately.
- Verify storage isolation between tests.
- Assert status code and response body together.
- Distinguish model validation, service validation, missing records, and conflicts.

## Prerequisites

You should understand FastAPI route handlers, dependencies, Pydantic contracts, and HTTP semantics. This lesson is about test coverage that protects those boundaries.

## Mental model

**Term: API test** means a test that enters through HTTP and asserts the public contract.

**Term: dependency override test** means an API test where the real service or repository is replaced to isolate route behavior.

**Term: isolation** means one test cannot change another test's storage, dependency overrides, or app state.

## Core idea

Do not write only one happy-path test. A serious FastAPI slice tests categories:

```python
def test_create_returns_created(client: TestClient) -> None: ...
def test_duplicate_create_returns_conflict(client: TestClient) -> None: ...
def test_missing_update_returns_not_found(client: TestClient) -> None: ...
def test_invalid_body_returns_422(client: TestClient) -> None: ...
```

Each test exists because each outcome means something different to a client.

## Worked example

A fixture can create an isolated app:

```python
@pytest.fixture
def client(tmp_path: Path) -> TestClient:
    return TestClient(create_app(tmp_path / "notes.json"))
```

An override test can prove the route does not construct its own service:

```python
app = create_app(tmp_path / "notes.json")
app.dependency_overrides[get_notes_service] = lambda: FakeService()
client = TestClient(app)
```

The fake controls the service outcome. The route must translate it correctly.

## Production transfer

FastAPI testing depth prevents regressions when the persistence layer changes. If the app moves from JSON to SQLite, the API tests should still describe the same client-facing behavior. Repository tests can change; route contracts should not drift accidentally.

This is also where you learn to test like an API consumer instead of like the implementation author.

## Common mistakes

- Sharing one global app across tests and leaking overrides.
- Testing only response JSON and ignoring status codes.
- Testing only status codes and ignoring response shape.
- Letting route tests depend on test execution order.
- Converting every route test into an end-to-end database test.
- Mocking so much that the test no longer proves the public API contract.

## Testing strategy

Use three levels:

- Model tests for Pydantic contracts.
- Service tests with fake repositories.
- API tests with `TestClient`, temporary storage, and dependency overrides.

Keep each level honest. Do not use route tests to prove every line of repository code.

## Debugging strategy

If a route test fails:

- `422` means the request did not satisfy the Pydantic model.
- `400`, `404`, or `409` usually means the handler translated a service outcome.
- Wrong response JSON means a response model or returned dictionary is wrong.
- Flaky behavior often means shared app state or shared storage.

Make a new app per test until you have a deliberate reason not to.

## Exercise connection

`TestNotesApiDepth` asks you to provide both the app and a `make_test_client` helper. The tests verify isolated storage, dependency override behavior, validation, not-found behavior, and conflict behavior.

## Project connection

This milestone locks in the API contract before SQLite arrives. Once tests are deep enough, the persistence swap becomes less risky.

## Check yourself

- What should be tested with a fake service?
- What should be tested with real temporary storage?
- Why should `422` remain different from `400`?
- How can dependency overrides leak between tests?

## Source reference notes

- FastAPI testing docs anchor `TestClient`.
- FastAPI dependency override docs anchor service replacement.
- pytest `tmp_path` guidance anchors isolated app storage.
