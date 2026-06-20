export type LogLevel = 'debug' | 'info' | 'warn' | 'error'

export type OutputFormat = 'table' | 'json'

export type CommandRequest = {
  readonly level: LogLevel
  readonly limit: number
  readonly includeArchived: boolean
  readonly format: OutputFormat
}

export type ParseError = {
  readonly option: string
  readonly message: string
}

export type ParseResult =
  | { readonly ok: true; readonly value: CommandRequest }
  | { readonly ok: false; readonly error: ParseError }

export type ExitResult = {
  readonly exitCode: number
  readonly stdout: string
  readonly stderr: string
}

export function parseProcessArgv(argv: readonly string[]): readonly string[] {
  throw new Error('Not implemented')
}

export function parseOutputFormat(value: string | undefined): OutputFormat {
  throw new Error('Not implemented')
}

export function parseCommandSafely(args: readonly string[]): ParseResult {
  throw new Error('Not implemented')
}

export function toExitResult(result: ParseResult): ExitResult {
  throw new Error('Not implemented')
}

export function parseCommandRequest(args: readonly string[]): CommandRequest {
  throw new Error('Not implemented')
}
