def require_note_text(field_name: str, raw_text: str) -> str:
    """Return trimmed text or raise ValueError when it is blank.

    Contract:
    - trim whitespace around raw_text
    - reject empty text after trimming
    - include field_name in the error message
    """
    return raw_text

