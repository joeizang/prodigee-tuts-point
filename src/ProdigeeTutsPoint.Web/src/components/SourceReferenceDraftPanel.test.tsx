import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { SourceReferenceDraftPanel } from './SourceReferenceDraftPanel'

describe('SourceReferenceDraftPanel', () => {
  it('builds and stores local YAML source reference drafts', () => {
    const { container } = render(
      <SourceReferenceDraftPanel
        books={[
          {
            id: 'programming-csharp-12',
            title: 'Programming C# 12',
            author: 'Ian Griffiths',
            edition: undefined,
            publisher: "O'Reilly",
            ownershipStatus: 'Owned',
            references: [],
          },
        ]}
      />,
    )

    fireEvent.change(screen.getByLabelText(/target id/i), {
      target: { value: 'streaming-large-text-input' },
    })
    fireEvent.change(screen.getByLabelText(/chapter/i), {
      target: { value: 'Streams and Files' },
    })
    fireEvent.change(screen.getByLabelText(/topic/i), {
      target: { value: 'streaming reads and iterator boundaries' },
    })
    fireEvent.click(screen.getByRole('button', { name: /save draft locally/i }))

    expect(screen.getByText(/book: programming-csharp-12/)).toBeInTheDocument()
    expect(screen.getByText(/chapter: Streams and Files/)).toBeInTheDocument()
    expect(screen.getByText(/usage: QualityAnchor/)).toBeInTheDocument()
    expect(container.querySelector('.source-draft-yaml')).toHaveTextContent('- book: programming-csharp-12')
    expect(screen.getAllByText(/streaming reads and iterator boundaries/).length).toBeGreaterThan(0)
    expect(localStorage.getItem('prodigee.sourceReferenceDrafts')).toContain('streaming-large-text-input')
  })
})
