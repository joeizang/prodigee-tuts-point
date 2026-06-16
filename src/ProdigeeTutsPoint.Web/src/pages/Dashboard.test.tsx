import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { Dashboard } from './Dashboard'

describe('Dashboard', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async (input: RequestInfo | URL) => {
        const url = input.toString()

        if (url.includes('/api/curriculum/tracks')) {
          return jsonResponse([
            {
              id: 'csharp',
              title: 'C# Language',
              slug: 'csharp',
              description: 'Project-backed mastery of modern C#.',
              language: 'CSharp',
            },
          ])
        }

        if (url.includes('/api/curriculum/projects/wordfreq-csharp/milestones/pure-word-counting-core/theory-cluster')) {
          return jsonResponse({
            projectId: 'wordfreq-csharp',
            milestoneId: 'pure-word-counting-core',
            title: 'Pure Word Counting Core',
            summary: 'Study the pure core.',
            items: [
              {
                lessonId: 'text-as-data-csharp',
                title: 'Text as Data in C#',
                summary: 'Study strings, chars, and immutability.',
                sources: [
                  {
                    id: 'text-as-data-csharp:csharp-12-in-a-nutshell:string-char-immutability-indexing',
                    bookId: 'csharp-12-in-a-nutshell',
                    bookTitle: 'C# 12 in a Nutshell',
                    chapter: 'Strings and Characters',
                    topic: 'string, char, immutability, indexing',
                    usage: 'QualityAnchor',
                  },
                ],
              },
            ],
          })
        }

        if (url.includes('/api/learner/diagnostics')) {
          return jsonResponse(null)
        }

        if (url.includes('/api/learner/mastery/concepts')) {
          return jsonResponse([])
        }

        if (url.includes('/api/learner/summary')) {
          return jsonResponse({
            reviewDueCount: 0,
            studyStreakDays: 0,
            totalStudySeconds: 0,
            milestonesCompleted: 0,
            milestoneCount: 1,
            exercisesPassed: 0,
            exerciseCount: 10,
            reliableConcepts: 0,
            conceptCount: 6,
            gamificationPolicy: 'Private personal progress only.',
          })
        }

        return jsonResponse({})
      }),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders theory cluster lessons and source anchors as study links', async () => {
    render(
      <MemoryRouter>
        <Dashboard profile={{ id: 'default-profile', displayName: 'Default Profile' }} />
      </MemoryRouter>,
    )

    const lesson = await screen.findByRole('link', { name: /text as data in c#/i })
    expect(lesson).toHaveAttribute('href', '/lessons/text-as-data-csharp')

    const source = await screen.findByRole('link', {
      name: /c# 12 in a nutshell - strings and characters/i,
    })
    expect(source).toHaveAttribute(
      'href',
      '/sources#text-as-data-csharp:csharp-12-in-a-nutshell:string-char-immutability-indexing',
    )
  })
})

function jsonResponse(value: unknown) {
  return new Response(JSON.stringify(value), {
    headers: { 'Content-Type': 'application/json' },
    status: 200,
  })
}
