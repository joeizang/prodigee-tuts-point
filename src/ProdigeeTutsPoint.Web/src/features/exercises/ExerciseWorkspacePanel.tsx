import Editor, { type OnMount } from '@monaco-editor/react'
import { AlertTriangle, Play, Save } from 'lucide-react'
import { useEffect, useMemo, useRef } from 'react'
import { postJson } from '../../api'
import type {
  ExerciseCompletionItem,
  ExerciseCodeActionsResult,
  ExerciseCompletionsResult,
  ExerciseDiagnostic,
  ExerciseDiagnosticsResult,
  ExerciseFormatResult,
  ExerciseHoverResult,
  ExerciseSignatureHelpResult,
  ExerciseRunResult,
  ExerciseWorkspace,
  ExerciseWorkspaceFile,
} from '../../api'
import type { Theme } from '../../types'
import {
  toMonacoCodeAction,
  toMonacoRange,
  type MonacoFormattingModel,
  type MonacoLanguageModel,
  type MonacoReplacementRange,
} from './monacoCodeActions'
import {
  configureTypeScriptLanguageService,
  isTypeScriptRuntime,
  monacoLanguageForPath,
  shouldUseBackendLanguageService,
  syncWorkspaceModels,
} from './typescriptLanguageService'

const backendProviderRegistrations = new Set<string>()
let editorThemesRegistered = false
let swiftLanguageRegistered = false
let currentLanguageContext: (() => LanguageServiceContext | null) | null = null

