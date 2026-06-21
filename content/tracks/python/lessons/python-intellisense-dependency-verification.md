# Python IntelliSense Dependency Verification

## Learning objectives

- Verify Pyright or basedpyright resolves installed third-party packages.
- Verify Ruff can lint and organize imports in generated Python workspaces.
- Keep editor tooling and the uv runner on the same dependency environment.
- Treat IntelliSense as a tested feature, not a manual hope.

## Prerequisites

You should understand generated Python workspaces, uv dependency management, Pyright, Ruff, and the FastAPI/Pydantic/SQLAlchemy package set.

## Mental model

Editor intelligence is only trustworthy when it sees the same environment as the test runner. If uv can run SQLAlchemy but Pyright cannot resolve it, the learner gets false red squiggles and weak completions.

**Term: semantic import resolution** means the language server can locate and type-check imported packages.

**Term: tooling parity** means editor diagnostics and test execution use the same installed dependencies.

## Core idea

Verification should cover key imports:

```python
from fastapi import FastAPI
from pydantic import BaseModel
from sqlalchemy.orm import Session
from alembic.config import Config
import psycopg
import pytest
```

The check should expect no missing-import diagnostics for installed packages and should run Ruff against the same source tree.

## Production transfer

This protects the Monaco experience. As curriculum dependencies grow, dependency verification prevents a regression where tests pass but IntelliSense degrades.

## Common mistakes

- Installing packages for pytest but not the language server.
- Testing only Python syntax highlighting.
- Ignoring missing-import diagnostics as cosmetic.
- Letting generated workspaces drift from root uv dependencies.

## Testing strategy

Test the dependency list, expected imports, Pyright command, Ruff command, and pass criteria. Later, wire this to actual language-service integration tests.

## Debugging strategy

When IntelliSense fails, inspect uv project selection, `PYTHONPATH`, generated workspace paths, and whether Pyright points at the root environment.

## Exercise connection

`VerifyPythonIntelliSenseDeps` asks you to define the dependency verification contract for the Python workspaces.

## Project connection

This keeps the "world-class IntelliSense" promise connected to real packages and tests.

## Check yourself

- Which packages must resolve?
- Which command checks Pyright?
- Which command checks Ruff?

## Source reference notes

Use Python module import rules and pytest tooling tests as the implementation anchors.
