import { Link } from 'react-router-dom'
import type {
  ConceptMasterySummary,
  DiagnosticAttempt,
  LearnerSummary,
  TheoryCluster,
  TrackSummary,
} from '../api'
import { Panel } from '../components/Page'
import { Metric } from '../components/PrimitiveLists'
import { useApi } from '../hooks/useApi'
import type { LocalProfile as LocalProfileType } from '../types'

export function Dashboard({ profile }: { profile: LocalProfileType }) {
  const { data: tracks } = useApi<TrackSummary[]>('/api/curriculum/tracks')
  const { data: latestDiagnostic } = useApi<DiagnosticAttempt | null>(
    `/api/learner/diagnostics/csharp/latest?profileId=${encodeURIComponent(profile.id)}`,
  )
  const { data: mastery } = useApi<ConceptMasterySummary[]>(
    `/api/learner/mastery/concepts?profileId=${encodeURIComponent(profile.id)}`,
  )
  const { data: summary } = useApi<LearnerSummary>(
    `/api/learner/summary?profileId=${encodeURIComponent(profile.id)}`,
  )
  const { data: theoryCluster } = useApi<TheoryCluster>(
    '/api/curriculum/projects/wordfreq-csharp/milestones/pure-word-counting-core/theory-cluster',
  )
  const csharpTrack = tracks?.find((track) => track.id === 'csharp')
  const masteryLabel =
    summary
      ? `${summary.reliableConcepts}/${summary.conceptCount}`
      : '0/6'
  const diagnosticLabel = latestDiagnostic
    ? `${latestDiagnostic.score}/${latestDiagnostic.maxScore}`
    : 'Not taken'

  return (
    <section className="page-grid">
      <section className="study-header">
        <div>
          <h2>wordfreq-csharp</h2>
          <p>
            {csharpTrack?.description ??
              'Pure word counting core with six serious lessons, ten focused exercises, and project validation through tests, analysis, and AI review.'}
          </p>
        </div>
        <Link className="primary-link" to="/projects/wordfreq-csharp/milestones/pure-word-counting-core">
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
        <Panel title="Theory Cluster">
          <div className="theory-cluster-list">
            {(theoryCluster?.items ?? fallbackTheoryCluster).map((item, index) => (
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
            Start with normalization before moving into tokenization, dictionaries, and
            deterministic output.
          </p>
          <Link className="secondary-link" to="/exercises/normalize-to-lowercase">
            Open exercise
          </Link>
        </Panel>
      </section>
      <Panel title="Mastery Status">
        <div className="mastery-grid">
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

const fallbackTheoryCluster: TheoryCluster['items'] = [
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
