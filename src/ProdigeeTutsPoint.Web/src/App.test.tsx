import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'

describe('App shell', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.stubGlobal(
      'fetch',
      vi.fn(async () =>
        new Response(
          JSON.stringify([
            {
              id: 'csharp',
              title: 'C# Language',
              slug: 'csharp',
              description: 'Project-backed mastery of modern C#.',
              language: 'CSharp',
            },
          ]),
          { headers: { 'Content-Type': 'application/json' }, status: 200 },
        ),
      ),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('persists theme per local profile', async () => {
    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByTitle('Dark'))

    await waitFor(() => {
      expect(document.documentElement.dataset.theme).toBe('dark')
      expect(localStorage.getItem('prodigee.theme.default-profile')).toBe('dark')
    })
  })

  it('dispatches save and run workspace commands from keyboard shortcuts', () => {
    const onSave = vi.fn()
    const onRun = vi.fn()
    window.addEventListener('prodigee:save', onSave)
    window.addEventListener('prodigee:run', onRun)

    try {
      render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
      )

      fireEvent.keyDown(window, { ctrlKey: true, key: 's' })
      fireEvent.keyDown(window, { ctrlKey: true, key: 'Enter' })

      expect(onSave).toHaveBeenCalledOnce()
      expect(onRun).toHaveBeenCalledOnce()
    } finally {
      window.removeEventListener('prodigee:save', onSave)
      window.removeEventListener('prodigee:run', onRun)
    }
  })
})
