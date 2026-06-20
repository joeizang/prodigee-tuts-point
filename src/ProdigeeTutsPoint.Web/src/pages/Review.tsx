import { CheckCircle2 } from 'lucide-react'
import { useState } from 'react'
import { postJson } from '../api'
import type { Diagnostic, DiagnosticAttempt, ReviewCard } from '../api'
import { AsyncState } from '../components/AsyncState'
import { Page, Panel } from '../components/Page'
import { useApi } from '../hooks/useApi'
import { useStudyTime } from '../hooks/useStudyTime'
import { useActiveLearning } from '../state/ActiveLearningContext'
import type { LocalProfile } from '../types'

export function Review({ profile }: { profile: LocalProfile }) {
  const { selectedTrackId, activeTrack } = useActiveLearning()
  const diagnosticAvailable = selectedTrackId === 'csharp'
  useStudyTime({ profileId: profile.id, targetType: 'review', targetId: selectedTrackId })
  const { data: diagnostic, error, isLoading } = useApi<Diagnostic>(
    diagnosticAvailable ? '/api/learner/diagnostics/csharp' : null,
  )
  const { data: latest } = useApi<DiagnosticAttempt | null>(
    diagnosticAvailable
      ? `/api/learner/diagnostics/csharp/latest?profileId=${encodeURIComponent(profile.id)}`
      : null,
  )
  const { data: initialCards } = useApi<ReviewCard[]>(
    `/api/learner/review/cards?profileId=${encodeURIComponent(profile.id)}&trackId=${encodeURIComponent(selectedTrackId)}`,
  )
  const [answers, setAnswers] = useState<Record<string, string>>({})
  const [result, setResult] = useState<DiagnosticAttempt | null>(null)
  const [cardsOverride, setCardsOverride] = useState<ReviewCard[] | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const submitDiagnostic = async () => {
    if (!diagnostic) {
      return
    }

    setIsSubmitting(true)
    try {
      const response = await postJson<DiagnosticAttempt>('/api/learner/diagnostics/csharp/attempts', {
        profileId: profile.id,
        answers: diagnostic.questions.map((question) => ({
          questionId: question.id,
          answerId: answers[question.id] ?? '',
        })),
      })
      setResult(response)
    } finally {
      setIsSubmitting(false)
    }
  }

  const visibleResult = result ?? latest
  const cards = cardsOverride ?? initialCards ?? []

  const rateCard = async (card: ReviewCard, rating: string) => {
    await postJson(`/api/learner/review/cards/${card.id}/attempts`, {
      profileId: profile.id,
      rating,
    })
    setCardsOverride((current) =>
      (current ?? cards).map((candidate) =>
        candidate.id === card.id
          ? { ...candidate, lastReviewedAt: new Date().toISOString(), isDue: rating === 'again' }
          : candidate,
      ),
    )
  }

  return (
    <Page title="Review">
      <AsyncState error={error} isLoading={isLoading} />
      {!diagnosticAvailable && (
        <Panel title={`${activeTrack?.title ?? selectedTrackId} review`}>
          <p className="body-copy">
            A diagnostic has not been authored for this track yet. Review cards from completed
            exercises still appear here when they become due.
          </p>
        </Panel>
      )}
      {diagnostic && (
        <div className="content-stack">
          <Panel title="Due Review Cards">
            <div className="review-card-list">
              {cards.filter((card) => card.isDue).length === 0 ? (
                <p className="body-copy">No cards are due right now.</p>
              ) : (
                cards
                  .filter((card) => card.isDue)
                  .map((card) => (
                    <article className="review-card" key={card.id}>
                      <strong>{card.prompt}</strong>
                      <p>{card.answer}</p>
                      <small>{card.conceptId}</small>
                      <div className="review-rating-row">
                        {['again', 'hard', 'good', 'easy'].map((rating) => (
                          <button
                            className="secondary-action"
                            key={rating}
                            type="button"
                            onClick={() => void rateCard(card, rating)}
                          >
                            {rating}
                          </button>
                        ))}
                      </div>
                    </article>
                  ))
              )}
            </div>
          </Panel>
          <p className="body-copy">{diagnostic.summary}</p>
          {visibleResult && (
            <section className="diagnostic-result">
              <strong>
                {visibleResult.score}/{visibleResult.maxScore} - {visibleResult.recommendationLevel}
              </strong>
              <span>{visibleResult.recommendationSummary}</span>
            </section>
          )}
          <div className="diagnostic-list">
            {diagnostic.questions.map((question) => (
              <fieldset className="diagnostic-question" key={question.id}>
                <legend>{question.prompt}</legend>
                {question.answers.map((answer) => (
                  <label key={answer.id}>
                    <input
                      checked={answers[question.id] === answer.id}
                      name={question.id}
                      type="radio"
                      value={answer.id}
                      onChange={() =>
                        setAnswers((current) => ({ ...current, [question.id]: answer.id }))
                      }
                    />
                    <span>{answer.text}</span>
                  </label>
                ))}
              </fieldset>
            ))}
          </div>
          <button
            className="primary-action"
            disabled={isSubmitting}
            type="button"
            onClick={submitDiagnostic}
          >
            <CheckCircle2 size={18} />
            <span>{isSubmitting ? 'Submitting...' : 'Submit diagnostic'}</span>
          </button>
        </div>
      )}
    </Page>
  )
}
