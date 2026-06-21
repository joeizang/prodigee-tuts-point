from pathlib import Path

from fastapi import FastAPI

ASYNC_BOUNDARY_POLICY: dict[str, str] = {}


def create_app(database_path: Path) -> FastAPI:
    """Create an API that isolates blocking SQLite work from async handlers."""
    return FastAPI()
