export type FileReadResult =
  | { readonly ok: true; readonly text: string }
  | { readonly ok: false; readonly message: string }

export type InputSource =
  | { readonly kind: 'stdin' }
  | { readonly kind: 'file'; readonly path: string }

export type LogEvent = {
  readonly timestamp: string
  readonly level: string
  readonly message: string
}

export type CountEntry = {
  readonly key: string
  readonly count: number
}

export type StreamingRunResult = {
  readonly exitCode: number
  readonly stdout: string
  readonly stderr: string
}

export type TextFileReader = (path: string) => Promise<string>

export async function readTextFileSafely(path: string, readFile: TextFileReader): Promise<FileReadResult> {
  throw new Error('Not implemented')
}

export async function loadLogLines(path: string, readFile: TextFileReader): Promise<readonly string[]> {
  throw new Error('Not implemented')
}

export async function resolveInputSource(
  source: InputSource,
  readStdin: () => Promise<string>,
  readFile: TextFileReader,
): Promise<FileReadResult> {
  throw new Error('Not implemented')
}

export async function collectAsyncLines(lines: AsyncIterable<string>): Promise<readonly string[]> {
  throw new Error('Not implemented')
}

export async function countLevelsFromLines(lines: AsyncIterable<string>): Promise<ReadonlyMap<string, number>> {
  throw new Error('Not implemented')
}

export function takeTopCounts(counts: ReadonlyMap<string, number>, limit: number): readonly CountEntry[] {
  throw new Error('Not implemented')
}

export async function runStreamingLogprobe(
  lines: AsyncIterable<string>,
  limit: number,
): Promise<StreamingRunResult> {
  throw new Error('Not implemented')
}
