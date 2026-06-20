# Python/FastAPI Workspaces With Pyright And Ruff

Python exercises will use generated real Python project workspaces rather than single-file snippets. Each exercise workspace must include `pyproject.toml`, editable source files under `src/`, visible and hidden pytest tests, and a deterministic dependency profile appropriate to the exercise level.

Python and FastAPI editor intelligence must use a real semantic language-service path behind Monaco. Monaco-only syntax highlighting, keyword completions, or handcrafted FastAPI suggestions are not considered complete. The target quality bar matches the existing C#, TypeScript, and Swift standard: current-buffer-aware locals and parameters, workspace-aware imports, installed-package symbols, diagnostics with line and column ranges, hover documentation, signature help, formatting, organize imports, and supported code actions.

The semantic backend should use Pyright or basedpyright for Python analysis and Ruff for linting, formatting, organize imports, and quick fixes where available. The Python runner and the language service must resolve the same exercise dependencies so FastAPI, Pydantic, pytest, httpx, and standard-library imports behave consistently between editor feedback and test execution.

FastAPI exercises should build on the same Python workspace model instead of using a separate framework-specific editor path. A FastAPI workspace may include `src/app.py`, `src/schemas.py`, `src/routes.py`, `src/services.py`, and route tests using FastAPI's test client or httpx. IntelliSense is not acceptable for FastAPI until route decorators, dependency declarations, Pydantic models, request bodies, response models, and common exception patterns resolve through the real dependency environment.

The curriculum for Python must assume the learner is new to the language. Python foundations should progress more slowly than existing language tracks, with explicit mental models for indentation, names, object references, mutability, truthiness, `None`, exceptions, imports, virtual environments, packages, type hints, and testing before FastAPI becomes the primary vehicle.
