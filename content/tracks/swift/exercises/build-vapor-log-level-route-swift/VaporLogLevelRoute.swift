import Vapor

public struct LogBody: Content, Equatable, Sendable {
    public let text: String

    public init(text: String) {
        self.text = text
    }
}

public struct LevelCountResponse: Content, Equatable, Sendable {
    public let level: String
    public let count: Int

    public init(level: String, count: Int) {
        self.level = level
        self.count = count
    }
}

public func routes(_ app: Application) throws {
    app.post("log-levels") { request async throws -> [LevelCountResponse] in
        throw Abort(.notImplemented, reason: "Register the log-level route behavior.")
    }
}
