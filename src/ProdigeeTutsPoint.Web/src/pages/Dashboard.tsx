import { Link } from 'react-router-dom'
import type {
  ConceptMasterySummary,
  DiagnosticAttempt,
  LearnerSummary,
  MilestoneDetail,
  ProjectDetail,
  TheoryCluster,
  TrackDetail,
} from '../api'
import { Panel } from '../components/Page'
import { Metric } from '../components/PrimitiveLists'
import { useApi } from '../hooks/useApi'
import { useActiveLearning } from '../state/ActiveLearningContext'
import type { LocalProfile as LocalProfileType } from '../types'

export function Dashboard({ profile }: { profile: LocalProfileType }) {
  const { selectedTrackId } = useActiveLearning()
  const { data: track } = useApi<TrackDetail>(`/api/curriculum/tracks/${selectedTrackId}`)
  const primaryProject = track?.projects?.[0] ?? null
  const { data: project } = useApi<ProjectDetail>(
    primaryProject ? `/api/curriculum/projects/${primaryProject.id}` : null,
  )
  const primaryMilestone = project?.milestones?.[0] ?? null
  const { data: latestDiagnostic } = useApi<DiagnosticAttempt | null>(
    selectedTrackId === 'csharp'
      ? `/api/learner/diagnostics/csharp/latest?profileId=${encodeURIComponent(profile.id)}`
      : null,
  )
  const { data: mastery } = useApi<ConceptMasterySummary[]>(
    `/api/learner/mastery/concepts?profileId=${encodeURIComponent(profile.id)}&trackId=${encodeURIComponent(selectedTrackId)}`,
  )
  const { data: summary } = useApi<LearnerSummary>(
    `/api/learner/summary?profileId=${encodeURIComponent(profile.id)}&trackId=${encodeURIComponent(selectedTrackId)}`,
  )
  const { data: milestone } = useApi<MilestoneDetail>(
    primaryProject && primaryMilestone
      ? `/api/curriculum/projects/${primaryProject.id}/milestones/${primaryMilestone.id}`
      : null,
  )
  const { data: theoryCluster } = useApi<TheoryCluster>(
    primaryProject && primaryMilestone
      ? `/api/curriculum/projects/${primaryProject.id}/milestones/${primaryMilestone.id}/theory-cluster`
      : null,
  )
  const masteryLabel =
    summary
      ? `${summary.reliableConcepts}/${summary.conceptCount}`
      : '0/6'
  const diagnosticLabel = latestDiagnostic
    ? `${latestDiagnostic.score}/${latestDiagnostic.maxScore}`
    : 'Not taken'

  const continuePath = primaryProject && primaryMilestone
    ? `/projects/${primaryProject.id}/milestones/${primaryMilestone.id}`
    : `/tracks/${selectedTrackId}`
  const projectTitle = primaryProject?.title ?? track?.title ?? 'Selected Track'
  const projectDescription =
    primaryProject?.description ??
    track?.description ??
    'Choose a track to see its current project, theory cluster, exercises, and mastery status.'
  const nextExercise = milestone?.exercises[0] ?? null
  const theoryItems = theoryCluster?.items ?? fallbackTheoryClusterFor(selectedTrackId)

  return (
    <section className="page-grid">
      <section className="study-header">
        <div>
          <span className="active-track-eyebrow">
            Active track: {track?.title ?? formatTrackId(selectedTrackId)}
          </span>
          <h2>{projectTitle}</h2>
          <p>{projectDescription}</p>
        </div>
        <Link className="primary-link" to={continuePath}>
          Continue
        </Link>
      </section>

      <section className="status-strip" aria-label="Progress summary">
        <Metric label="Milestone" value={`${summary?.milestonesCompleted ?? 0}/${summary?.milestoneCount ?? 1}`} />
        <Metric label="Exercises" value={`${summary?.exercisesPassed ?? 0}/${summary?.exerciseCount ?? 10}`} />
        <Metric label="Diagnostic" value={diagnosticLabel} />
        <Metric label="Mastery" value={masteryLabel} />
        <Metric label="Review Due" value={`${summary?.reviewDueCount ?? 0}`} />
        <Metric label="Streak" value={`${summary?.studyStreakDays ?? 0}d`} />
      </section>

      <section className="two-column">
        <Panel title="Theory Cluster" className="scroll-panel">
          <div className="theory-cluster-list panel-scroll-region">
            {theoryItems.map((item, index) => (
              <article className="theory-cluster-row" key={item.lessonId}>
                <Link className="theory-lesson-link" to={`/lessons/${item.lessonId}`}>
                  <span>{String(index + 1).padStart(2, '0')}</span>
                  <strong>{item.title}</strong>
                  <small>{item.summary}</small>
                </Link>
                {item.sources.length > 0 && (
                  <div className="theory-source-links" aria-label={`Source anchors for ${item.title}`}>
                    {item.sources.map((source) => (
                      <Link key={source.id} to={`/sources#${source.id}`}>
                        {source.bookTitle}
                        {source.chapter ? ` - ${source.chapter}` : ''}
                      </Link>
                    ))}
                  </div>
                )}
              </article>
            ))}
          </div>
        </Panel>
        <Panel title="Next Exercise">
          <p className="body-copy">
            {nextExercise?.summary ??
              'Open the next focused exercise for the selected track when it is available.'}
          </p>
          {nextExercise ? (
            <Link className="secondary-link" to={`/exercises/${nextExercise.id}`}>
              Open exercise
            </Link>
          ) : (
            <Link className="secondary-link" to={`/tracks/${selectedTrackId}`}>
              View track
            </Link>
          )}
        </Panel>
      </section>
      <Panel title="Mastery Status" className="scroll-panel">
        <div className="mastery-grid panel-scroll-region mastery-scroll-region">
          {(mastery ?? []).map((concept) => (
            <article className="mastery-card" key={concept.conceptId}>
              <strong>{concept.title}</strong>
              <span>{concept.status}</span>
              <small>
                {concept.score}/{concept.maxScore || 1} from {concept.evidenceCount} evidence item(s)
              </small>
            </article>
          ))}
        </div>
        <p className="body-copy">
          {summary?.gamificationPolicy ?? 'Private personal progress only. No social comparison is recorded or displayed.'}
        </p>
      </Panel>
    </section>
  )
}

