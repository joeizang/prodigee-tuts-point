export type LogLevel = 'debug' | 'info' | 'warn' | 'error'

export type CommandRequest = {
  readonly level: LogLevel
  readonly limit: number
  readonly includeArchived: boolean
}

export function parseCommandRequest(args: readonly string[]): CommandRequest {
  throw new Error('Not implemented')
}
