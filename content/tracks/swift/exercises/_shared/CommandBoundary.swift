public enum LogLevel: String, Equatable {
    case debug
    case info
    case warn
    case error
}

public struct CommandRequest: Equatable {
    public let level: LogLevel
    public let limit: Int
    public let includeArchived: Bool

    public init(level: LogLevel, limit: Int, includeArchived: Bool) {
        self.level = level
        self.limit = limit
        self.includeArchived = includeArchived
    }
}

public enum CommandRequestError: Error, Equatable {
    case unknownOption(String)
    case missingValue(String)
    case invalidLevel(String)
    case invalidLimit(String)
}

public func parseCommandRequest(_ args: [String]) throws -> CommandRequest {
    throw CommandRequestError.unknownOption("not implemented")
}
