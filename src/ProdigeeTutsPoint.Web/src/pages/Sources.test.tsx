import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ActiveLearningProvider } from '../state/ActiveLearningContext'
import { Sources } from './Sources'

describe('Sources', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.stubGlobal(
      'fetch',
      vi.fn(async (input: RequestInfo | URL) => {
        const url = input.toString()

        if (url.includes('/api/curriculum/tracks/swift')) {
          return jsonResponse({
            id: 'swift',
            title: 'Swift and Server-Side Swift',
            slug: 'swift',
            description: 'Project-backed mastery of Swift.',
            language: 'Swift',
            modules: [],
            projects: [
              {
                id: 'logprobe-swift',
                title: 'logprobe-swift',
                slug: 'logprobe-swift',
                description: 'Swift log processing.',
                language: 'Swift',
              },
            ],
          })
        }

        if (url.includes('/api/curriculum/projects/logprobe-swift/milestones/swiftpm-command-boundary')) {
          return jsonResponse({
            id: 'swiftpm-command-boundary',
            projectId: 'logprobe-swift',
            title: 'SwiftPM command boundary',
            summary: 'Package boundary.',
            markdown: '',
            lessons: [],
            exercises: [],
            sources: [],
            softLocks: [],
          })
        }

        if (url.includes('/api/curriculum/projects/logprobe-swift')) {
          return jsonResponse({
            id: 'logprobe-swift',
            trackId: 'swift',
            title: 'logprobe-swift',
            slug: 'logprobe-swift',
            description: 'Swift log processing.',
            language: 'Swift',
            milestones: [
              {
                id: 'swiftpm-command-boundary',
                title: 'SwiftPM command boundary',
                summary: 'Package boundary.',
              },
            ],
          })
        }

        if (url.includes('/api/curriculum/sources')) {
          return jsonResponse([
            {
              id: 'the-swift-programming-language',
              title: 'The Swift Programming Language',
              author: 'Apple',
              edition: null,
              publisher: 'Apple',
              ownershipStatus: 'Owned',
              references: [
                {
                  id: 'swift-reference',
                  bookId: 'the-swift-programming-language',
                  bookTitle: 'The Swift Programming Language',
                  chapter: 'Concurrency',
                  pages: null,
                  topic: 'AsyncSequence and for-await processing',
                  usage: 'QualityAnchor',
                },
              ],
            },
          ])
        }

        if (url.includes('/api/learner/notes')) {
          return jsonResponse(null)
        }

        return jsonResponse([])
      }),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('requests source books for the active track', async () => {
    localStorage.setItem('prodigee.selectedTrack.default-profile', 'swift')

    render(
      <MemoryRouter>
        <ActiveLearningProvider profile={{ id: 'default-profile', displayName: 'Default Profile' }}>
          <Sources profile={{ id: 'default-profile', displayName: 'Default Profile' }} />
        </ActiveLearningProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'The Swift Programming Language' })).toBeInTheDocument()
    expect(screen.queryByText('C# 12 in a Nutshell')).not.toBeInTheDocument()

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledWith(
        '/api/curriculum/sources?trackId=swift',
        expect.objectContaining({ signal: expect.any(AbortSignal) }),
      )
    })
  })
})

function jsonResponse(value: unknown) {
  return new Response(JSON.stringify(value), {
    headers: { 'Content-Type': 'application/json' },
    status: 200,
  })
}
