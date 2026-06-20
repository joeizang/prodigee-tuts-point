from pathlib import Path

from fastapi import FastAPI


def package_map() -> dict[str, str]:
    """Describe the production package responsibility map."""
    return {}


def create_app(database_path: Path) -> FastAPI:
    """Create the production-shaped notes API."""
    return FastAPI()
