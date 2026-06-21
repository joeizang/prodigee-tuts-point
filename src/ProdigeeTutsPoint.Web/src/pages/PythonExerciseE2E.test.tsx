import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ActiveLearningProvider } from '../state/ActiveLearningContext'
import { ExerciseDetail } from './ExerciseDetail'

const monacoState = vi.hoisted(() => ({
  codeActionProviders: new Map<string, unknown>(),
  completionProviders: new Map<string, unknown>(),
  formatProviders: new Map<string, unknown>(),
  hoverProviders: new Map<string, unknown>(),
  lastEditorProps: null as null | {
    language: string
    path: string
    value: string
  },
  markers: [] as unknown[][],
  signatureProviders: new Map<string, unknown>(),
}))

vi.mock('@monaco-editor/react', async () => {
  const React = await vi.importActual<typeof import('react')>('react')

  function Editor(props: {
    beforeMount?: (monaco: ReturnType<typeof createMonacoMock>) => void
    language: string
    onChange?: (value: string | undefined) => void
    onMount?: (
      editor: ReturnType<typeof createEditorMock>,
      monaco: ReturnType<typeof createMonacoMock>,
    ) => void
    path: string
    value: string
  }) {
    React.useEffect(() => {
      const monaco = createMonacoMock()
      const editor = createEditorMock(props.value)
      monacoState.lastEditorProps = {
        language: props.language,
        path: props.path,
        value: props.value,
      }
      props.beforeMount?.(monaco)
      props.onMount?.(editor, monaco)
    }, [props])

    return React.createElement('textarea', {
      'aria-label': 'Monaco editor',
      'data-language': props.language,
      'data-path': props.path,
      onChange: (event: React.ChangeEvent<HTMLTextAreaElement>) =>
        props.onChange?.(event.currentTarget.value),
      value: props.value,
    })
  }

  return { default: Editor }
})

