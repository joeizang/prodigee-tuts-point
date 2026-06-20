from pathlib import Path

from fastapi import FastAPI


def create_app(notes_path: Path) -> FastAPI:
    """Create the notes API with deliberate HTTP semantics."""
    app = FastAPI()
    return app
