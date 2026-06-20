from pathlib import Path


def run_add_command(args: list[str], notes_path: Path) -> str:
    """Run the py-notes add command workflow.

    Contract:
    - parse --title, --body, and optional --tags from args
    - normalize the title by trimming, collapsing whitespace, and lowercasing
    - parse tags by comma, lowercase them, remove duplicates, and reject spaces
    - append a note record to the JSON file at notes_path
    - create the notes file when it does not exist
    - return "Added note: <normalized-title>"
    """
    return ""
