# Async Iterables for Lines

## Learning objectives

- Use `AsyncIterable<string>` as a testable abstraction for data that arrives over time.
- Process log lines incrementally instead of loading every byte before doing useful work.
- Keep streaming functions deterministic even when the source is asynchronous.

## Prerequisites

You should understand `for await...of`, `Promise`, and basic map updates. You do not need to know Node streams deeply yet. The exercise API uses `AsyncIterable<string>` because it captures the important idea without forcing the whole Node stream API into the first streaming lesson.

## Mental model

**Term: async iterable** means a source that can produce values one at a time, where each next value may require waiting. A file reader, network stream, paged API, or queue consumer can all be adapted to this shape.

**Term: incremental processing** means doing work as each item arrives. Instead of reading a whole log file into a string, splitting it into an array, and then counting, the code updates the summary per line.

```typescript
export async function collectAsyncLines(
  lines: AsyncIterable<string>,
): Promise<readonly string[]> {
  const result: string[] = []
  for await (const line of lines) {
    result.push(line)
  }
  return result
}
```

`collectAsyncLines` is useful as a first exercise because it teaches the mechanics. In production, collecting everything is often exactly what you avoid. The next step is to update a map or write output as lines arrive.

## Production transfer

Large inputs punish designs that assume everything fits in memory. A log investigation tool may be pointed at a multi-gigabyte file during an incident. Server-side Node can read that file with streams, but the core processing code should not need to care whether lines come from a file, a socket, or a test generator.

```typescript
export async function countLevelsFromLines(
  lines: AsyncIterable<string>,
): Promise<ReadonlyMap<string, number>> {
  const counts = new Map<string, number>()
  for await (const line of lines) {
    const level = line.split(/\s+/, 3)[1]
    if (level) counts.set(level, (counts.get(level) ?? 0) + 1)
  }
  return counts
}
```

The important detail is not the exact parsing rule. The important detail is that the count map grows with the number of distinct levels, not the number of log lines.

## Exercise connection

The exercises move from collecting async lines to counting levels as they arrive. Hidden tests use async generators so you must implement the protocol correctly instead of assuming an array.

## Project connection

This milestone turns `logprobe-typescript` toward real operational use. The project is no longer just a parser; it starts to become a tool that can survive larger files and slower inputs.

## Check yourself

- When is collecting an async iterable into an array acceptable?
- What memory grows when you count levels incrementally?
- Why does `AsyncIterable<string>` make tests simpler than a concrete Node stream type?

## Source reference notes

Use Node.js material to study streams and line readers. Use TypeScript material to understand `AsyncIterable`, `for await...of`, and how generic asynchronous sources can be kept out of business logic.
