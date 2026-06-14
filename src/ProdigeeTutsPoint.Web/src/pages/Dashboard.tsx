import { Link } from 'react-router-dom'
import type { ConceptMasterySummary, DiagnosticAttempt, TrackSummary } from '../api'
import { Panel } from '../components/Page'
import { List, Metric } from '../components/PrimitiveLists'
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
  const csharpTrack = tracks?.find((track) => track.id === 'csharp')
  const masteredConcepts = mastery?.filter((concept) => concept.maxScore > 0 && concept.score === concept.maxScore)
    .length
  const attemptedConcepts = mastery?.filter((concept) => concept.evidenceCount > 0).length
  const masteryLabel =
    masteredConcepts !== undefined && attemptedConcepts !== undefined
      ? `${masteredConcepts}/${Math.max(attemptedConcepts, 6)}`
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
        <Metric label="Milestone" value="0/1" />
        <Metric label="Exercises" value="0/10" />
        <Metric label="Diagnostic" value={diagnosticLabel} />
        <Metric label="Mastery" value={masteryLabel} />
      </section>

      <section className="two-column">
        <Panel title="Theory Cluster">
          <List
            items={[
              'Text as Data in C#',
              'Normalization and Tokenization',
              'Dictionaries as Frequency Maps',
              'Deterministic Ordering',
              'Testing Pure Functions with xUnit',
              'Designing a Small Core API',
            ]}
          />
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
    </section>
  )
}
