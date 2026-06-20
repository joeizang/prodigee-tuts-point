import { render, screen, within } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AiReviewPanel, MilestoneDetail } from './MilestoneDetail'
import { ExerciseFirstLoopPanel, TheoryClusterPanel } from '../components/TheoryClusterPanel'

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
            model: 'gemma4:31b-mlx',
            secretName: null,
            isEnabled: false,
          },
          {
            id: 'local-ollama-qwen',
            displayName: 'Local Ollama - Qwen 3.6 35B MLX',
            preset: 'LocalOllama',
            endpoint: 'http://127.0.0.1:11434/v1',
            model: 'qwen3.6:35b-mlx',
            secretName: null,
            isEnabled: false,
          },
        ]}
        reviews={[
          {
            id: 'review-1',
            providerId: 'local-ollama',
            providerPreset: 'LocalOllama',
            model: 'gemma4:31b-mlx',
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
    expect(screen.getByRole('combobox')).toHaveValue('local-ollama')
    expect(screen.getByText(/Local Ollama using gemma4:31b-mlx at http:\/\/127\.0\.0\.1:11434\/v1/)).toBeInTheDocument()
    expect(screen.getByText('82/100 - Advisory')).toBeInTheDocument()
    expect(screen.getByText('Solid advisory review.')).toBeInTheDocument()
  })
})

describe('Milestone study panels', () => {
  it('renders theory-cluster source links and exercise-first study sequencing', () => {
    render(
      <MemoryRouter>
        <TheoryClusterPanel
          cluster={{
            projectId: 'wordfreq-csharp',
            milestoneId: 'streaming-and-scale',
            title: 'Streaming and Scale',
            summary: 'Study streaming.',
            items: [
              {
                lessonId: 'streaming-large-text-input',
                title: 'Streaming Large Text Input',
                summary: 'Process text as lines.',
                sources: [
                  {
                    id: 'streaming-large-text-input:programming-csharp-12:streaming',
                    bookId: 'programming-csharp-12',
                    bookTitle: 'Programming C# 12',
                    chapter: 'Streams and Files',
                    topic: 'streaming reads',
                    usage: 'QualityAnchor',
                  },
                ],
              },
            ],
          }}
        />
        <ExerciseFirstLoopPanel
          exercises={[
            {
              id: 'stream-wordfreq-lines',
              title: 'StreamWordfreqLines',
              summary: 'Count words from streamed lines.',
            },
          ]}
          lessons={[
            {
              id: 'streaming-large-text-input',
              title: 'Streaming Large Text Input',
              summary: 'Process text as lines.',
            },
          ]}
        />
      </MemoryRouter>,
    )

    const studyLinks = screen.getByRole('heading', { name: 'Study Links' }).closest('section')!
    const exerciseLoop = screen.getByRole('heading', { name: 'Exercise-First Loop' }).closest('section')!

    expect(within(studyLinks).getByRole('link', { name: /streaming large text input/i })).toHaveAttribute(
      'href',
      '/lessons/streaming-large-text-input',
    )
    expect(within(studyLinks).getByRole('link', { name: /programming c# 12 - streams and files/i })).toHaveAttribute(
      'href',
      '/sources#streaming-large-text-input:programming-csharp-12:streaming',
    )
    expect(within(exerciseLoop).getByRole('link', { name: /streamwordfreqlines/i })).toHaveAttribute(
      'href',
      '/exercises/stream-wordfreq-lines',
    )
    expect(within(exerciseLoop).getByRole('link', { name: /streaming large text input/i })).toHaveAttribute(
      'href',
      '/lessons/streaming-large-text-input',
    )
  })

  it('renders study links and exercise-first loop before the legacy milestone lists', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async (input: RequestInfo | URL) => {
        const url = input.toString()

        if (url.includes('/theory-cluster')) {
          return jsonResponse({
            projectId: 'wordfreq-csharp',
            milestoneId: 'streaming-and-scale',
            title: 'Streaming and Scale',
            summary: 'Study streaming.',
            items: [
              {
                lessonId: 'streaming-large-text-input',
                title: 'Streaming Large Text Input',
                summary: 'Process text as lines.',
                sources: [],
              },
            ],
          })
        }

        if (url.includes('/api/ai/providers')) {
          return jsonResponse([])
        }

        if (url.includes('/api/ai/reviews')) {
          return jsonResponse([])
        }

        return jsonResponse({
          id: 'streaming-and-scale',
          projectId: 'wordfreq-csharp',
          title: 'Streaming and Scale',
          summary: 'Scale the CLI.',
          markdown: '# Streaming and Scale\n\nBody.',
          lessons: [
            {
              id: 'streaming-large-text-input',
              title: 'Streaming Large Text Input',
              summary: 'Process text as lines.',
            },
          ],
          exercises: [
            {
              id: 'stream-wordfreq-lines',
              title: 'StreamWordfreqLines',
              summary: 'Count words from lines.',
              language: 'CSharp',
            },
          ],
          sources: [],
          softLocks: [],
        })
      }),
    )

    render(
      <MemoryRouter initialEntries={['/projects/wordfreq-csharp/milestones/streaming-and-scale']}>
        <Routes>
          <Route
            path="/projects/:projectId/milestones/:milestoneId"
            element={<MilestoneDetail profile={{ id: 'default-profile', displayName: 'Default Profile' }} />}
          />
        </Routes>
      </MemoryRouter>,
    )

    await screen.findByRole('heading', { name: 'Study Links' })
    const headings = screen.getAllByRole('heading').map((heading) => heading.textContent)

    expect(headings.indexOf('Study Links')).toBeLessThan(headings.indexOf('Required Lessons'))
    expect(headings.indexOf('Exercise-First Loop')).toBeLessThan(headings.indexOf('Required Lessons'))
    expect(headings.indexOf('Exercise-First Loop')).toBeLessThan(headings.indexOf('Focused Exercises'))
  })
})

afterEach(() => {
  vi.unstubAllGlobals()
})

function jsonResponse(value: unknown) {
  return new Response(JSON.stringify(value), {
    headers: { 'Content-Type': 'application/json' },
    status: 200,
  })
}
