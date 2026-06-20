public enum LogLevel: String, Equatable {
    case debug
    case info
    case warn
    case error
}

public enum OutputFormat: String, Equatable {
    case table
    case json
}

public enum CliParseError: Error, Equatable {
    case unsupportedFormat(String)
    case missingPath
}

public enum FileReadResult: Equatable {
    case success(String)
    case failure(String)
}

public enum InputSource: Equatable {
    case stdin
    case file(String)
}

public struct LevelCount: Equatable {
    public let level: String
    public let count: Int

    public init(level: String, count: Int) {
        self.level = level
        self.count = count
    }
}

public struct LogprobeCommandRequest: Equatable {
    public let source: InputSource
    public let format: OutputFormat
    public let limit: Int

    public init(source: InputSource, format: OutputFormat, limit: Int) {
        self.source = source
        self.format = format
        self.limit = limit
    }
}

public enum LogprobeCommandResult: Equatable {
    case rendered(String)
    case failed(String)
}

public struct HttpLogprobeRequest: Equatable {
    public let query: [String: String]
    public let body: String?

    public init(query: [String: String], body: String?) {
        self.query = query
        self.body = body
    }
}

public struct HttpLogprobeResponse: Equatable {
    public let status: Int
    public let contentType: String
    public let body: String

    public init(status: Int, contentType: String, body: String) {
        self.status = status
        self.contentType = contentType
        self.body = body
    }
}

public struct LogprobeTelemetry: Equatable {
    public let requestId: String
    public let status: Int
    public let durationMs: Int
    public let outcome: String

    public init(requestId: String, status: Int, durationMs: Int, outcome: String) {
        self.requestId = requestId
        self.status = status
        self.durationMs = durationMs
        self.outcome = outcome
    }
}

public struct HardenedLogprobeResponse: Equatable {
    public let response: HttpLogprobeResponse
    public let telemetry: LogprobeTelemetry

    public init(response: HttpLogprobeResponse, telemetry: LogprobeTelemetry) {
        self.response = response
        self.telemetry = telemetry
    }
}

public func parseOutputFormat(_ value: String?) throws -> OutputFormat {
    throw CliParseError.unsupportedFormat("not implemented")
}

public func resolveInputSource(
    _ source: InputSource,
    readStdin: () async -> String,
    readFile: (String) async throws -> String
) async -> FileReadResult {
    return .failure("not implemented")
}

public func countLevels<S: AsyncSequence>(
    from lines: S,
    limit: Int
) async throws -> [LevelCount] where S.Element == String {
    return []
}

public func renderLevelCounts(_ counts: [LevelCount], format: OutputFormat) -> String {
    return "not implemented"
}

public func runLogprobeCommand(
    _ request: LogprobeCommandRequest,
    readStdin: () async -> String,
    readFile: (String) async throws -> String
) async throws -> LogprobeCommandResult {
    return .failed("not implemented")
}

public func handleLogprobeRequest(_ request: HttpLogprobeRequest) async -> HttpLogprobeResponse {
    return HttpLogprobeResponse(status: 500, contentType: "text/plain", body: "not implemented")
}

public func handleHardenedLogprobeRequest(
    _ request: HttpLogprobeRequest,
    requestId: String,
    startedAtMs: Int,
    deadlineMs: Int,
    maxBodyBytes: Int,
    nowMs: () async -> Int
) async -> HardenedLogprobeResponse {
    return HardenedLogprobeResponse(
        response: HttpLogprobeResponse(status: 500, contentType: "text/plain", body: "not implemented"),
        telemetry: LogprobeTelemetry(requestId: requestId, status: 500, durationMs: 0, outcome: "not-implemented")
    )
}
