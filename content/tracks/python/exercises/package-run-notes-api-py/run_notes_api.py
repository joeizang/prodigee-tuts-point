from pathlib import Path

from fastapi import FastAPI


APP_IMPORT_TARGET = "notes_api.main:create_app"


def create_app(database_path: Path, environment: str = "dev") -> FastAPI:
    """Create the packaged notes API."""
    return FastAPI()


def build_uvicorn_command(host: str = "127.0.0.1", port: int = 8000) -> list[str]:
    """Build the local uvicorn run command."""
    return []
