import { CheckCircle2 } from 'lucide-react'
import { useState } from 'react'
import { postJson } from '../api'
import type { Diagnostic, DiagnosticAttempt } from '../api'
import { AsyncState } from '../components/AsyncState'
import { Page } from '../components/Page'
import { useApi } from '../hooks/useApi'
import type { LocalProfile } from '../types'

export function Review({ profile }: { profile: LocalProfile }) {
  const { data: diagnostic, error, isLoading } = useApi<Diagnostic>('/api/learner/diagnostics/csharp')
  const { data: latest } = useApi<DiagnosticAttempt | null>(
    `/api/learner/diagnostics/csharp/latest?profileId=${encodeURIComponent(profile.id)}`,
  )
  const [answers, setAnswers] = useState<Record<string, string>>({})
  const [result, setResult] = useState<DiagnosticAttempt | null>(null)
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

  return (
    <Page title="Review">
      <AsyncState error={error} isLoading={isLoading} />
      {diagnostic && (
        <div className="content-stack">
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
