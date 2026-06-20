from fastapi import FastAPI


def create_app() -> FastAPI:
    """Create an observable notes API."""
    return FastAPI()
