export type TrackSummary = {
  id: string
  title: string
  slug: string
  description: string
  language: string
}

export type TrackDetail = TrackSummary & {
  modules: ModuleSummary[]
  projects: ProjectSummary[]
}

export type ModuleSummary = {
  id: string
  title: string
  description: string
}

export type ProjectSummary = {
  id: string
  title: string
  slug: string
  description: string
  language: string
}

export type ProjectDetail = ProjectSummary & {
  trackId: string
  milestones: MilestoneSummary[]
}

export type MilestoneSummary = {
  id: string
  title: string
  summary: string
}

export type MilestoneDetail = MilestoneSummary & {
  projectId: string
  markdown: string
  lessons: LessonSummary[]
  exercises: ExerciseSummary[]
  sources: SourceReference[]
  softLocks: SoftLock[]
}

export type LessonSummary = {
  id: string
  title: string
  summary: string
}

export type LessonDetail = LessonSummary & {
  trackId: string
  markdown: string
  sources: SourceReference[]
  softLocks: SoftLock[]
}

export type ExerciseSummary = {
  id: string
  title: string
  summary: string
  language: string
}

export type ExerciseDetail = ExerciseSummary & {
  trackId: string
  kind: string
  directoryPath: string
  softLocks: SoftLock[]
}

export type ConceptDetail = {
  id: string
  trackId: string
  title: string
  description: string
}

export type SourceReference = {
  id: string
  bookId: string
  bookTitle: string
  chapter?: string
  pages?: string
  topic: string
  usage: string
}

export type SoftLock = {
  targetType: string
  targetId: string
  title: string
  reason: string
}

export type SourceBook = {
  id: string
  title: string
  author: string
  edition?: string
  publisher?: string
  ownershipStatus: string
  references: SourceReference[]
}

export type SearchResult = {
  kind: string
  id: string
  title: string
  summary: string
  path: string
  metadata?: string
}

export type NavigationItem = {
  kind: string
  label: string
  path: string
  summary: string
}

export type PersonalNote = {
  id: string
  profileId: string
  targetType: string
  targetId: string
  body: string
  updatedAt: string
}

export type Diagnostic = {
  trackId: string
  title: string
  summary: string
  questions: DiagnosticQuestion[]
}

export type DiagnosticQuestion = {
  id: string
  prompt: string
  conceptId: string
  answers: DiagnosticAnswer[]
}

export type DiagnosticAnswer = {
  id: string
  text: string
}

export type DiagnosticAttempt = {
  id: string
  trackId: string
  score: number
  maxScore: number
  recommendationLevel: string
  recommendationTargetId: string
  recommendationSummary: string
  submittedAt: string
}

export type ConceptMasterySummary = {
  conceptId: string
  score: number
  maxScore: number
  evidenceCount: number
}

export type ExerciseWorkspace = {
  exerciseId: string
  title: string
  workspacePath: string
  languageServiceMessage: string
  files: ExerciseWorkspaceFile[]
  lastStatus: string
  lastOutput: string
  lastDiagnostics: string
}

export type ExerciseWorkspaceFile = {
  path: string
  role: string
  editable: boolean
  content?: string | null
}

export type ExerciseRunResult = {
  status: string
  visiblePassed: boolean
  hiddenPassed: boolean
  timedOut: boolean
  exitCode?: number | null
  output: string
  diagnostics: string
  staticAnalysis: StaticAnalysisDiagnostic[]
}

export type StaticAnalysisDiagnostic = {
  ruleId: string
  severity: string
  message: string
  filePath: string
  line?: number | null
  column?: number | null
}

export type ExerciseRunHistory = {
  id: string
  status: string
  visiblePassed: boolean
  hiddenPassed: boolean
  timedOut: boolean
  exitCode?: number | null
  summary: string
  staticAnalysisErrorCount: number
  staticAnalysisWarningCount: number
  createdAt: string
}

export type ExerciseAssistance = {
  hints: ExerciseHint[]
  solutionAvailable: boolean
  solution?: ExerciseSolution | null
}

export type ExerciseHint = {
  id: string
  level: string
  title: string
  body: string
  used: boolean
}

export type ExerciseSolution = {
  title: string
  body: string
  code: string
}

export type AiProvider = {
  id: string
  displayName: string
  preset: string
  endpoint: string
  model: string
  secretName?: string | null
  isEnabled: boolean
}

export type AiReview = {
  id: string
  providerId: string
  providerPreset: string
  model: string
  promptVersion: string
  rubricVersion: string
  policy: string
  status: string
  score: number
  maxScore: number
  summary: string
  strengths: string[]
  risks: string[]
  nextSteps: string[]
  createdAt: string
}

export type ExerciseDiagnosticsResult = {
  diagnostics: ExerciseDiagnostic[]
  setupMessages: string[]
}

export type ExerciseDiagnostic = {
  id: string
  message: string
  severity: string
  startLineNumber: number
  startColumn: number
  endLineNumber: number
  endColumn: number
}

export type ExerciseCompletionsResult = {
  items: ExerciseCompletionItem[]
  setupMessages: string[]
}

export type ExerciseCompletionItem = {
  label: string
  insertText: string
  filterText: string
  sortText: string
  detail: string
  tags: string[]
}

export type ExerciseFormatResult = {
  content: string
  setupMessages: string[]
}

export type ExerciseHoverResult = {
  contents: string[]
  range?: ExerciseRange | null
  setupMessages: string[]
}

export type ExerciseSignatureHelpResult = {
  signatures: ExerciseSignatureItem[]
  activeSignature: number
  activeParameter: number
  setupMessages: string[]
}

export type ExerciseSignatureItem = {
  label: string
  documentation: string
  parameters: ExerciseSignatureParameter[]
}

export type ExerciseSignatureParameter = {
  label: string
  name: string
  documentation: string
}

export type ExerciseCodeActionsResult = {
  actions: ExerciseCodeActionItem[]
  setupMessages: string[]
}

export type ExerciseCodeActionItem = {
  title: string
  kind: string
  edits: ExerciseTextEdit[]
}

export type ExerciseTextEdit = ExerciseRange & {
  text: string
}

export type ExerciseRange = {
  startLineNumber: number
  startColumn: number
  endLineNumber: number
  endColumn: number
}

export async function getJson<T>(url: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(url, { signal })

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`)
  }

  return (await response.json()) as T
}

export async function putJson<T>(url: string, body: unknown, signal?: AbortSignal): Promise<T> {
  const response = await fetch(url, {
    body: JSON.stringify(body),
    headers: { 'Content-Type': 'application/json' },
    method: 'PUT',
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`)
  }

  return (await response.json()) as T
}

export async function postJson<T>(url: string, body: unknown, signal?: AbortSignal): Promise<T> {
  const response = await fetch(url, {
    body: JSON.stringify(body),
    headers: { 'Content-Type': 'application/json' },
    method: 'POST',
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`)
  }

  return (await response.json()) as T
}
