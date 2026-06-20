import { createContext, useContext, useEffect, useMemo, useReducer, type ReactNode } from 'react'
import type {
  ExerciseSummary,
  LessonSummary,
  MilestoneDetail,
  MilestoneSummary,
  ProjectDetail,
  ProjectSummary,
  TrackDetail,
} from '../api'
import { useApi } from '../hooks/useApi'
import type { LocalProfile } from '../types'

const selectedTrackKey = 'prodigee.selectedTrack'

type ActiveLearningState = {
  selectedTrackId: string
}

type ActiveLearningAction =
  | { type: 'select-track'; trackId: string }
  | { type: 'sync-track'; trackId: string }

type ActiveLearningContextValue = ActiveLearningState & {
  activeTrack: TrackDetail | null
  primaryProject: ProjectSummary | null
  primaryProjectDetail: ProjectDetail | null
  primaryMilestone: MilestoneSummary | null
  primaryMilestoneDetail: MilestoneDetail | null
  primaryLesson: LessonSummary | null
  primaryExercise: ExerciseSummary | null
  projectPath: string
  milestonePath: string
  lessonPath: string
  exercisePath: string
  selectTrack: (trackId: string) => void
  syncTrack: (trackId: string) => void
}

const ActiveLearningContext = createContext<ActiveLearningContextValue | null>(null)

export function ActiveLearningProvider({
  children,
  profile,
}: {
  children: ReactNode
  profile: LocalProfile
}) {
  const [state, dispatch] = useReducer(activeLearningReducer, profile.id, (profileId) => ({
    selectedTrackId: localStorage.getItem(profileSelectedTrackKey(profileId)) ?? 'csharp',
  }))
  const { data: activeTrackResponse } = useApi<TrackDetail>(
    `/api/curriculum/tracks/${encodeURIComponent(state.selectedTrackId)}`,
  )
  const activeTrack = isTrackDetail(activeTrackResponse) ? activeTrackResponse : null
  const primaryProject = activeTrack?.projects[0] ?? null
  const { data: primaryProjectDetail } = useApi<ProjectDetail>(
    primaryProject ? `/api/curriculum/projects/${encodeURIComponent(primaryProject.id)}` : null,
  )
  const primaryMilestone = primaryProjectDetail?.milestones[0] ?? null
  const { data: primaryMilestoneDetail } = useApi<MilestoneDetail>(
    primaryProject && primaryMilestone
      ? `/api/curriculum/projects/${encodeURIComponent(primaryProject.id)}/milestones/${encodeURIComponent(primaryMilestone.id)}`
      : null,
  )
  const primaryLesson = primaryMilestoneDetail?.lessons[0] ?? null
  const primaryExercise = primaryMilestoneDetail?.exercises[0] ?? null

  useEffect(() => {
    localStorage.setItem(profileSelectedTrackKey(profile.id), state.selectedTrackId)
  }, [profile.id, state.selectedTrackId])

  const value = useMemo<ActiveLearningContextValue>(
    () => ({
      ...state,
      activeTrack: activeTrack ?? null,
      primaryProject,
      primaryProjectDetail: primaryProjectDetail ?? null,
      primaryMilestone,
      primaryMilestoneDetail: primaryMilestoneDetail ?? null,
      primaryLesson,
      primaryExercise,
      projectPath: primaryProject ? `/projects/${primaryProject.id}` : `/tracks/${state.selectedTrackId}`,
      milestonePath:
        primaryProject && primaryMilestone
          ? `/projects/${primaryProject.id}/milestones/${primaryMilestone.id}`
          : `/tracks/${state.selectedTrackId}`,
      lessonPath: primaryLesson ? `/lessons/${primaryLesson.id}` : `/tracks/${state.selectedTrackId}`,
      exercisePath: primaryExercise ? `/exercises/${primaryExercise.id}` : `/tracks/${state.selectedTrackId}`,
      selectTrack: (trackId) => dispatch({ type: 'select-track', trackId }),
      syncTrack: (trackId) => dispatch({ type: 'sync-track', trackId }),
    }),
    [
      activeTrack,
      primaryExercise,
      primaryLesson,
      primaryMilestone,
      primaryMilestoneDetail,
      primaryProject,
      primaryProjectDetail,
      state,
    ],
  )

  return <ActiveLearningContext.Provider value={value}>{children}</ActiveLearningContext.Provider>
}

export function useActiveLearning() {
  const value = useContext(ActiveLearningContext)
  if (!value) {
    throw new Error('useActiveLearning must be used inside ActiveLearningProvider.')
  }

  return value
}

function activeLearningReducer(
  state: ActiveLearningState,
  action: ActiveLearningAction,
): ActiveLearningState {
  switch (action.type) {
    case 'select-track':
    case 'sync-track':
      return state.selectedTrackId === action.trackId
        ? state
        : { ...state, selectedTrackId: action.trackId }
    default:
      return state
  }
}

function profileSelectedTrackKey(profileId: string) {
  return `${selectedTrackKey}.${profileId}`
}

function isTrackDetail(value: TrackDetail | null): value is TrackDetail {
  return Boolean(value && !Array.isArray(value) && Array.isArray(value.projects))
}
