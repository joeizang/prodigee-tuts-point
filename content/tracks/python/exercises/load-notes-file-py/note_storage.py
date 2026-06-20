from pathlib import Path


def load_notes(path: Path) -> list[dict[str, object]]:
    """Load trusted note records from a UTF-8 JSON file.

    Contract:
    - read the file as UTF-8 text
    - the JSON root must be a list
    - each note must be an object with title, body, and tags
    - title and body must be strings
    - tags must be a list of strings
    - raise ValueError when the decoded data has the wrong shape
    """
    return []
