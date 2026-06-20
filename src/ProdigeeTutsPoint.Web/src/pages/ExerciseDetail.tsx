import { useCallback, useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getJson, postJson, putJson } from '../api'
import type {
  ExerciseAssistance,
  ExerciseDetail as ExerciseDetailModel,
  ExerciseHint,
  ExerciseRunResult,
  ExerciseRunHistory,
  ExerciseSolution,
  ExerciseWorkspace,
  StaticAnalysisDiagnostic,
} from '../api'
import { AsyncState } from '../components/AsyncState'
import { MarkdownText } from '../components/MarkdownText'
import { NotesPanel } from '../components/NotesPanel'
import { Page, Panel } from '../components/Page'
import { SoftLockNotice } from '../components/SoftLockNotice'
import {
  ExerciseWorkspacePanel,
} from '../features/exercises/ExerciseWorkspacePanel'
import { selectActiveWorkspaceFile } from '../features/exercises/workspaceFiles'
import { useApi } from '../hooks/useApi'
import { useStudyTime } from '../hooks/useStudyTime'
import type { LocalProfile, Theme } from '../types'

export function ExerciseDetail({ profile, theme }: { profile: LocalProfile; theme: Theme }) {
  const { exerciseId = 'normalize-to-lowercase' } = useParams()
  useStudyTime({ profileId: profile.id, targetType: 'exercise', targetId: exerciseId })
  const { data: exercise, error, isLoading } = useApi<ExerciseDetailModel>(
    `/api/curriculum/exercises/${exerciseId}`,
  )
  const {
    data: workspace,
    error: workspaceError,
    isLoading: workspaceLoading,
  } = useApi<ExerciseWorkspace>(
    `/api/exercises/${exerciseId}/workspace?profileId=${encodeURIComponent(profile.id)}`,
  )
  const { data: initialAssistance } = useApi<ExerciseAssistance>(
    `/api/exercises/${exerciseId}/assistance?profileId=${encodeURIComponent(profile.id)}`,
  )
  const { data: initialAttempts } = useApi<ExerciseRunHistory[]>(
    `/api/exercises/${exerciseId}/attempts?profileId=${encodeURIComponent(profile.id)}`,
  )
  const [selectedPath, setSelectedPath] = useState<string | null>(null)
  const [fileEdits, setFileEdits] = useState<Record<string, string>>({})
  const [runResult, setRunResult] = useState<ExerciseRunResult | null>(null)
  const [assistanceOverride, setAssistanceOverride] = useState<ExerciseAssistance | null>(null)
  const [attemptsOverride, setAttemptsOverride] = useState<ExerciseRunHistory[] | null>(null)
  const [isRunning, setIsRunning] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isUnlockingSolution, setIsUnlockingSolution] = useState(false)

  const activeFile = selectActiveWorkspaceFile(workspace?.files ?? [], selectedPath)
  const activeContent = activeFile ? fileEdits[activeFile.path] ?? activeFile.content ?? '' : ''
  const assistance = assistanceOverride ?? initialAssistance ?? null
  const attempts = attemptsOverride ?? initialAttempts ?? []

  useEffect(() => {
    setSelectedPath(null)
    setFileEdits({})
    setRunResult(null)
    setAssistanceOverride(null)
    setAttemptsOverride(null)
  }, [exerciseId])

  const saveActiveFile = useCallback(async () => {
    if (!activeFile?.editable) {
      return
    }

    setIsSaving(true)
    try {
      const savedWorkspace = await putJson<ExerciseWorkspace>(
        `/api/exercises/${exerciseId}/workspace/files?profileId=${encodeURIComponent(profile.id)}`,
        { path: activeFile.path, content: activeContent },
      )
      setFileEdits((current) => ({ ...current, [activeFile.path]: activeContent }))
      setRunResult(null)
      if (savedWorkspace.lastDiagnostics) {
        setRunResult({
          status: savedWorkspace.lastStatus,
          visiblePassed: false,
          hiddenPassed: false,
          timedOut: false,
          exitCode: null,
          output: savedWorkspace.lastOutput,
          diagnostics: savedWorkspace.lastDiagnostics,
          staticAnalysis: [],
        })
      }
    } finally {
      setIsSaving(false)
    }
  }, [activeContent, activeFile?.editable, activeFile?.path, exerciseId, profile.id])

  const refreshAssistance = useCallback(async () => {
    const next = await getJson<ExerciseAssistance>(
      `/api/exercises/${exerciseId}/assistance?profileId=${encodeURIComponent(profile.id)}`,
    )
    setAssistanceOverride(next)
  }, [exerciseId, profile.id])

  const refreshAttempts = useCallback(async () => {
    const next = await getJson<ExerciseRunHistory[]>(
      `/api/exercises/${exerciseId}/attempts?profileId=${encodeURIComponent(profile.id)}`,
    )
    setAttemptsOverride(next)
  }, [exerciseId, profile.id])

  const runExercise = useCallback(async () => {
    if (!workspace) {
      return
    }

    setIsRunning(true)
    try {
      const result = await postJson<ExerciseRunResult>(`/api/exercises/${exerciseId}/run`, {
        profileId: profile.id,
        files: workspace.files
          .filter((file) => file.editable)
          .map((file) => ({
            path: file.path,
            content: fileEdits[file.path] ?? file.content ?? '',
          })),
      })
      setRunResult(result)
      await refreshAttempts()
      await refreshAssistance()
    } finally {
      setIsRunning(false)
    }
  }, [exerciseId, fileEdits, profile.id, refreshAssistance, refreshAttempts, workspace])

  const useHint = useCallback(
    async (hint: ExerciseHint) => {
      const used = await postJson<ExerciseHint>(
        `/api/exercises/${exerciseId}/hints/${hint.id}/use`,
        { profileId: profile.id },
      )
      setAssistanceOverride((current) =>
        (current ?? assistance)
          ? {
              ...(current ?? assistance)!,
              hints: (current ?? assistance)!.hints.map((candidate) =>
                candidate.id === used.id ? used : candidate,
              ),
            }
          : current,
      )
    },
    [assistance, exerciseId, profile.id],
  )

  const unlockSolution = useCallback(async () => {
    setIsUnlockingSolution(true)
    try {
      const solution = await postJson<ExerciseSolution>(
        `/api/exercises/${exerciseId}/solution/unlock`,
        { profileId: profile.id, reason: 'Intentional learner unlock' },
      )
      setAssistanceOverride((current) =>
        current ?? assistance
          ? { ...(current ?? assistance)!, solutionAvailable: true, solution }
          : { hints: [], solutionAvailable: true, solution },
      )
    } finally {
      setIsUnlockingSolution(false)
    }
  }, [assistance, exerciseId, profile.id])

  useEffect(() => {
    const onSave = () => {
      void saveActiveFile()
    }
    const onRun = () => {
      void runExercise()
    }

    window.addEventListener('prodigee:save', onSave)
    window.addEventListener('prodigee:run', onRun)
    return () => {
      window.removeEventListener('prodigee:save', onSave)
      window.removeEventListener('prodigee:run', onRun)
    }
  }, [runExercise, saveActiveFile])

  return (
    <Page wide title={exercise?.title ?? 'Exercise'}>
      <AsyncState error={error} isLoading={isLoading} />
      <AsyncState error={workspaceError} isLoading={workspaceLoading} />
      {exercise && (
        <div className="content-stack exercise-stack">
          <SoftLockNotice locks={exercise.softLocks} title="Recommended before this exercise" />
          <p className="body-copy">{exercise.summary}</p>
          {workspace && (
            <ExerciseWorkspacePanel
              activeContent={activeContent}
              activeFile={activeFile}
              fileEdits={fileEdits}
              profileId={profile.id}
              isRunning={isRunning}
              isSaving={isSaving}
              runResult={runResult}
              theme={theme}
              workspace={workspace}
              onFileChange={(path, content) =>
                setFileEdits((current) => ({ ...current, [path]: content }))
              }
              onRun={runExercise}
              onSave={saveActiveFile}
              onSelectFile={setSelectedPath}
            />
          )}
          <ExerciseAssistancePanel
            assistance={assistance}
            language={exercise.language}
            isUnlockingSolution={isUnlockingSolution}
            onUnlockSolution={unlockSolution}
            onUseHint={useHint}
          />
          <StaticAnalysisPanel diagnostics={runResult?.staticAnalysis ?? []} />
          <AttemptHistoryPanel attempts={attempts} />
          <NotesPanel profile={profile} targetId={exercise.id} targetType="exercise" />
        </div>
      )}
    </Page>
  )
}

