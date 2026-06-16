import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { MarkdownText } from './MarkdownText'

describe('MarkdownText', () => {
  it('renders authored key terms as accessible definition bubbles', () => {
    render(
      <MarkdownText markdown="**Term: deterministic output** means the same input always produces the same observable order." />,
    )

    const term = screen.getByRole('button', { name: /key term: deterministic output/i })
    expect(term).toHaveClass('term-keyword')
    expect(term).toHaveAttribute('tabindex', '0')
    expect(term).toHaveAttribute('aria-describedby')
    expect(screen.getByRole('tooltip')).toHaveTextContent(
      'the same input always produces the same observable order',
    )
  })

  it('keeps non-term strong text as normal emphasis', () => {
    render(<MarkdownText markdown="This is **important**, but it is not a term marker." />)

    expect(screen.getByText('important').tagName).toBe('STRONG')
    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument()
  })

  it('bounds each term definition at the sentence terminator before later mentions', () => {
    render(
      <MarkdownText markdown="**Term: streaming** means processing input incrementally. Streaming also changes failure handling. **Term: accumulator** means owned mutable state." />,
    )

    const tooltips = screen.getAllByRole('tooltip')
    expect(tooltips[0]).toHaveTextContent('processing input incrementally')
    expect(tooltips[0]).not.toHaveTextContent('Streaming also changes failure handling')
    expect(tooltips[1]).toHaveTextContent('owned mutable state')
  })
})
