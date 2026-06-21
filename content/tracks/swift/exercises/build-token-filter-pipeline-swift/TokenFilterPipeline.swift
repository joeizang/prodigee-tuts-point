public protocol TokenFilter {
    func include(_ token: String) -> Bool
}

public struct MinimumLengthFilter: TokenFilter {
    public let minimumLength: Int

    public init(minimumLength: Int) {
        self.minimumLength = minimumLength
    }

    public func include(_ token: String) -> Bool {
        fatalError("Implement MinimumLengthFilter.include")
    }
}

public struct PrefixFilter: TokenFilter {
    public let prefix: String

    public init(prefix: String) {
        self.prefix = prefix
    }

    public func include(_ token: String) -> Bool {
        fatalError("Implement PrefixFilter.include")
    }
}

public struct TokenFilterPipeline {
    public let filters: [any TokenFilter]

    public init(filters: [any TokenFilter]) {
        self.filters = filters
    }

    public func run(_ tokens: [String]) -> [String] {
        fatalError("Implement TokenFilterPipeline.run")
    }
}