function formatTrackId(trackId: string) {
  return trackId
    .split('-')
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ')
}

function fallbackTheoryClusterFor(trackId: string): TheoryCluster['items'] {
  if (trackId === 'python') {
    return [
      {
        lessonId: 'text-as-data-python',
        title: 'Text as Data in Python',
        summary: 'Learn names, immutable strings, return values, whitespace contracts, tests, and editor feedback.',
        sources: [],
      },
    ]
  }

  if (trackId === 'typescript') {
    return [
      {
        lessonId: 'typed-command-boundaries',
        title: 'Typed Command Boundaries',
        summary: 'Convert raw command inputs into typed server-side TypeScript request objects.',
        sources: [],
      },
    ]
  }

  return fallbackCSharpTheoryCluster
}

const fallbackCSharpTheoryCluster: TheoryCluster['items'] = [
  {
    lessonId: 'text-as-data-csharp',
    title: 'Text as Data in C#',
    summary: 'Start with strings, chars, immutability, indexing, and the milestone text boundary.',
    sources: [],
  },
  {
    lessonId: 'normalization-and-tokenization',
    title: 'Normalization and Tokenization',
    summary: 'Study the ASCII-first rule that turns raw input into stable words.',
    sources: [],
  },
  {
    lessonId: 'dictionaries-as-frequency-maps',
    title: 'Dictionaries as Frequency Maps',
    summary: 'Use Dictionary<string, int> as the core model for word counts.',
    sources: [],
  },
  {
    lessonId: 'deterministic-ordering',
    title: 'Deterministic Ordering',
    summary: 'Make output stable with count-descending and word-ascending ordering.',
    sources: [],
  },
  {
    lessonId: 'testing-pure-functions-xunit',
    title: 'Testing Pure Functions with xUnit',
    summary: 'Build focused tests around deterministic behavior and edge cases.',
    sources: [],
  },
  {
    lessonId: 'designing-small-core-api',
    title: 'Designing a Small Core API',
    summary: 'Shape a reusable analyzer core before CLI and file-system concerns.',
    sources: [],
  },
]
