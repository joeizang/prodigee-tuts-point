import { render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { Tracks } from './Tracks'

describe('Tracks', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async () =>
        jsonResponse([
          {
            id: 'csharp',
            title: 'C# Language',
            slug: 'csharp',
            description: 'Project-backed mastery of modern C#.',
            language: 'CSharp',
          },
          {
            id: 'typescript',
            title: 'TypeScript and Node.js Servers',
            slug: 'typescript',
            description: 'Project-backed mastery of TypeScript and Node.js.',
            language: 'TypeScript',
          },
        ]),
      ),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('marks the selected track as active on the tracks page', async () => {
    render(
      <MemoryRouter>
        <Tracks selectedTrackId="typescript" />
      </MemoryRouter>,
    )

    const activeTrack = await screen.findByRole('link', {
      name: /typescript and node\.js servers/i,
    })
    const inactiveTrack = await screen.findByRole('link', { name: /c# language/i })

    expect(activeTrack).toHaveAttribute('aria-current', 'true')
    expect(activeTrack).toHaveClass('active-track-row')
    expect(within(activeTrack).getByText('Active')).toBeInTheDocument()
    expect(inactiveTrack).not.toHaveAttribute('aria-current')
    expect(inactiveTrack).not.toHaveClass('active-track-row')
  })
})

function jsonResponse(value: unknown) {
  return new Response(JSON.stringify(value), {
    headers: { 'Content-Type': 'application/json' },
    status: 200,
  })
}
