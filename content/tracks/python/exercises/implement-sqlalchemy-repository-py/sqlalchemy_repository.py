def build_repository_blueprint() -> dict[str, object]:
    """Return the concrete SQLAlchemy repository implementation blueprint."""
    return {}


def dependency_manifest() -> list[str]:
    """Return the root uv dependencies required for the real implementation."""
    return []