describe('Python exercise browser workflow', () => {
  beforeEach(() => {
    localStorage.clear()
    localStorage.setItem('prodigee.selectedTrack.default-profile', 'csharp')
    monacoState.codeActionProviders.clear()
    monacoState.completionProviders.clear()
    monacoState.formatProviders.clear()
    monacoState.hoverProviders.clear()
    monacoState.lastEditorProps = null
    monacoState.markers = []
    monacoState.signatureProviders.clear()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('switches to Python and drives the Python/FastAPI workspace through Monaco, Pyright, Ruff, and pytest paths', async () => {
    const requests: Array<{ body: unknown; method: string; url: string }> = []
    vi.stubGlobal(
      'fetch',
      vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
        const url = input.toString()
        const method = init?.method ?? 'GET'
        const body = init?.body ? JSON.parse(String(init.body)) : null
        requests.push({ body, method, url })

        if (url === '/api/curriculum/tracks/csharp') {
          return jsonResponse(trackDetail('csharp', 'C# Language', 'CSharp'))
        }

        if (url === '/api/curriculum/tracks/python') {
          return jsonResponse(trackDetail('python', 'Python and FastAPI', 'Python'))
        }

        if (url === '/api/curriculum/exercises/create-notes-api-py') {
          return jsonResponse({
            directoryPath: 'tracks/python/exercises/create-notes-api-py',
            id: 'create-notes-api-py',
            kind: 'focused',
            language: 'Python',
            softLocks: [],
            summary: 'Create a FastAPI app through a clean service boundary.',
            title: 'Create Notes API',
            trackId: 'python',
          })
        }

        if (url.includes('/api/exercises/create-notes-api-py/workspace')) {
          return jsonResponse(pythonFastApiWorkspace())
        }

        if (url.includes('/api/exercises/create-notes-api-py/assistance')) {
          return jsonResponse({ hints: [], solution: null, solutionAvailable: false })
        }

        if (url.includes('/api/exercises/create-notes-api-py/attempts')) {
          return jsonResponse([])
        }

        if (url.includes('/api/learner/notes')) {
          return jsonResponse(null)
        }

        if (url === '/api/exercises/create-notes-api-py/language/diagnostics') {
          return jsonResponse({
            diagnostics: [
              {
                endColumn: 18,
                endLineNumber: 2,
                id: 'reportMissingParameterType',
                message: 'Type annotation is missing for parameter "service".',
                severity: 'Warning',
                startColumn: 13,
                startLineNumber: 2,
              },
            ],
            setupMessages: [],
          })
        }

        if (url === '/api/exercises/create-notes-api-py/language/completions') {
          return jsonResponse({
            items: [
              {
                detail: 'class FastAPI',
                filterText: 'FastAPI',
                insertText: 'FastAPI',
                label: 'FastAPI',
                sortText: '0000',
                tags: ['Class'],
              },
            ],
            setupMessages: [],
          })
        }

        if (url === '/api/exercises/create-notes-api-py/language/format') {
          return jsonResponse({
            content: 'from fastapi import FastAPI\n\n\napp = FastAPI()\n',
            setupMessages: [],
          })
        }

        if (url === '/api/exercises/create-notes-api-py/run') {
          return jsonResponse({
            diagnostics: '',
            exitCode: 0,
            hiddenPassed: true,
            output: '2 passed',
            staticAnalysis: [],
            status: 'Passed',
            timedOut: false,
            visiblePassed: true,
          })
        }

        return jsonResponse({})
      }),
    )

    render(
      <MemoryRouter initialEntries={['/exercises/create-notes-api-py']}>
        <ActiveLearningProvider profile={{ id: 'default-profile', displayName: 'Default Profile' }}>
          <Routes>
            <Route
              path="/exercises/:exerciseId"
              element={
                <ExerciseDetail
                  profile={{ id: 'default-profile', displayName: 'Default Profile' }}
                  theme="neutral"
                />
              }
            />
          </Routes>
        </ActiveLearningProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Create Notes API' })).toBeInTheDocument()
    await waitFor(() => {
      expect(localStorage.getItem('prodigee.selectedTrack.default-profile')).toBe('python')
    })

    expect(screen.getByText(/Pyright or basedpyright powers Python IntelliSense/)).toBeInTheDocument()
    expect(screen.getByText('notes_api.py')).toBeInTheDocument()
    expect(screen.getByText('test_notes_api_visible.py')).toBeInTheDocument()
    expect(screen.getByText('test_notes_api_hidden.py')).toBeInTheDocument()
    expect(screen.getByText('pyproject.toml')).toBeInTheDocument()
    expect(screen.queryByText(/WordFrequencyAnalyzer\.cs/)).not.toBeInTheDocument()
    expect(screen.queryByText(/Exercise\.csproj/)).not.toBeInTheDocument()

    const editor = screen.getByLabelText('Monaco editor')
    expect(editor).toHaveAttribute('data-language', 'python')
    expect(editor).toHaveAttribute('data-path', '/workspace/src/notes_api.py')
    expect((editor as HTMLTextAreaElement).value).toContain('from fastapi import FastAPI')

    await waitFor(() => {
      expect(monacoState.markers).toEqual(
        expect.arrayContaining([
          expect.arrayContaining([
            expect.objectContaining({
              message: 'Type annotation is missing for parameter "service".',
            }),
          ]),
        ]),
      )
    })
    expect(requests).toContainEqual(
      expect.objectContaining({
        body: expect.objectContaining({
          path: 'src/notes_api.py',
          profileId: 'default-profile',
        }),
        method: 'POST',
        url: '/api/exercises/create-notes-api-py/language/diagnostics',
      }),
    )

    const completionProvider = monacoState.completionProviders.get('python') as {
      provideCompletionItems: (
        model: ReturnType<typeof createLanguageModel>,
        position: { column: number; lineNumber: number },
      ) => Promise<{ suggestions: unknown[] }>
    }
    const completion = await completionProvider.provideCompletionItems(
      createLanguageModel('app = Fast', 'Fast'),
      { column: 11, lineNumber: 1 },
    )
    expect(completion.suggestions).toContainEqual(
      expect.objectContaining({
        insertText: 'FastAPI',
        label: 'FastAPI',
      }),
    )

    const formatProvider = monacoState.formatProviders.get('python') as {
      provideDocumentFormattingEdits: (
        model: ReturnType<typeof createLanguageModel>,
      ) => Promise<Array<{ text: string }>>
    }
    const edits = await formatProvider.provideDocumentFormattingEdits(
      createLanguageModel('from fastapi import FastAPI\napp=FastAPI()\n', 'app'),
    )
    expect(edits).toEqual([
      expect.objectContaining({
        text: 'from fastapi import FastAPI\n\n\napp = FastAPI()\n',
      }),
    ])
    expect(requests).toContainEqual(
      expect.objectContaining({
        method: 'POST',
        url: '/api/exercises/create-notes-api-py/language/format',
      }),
    )

    fireEvent.change(editor, {
      target: {
        value: 'from fastapi import FastAPI\n\n\ndef create_app() -> FastAPI:\n    return FastAPI()\n',
      },
    })
    fireEvent.click(screen.getByRole('button', { name: /run tests/i }))

    expect(await screen.findByText('Runner Output')).toBeInTheDocument()
    expect(screen.getByText('Passed')).toBeInTheDocument()
    expect(screen.getByText('Visible: passed')).toBeInTheDocument()
    expect(screen.getByText('Hidden: passed')).toBeInTheDocument()
    expect(requests).toContainEqual(
      expect.objectContaining({
        body: expect.objectContaining({
          files: [
            {
              content:
                'from fastapi import FastAPI\n\n\ndef create_app() -> FastAPI:\n    return FastAPI()\n',
              path: 'src/notes_api.py',
            },
          ],
          profileId: 'default-profile',
        }),
        method: 'POST',
        url: '/api/exercises/create-notes-api-py/run',
      }),
    )
  })

  it('shows Python static-analysis blocks distinctly before a corrected rerun passes', async () => {
    const requests: Array<{ body: unknown; method: string; url: string }> = []
    let runCount = 0
    vi.stubGlobal(
      'fetch',
      vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
        const url = input.toString()
        const method = init?.method ?? 'GET'
        const body = init?.body ? JSON.parse(String(init.body)) : null
        requests.push({ body, method, url })

        if (url === '/api/curriculum/tracks/csharp') {
          return jsonResponse(trackDetail('csharp', 'C# Language', 'CSharp'))
        }

        if (url === '/api/curriculum/tracks/python') {
          return jsonResponse(trackDetail('python', 'Python and FastAPI', 'Python'))
        }

        if (url === '/api/curriculum/exercises/create-notes-api-py') {
          return jsonResponse({
            directoryPath: 'tracks/python/exercises/create-notes-api-py',
            id: 'create-notes-api-py',
            kind: 'focused',
            language: 'Python',
            softLocks: [],
            summary: 'Create a FastAPI app through a clean service boundary.',
            title: 'Create Notes API',
            trackId: 'python',
          })
        }

        if (url.includes('/api/exercises/create-notes-api-py/workspace')) {
          return jsonResponse(pythonFastApiWorkspace())
        }

        if (url.includes('/api/exercises/create-notes-api-py/assistance')) {
          return jsonResponse({ hints: [], solution: null, solutionAvailable: false })
        }

        if (url.includes('/api/exercises/create-notes-api-py/attempts')) {
          return jsonResponse([])
        }

        if (url.includes('/api/learner/notes')) {
          return jsonResponse(null)
        }

        if (url === '/api/exercises/create-notes-api-py/language/diagnostics') {
          return jsonResponse({ diagnostics: [], setupMessages: [] })
        }

        if (url === '/api/exercises/create-notes-api-py/run') {
          runCount += 1
          if (runCount === 1) {
            return jsonResponse({
              diagnostics:
                'Static analysis failed.\nBasedPyright:\nsrc/notes_api.py:4:12 - error: Type "int" is not assignable to return type "str" (reportReturnType)',
              exitCode: 0,
              hiddenPassed: true,
              output: 'Visible tests:\n2 passed\n\nHidden tests:\nHidden tests passed.',
              staticAnalysis: [
                {
                  column: 12,
                  filePath: 'src/notes_api.py',
                  line: 4,
                  message: 'Type "int" is not assignable to return type "str"',
                  ruleId: 'reportReturnType',
                  severity: 'error',
                },
              ],
              status: 'FailedStaticAnalysis',
              timedOut: false,
              visiblePassed: true,
            })
          }

          return jsonResponse({
            diagnostics: '',
            exitCode: 0,
            hiddenPassed: true,
            output: 'Visible tests:\n2 passed\n\nHidden tests:\nHidden tests passed.',
            staticAnalysis: [],
            status: 'Passed',
            timedOut: false,
            visiblePassed: true,
          })
        }

        return jsonResponse({})
      }),
    )

    render(
      <MemoryRouter initialEntries={['/exercises/create-notes-api-py']}>
        <ActiveLearningProvider profile={{ id: 'default-profile', displayName: 'Default Profile' }}>
          <Routes>
            <Route
              path="/exercises/:exerciseId"
              element={
                <ExerciseDetail
                  profile={{ id: 'default-profile', displayName: 'Default Profile' }}
                  theme="neutral"
                />
              }
            />
          </Routes>
        </ActiveLearningProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Create Notes API' })).toBeInTheDocument()
    const editor = screen.getByLabelText('Monaco editor')

    fireEvent.change(editor, {
      target: {
        value: 'from fastapi import FastAPI\n\n\ndef create_app() -> str:\n    return 42\n',
      },
    })
    fireEvent.click(screen.getByRole('button', { name: /run tests/i }))

    expect(await screen.findByText('Quality gate blocked')).toBeInTheDocument()
    expect(
      screen.getByText(/Tests passed, but Ruff or BasedPyright found quality issues/),
    ).toBeInTheDocument()
    expect(screen.getByText('reportReturnType')).toBeInTheDocument()
    expect(screen.getAllByText(/Type "int" is not assignable to return type "str"/).length).toBeGreaterThan(0)

    fireEvent.change(editor, {
      target: {
        value: 'from fastapi import FastAPI\n\n\ndef create_app() -> FastAPI:\n    return FastAPI()\n',
      },
    })
    fireEvent.click(screen.getByRole('button', { name: /run tests/i }))

    expect(await screen.findByText('Passed')).toBeInTheDocument()
    expect(screen.queryByText('Quality gate blocked')).not.toBeInTheDocument()
    expect(runCount).toBe(2)
    expect(requests.filter((request) => request.url === '/api/exercises/create-notes-api-py/run')).toHaveLength(2)
  })
})