export function ExerciseWorkspacePanel({
  activeContent,
  activeFile,
  fileEdits,
  isRunning,
  isSaving,
  onFileChange,
  onRun,
  onSave,
  onSelectFile,
  profileId,
  runResult,
  theme,
  workspace,
}: {
  activeContent: string
  activeFile: ExerciseWorkspaceFile | null
  fileEdits: Record<string, string>
  isRunning: boolean
  isSaving: boolean
  onFileChange: (path: string, content: string) => void
  onRun: () => void
  onSave: () => void
  onSelectFile: (path: string) => void
  profileId: string
  runResult: ExerciseRunResult | null
  theme: Theme
  workspace: ExerciseWorkspace
}) {
  const editorRef = useRef<Parameters<OnMount>[0] | null>(null)
  const monacoRef = useRef<Parameters<OnMount>[1] | null>(null)
  const languageContextRef = useRef<LanguageServiceContext | null>(null)
  const editorTheme = monacoThemeName(theme)
  const languageContext = useMemo(
    () => {
      if (!activeFile || !shouldUseBackendLanguageService(activeFile)) {
        return null
      }

      return {
        activeContent,
        exerciseId: workspace.exerciseId,
        path: activeFile.path,
        profileId,
      }
    },
    [activeContent, activeFile, profileId, workspace.exerciseId],
  )

  useEffect(() => {
    languageContextRef.current = languageContext
  }, [languageContext])

  useEffect(() => {
    currentLanguageContext = () => languageContextRef.current
    return () => {
      currentLanguageContext = null
    }
  }, [])

  useEffect(() => {
    monacoRef.current?.editor.setTheme(editorTheme)
  }, [editorTheme])

  useEffect(() => {
    const monaco = monacoRef.current
    const editor = editorRef.current
    const model = editor?.getModel()
    const context = languageContext

    if (!monaco || !model || !context) {
      return
    }

    const timeout = window.setTimeout(() => {
      void updateBackendDiagnostics(monaco, model, context, runResult?.diagnostics ?? '')
    }, 350)

    return () => window.clearTimeout(timeout)
  }, [languageContext, runResult?.diagnostics])

  const handleEditorMount: OnMount = (editor, monaco) => {
    editorRef.current = editor
    monacoRef.current = monaco
    monaco.editor.setTheme(editorTheme)
    if (isTypeScriptRuntime(workspace)) {
      void configureTypeScriptLanguageService(monaco)
      syncWorkspaceModels(monaco, workspace, fileEdits)
    }
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Period, () => {
      triggerCodeActionMenu(editor)
    })
    setupSwiftSyntaxHighlighting(monaco)
    setupBackendLanguageProviders(monaco, 'csharp')
    setupBackendLanguageProviders(monaco, 'swift')
    setupBackendLanguageProviders(monaco, 'python')
    void updateBackendDiagnostics(
      monaco,
      editor.getModel(),
      languageContext,
      runResult?.diagnostics ?? '',
    )
  }

  const handleChange = (value: string | undefined) => {
    if (!activeFile?.editable) {
      return
    }

    const content = value ?? ''
    onFileChange(activeFile.path, content)

    languageContextRef.current = shouldUseBackendLanguageService(activeFile)
      ? {
          activeContent: content,
          exerciseId: workspace.exerciseId,
          path: activeFile.path,
          profileId,
        }
      : null
  }

  useEffect(() => {
    const monaco = monacoRef.current
    if (!monaco || !isTypeScriptRuntime(workspace)) {
      return
    }

    void configureTypeScriptLanguageService(monaco)
    syncWorkspaceModels(monaco, workspace, fileEdits)
  }, [fileEdits, workspace])

  const hiddenCount = workspace.files.filter((file) => file.role === 'hidden-test').length

  return (
    <section className="workspace-panel">
      <div className="workspace-toolbar">
        <div>
          <h3>Exercise Workspace</h3>
          <span>{workspace.languageServiceMessage}</span>
        </div>
        <div className="workspace-actions">
          <button
            className="secondary-action"
            disabled={!activeFile?.editable || isSaving}
            type="button"
            onClick={onSave}
          >
            <Save size={16} />
            <span>{isSaving ? 'Saving...' : 'Save'}</span>
          </button>
          <button className="primary-action" disabled={isRunning} type="button" onClick={onRun}>
            <Play size={16} />
            <span>{isRunning ? 'Running...' : 'Run tests'}</span>
          </button>
        </div>
      </div>

      <div className="monaco-workspace">
        <div className="file-rail">
          {workspace.files.map((file) => (
            <button
              className={activeFile?.path === file.path ? 'file-tab active' : 'file-tab'}
              key={file.path}
              type="button"
              onClick={() => onSelectFile(file.path)}
            >
              <span>{file.path.split('/').at(-1)}</span>
              <small>{file.role}</small>
              {file.editable && fileEdits[file.path] !== undefined && <strong>modified</strong>}
            </button>
          ))}
        </div>
        <div className="editor-surface">
          {activeFile?.content === null || activeFile?.content === undefined ? (
            <div className="hidden-file-message">
              <AlertTriangle size={20} />
              <strong>Hidden test file</strong>
              <span>{hiddenCount} hidden test file is available to the runner but not exposed here.</span>
            </div>
          ) : (
            <Editor
              beforeMount={setupEditorThemes}
              height="clamp(560px, calc(100vh - 330px), 820px)"
              key={`${workspace.exerciseId}:${activeFile.path}`}
              language={monacoLanguageForPath(activeFile.path, workspace.language)}
              options={{
                acceptSuggestionOnCommitCharacter: true,
                fontSize: 14,
                formatOnPaste: true,
                formatOnType: true,
                minimap: { enabled: false },
                quickSuggestions: { comments: false, other: true, strings: false },
                quickSuggestionsDelay: 50,
                readOnly: !activeFile.editable,
                scrollBeyondLastLine: false,
                suggest: {
                  showClasses: true,
                  showFields: true,
                  showFunctions: true,
                  showInterfaces: true,
                  showKeywords: true,
                  showMethods: true,
                  showProperties: true,
                  showSnippets: true,
                  showStructs: true,
                  showVariables: true,
                },
                wordBasedSuggestions: 'off',
                wordWrap: 'on',
              }}
              path={`/workspace/${activeFile.path}`}
              theme={editorTheme}
              value={activeContent}
              onChange={handleChange}
              onMount={handleEditorMount}
            />
          )}
        </div>
      </div>

      {(runResult || workspace.lastOutput || workspace.lastDiagnostics) && (
        <section className="runner-output">
          <h3>Runner Output</h3>
          {runResult && (
            <div className={`run-status ${runResult.status.toLowerCase()}`}>
              <strong>{runResult.status}</strong>
              <span>Visible: {runResult.visiblePassed ? 'passed' : 'not passed'}</span>
              <span>Hidden: {runResult.hiddenPassed ? 'passed' : 'not passed'}</span>
            </div>
          )}
          <pre>{runResult?.output || workspace.lastOutput || workspace.lastDiagnostics}</pre>
        </section>
      )}
    </section>
  )
}

