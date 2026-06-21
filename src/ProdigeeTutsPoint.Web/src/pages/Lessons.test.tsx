import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ActiveLearningProvider } from '../state/ActiveLearningContext'
import { Lessons } from './Lessons'

describe('Lessons', () => {
  beforeEach(() => {
    localStorage.setItem('prodigee.selectedTrack.default-profile', 'python')
    vi.stubGlobal(
      'fetch',
      vi.fn(async (input: RequestInfo | URL) => {
        const url = input.toString()

        if (url.includes('/api/curriculum/tracks/python')) {
          return jsonResponse({
            id: 'python',
            title: 'Python and FastAPI',
            slug: 'python',
            description: 'Beginner-to-proficient Python and FastAPI.',
            language: 'Python',
            modules: [],
            projects: [
              {
                id: 'py-notes-cli',
                title: 'py-notes-cli',
                slug: 'py-notes-cli',
                description: 'Build notes.',
                language: 'Python',
              },
            ],
          })
        }

        if (url.endsWith('/api/curriculum/projects/py-notes-cli')) {
          return jsonResponse({
            id: 'py-notes-cli',
            title: 'py-notes-cli',
            slug: 'py-notes-cli',
            description: 'Build notes.',
            language: 'Python',
            trackId: 'python',
            milestones: [
              {
                id: 'py-notes-python-first-contact',
                title: 'Python first contact',
                summary: 'Learn Python from the first syntax.',
              },
              {
                id: 'py-notes-control-flow-collections',
                title: 'Control flow and collection processing',
                summary: 'Process note facts and collections.',
              },
            ],
          })
        }

        if (url.includes('/api/curriculum/projects/py-notes-cli/milestones/py-notes-python-first-contact')) {
          return jsonResponse({
            id: 'py-notes-python-first-contact',
            projectId: 'py-notes-cli',
            title: 'Python first contact',
            summary: 'Learn Python from the first syntax.',
            markdown: '',
            lessons: [
              {
                id: 'python-first-names-values-tests',
                title: 'Names, Values, and First Tests',
                summary: 'Start Python from zero.',
              },
            ],
            exercises: [],
            sources: [],
            softLocks: [],
          })
        }

        if (url.includes('/api/curriculum/projects/py-notes-cli/milestones/py-notes-control-flow-collections')) {
          return jsonResponse({
            id: 'py-notes-control-flow-collections',
            projectId: 'py-notes-cli',
            title: 'Control flow and collection processing',
            summary: 'Process note facts and collections.',
            markdown: '',
            lessons: [
              {
                id: 'python-control-flow-classification',
                title: 'Control Flow and Classification',
                summary: 'Use branch order deliberately.',
              },
            ],
            exercises: [],
            sources: [],
            softLocks: [],
          })
        }

        return jsonResponse([])
      }),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    localStorage.clear()
  })

  it('renders active-track lessons as direct links', async () => {
    render(
      <MemoryRouter>
        <ActiveLearningProvider profile={{ id: 'default-profile', displayName: 'Default Profile' }}>
          <Lessons />
        </ActiveLearningProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByText('Python and FastAPI lessons, ordered by the current learning path.')).toBeInTheDocument()
    expect(await screen.findByRole('heading', { name: /python first contact/i })).toBeInTheDocument()
    expect(await screen.findByRole('heading', { name: /control flow and collection processing/i })).toBeInTheDocument()
    expect(await screen.findByRole('link', { name: /names, values, and first tests/i })).toHaveAttribute(
      'href',
      '/lessons/python-first-names-values-tests',
    )
    expect(await screen.findByRole('link', { name: /control flow and classification/i })).toHaveAttribute(
      'href',
      '/lessons/python-control-flow-classification',
    )
  })
})

function jsonResponse(value: unknown) {
  return new Response(JSON.stringify(value), {
    headers: { 'Content-Type': 'application/json' },
    status: 200,
  })
}