function trackDetail(id: string, title: string, language: string) {
  return {
    description: `${title} track`,
    id,
    language,
    modules: [],
    projects: [],
    slug: id,
    title,
  }
}

function pythonFastApiWorkspace() {
  return {
    exerciseId: 'create-notes-api-py',
    files: [
      {
        content: 'from fastapi import FastAPI\n\n\ndef create_app() -> FastAPI:\n    return FastAPI()\n',
        editable: true,
        path: 'src/notes_api.py',
        role: 'editable',
      },
      {
        content: 'from notes_api import create_app\n',
        editable: false,
        path: 'tests/test_notes_api_visible.py',
        role: 'visible-test',
      },
      {
        content: null,
        editable: false,
        path: 'tests/test_notes_api_hidden.py',
        role: 'hidden-test',
      },
      {
        content: '[tool.basedpyright]\ninclude = ["src", "tests"]\n',
        editable: false,
        path: 'pyproject.toml',
        role: 'readonly',
      },
    ],
    language: 'Python',
    languageServiceMessage:
      'Python project workspace generation is active. Pyright or basedpyright powers Python IntelliSense when installed, Ruff powers formatting and linting when installed, and pytest runs visible and hidden tests.',
    lastDiagnostics: '',
    lastOutput: '',
    lastStatus: 'NotRun',
    runtime: 'python-pytest',
    title: 'Create Notes API',
    workspacePath: '/tmp/prodigee-python-workspace',
  }
}

