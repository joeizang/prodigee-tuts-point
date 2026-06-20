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
          if (url.includes('/api/curriculum/tracks/typescript')) {
            return jsonResponse({
              id: 'typescript',
              title: 'TypeScript and Node.js Servers',
              slug: 'typescript',
              description: 'Project-backed mastery of TypeScript.',
              language: 'TypeScript',
              modules: [],
              projects: [
                {
                  id: 'logprobe-typescript',
                  title: 'logprobe-typescript',
                  slug: 'logprobe-typescript',
                  description: 'Build a typed Node log investigation CLI.',
                  language: 'TypeScript',
                },
              ],
            })
          }

          if (url.includes('/api/curriculum/tracks/csharp')) {
            return jsonResponse({
              id: 'csharp',
              title: 'C# Language',
              slug: 'csharp',
              description: 'Project-backed mastery of modern C#.',
              language: 'CSharp',
              modules: [],
              projects: [
                {
                  id: 'wordfreq-csharp',
                  title: 'wordfreq-csharp',
                  slug: 'wordfreq-csharp',
                  description: 'Pure word counting core.',
                  language: 'CSharp',
                },
              ],
            })
          }

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

        if (url.includes('/api/curriculum/projects/logprobe-typescript/milestones/logprobe-typed-command-boundary/theory-cluster')) {
          return jsonResponse({
            projectId: 'logprobe-typescript',
            milestoneId: 'logprobe-typed-command-boundary',
            title: 'Typed command boundary',
            summary: 'Study the typed boundary.',
            items: [
              {
                lessonId: 'typed-command-boundaries',
                title: 'Typed Command Boundaries',
                summary: 'Convert raw command inputs into typed request objects.',
                sources: [],
              },
            ],
          })
        }

        if (url.includes('/api/curriculum/projects/logprobe-typescript/milestones/logprobe-typed-command-boundary')) {
          return jsonResponse({
            id: 'logprobe-typed-command-boundary',
            projectId: 'logprobe-typescript',
            title: 'Typed command boundary',
            summary: 'Create the typed request parser.',
            markdown: '',
            lessons: [{ id: 'typed-command-boundaries', title: 'Typed Command Boundaries', summary: 'Typed inputs.' }],
            exercises: [{ id: 'parse-command-request-ts', title: 'ParseCommandRequest', summary: 'Parse typed requests.', language: 'TypeScript' }],
            sources: [],
            softLocks: [],
          })
        }

        if (url.includes('/api/curriculum/projects/logprobe-typescript')) {
          return jsonResponse({
            id: 'logprobe-typescript',
            title: 'logprobe-typescript',
            slug: 'logprobe-typescript',
            description: 'Build a typed Node log investigation CLI.',
            language: 'TypeScript',
            trackId: 'typescript',
            milestones: [
              {
                id: 'logprobe-typed-command-boundary',
                title: 'Typed command boundary',
                summary: 'Create the typed request parser.',
              },
            ],
          })
        }

        if (url.includes('/api/curriculum/projects/wordfreq-csharp/milestones/pure-word-counting-core')) {
          return jsonResponse({
            id: 'pure-word-counting-core',
            projectId: 'wordfreq-csharp',
            title: 'Pure Word Counting Core',
            summary: 'Study the pure core.',
            markdown: '',
            lessons: [{ id: 'text-as-data-csharp', title: 'Text as Data in C#', summary: 'Study strings.' }],
            exercises: [{ id: 'normalize-to-lowercase', title: 'NormalizeToLowercase', summary: 'Normalize text.', language: 'CSharp' }],
            sources: [],
            softLocks: [],
          })
        }

        if (url.includes('/api/curriculum/projects/wordfreq-csharp')) {
          return jsonResponse({
            id: 'wordfreq-csharp',
            title: 'wordfreq-csharp',
            slug: 'wordfreq-csharp',
            description: 'Pure word counting core.',
            language: 'CSharp',
            trackId: 'csharp',
            milestones: [
              {
                id: 'pure-word-counting-core',
                title: 'Pure Word Counting Core',
                summary: 'Study the pure core.',
              },
            ],
          })
        }

        if (url.includes('/api/learner/diagnostics')) {
          return jsonResponse(null)
        }

        if (url.includes('/api/learner/mastery/concepts')) {
          if (url.includes('trackId=typescript')) {
            return jsonResponse([
              {
                conceptId: 'ts-type-boundaries',
                title: 'Typed Boundaries',
                score: 0,
                maxScore: 0,
                evidenceCount: 0,
                status: 'NotStarted',
                lastActivityAt: null,
              },
            ])
          }

          return jsonResponse([
            {
              conceptId: 'csharp-strings',
              title: 'Text as Data',
              score: 0,
              maxScore: 0,
              evidenceCount: 0,
              status: 'NotStarted',
              lastActivityAt: null,
            },
          ])
        }

        if (url.includes('/api/learner/summary')) {
          return jsonResponse({
            reviewDueCount: 0,
            studyStreakDays: 0,
            totalStudySeconds: 0,
            milestonesCompleted: 0,
            milestoneCount: 7,
            exercisesPassed: 0,
            exerciseCount: 24,
            reliableConcepts: 0,
            conceptCount: 20,
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
        <Dashboard
          profile={{ id: 'default-profile', displayName: 'Default Profile' }}
          selectedTrackId="csharp"
        />
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

  it('filters dashboard mastery and links by selected track', async () => {
    render(
      <MemoryRouter>
        <Dashboard
          profile={{ id: 'default-profile', displayName: 'Default Profile' }}
          selectedTrackId="typescript"
        />
      </MemoryRouter>,
    )

    expect(await screen.findByText('logprobe-typescript')).toBeInTheDocument()
    expect(await screen.findByText('Active track: TypeScript and Node.js Servers')).toBeInTheDocument()
    expect(await screen.findByText('Typed Boundaries')).toBeInTheDocument()
    expect(screen.queryByText('Text as Data')).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: /continue/i })).toHaveAttribute(
      'href',
      '/projects/logprobe-typescript/milestones/logprobe-typed-command-boundary',
    )
    expect(await screen.findByRole('link', { name: /open exercise/i })).toHaveAttribute(
      'href',
      '/exercises/parse-command-request-ts',
    )
  })
})

function jsonResponse(value: unknown) {
  return new Response(JSON.stringify(value), {
    headers: { 'Content-Type': 'application/json' },
    status: 200,
  })
}
