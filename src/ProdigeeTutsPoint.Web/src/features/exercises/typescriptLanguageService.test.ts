import { describe, expect, it, vi } from 'vitest'
import type { ExerciseWorkspace } from '../../api'
import {
  configureTypeScriptLanguageService,
  monacoLanguageForPath,
  shouldUseBackendLanguageService,
  syncWorkspaceModels,
  typeScriptCompilerOptions,
} from './typescriptLanguageService'

describe('TypeScript Monaco language service setup', () => {
  it('maps workspace file paths to Monaco languages', () => {
    expect(monacoLanguageForPath('src/exercise.ts', 'TypeScript')).toBe('typescript')
    expect(monacoLanguageForPath('package.json', 'TypeScript')).toBe('json')
    expect(monacoLanguageForPath('src/Exercise/Analyzer.cs', 'CSharp')).toBe('csharp')
    expect(monacoLanguageForPath('Sources/Exercise/Exercise.swift', 'Swift')).toBe('swift')
    expect(monacoLanguageForPath('src/note_titles.py', 'Python')).toBe('python')
    expect(monacoLanguageForPath('README.md', 'TypeScript')).toBe('typescript')
  })

  it('uses strict compiler options for exercise workspaces', () => {
    const monaco = monacoStub()
    const options = typeScriptCompilerOptions(monaco as never)

    expect(options.strict).toBe(true)
    expect(options.noImplicitAny).toBe(true)
    expect(options.strictNullChecks).toBe(true)
    expect(options.noUncheckedIndexedAccess).toBe(true)
    expect(options.noImplicitReturns).toBe(true)
    expect(options.types).toContain('node')
  })

  it('routes editable C# and Swift files through backend semantic language services', () => {
    expect(
      shouldUseBackendLanguageService({
        content: 'public enum LogLevel {}',
        editable: true,
        path: 'Sources/Exercise/Exercise.swift',
        role: 'editable',
      }),
    ).toBe(true)
    expect(
      shouldUseBackendLanguageService({
        content: 'namespace Exercise;',
        editable: true,
        path: 'src/Exercise/WordFrequencyAnalyzer.cs',
        role: 'editable',
      }),
    ).toBe(true)
    expect(
      shouldUseBackendLanguageService({
        content: 'def normalize_title(raw_title: str) -> str: ...',
        editable: true,
        path: 'src/note_titles.py',
        role: 'editable',
      }),
    ).toBe(true)
    expect(
      shouldUseBackendLanguageService({
        content: 'import XCTest',
        editable: false,
        path: 'Tests/ExerciseVisibleTests/VisibleTests.swift',
        role: 'visible-test',
      }),
    ).toBe(false)
  })

  it('registers TypeScript defaults and local Node type declarations', async () => {
    const monaco = monacoStub()

    await configureTypeScriptLanguageService(monaco as never)

    expect(monaco.languages.typescript.typescriptDefaults.setCompilerOptions).toHaveBeenCalled()
    expect(monaco.languages.typescript.typescriptDefaults.setDiagnosticsOptions).toHaveBeenCalledWith({
      noSemanticValidation: false,
      noSyntaxValidation: false,
    })
    expect(monaco.languages.typescript.typescriptDefaults.setEagerModelSync).toHaveBeenCalledWith(true)
    expect(monaco.languages.typescript.typescriptDefaults.addExtraLib).toHaveBeenCalled()
  })

  it('creates Monaco models for every visible workspace file and keeps edits in sync', () => {
    const monaco = monacoStub()
    const workspace: ExerciseWorkspace = {
      exerciseId: 'parse-command-request-ts',
      files: [
        { path: 'src/exercise.ts', role: 'editable', editable: true, content: 'export const value = 1' },
        {
          path: 'tests/visible.test.ts',
          role: 'visible-test',
          editable: false,
          content: 'import { value } from "../src/exercise"',
        },
        { path: 'tests/hidden.test.ts', role: 'hidden-test', editable: false, content: null },
        { path: 'tsconfig.json', role: 'readonly', editable: false, content: '{}' },
      ],
      language: 'TypeScript',
      languageServiceMessage: 'ready',
      lastDiagnostics: '',
      lastOutput: '',
      lastStatus: 'NotStarted',
      runtime: 'node-typescript',
      title: 'ParseCommandRequest',
      workspacePath: '/tmp/workspace',
    }

    syncWorkspaceModels(monaco as never, workspace, {
      'src/exercise.ts': 'export const value = 2',
    })

    expect(monaco.editor.createModel).toHaveBeenCalledTimes(3)
    expect(monaco.editor.createModel).toHaveBeenCalledWith(
      'export const value = 2',
      'typescript',
      expect.objectContaining({ path: '/workspace/src/exercise.ts' }),
    )
    expect(monaco.editor.createModel).not.toHaveBeenCalledWith(
      expect.anything(),
      expect.anything(),
      expect.objectContaining({ path: '/workspace/tests/hidden.test.ts' }),
    )
  })
})

function monacoStub() {
  const models = new Map<string, { getLanguageId: () => string; getValue: () => string; setValue: (value: string) => void }>()

  return {
    Uri: {
      file: (path: string) => ({ path, toString: () => `file://${path}` }),
    },
    editor: {
      createModel: vi.fn((content: string, language: string, uri: { path: string }) => {
        models.set(uri.path, {
          getLanguageId: () => language,
          getValue: () => content,
          setValue: vi.fn(),
        })
      }),
      getModel: vi.fn((uri: { path: string }) => models.get(uri.path) ?? null),
      setModelLanguage: vi.fn(),
    },
    languages: {
      typescript: {
        ModuleKind: { NodeNext: 199 },
        ModuleResolutionKind: { NodeJs: 2 },
        ScriptTarget: { ES2024: 11 },
        javascriptDefaults: {
          setCompilerOptions: vi.fn(),
        },
        typescriptDefaults: {
          addExtraLib: vi.fn(() => ({ dispose: vi.fn() })),
          setCompilerOptions: vi.fn(),
          setDiagnosticsOptions: vi.fn(),
          setEagerModelSync: vi.fn(),
        },
      },
    },
  }
}