export function ExerciseAssistancePanel({
  assistance,
  language = 'csharp',
  isUnlockingSolution,
  onUnlockSolution,
  onUseHint,
}: {
  assistance: ExerciseAssistance | null
  language?: string
  isUnlockingSolution: boolean
  onUnlockSolution: () => void
  onUseHint: (hint: ExerciseHint) => void
}) {
  if (!assistance) {
    return null
  }

  return (
    <Panel title="Hints and Model Solution">
      <div className="hint-grid">
        {assistance.hints.map((hint) => (
          <article className={hint.used ? 'hint-card used' : 'hint-card'} key={hint.id}>
            <div>
              <strong>{hint.title}</strong>
              <small>{hint.level}</small>
            </div>
            {hint.used ? (
              <p>{hint.body}</p>
            ) : (
              <button className="secondary-action" type="button" onClick={() => onUseHint(hint)}>
                Reveal hint
              </button>
            )}
          </article>
        ))}
      </div>

      <div className="solution-panel">
        {assistance.solutionAvailable && assistance.solution ? (
          <>
            <strong>{assistance.solution.title}</strong>
            <p>{assistance.solution.body}</p>
            <MarkdownText markdown={`\`\`\`${language}\n${assistance.solution.code}\n\`\`\``} />
          </>
        ) : (
          <button
            className="secondary-action"
            disabled={isUnlockingSolution}
            type="button"
            onClick={onUnlockSolution}
          >
            {isUnlockingSolution ? 'Unlocking...' : 'Unlock model solution'}
          </button>
        )}
      </div>
    </Panel>
  )
}

