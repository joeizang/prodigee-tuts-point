def database_engine_settings(database_url: str, app_env: str) -> dict[str, object]:
    """Return SQLAlchemy engine/session settings for a database URL."""
    return {}


def session_dependency_contract() -> dict[str, object]:
    """Return the FastAPI session dependency boundary contract."""
    return {}
