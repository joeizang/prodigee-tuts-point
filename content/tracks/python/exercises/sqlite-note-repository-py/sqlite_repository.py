from pathlib import Path


class SqliteNoteRepository:
    """SQLite-backed repository for notes."""

    def __init__(self, path: Path) -> None:
        self.path = path