export function StaticAnalysisPanel({ diagnostics }: { diagnostics: StaticAnalysisDiagnostic[] }) {
  return (
    <Panel title="Static Analysis">
      {diagnostics.length === 0 ? (
        <p className="body-copy">No static-analysis diagnostics from the latest run.</p>
      ) : (
        <div className="analysis-list">
          {diagnostics.map((diagnostic) => (
            <article className="analysis-row" key={`${diagnostic.ruleId}-${diagnostic.filePath}-${diagnostic.line}-${diagnostic.column}`}>
              <strong>{diagnostic.ruleId}</strong>
              <span>{diagnostic.severity}</span>
              <p>{diagnostic.message}</p>
              <small>
                {diagnostic.filePath}
                {diagnostic.line ? `:${diagnostic.line}:${diagnostic.column ?? 1}` : ''}
              </small>
            </article>
          ))}
        </div>
      )}
    </Panel>
  )
}

export function AttemptHistoryPanel({ attempts }: { attempts: ExerciseRunHistory[] }) {
  return (
    <Panel title="Attempt History">
      {attempts.length === 0 ? (
        <p className="body-copy">No runs recorded yet.</p>
      ) : (
        <div className="attempt-list">
          {attempts.map((attempt) => (
            <article className="attempt-row" key={attempt.id}>
              <strong>{attempt.status}</strong>
              <span>{new Date(attempt.createdAt).toLocaleString()}</span>
              <p>{attempt.summary}</p>
              <small>
                Static analysis: {attempt.staticAnalysisErrorCount} error(s),{' '}
                {attempt.staticAnalysisWarningCount} warning(s)
              </small>
            </article>
          ))}
        </div>
      )}
    </Panel>
  )
}
