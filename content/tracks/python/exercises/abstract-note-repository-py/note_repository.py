from pathlib import Path


class NotesService:
    """Application service backed by a repository abstraction."""

    def __init__(self, repository) -> None:
        self.repository = repository


class JsonNoteRepository:
    """JSON-backed note repository."""

    def __init__(self, path: Path) -> None:
        self.path = path
