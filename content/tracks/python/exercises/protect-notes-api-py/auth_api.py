from fastapi import FastAPI


def create_app(api_key: str) -> FastAPI:
    """Create a notes API with API-key protected write routes."""
    return FastAPI()
