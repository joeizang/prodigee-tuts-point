# Host Process First Code Execution

Prodigee Tuts Point will initially execute learner code in host-created temporary workspaces with strict timeouts, output limits, cleanup, and allowlisted commands. This is acceptable because the app is single-user and local-first, while the runner abstraction must remain container-ready so Docker/Podman execution can replace host execution later without changing curriculum or attempt models.
