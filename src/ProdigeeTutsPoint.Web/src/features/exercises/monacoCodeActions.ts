import type { OnMount } from '@monaco-editor/react'
import type { ExerciseCodeActionsResult } from '../../api'

export function toMonacoCodeAction(
  monaco: Parameters<OnMount>[1],
  model: MonacoLanguageModel,
  action: ExerciseCodeActionsResult['actions'][number],
) {
  return {
    edit: {
      edits: action.edits.map((edit) => ({
        resource: model.uri,
        textEdit: {
          range: toMonacoRange(monaco, model, edit),
          text: edit.text,
        },
        versionId: undefined,
      })),
    },
    kind: action.kind,
    title: action.title,
  }
}

export type MonacoFormattingModel = {
  getFullModelRange: () => MonacoReplacementRange
  getValue: () => string
  getVersionId: () => number
}

export type MonacoLanguageModel = MonacoFormattingModel & {
  uri: unknown
}

export type MonacoReplacementRange = {
  endColumn: number
  endLineNumber: number
  startColumn: number
  startLineNumber: number
}

export function toMonacoRange(
  monaco: Parameters<OnMount>[1],
  model: MonacoFormattingModel,
  range: {
    endColumn: number
    endLineNumber: number
    startColumn: number
    startLineNumber: number
  },
) {
  if (range.endLineNumber === 2147483647 || range.endColumn === 2147483647) {
    return model.getFullModelRange()
  }

  return new monaco.Range(
    range.startLineNumber,
    range.startColumn,
    range.endLineNumber,
    range.endColumn,
  )
}
