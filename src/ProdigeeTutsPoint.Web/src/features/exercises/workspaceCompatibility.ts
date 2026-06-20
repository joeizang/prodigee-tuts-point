import type { ExerciseDetail, ExerciseWorkspace } from '../../api'

export function isWorkspaceCompatibleWithExercise(
  workspace: ExerciseWorkspace | null,
  exercise: ExerciseDetail | null,
) {
  if (!workspace || !exercise) {
    return false
  }

  if (workspace.exerciseId !== exercise.id) {
    return false
  }

  const language = exercise.language.toLowerCase()
  const runtime = workspace.runtime.toLowerCase()
  const filePaths = workspace.files.map((file) => file.path.toLowerCase())

  if (language === 'python') {
    return runtime === 'python-pytest'
      && filePaths.some((path) => path.endsWith('.py') && path.startsWith('src/'))
      && filePaths.every((path) => !path.endsWith('.cs') && !path.endsWith('.csproj') && !path.endsWith('.sln'))
  }

  if (language === 'swift') {
    return runtime === 'swiftpm'
      && filePaths.some((path) => path.endsWith('.swift') && path.startsWith('sources/'))
      && filePaths.every((path) => !path.endsWith('.cs') && !path.endsWith('.csproj') && !path.endsWith('.sln'))
  }

  if (language === 'typescript') {
    return runtime === 'node-typescript'
      && filePaths.some((path) => path.endsWith('.ts') && path.startsWith('src/'))
  }

  return runtime === 'dotnet'
}
