export type HttpMethod = 'GET' | 'POST'

export type LogQueryHttpRequest = {
  readonly method: string
  readonly url: string
  readonly body?: string
}

export type LogQueryRequest = {
  readonly level?: string
  readonly limit: number
}

export type HttpResponse = {
  readonly status: number
  readonly headers: Readonly<Record<string, string>>
  readonly body: string
}

export type HttpRouteResult =
  | { readonly ok: true; readonly value: LogQueryRequest }
  | { readonly ok: false; readonly status: number; readonly message: string }

export type LogSummaryReader = (request: LogQueryRequest) => Promise<readonly { readonly level: string; readonly count: number }[]>

export function parseLogQueryUrl(url: string): HttpRouteResult {
  throw new Error('Not implemented')
}

export function rejectUnsupportedMethod(method: string): HttpRouteResult | null {
  throw new Error('Not implemented')
}

export function toHttpResponse(result: HttpRouteResult): HttpResponse {
  throw new Error('Not implemented')
}

export async function handleLogprobeRequest(
  request: LogQueryHttpRequest,
  readSummary: LogSummaryReader,
): Promise<HttpResponse> {
  throw new Error('Not implemented')
}
