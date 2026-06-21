

class AppSettings:
    """Validated application settings."""


def load_settings(env: dict[str, str]) -> AppSettings:
    """Load settings from an environment-like dictionary."""
    raise NotImplementedError
