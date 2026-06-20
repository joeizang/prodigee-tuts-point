# Bounded Streaming Summaries

## Learning objectives

- Produce deterministic top-N summaries from maps without changing the streaming input contract.
- Separate aggregation from presentation so output is easy to test.
- Reason about memory and sorting costs before optimizing prematurely.

## Prerequisites

You should be able to update a `Map<string, number>` and sort an array with a comparator. You should also understand that streaming reduces input memory pressure but does not automatically make every later step cheap.

## Mental model

**Term: bounded summary** means output that has an explicit size limit, such as the top 10 levels, users, routes, or error codes. The input may be huge, but the user-facing answer stays small.

**Term: deterministic tie-breaker** means a secondary ordering rule that makes equal counts stable. For `logprobe-typescript`, count descending and key ascending is enough for predictable tests and automation.

```typescript
export function takeTopCounts(
  counts: ReadonlyMap<string, number>,
  limit: number,
): readonly CountEntry[] {
  return [...counts]
    .map(([key, count]) => ({ key, count }))
    .sort((left, right) => right.count - left.count || left.key.localeCompare(right.key))
    .slice(0, Math.max(0, limit))
}
```

This is not the most sophisticated algorithm for every dataset. It is a correct and readable baseline. Senior engineers should know when this is enough and when a heap or approximate counter becomes worth the extra complexity.

## Production transfer

Operational tools are used under stress. A summary that changes order between runs creates doubt. A tool that prints thousands of rows by default overwhelms the user. A tool that hides its limit can make automation fragile.

```typescript
export async function runStreamingLogprobe(
  lines: AsyncIterable<string>,
  limit: number,
): Promise<StreamingRunResult> {
  const counts = await countLevelsFromLines(lines)
  const rows = takeTopCounts(counts, limit)
  return { exitCode: 0, stdout: rows.map((row) => `${row.key}\t${row.count}`).join('\n'), stderr: '' }
}
```

The design has three stages: consume lines, summarize counts, format output. Each stage can be tested independently, then assembled into the project milestone.

## Exercise connection

The exercises ask you to build top-N ordering and a small streaming runner. The hidden tests check tie-breaking, zero limits, malformed lines, and async generator behavior.

## Project connection

After this milestone, `logprobe-typescript` has the shape of a real command-line investigation tool: typed command boundaries, controlled input, streaming aggregation, deterministic output, and testable failure paths. Later HTTP/server work can reuse the same core.

## Check yourself

- Why is a deterministic tie-breaker part of correctness, not polish?
- What data structure grows with distinct keys rather than input lines?
- When would sorting every distinct key become too expensive?

## Source reference notes

Use algorithm books for complexity tradeoffs and top-N thinking. Use TypeScript and Node.js sources for map iteration, async boundaries, and testable output formatting.
