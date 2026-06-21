from alembic.config import Config


def create_alembic_config(database_url: str, script_location: str = "migrations") -> Config:
    """Create and return a real Alembic Config object."""
    config = Config()
    config.set_main_option("script_location", script_location)
    config.set_main_option("sqlalchemy.url", database_url)
    return config


def render_env_py() -> str:
    """Return the env.py content that imports application metadata."""
    return ""


def alembic_commands(message: str) -> dict[str, str]:
    """Return the project migration command contract."""
    return {}


def revision_review_policy() -> dict[str, object]:
    """Return the required review policy for generated revisions."""
    return {}


def migration_smoke_tests() -> list[str]:
    """Return the migration smoke tests that should run in CI."""
    return []
