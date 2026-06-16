import { describe, expect, it } from 'vitest'
import { toMonacoCodeAction } from './monacoCodeActions'

describe('ExerciseWorkspacePanel Monaco code actions', () => {
  it('maps backend edits to Monaco textEdit workspace edits', () => {
    const range = { endColumn: 2, endLineNumber: 1, startColumn: 1, startLineNumber: 1 }
    const monaco = {
      Range: class {
        endColumn: number
        endLineNumber: number
        startColumn: number
        startLineNumber: number

        constructor(
          startLineNumber: number,
          startColumn: number,
          endLineNumber: number,
          endColumn: number,
        ) {
          this.startLineNumber = startLineNumber
          this.startColumn = startColumn
          this.endLineNumber = endLineNumber
          this.endColumn = endColumn
        }
      },
    }
    const model = {
      getFullModelRange: () => range,
      getValue: () => '',
      getVersionId: () => 1,
      uri: { path: '/exercise.cs' },
    }

    const action = toMonacoCodeAction(monaco as never, model, {
      edits: [{ ...range, text: 'replacement' }],
      kind: 'refactor.rewrite',
      title: 'Use expression body for method',
    })

    expect(action.edit.edits[0]).toMatchObject({
      resource: model.uri,
      textEdit: {
        text: 'replacement',
      },
      versionId: undefined,
    })
    expect(action.edit.edits[0]).not.toHaveProperty('edit')
  })
})
