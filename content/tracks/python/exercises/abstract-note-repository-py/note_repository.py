from pathlib import Path
from typing import Protocol


class NoteRepository(Protocol):
    def list_notes(self) -> list[dict[str, object]]: ...
    def get_note(self, title: str) -> dict[str, object] | None: ...
    def add_note(self, note: dict[str, object]) -> None: ...
    def update_note(self, title: str, changes: dict[str, object]) -> dict[str, object]: ...
    def delete_note(self, title: str) -> None: ...


class NotesService:
    """Application service backed by a repository abstraction."""

    def __init__(self, repository: NoteRepository) -> None:
        self.repository = repository


class JsonNoteRepository:
    """JSON-backed note repository."""

    def __init__(self, path: Path) -> None:
        self.path = path