function monacoThemeName(theme: Theme) {
  if (theme === 'light') {
    return 'prodigee-light-editor'
  }

  if (theme === 'neutral') {
    return 'prodigee-neutral-editor'
  }

  return 'prodigee-dark-editor'
}

function setupEditorThemes(monaco: Parameters<OnMount>[1]) {
  setupSwiftSyntaxHighlighting(monaco)

  if (editorThemesRegistered) {
    return
  }

  editorThemesRegistered = true
  monaco.editor.defineTheme('prodigee-light-editor', {
    base: 'vs',
    inherit: true,
    rules: [
      { foreground: '0f5aa6', token: 'keyword' },
      { foreground: '23723f', token: 'string' },
      { foreground: '7a3fb0', token: 'type' },
      { foreground: '8a4f00', token: 'number' },
      { foreground: '617083', token: 'comment' },
    ],
    colors: {
      'editor.background': '#f7f8f5',
      'editor.foreground': '#17202a',
      'editorLineNumber.foreground': '#7b8490',
      'editorLineNumber.activeForeground': '#202832',
      'editorCursor.foreground': '#2563eb',
      'editor.selectionBackground': '#cfe0ff',
      'editor.inactiveSelectionBackground': '#e6edf8',
      'editor.lineHighlightBackground': '#eef2f7',
      'editorGutter.background': '#f0f2ef',
    },
  })

  monaco.editor.defineTheme('prodigee-neutral-editor', {
    base: 'vs',
    inherit: true,
    rules: [
      { foreground: '5e81ac', token: 'keyword' },
      { foreground: 'a3be8c', token: 'string' },
      { foreground: '8fbcbb', token: 'type' },
      { foreground: 'b48ead', token: 'number' },
      { foreground: '616e88', fontStyle: 'italic', token: 'comment' },
    ],
    colors: {
      'editor.background': '#e5e9f0',
      'editor.foreground': '#2e3440',
      'editorLineNumber.foreground': '#748094',
      'editorLineNumber.activeForeground': '#2e3440',
      'editorCursor.foreground': '#5e81ac',
      'editor.selectionBackground': '#b7c5d9',
      'editor.inactiveSelectionBackground': '#d8dee9',
      'editor.lineHighlightBackground': '#d8dee9',
      'editorGutter.background': '#d8dee9',
      'editorIndentGuide.background1': '#c8d0dc',
      'editorIndentGuide.activeBackground1': '#7b879a',
      'editorSuggestWidget.background': '#eceff4',
      'editorSuggestWidget.foreground': '#2e3440',
      'editorSuggestWidget.selectedBackground': '#d8dee9',
      'editorWidget.background': '#eceff4',
      'editorWidget.border': '#c8d0dc',
    },
  })

  monaco.editor.defineTheme('prodigee-dark-editor', {
    base: 'vs-dark',
    inherit: true,
    rules: [
      { foreground: '7db7ff', token: 'keyword' },
      { foreground: '8ce3a6', token: 'string' },
      { foreground: 'd2a8ff', token: 'type' },
      { foreground: 'f2bb6b', token: 'number' },
      { foreground: '758397', token: 'comment' },
    ],
    colors: {
      'editor.background': '#11161d',
      'editor.foreground': '#d8e2ee',
      'editorLineNumber.foreground': '#728093',
      'editorLineNumber.activeForeground': '#d8e2ee',
      'editorCursor.foreground': '#77a8ff',
      'editor.selectionBackground': '#29456f',
      'editor.inactiveSelectionBackground': '#223348',
      'editor.lineHighlightBackground': '#17202a',
      'editorGutter.background': '#0f141b',
    },
  })
}

