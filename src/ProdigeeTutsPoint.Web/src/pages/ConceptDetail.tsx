import { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import type { ConceptDetail as ConceptDetailModel } from '../api'
import { AsyncState } from '../components/AsyncState'
import { NotesPanel } from '../components/NotesPanel'
import { Page, Panel } from '../components/Page'
import { useApi } from '../hooks/useApi'
import { useActiveLearning } from '../state/ActiveLearningContext'
import type { LocalProfile } from '../types'

export function ConceptDetail({ profile }: { profile: LocalProfile }) {
  const { syncTrack } = useActiveLearning()
  const { conceptId = '' } = useParams()
  const { data: concept, error, isLoading } = useApi<ConceptDetailModel>(
    `/api/curriculum/concepts/${conceptId}`,
  )

  useEffect(() => {
    if (concept?.trackId) {
      syncTrack(concept.trackId)
    }
  }, [concept?.trackId, syncTrack])

  return (
    <Page title={concept?.title ?? 'Concept'}>
      <AsyncState error={error} isLoading={isLoading} />
      {concept && (
        <div className="content-stack">
          <Panel title={concept.title}>
            <p className="body-copy">{concept.description}</p>
          </Panel>
          <NotesPanel profile={profile} targetId={concept.id} targetType="concept" />
        </div>
      )}
    </Page>
  )
}
