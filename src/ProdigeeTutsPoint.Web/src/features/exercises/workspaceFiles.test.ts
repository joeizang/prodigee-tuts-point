import { describe, expect, it } from 'vitest'
import type { ExerciseWorkspaceFile } from '../../api'
import { selectActiveWorkspaceFile } from './workspaceFiles'

const files: ExerciseWorkspaceFile[] = [
  {
    path: 'tests/Exercise.Tests/VisibleTests.cs',
    role: 'visible-test',
    editable: false,
    content: 'visible',
  },
  {
    path: 'src/Exercise/WordFrequencyAnalyzer.cs',
    role: 'editable',
    editable: true,
    content: 'starter',
  },
]

describe('selectActiveWorkspaceFile', () => {
  it('keeps the selected file when it still exists', () => {
    expect(selectActiveWorkspaceFile(files, 'tests/Exercise.Tests/VisibleTests.cs')?.path).toBe(
      'tests/Exercise.Tests/VisibleTests.cs',
    )
  })

  it('falls back to the editable file before readonly files', () => {
    expect(selectActiveWorkspaceFile(files, 'missing.cs')?.path).toBe(
      'src/Exercise/WordFrequencyAnalyzer.cs',
    )
  })
})
