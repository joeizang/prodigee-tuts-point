export type RuntimeRequest = {
  readonly method: string
  readonly url: string
  readonly headers: Readonly<Record<string, string>>
  readonly body: string
}

export type RuntimeResponse = {
  readonly status: number
  readonly headers: Readonly<Record<string, string>>
  readonly body: string
}

export type WritableResponse = {
  statusCode: number
  setHeader(name: string, value: string): void
  end(body: string): void
}

export type RequestContext = {
  readonly requestId: string
  readonly startedAtMs: number
}

export type LogSummaryRow = {
  readonly level: string
  readonly count: number
}

export type LogSummaryReader = (request: RuntimeRequest, context: RequestContext) => Promise<readonly LogSummaryRow[]>

export type RuntimeDependencies = {
  readonly readSummary: LogSummaryReader
  readonly nowMs: () => number
  readonly newRequestId: () => string
}

export type TelemetryEvent = {
  readonly requestId: string
  readonly method: string
  readonly url: string
  readonly status: number
  readonly durationMs: number
}

export type TelemetrySink = (event: TelemetryEvent) => void

export async function adaptNodeRequest(input: {
  readonly method?: string
  readonly url?: string
  readonly headers: Readonly<Record<string, string | readonly string[] | undefined>>
  readonly bodyChunks: AsyncIterable<string | Buffer>
}): Promise<RuntimeRequest> {
  throw new Error('Not implemented')
}

export function writeNodeResponse(response: RuntimeResponse, writable: WritableResponse): void {
  throw new Error('Not implemented')
}

export async function composeNodeServerHandler(
  request: RuntimeRequest,
  dependencies: RuntimeDependencies,
): Promise<RuntimeResponse> {
  throw new Error('Not implemented')
}

export function createRouteDependencies(readSummary: LogSummaryReader): RuntimeDependencies {
  throw new Error('Not implemented')
}

export function wrapHandlerErrors(
  handler: (request: RuntimeRequest, context: RequestContext) => Promise<RuntimeResponse>,
): (request: RuntimeRequest, context: RequestContext) => Promise<RuntimeResponse> {
  throw new Error('Not implemented')
}

export async function applyRequestTimeout<T>(
  operation: Promise<T>,
  timeoutMs: number,
  timeoutValue: T,
): Promise<T> {
  throw new Error('Not implemented')
}

export function createRequestContext(headers: Readonly<Record<string, string>>, nowMs: () => number): RequestContext {
  throw new Error('Not implemented')
}

export async function recordHandlerTelemetry(
  request: RuntimeRequest,
  context: RequestContext,
  handle: () => Promise<RuntimeResponse>,
  nowMs: () => number,
  sink: TelemetrySink,
): Promise<RuntimeResponse> {
  throw new Error('Not implemented')
}
