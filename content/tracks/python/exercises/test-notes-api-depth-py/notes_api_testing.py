from pathlib import Path

from fastapi import FastAPI
from fastapi.testclient import TestClient


def create_app(notes_path: Path) -> FastAPI:
    """Create the notes API for deep FastAPI testing."""
    return FastAPI()


def make_test_client(notes_path: Path) -> TestClient:
    """Create an isolated test client for the notes API."""
    return TestClient(create_app(notes_path))
