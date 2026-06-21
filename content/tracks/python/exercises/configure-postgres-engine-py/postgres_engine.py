def database_engine_settings(database_url: str, app_env: str) -> dict[str, object]:
    """Return SQLAlchemy engine/session settings for a database URL."""
    return {}


def create_engine_for_settings(database_url: str, app_env: str):
    """Create and return a real SQLAlchemy Engine."""
    return None


def create_session_factory(database_url: str, app_env: str):
    """Create and return a real SQLAlchemy sessionmaker."""
    return None


def session_dependency_contract() -> dict[str, object]:
    """Return the FastAPI session dependency boundary contract."""
    return {}
