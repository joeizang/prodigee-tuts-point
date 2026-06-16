import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { postJson } from '../api'
import type { AiProvider, AiReview, MilestoneDetail as MilestoneDetailModel } from '../api'
import { AsyncState } from '../components/AsyncState'
import { MarkdownText } from '../components/MarkdownText'
import { NotesPanel } from '../components/NotesPanel'
import { Page, Panel } from '../components/Page'
import { LinkedList, SourceList } from '../components/PrimitiveLists'
import { SoftLockNotice } from '../components/SoftLockNotice'
import { useApi } from '../hooks/useApi'
import { useStudyTime } from '../hooks/useStudyTime'
import type { LocalProfile } from '../types'

export function MilestoneDetail({ profile }: { profile: LocalProfile }) {
  const { milestoneId = 'pure-word-counting-core', projectId = 'wordfreq-csharp' } = useParams()
  useStudyTime({ profileId: profile.id, targetType: 'milestone', targetId: milestoneId })
  const { data: milestone, error, isLoading } = useApi<MilestoneDetailModel>(
    `/api/curriculum/projects/${projectId}/milestones/${milestoneId}`,
  )
  const { data: providers } = useApi<AiProvider[]>('/api/ai/providers')
  const { data: initialReviews } = useApi<AiReview[]>(
    `/api/ai/reviews?profileId=${encodeURIComponent(profile.id)}&projectId=${encodeURIComponent(projectId)}&milestoneId=${encodeURIComponent(milestoneId)}`,
  )

  return (
    <Page title={milestone?.title ?? 'Milestone'}>
      <AsyncState error={error} isLoading={isLoading} />
      {milestone && (
        <div className="content-stack">
          <SoftLockNotice locks={milestone.softLocks} title="Recommended before completion" />
          <MarkdownText markdown={milestone.markdown} omitFirstHeading />
          <Panel title="Required Lessons">
            <LinkedList items={milestone.lessons} pathPrefix="/lessons" />
          </Panel>
          <Panel title="Focused Exercises">
            <LinkedList items={milestone.exercises} pathPrefix="/exercises" />
          </Panel>
          <AiReviewPanel
            milestoneId={milestone.id}
            profileId={profile.id}
            projectId={milestone.projectId}
            providers={providers ?? []}
            reviews={initialReviews ?? []}
          />
          <SourceList sources={milestone.sources} />
          <NotesPanel profile={profile} targetId={milestone.id} targetType="milestone" />
        </div>
      )}
    </Page>
  )
}

export function AiReviewPanel({
  milestoneId,
  profileId,
  projectId,
  providers,
  reviews,
}: {
  milestoneId: string
  profileId: string
  projectId: string
  providers: AiProvider[]
  reviews: AiReview[]
}) {
  const [selectedProviderId, setSelectedProviderId] = useState(providers[0]?.id ?? 'local-ollama')
  const [providerOverride, setProviderOverride] = useState<AiProvider[] | null>(null)
  const [reviewOverride, setReviewOverride] = useState<AiReview[]>([])
  const [status, setStatus] = useState('')
  const providerState = providerOverride ?? providers
  const reviewState = [...reviewOverride, ...reviews.filter((review) => reviewOverride.every((item) => item.id !== review.id))]
  const selectedProvider = providerState.find((provider) => provider.id === selectedProviderId)

  const testProvider = async () => {
    if (!selectedProvider) {
      return
    }

    setStatus('Testing provider...')
    const result = await postJson<{ providerId: string; success: boolean; message: string }>(
      `/api/ai/providers/${selectedProvider.id}/test`,
      {},
    )
    setStatus(result.message)
    setProviderOverride(
      providerState.map((provider) =>
        provider.id === result.providerId ? { ...provider, isEnabled: result.success } : provider,
      ),
    )
  }

  const runReview = async () => {
    if (!selectedProvider) {
      return
    }

    setStatus('Running advisory AI review...')
    const review = await postJson<AiReview>('/api/ai/reviews', {
      profileId,
      projectId,
      milestoneId,
      providerId: selectedProvider.id,
    })
    setReviewOverride((current) => [review, ...current])
    setStatus('Advisory review stored.')
  }

  return (
    <Panel title="AI Review">
      <div className="ai-review-controls">
        <select value={selectedProviderId} onChange={(event) => setSelectedProviderId(event.target.value)}>
          {providerState.map((provider) => (
            <option key={provider.id} value={provider.id}>
              {provider.displayName} - {provider.model}
            </option>
          ))}
        </select>
        <button className="secondary-action" type="button" onClick={testProvider}>
          Test provider
        </button>
        <button
          className="primary-action"
          disabled={!selectedProvider?.isEnabled}
          type="button"
          onClick={runReview}
        >
          Run advisory review
        </button>
      </div>
      {selectedProvider && (
        <p className="body-copy">
          {selectedProvider.preset} at {selectedProvider.endpoint}. Secret:{' '}
          {selectedProvider.secretName ?? 'not required'}. Status:{' '}
          {selectedProvider.isEnabled ? 'enabled' : 'not enabled'}.
        </p>
      )}
      {status && <p className="body-copy">{status}</p>}
      <div className="attempt-list">
        {reviewState.map((review) => (
          <article className="attempt-row" key={review.id}>
            <strong>
              {review.score}/{review.maxScore} - {review.policy}
            </strong>
            <span>
              {review.providerPreset} / {review.model}
            </span>
            <p>{review.summary}</p>
            {review.nextSteps.length > 0 && <small>{review.nextSteps.join(' ')}</small>}
          </article>
        ))}
      </div>
    </Panel>
  )
}
