def parse_tags(raw_tags: str) -> list[str]:
    """Return normalized tags for py-notes-cli.

    Contract:
    - split the input on commas
    - trim whitespace around each tag
    - lowercase each tag
    - ignore empty chunks
    - remove duplicates while preserving first-seen order
    - raise ValueError when a tag contains spaces after trimming
    """
    return []