function setupSwiftSyntaxHighlighting(monaco: Parameters<OnMount>[1]) {
  if (swiftLanguageRegistered) {
    return
  }

  swiftLanguageRegistered = true
  monaco.languages.register({ id: 'swift' })
  monaco.languages.setMonarchTokensProvider('swift', {
    tokenizer: {
      root: [
        [/\b(actor|as|async|await|case|catch|class|defer|do|else|enum|extension|false|for|func|guard|if|import|in|init|let|nil|private|protocol|public|return|self|static|struct|switch|throws|throw|true|try|var|while)\b/, 'keyword'],
        [/\b(Bool|Double|Error|Float|Int|String|Void)\b/, 'type'],
        [/".*?"/, 'string'],
        [/\/\/.*$/, 'comment'],
        [/\b\d+\b/, 'number'],
      ],
    },
  })
}

function setupBackendLanguageProviders(monaco: Parameters<OnMount>[1], languageId: 'csharp' | 'swift' | 'python') {
  if (backendProviderRegistrations.has(languageId)) {
    return
  }

  backendProviderRegistrations.add(languageId)
  monaco.languages.registerCompletionItemProvider(languageId, {
    triggerCharacters: languageId === 'csharp' ? ['.', '(', ',', '<'] : ['.', '(', ','],
    provideCompletionItems: async (
      _model: unknown,
      position: { lineNumber: number; column: number },
    ) => {
      const context = currentLanguageContext?.()
      if (!context) {
        return { suggestions: [] }
      }

      const model = _model as MonacoCompletionModel
      const word = model.getWordUntilPosition(position)
      const replacementRange = {
        endColumn: word.endColumn,
        endLineNumber: position.lineNumber,
        startColumn: word.startColumn,
        startLineNumber: position.lineNumber,
      }

      try {
        const response = await postJson<ExerciseCompletionsResult>(
          `/api/exercises/${context.exerciseId}/language/completions`,
          {
            column: position.column,
            content: model.getValue(),
            lineNumber: position.lineNumber,
            path: context.path,
            profileId: context.profileId,
          },
        )

        return {
          suggestions: response.items.map((item) => toMonacoCompletion(monaco, item, replacementRange)),
        }
      } catch {
        return { suggestions: [] }
      }
    },
  })

  monaco.languages.registerHoverProvider(languageId, {
    provideHover: async (_model: unknown, position: { lineNumber: number; column: number }) => {
      const context = currentLanguageContext?.()
      if (!context) {
        return null
      }

      const model = _model as MonacoLanguageModel
      try {
        const response = await postJson<ExerciseHoverResult>(
          `/api/exercises/${context.exerciseId}/language/hover`,
          {
            column: position.column,
            content: model.getValue(),
            lineNumber: position.lineNumber,
            path: context.path,
            profileId: context.profileId,
          },
        )

        if (response.contents.length === 0) {
          return null
        }

        return {
          contents: response.contents.map((value) => ({ value })),
          range: response.range ? toMonacoRange(monaco, model, response.range) : undefined,
        }
      } catch {
        return null
      }
    },
  })

  monaco.languages.registerSignatureHelpProvider(languageId, {
    signatureHelpTriggerCharacters: ['(', ','],
    signatureHelpRetriggerCharacters: [',', ')'],
    provideSignatureHelp: async (_model: unknown, position: { lineNumber: number; column: number }) => {
      const context = currentLanguageContext?.()
      if (!context) {
        return null
      }

      const model = _model as MonacoLanguageModel
      try {
        const response = await postJson<ExerciseSignatureHelpResult>(
          `/api/exercises/${context.exerciseId}/language/signature-help`,
          {
            column: position.column,
            content: model.getValue(),
            lineNumber: position.lineNumber,
            path: context.path,
            profileId: context.profileId,
          },
        )

        if (response.signatures.length === 0) {
          return null
        }

        return {
          dispose: () => undefined,
          value: {
            activeParameter: response.activeParameter,
            activeSignature: response.activeSignature,
            signatures: response.signatures.map((signature) => ({
              documentation: signature.documentation,
              label: signature.label,
              parameters: signature.parameters.map((parameter) => ({
                documentation: parameter.documentation,
                label: parameter.label,
              })),
            })),
          },
        }
      } catch {
        return null
      }
    },
  })

  monaco.languages.registerCodeActionProvider(languageId, {
    providedCodeActionKinds: [
      'quickfix',
      'refactor',
      'refactor.extract',
      'refactor.inline',
      'refactor.rewrite',
      'source',
    ],
    provideCodeActions: async (
      _model: unknown,
      range: {
        endColumn: number
        endLineNumber: number
        startColumn: number
        startLineNumber: number
      },
    ) => {
      const context = currentLanguageContext?.()
      if (!context) {
        return { actions: [], dispose: () => undefined }
      }

      const model = _model as MonacoLanguageModel
      try {
        const response = await postJson<ExerciseCodeActionsResult>(
          `/api/exercises/${context.exerciseId}/language/code-actions`,
          {
            content: model.getValue(),
            endColumn: range.endColumn,
            endLineNumber: range.endLineNumber,
            path: context.path,
            profileId: context.profileId,
            startColumn: range.startColumn,
            startLineNumber: range.startLineNumber,
          },
        )

        return {
          actions: response.actions.map((action) => toMonacoCodeAction(monaco, model, action)),
          dispose: () => undefined,
        }
      } catch {
        return { actions: [], dispose: () => undefined }
      }
    },
  })

  monaco.languages.registerDocumentFormattingEditProvider(languageId, {
    provideDocumentFormattingEdits: async (_model: unknown) => {
      const context = currentLanguageContext?.()
      if (!context) {
        return []
      }

      const model = _model as MonacoFormattingModel
      try {
        const response = await postJson<ExerciseFormatResult>(
          `/api/exercises/${context.exerciseId}/language/format`,
          {
            content: model.getValue(),
            path: context.path,
            profileId: context.profileId,
          },
        )

        return [
          {
            range: model.getFullModelRange(),
            text: response.content,
          },
        ]
      } catch {
        return []
      }
    },
  })
}