function jsonResponse(value: unknown) {
  return new Response(JSON.stringify(value), {
    headers: { 'Content-Type': 'application/json' },
    status: 200,
  })
}

function createMonacoMock() {
  return {
    KeyCode: { Period: 190 },
    KeyMod: { CtrlCmd: 2048 },
    MarkerSeverity: { Error: 8, Hint: 1, Info: 2, Warning: 4 },
    Range: class Range {
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
    editor: {
      defineTheme: vi.fn(),
      setModelMarkers: vi.fn((_model: unknown, _owner: string, markers: unknown[]) => {
        monacoState.markers.push(markers)
      }),
      setTheme: vi.fn(),
    },
    languages: {
      CompletionItemKind: {
        Class: 6,
        Enum: 12,
        Field: 4,
        Interface: 7,
        Keyword: 13,
        Method: 1,
        Module: 9,
        Property: 10,
        Snippet: 27,
        Struct: 22,
        Text: 0,
        TypeParameter: 24,
        Variable: 5,
      },
      register: vi.fn(),
      registerCodeActionProvider: vi.fn((language: string, provider: unknown) => {
        monacoState.codeActionProviders.set(language, provider)
      }),
      registerCompletionItemProvider: vi.fn((language: string, provider: unknown) => {
        monacoState.completionProviders.set(language, provider)
      }),
      registerDocumentFormattingEditProvider: vi.fn((language: string, provider: unknown) => {
        monacoState.formatProviders.set(language, provider)
      }),
      registerHoverProvider: vi.fn((language: string, provider: unknown) => {
        monacoState.hoverProviders.set(language, provider)
      }),
      registerSignatureHelpProvider: vi.fn((language: string, provider: unknown) => {
        monacoState.signatureProviders.set(language, provider)
      }),
      setMonarchTokensProvider: vi.fn(),
    },
  }
}

function createEditorMock(value: string) {
  const model = createLanguageModel(value, '')
  return {
    addCommand: vi.fn(),
    focus: vi.fn(),
    getModel: () => model,
    trigger: vi.fn(),
  }
}

function createLanguageModel(value: string, word: string) {
  return {
    getFullModelRange: () => ({
      endColumn: value.length + 1,
      endLineNumber: 1,
      startColumn: 1,
      startLineNumber: 1,
    }),
    getValue: () => value,
    getVersionId: () => 1,
    getWordUntilPosition: (position: { column: number; lineNumber: number }) => ({
      endColumn: position.column,
      startColumn: Math.max(1, position.column - word.length),
      word,
    }),
  }
}
