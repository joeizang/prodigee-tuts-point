from pathlib import Path

from fastapi import FastAPI


def create_app(database_path: Path) -> FastAPI:
    """Create a SQLite-backed notes API."""
    return FastAPI()
