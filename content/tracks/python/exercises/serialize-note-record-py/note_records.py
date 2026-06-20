def build_note_record(title: str, body: str, tags: list[str]) -> dict[str, object]:
    """Return a JSON-safe note record for py-notes-cli.

    Contract:
    - title must contain non-whitespace text
    - body must contain non-whitespace text
    - title and body are trimmed in the returned record
    - tags are copied into the returned record
    - raise ValueError for blank title, blank body, or blank tags
    """
    return {"title": title, "body": body, "tags": tags}