function triggerCodeActionMenu(editor: Parameters<OnMount>[0]) {
  editor.focus()
  const codeActionArgs = { apply: 'never', kind: '' }
  try {
    editor.trigger('keyboard', 'editor.action.codeAction', codeActionArgs)
    return
  } catch {
    editor.trigger('keyboard', 'editor.action.quickFix', null)
  }

  window.setTimeout(() => {
    editor.trigger('keyboard', 'editor.action.refactor', { apply: 'never', kind: 'refactor' })
  }, 0)
}

function toMonacoCompletion(
  monaco: Parameters<OnMount>[1],
  item: ExerciseCompletionItem,
  replacementRange: MonacoReplacementRange,
) {
  return {
    detail: item.detail,
    filterText: item.filterText,
    insertText: item.insertText,
    kind: completionKind(monaco, item.tags),
    label: item.label,
    range: replacementRange,
    sortText: item.sortText,
  }
}

async function updateBackendDiagnostics(
  monaco: Parameters<OnMount>[1],
  model: ReturnType<Parameters<OnMount>[0]['getModel']>,
  context: LanguageServiceContext | null,
  runnerDiagnostics: string,
) {
  if (!model) {
    return
  }

  const versionAtRequestStart = model.getVersionId()
  const markers = runnerDiagnostics.trim()
    ? [runnerDiagnosticsMarker(monaco, runnerDiagnostics)]
    : []

  if (!context) {
    monaco.editor.setModelMarkers(model, 'backend-language-service', markers)
    return
  }

  try {
    const response = await postJson<ExerciseDiagnosticsResult>(
      `/api/exercises/${context.exerciseId}/language/diagnostics`,
      {
        content: model.getValue(),
        path: context.path,
        profileId: context.profileId,
      },
    )
    markers.push(...response.diagnostics.map((diagnostic) => toMonacoMarker(monaco, diagnostic)))
    markers.push(...setupMessageMarkers(monaco, response.setupMessages))
  } catch (error) {
    console.warn('Backend language diagnostics request failed.', error)
    return
  }

  if (model.getVersionId() !== versionAtRequestStart) {
    return
  }

  monaco.editor.setModelMarkers(model, 'backend-language-service', markers)
}

