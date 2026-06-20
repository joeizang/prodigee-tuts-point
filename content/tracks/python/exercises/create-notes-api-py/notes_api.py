from pathlib import Path

from fastapi import FastAPI


def create_app(notes_path: Path) -> FastAPI:
    """Create the py-notes FastAPI app backed by a JSON notes file."""
    app = FastAPI()
    return app
