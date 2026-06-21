class ReadinessChecks:
    """Replace with named database and schema readiness checks."""


def create_app(checks: ReadinessChecks | None = None):
    """Create the FastAPI app with health and readiness routes."""
    raise NotImplementedError


def startup_check(checks: ReadinessChecks) -> None:
    """Raise when the app should not start serving traffic."""
    raise NotImplementedError


def deployment_commands() -> list[str]:
    """Return the migration-before-startup command sequence."""
    return []
