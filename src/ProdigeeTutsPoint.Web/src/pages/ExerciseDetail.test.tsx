import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import {
  AttemptHistoryPanel,
  ExerciseAssistancePanel,
  StaticAnalysisPanel,
} from './ExerciseDetail'

describe('Exercise detail panels', () => {
  it('renders hidden hints as actions and revealed solutions as highlighted code', () => {
    const onUseHint = vi.fn()
    const onUnlockSolution = vi.fn()

    const { rerender } = render(
      <ExerciseAssistancePanel
        assistance={{
          hints: [
            {
              id: 'conceptual',
              level: 'conceptual',
              title: 'Separate input policy',
              body: 'Decide null behavior first.',
              used: false,
            },
          ],
          solutionAvailable: false,
          solution: null,
        }}
        language="swift"
        isUnlockingSolution={false}
        onUnlockSolution={onUnlockSolution}
        onUseHint={onUseHint}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /reveal hint/i }))
    fireEvent.click(screen.getByRole('button', { name: /unlock model solution/i }))

    expect(onUseHint).toHaveBeenCalledOnce()
    expect(onUnlockSolution).toHaveBeenCalledOnce()

    rerender(
      <ExerciseAssistancePanel
        assistance={{
          hints: [
            {
              id: 'conceptual',
              level: 'conceptual',
              title: 'Separate input policy',
              body: 'Decide null behavior first.',
              used: true,
            },
          ],
          solutionAvailable: true,
          solution: {
            title: 'Model solution',
            body: 'Pure and deterministic.',
            code: 'return text.lowercased()',
          },
        }}
        language="swift"
        isUnlockingSolution={false}
        onUnlockSolution={onUnlockSolution}
        onUseHint={onUseHint}
      />,
    )

    expect(screen.getByText('Decide null behavior first.')).toBeInTheDocument()
    expect(screen.getByText('Model solution')).toBeInTheDocument()
    expect(screen.getByText('return')).toHaveClass('token', 'keyword')
  })

  it('renders static analysis separately with rule and location detail', () => {
    render(
      <StaticAnalysisPanel
        diagnostics={[
          {
            ruleId: 'CS0103',
            severity: 'error',
            message: 'The name missingSymbol does not exist.',
            filePath: 'src/Exercise/WordFrequencyAnalyzer.cs',
            line: 9,
            column: 24,
          },
        ]}
      />,
    )

    expect(screen.getByText('Static Analysis')).toBeInTheDocument()
    expect(screen.getByText('CS0103')).toBeInTheDocument()
    expect(screen.getByText('error')).toBeInTheDocument()
    expect(screen.getByText(/WordFrequencyAnalyzer.cs:9:24/)).toBeInTheDocument()
  })

  it('renders attempt history with static-analysis counts', () => {
    render(
      <AttemptHistoryPanel
        attempts={[
          {
            id: 'run-1',
            status: 'FailedVisible',
            visiblePassed: false,
            hiddenPassed: false,
            timedOut: false,
            exitCode: 1,
            summary: 'FailedVisible: static analysis found 1 error(s) and 0 warning(s).',
            staticAnalysisErrorCount: 1,
            staticAnalysisWarningCount: 0,
            createdAt: '2026-06-14T12:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByText('Attempt History')).toBeInTheDocument()
    expect(screen.getByText('FailedVisible')).toBeInTheDocument()
    expect(screen.getAllByText(/1 error\(s\)/).length).toBeGreaterThan(0)
  })
})
