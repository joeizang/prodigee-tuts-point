import { describe, expect, it } from 'vitest'
import type { ExerciseDetail, ExerciseWorkspace } from '../../api'
import { isWorkspaceCompatibleWithExercise } from './workspaceCompatibility'

const pythonExercise: ExerciseDetail = {
  id: 'normalize-note-title-py',
  trackId: 'python',
  title: 'NormalizeNoteTitle',
  summary: 'Normalize note titles.',
  language: 'Python',
  kind: 'focused',
  directoryPath: 'tracks/python/exercises/normalize-note-title-py',
  softLocks: [],
}

describe('isWorkspaceCompatibleWithExercise', () => {
  it('rejects a stale C# workspace for a Python exercise', () => {
    const workspace: ExerciseWorkspace = {
      exerciseId: 'normalize-note-title-py',
      title: 'NormalizeNoteTitle',
      language: 'Python',
      runtime: 'dotnet',
      workspacePath: '/tmp/workspace',
      languageServiceMessage: 'Project-aware Roslyn services are active for editable C# files.',
      files: [
        {
          path: 'src/Exercise/WordFrequencyAnalyzer.cs',
          role: 'editable',
          editable: true,
          content: 'def normalize_title(raw_title: str) -> str:\n    return raw_title\n',
        },
        {
          path: 'tests/Exercise.Tests/VisibleTests.cs',
          role: 'visible-test',
          editable: false,
          content: '',
        },
      ],
      lastStatus: 'NotStarted',
      lastOutput: '',
      lastDiagnostics: '',
    }

    expect(isWorkspaceCompatibleWithExercise(workspace, pythonExercise)).toBe(false)
  })

  it('accepts a Python pytest workspace for a Python exercise', () => {
    const workspace: ExerciseWorkspace = {
      exerciseId: 'normalize-note-title-py',
      title: 'NormalizeNoteTitle',
      language: 'Python',
      runtime: 'python-pytest',
      workspacePath: '/tmp/workspace',
      languageServiceMessage: 'Python project workspace generation is active.',
      files: [
        {
          path: 'src/note_titles.py',
          role: 'editable',
          editable: true,
          content: 'def normalize_title(raw_title: str) -> str:\n    return raw_title\n',
        },
        {
          path: 'tests/test_note_titles_visible.py',
          role: 'visible-test',
          editable: false,
          content: '',
        },
      ],
      lastStatus: 'NotStarted',
      lastOutput: '',
      lastDiagnostics: '',
    }

    expect(isWorkspaceCompatibleWithExercise(workspace, pythonExercise)).toBe(true)
  })
})
