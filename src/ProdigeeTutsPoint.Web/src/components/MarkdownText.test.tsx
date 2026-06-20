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

  it('syntax highlights fenced Swift examples', () => {
    const { container } = render(
      <MarkdownText
        markdown={`\`\`\`swift
// swift-tools-version: 6.0
import PackageDescription

let package = Package(name: "ProdigeeSwiftExercise")
\`\`\``}
      />,
    )

    expect(container.querySelector('code.language-swift')).toBeInTheDocument()
    expect(screen.getByText('import')).toHaveClass('token', 'keyword')
    expect(screen.getByText('PackageDescription')).toHaveClass('token', 'type')
    expect(screen.getByText('"ProdigeeSwiftExercise"')).toHaveClass('token', 'string')
  })

  it('infers supported-language highlighting for untagged code fences', () => {
    const { container } = render(
      <MarkdownText
        markdown={`\`\`\`
public static string Normalize(string text)
{
    return text.ToLowerInvariant();
}
\`\`\``}
      />,
    )

    expect(container.querySelector('code.language-csharp')).toBeInTheDocument()
    expect(screen.getByText('public')).toHaveClass('token', 'keyword')
    expect(screen.getByText('Normalize')).toHaveClass('token', 'type')
  })
})
