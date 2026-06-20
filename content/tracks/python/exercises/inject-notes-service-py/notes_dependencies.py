from pathlib import Path

from fastapi import FastAPI


def create_app(notes_path: Path) -> FastAPI:
    """Create the notes API with a dependency-provided NotesService."""
    app = FastAPI()
    app.state.notes_path = notes_path
    return app


def get_notes_service():
    """Return the NotesService for the current request."""
    raise NotImplementedError
