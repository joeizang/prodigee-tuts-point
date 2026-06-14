import type { ExerciseWorkspaceFile } from '../../api'

export function selectActiveWorkspaceFile(
  files: ExerciseWorkspaceFile[],
  selectedPath: string | null,
) {
  return (
    files.find((file) => file.path === selectedPath) ??
    files.find((file) => file.editable) ??
    files[0] ??
    null
  )
}