function setupMessageMarkers(monaco: Parameters<OnMount>[1], setupMessages: string[] | undefined) {
  return (setupMessages ?? [])
    .filter((message) => message.trim().length > 0)
    .map((message) => ({
      endColumn: 1,
      endLineNumber: 1,
      message,
      severity: monaco.MarkerSeverity.Warning,
      startColumn: 1,
      startLineNumber: 1,
    }))
}

function runnerDiagnosticsMarker(monaco: Parameters<OnMount>[1], runnerDiagnostics: string) {
  return {
    endColumn: 1,
    endLineNumber: 1,
    message: runnerDiagnostics.slice(0, 500),
    severity: monaco.MarkerSeverity.Error,
    startColumn: 1,
    startLineNumber: 1,
  }
}

function toMonacoMarker(monaco: Parameters<OnMount>[1], diagnostic: ExerciseDiagnostic) {
  return {
    code: diagnostic.id,
    endColumn: Math.max(diagnostic.endColumn, diagnostic.startColumn + 1),
    endLineNumber: diagnostic.endLineNumber,
    message: diagnostic.message,
    severity: diagnosticSeverity(monaco, diagnostic.severity),
    startColumn: diagnostic.startColumn,
    startLineNumber: diagnostic.startLineNumber,
  }
}

function diagnosticSeverity(monaco: Parameters<OnMount>[1], severity: string) {
  if (severity === 'Error') {
    return monaco.MarkerSeverity.Error
  }

  if (severity === 'Warning') {
    return monaco.MarkerSeverity.Warning
  }

  if (severity === 'Info') {
    return monaco.MarkerSeverity.Info
  }

  return monaco.MarkerSeverity.Hint
}

function completionKind(monaco: Parameters<OnMount>[1], tags: string[]) {
  if (tags.includes('Method') || tags.includes('ExtensionMethod')) {
    return monaco.languages.CompletionItemKind.Method
  }

  if (tags.includes('Class')) {
    return monaco.languages.CompletionItemKind.Class
  }

  if (tags.includes('Struct') || tags.includes('Structure')) {
    return monaco.languages.CompletionItemKind.Struct
  }

  if (tags.includes('Interface')) {
    return monaco.languages.CompletionItemKind.Interface
  }

  if (tags.includes('Property')) {
    return monaco.languages.CompletionItemKind.Property
  }

  if (tags.includes('Field')) {
    return monaco.languages.CompletionItemKind.Field
  }

  if (tags.includes('Keyword')) {
    return monaco.languages.CompletionItemKind.Keyword
  }

  if (tags.includes('Local') || tags.includes('Parameter')) {
    return monaco.languages.CompletionItemKind.Variable
  }

  if (tags.includes('Snippet')) {
    return monaco.languages.CompletionItemKind.Snippet
  }

  if (tags.includes('Namespace')) {
    return monaco.languages.CompletionItemKind.Module
  }

  if (tags.includes('Enum')) {
    return monaco.languages.CompletionItemKind.Enum
  }

  if (tags.includes('TypeParameter')) {
    return monaco.languages.CompletionItemKind.TypeParameter
  }

  return monaco.languages.CompletionItemKind.Text
}

type LanguageServiceContext = {
  activeContent: string
  exerciseId: string
  path: string
  profileId: string
}

type MonacoCompletionModel = {
  getValue: () => string
  getWordUntilPosition: (position: { lineNumber: number; column: number }) => {
    endColumn: number
    startColumn: number
    word: string
  }
}
