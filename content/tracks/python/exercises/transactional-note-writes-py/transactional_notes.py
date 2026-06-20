from pathlib import Path


class TransactionalNoteRepository:
    """SQLite repository with transactional note and audit writes."""

    def __init__(self, path: Path) -> None:
        self.path = path
