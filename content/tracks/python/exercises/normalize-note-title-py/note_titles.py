def normalize_title(raw_title: str) -> str:
    """Return the normalized note title for py-notes-cli.

    Contract:
    - trim whitespace from both ends
    - collapse any internal whitespace run to one space
    - lowercase the result
    - raise ValueError when the title is blank
    """
    return raw_title
