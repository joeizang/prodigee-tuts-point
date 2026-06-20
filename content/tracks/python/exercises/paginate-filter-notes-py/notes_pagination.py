from fastapi import FastAPI


def create_app() -> FastAPI:
    """Create a notes API with filtering and pagination."""
    return FastAPI()
