import type { OnMount } from '@monaco-editor/react'
import type { ExerciseWorkspace, ExerciseWorkspaceFile } from '../../api'

type Monaco = Parameters<OnMount>[1]

const nodeTypeLibs = import.meta.glob('/node_modules/@types/node/**/*.d.ts', {
  import: 'default',
  query: '?raw',
}) as Record<string, () => Promise<string>>

const undiciTypeLibs = import.meta.glob('/node_modules/undici-types/**/*.d.ts', {
  import: 'default',
  query: '?raw',
}) as Record<string, () => Promise<string>>

let configured = false
let nodeTypeLibrariesRegistered = false
let extraLibDisposable: { dispose: () => void }[] = []

export function monacoLanguageForPath(path: string, fallbackLanguage: string) {
  const normalized = path.toLowerCase()
  if (normalized.endsWith('.ts') || normalized.endsWith('.tsx')) {
    return 'typescript'
  }

  if (normalized.endsWith('.js') || normalized.endsWith('.jsx') || normalized.endsWith('.mjs')) {
    return 'javascript'
  }

  if (normalized.endsWith('.json')) {
    return 'json'
  }

  if (normalized.endsWith('.cs')) {
    return 'csharp'
  }

  if (normalized.endsWith('.swift')) {
    return 'swift'
  }

  return fallbackLanguage.toLowerCase() === 'typescript' ? 'typescript' : 'plaintext'
}

export function workspaceUri(monaco: Monaco, path: string) {
  return monaco.Uri.file(`/workspace/${path}`)
}

export async function configureTypeScriptLanguageService(monaco: Monaco) {
  if (configured) {
    if (!nodeTypeLibrariesRegistered) {
      await registerNodeTypeLibraries(monaco)
    }
    return
  }

  configured = true
  const compilerOptions = typeScriptCompilerOptions(monaco)
  monaco.languages.typescript.typescriptDefaults.setCompilerOptions(compilerOptions)
  monaco.languages.typescript.javascriptDefaults.setCompilerOptions(compilerOptions)
  monaco.languages.typescript.typescriptDefaults.setDiagnosticsOptions({
    noSemanticValidation: false,
    noSyntaxValidation: false,
  })
  monaco.languages.typescript.typescriptDefaults.setEagerModelSync(true)
  await registerNodeTypeLibraries(monaco)
}

export function typeScriptCompilerOptions(monaco: Monaco) {
  const ts = monaco.languages.typescript
  return {
    allowNonTsExtensions: true,
    esModuleInterop: true,
    exactOptionalPropertyTypes: true,
    lib: ['es2024'],
    module: ts.ModuleKind.NodeNext,
    moduleResolution: ts.ModuleResolutionKind.NodeJs,
    noFallthroughCasesInSwitch: true,
    noImplicitAny: true,
    noImplicitReturns: true,
    noUncheckedIndexedAccess: true,
    strict: true,
    strictNullChecks: true,
    target: ts.ScriptTarget.ES2024,
    typeRoots: ['file:///node_modules/@types'],
    types: ['node'],
  }
}

export async function registerNodeTypeLibraries(monaco: Monaco) {
  if (nodeTypeLibrariesRegistered) {
    return
  }

  nodeTypeLibrariesRegistered = true
  extraLibDisposable.forEach((disposable) => disposable.dispose())
  extraLibDisposable = []

  for (const [path, loadSource] of Object.entries({ ...nodeTypeLibs, ...undiciTypeLibs })) {
    const source = await loadSource()
    const uri = path.startsWith('/node_modules/')
      ? `file://${path}`
      : `file:///node_modules/${path.replace(/^.*node_modules\//, '')}`
    extraLibDisposable.push(monaco.languages.typescript.typescriptDefaults.addExtraLib(source, uri))
  }
}

export function syncWorkspaceModels(
  monaco: Monaco,
  workspace: ExerciseWorkspace,
  fileEdits: Record<string, string>,
) {
  for (const file of workspace.files) {
    if (file.content === null || file.content === undefined) {
      continue
    }

    const language = monacoLanguageForPath(file.path, workspace.language)
    const uri = workspaceUri(monaco, file.path)
    const content = fileEdits[file.path] ?? file.content
    const existingModel = monaco.editor.getModel(uri)

    if (!existingModel) {
      monaco.editor.createModel(content, language, uri)
      continue
    }

    if (existingModel.getLanguageId() !== language) {
      monaco.editor.setModelLanguage(existingModel, language)
    }

    if (existingModel.getValue() !== content) {
      existingModel.setValue(content)
    }
  }
}

export function shouldUseBackendLanguageService(file: ExerciseWorkspaceFile | null) {
  if (!file?.editable) {
    return false
  }

  const path = file.path.toLowerCase()
  return path.endsWith('.cs') || path.endsWith('.swift')
}

export function shouldUseBackendCSharpLanguageService(file: ExerciseWorkspaceFile | null) {
  return Boolean(file?.editable && file.path.toLowerCase().endsWith('.cs'))
}

export function isTypeScriptRuntime(workspace: ExerciseWorkspace) {
  return workspace.runtime.toLowerCase() === 'node-typescript'
}
