namespace Exercise;

public sealed record LogEvent(DateTimeOffset Timestamp, string Level, string Message);

public sealed record LogQueryOptions(string? Level, string? Contains);

public sealed record LogQueryResult(int ExitCode, string Output, string Error);

public static class LogQuery
{
    public static bool TryParseLine(string line, out LogEvent? logEvent)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<LogEvent> ParseMany(IEnumerable<string> lines, out int malformedCount)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<LogEvent> Filter(IEnumerable<LogEvent> events, LogQueryOptions options)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<KeyValuePair<string, int>> CountByLevel(IEnumerable<LogEvent> events)
    {
        throw new NotImplementedException();
    }

    public static LogQueryResult Run(IEnumerable<string> lines, LogQueryOptions options)
    {
        throw new NotImplementedException();
    }
}
