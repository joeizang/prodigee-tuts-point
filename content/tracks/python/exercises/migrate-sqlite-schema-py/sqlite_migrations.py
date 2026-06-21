from pathlib import Path

TARGET_SCHEMA_VERSION = 2


def migrate(database_path: Path) -> int:
    """Apply SQLite migrations and return the current schema version."""
    return 0


def verify_schema(database_path: Path) -> None:
    """Raise when the database schema is not current."""
    raise NotImplementedError
