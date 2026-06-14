import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { AiReviewPanel } from './MilestoneDetail'

describe('Milestone AI review panel', () => {
  it('keeps review disabled until the selected provider is enabled and shows stored reviews', () => {
    render(
      <AiReviewPanel
        milestoneId="pure-word-counting-core"
        profileId="default-profile"
        projectId="wordfreq-csharp"
        providers={[
          {
            id: 'local-ollama',
            displayName: 'Local Ollama',
            preset: 'LocalOllama',
            endpoint: 'http://127.0.0.1:11434/v1',
            model: 'llama3.1',
            secretName: null,
            isEnabled: false,
          },
        ]}
        reviews={[
          {
            id: 'review-1',
            providerId: 'local-ollama',
            providerPreset: 'LocalOllama',
            model: 'llama3.1',
            promptVersion: 'ai-review-v1',
            rubricVersion: 'milestone.md:abcdef123456',
            policy: 'Advisory',
            status: 'Completed',
            score: 82,
            maxScore: 100,
            summary: 'Solid advisory review.',
            strengths: ['Pure function boundary'],
            risks: [],
            nextSteps: ['Add edge-case evidence'],
            createdAt: '2026-06-14T12:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByRole('button', { name: /run advisory review/i })).toBeDisabled()
    expect(screen.getByText(/LocalOllama at http:\/\/127\.0\.0\.1:11434\/v1/)).toBeInTheDocument()
    expect(screen.getByText('82/100 - Advisory')).toBeInTheDocument()
    expect(screen.getByText('Solid advisory review.')).toBeInTheDocument()
  })
})
