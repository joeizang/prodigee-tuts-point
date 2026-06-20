def parse_add_command(args: list[str]) -> dict[str, object]:
    """Parse py-notes add command options.

    Contract:
    - args contains option/value pairs without the program name
    - --title is required
    - --body is required
    - --tags is optional and defaults to an empty string
    - unknown options raise ValueError
    - missing option values raise ValueError
    """
    return {"title": "", "body": "", "tags": ""}
