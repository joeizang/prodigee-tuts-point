namespace Exercise;

public enum CliInputMode
{
    Stdin,
    Text,
    File,
}

public sealed record CliOptions(CliInputMode Mode, string? Value);

public sealed record CliResult(int ExitCode, string Output, string Error);

public sealed record WordFrequency(string Word, int Count);

public static class WordFrequencyCli
{
    public static CliOptions ParseOptions(string[] args)
    {
        throw new NotImplementedException();
    }

    public static string ReadInputFile(string path)
    {
        throw new NotImplementedException();
    }

    public static CliResult TryReadInputFile(string path)
    {
        throw new NotImplementedException();
    }

    public static string FormatTable(IEnumerable<WordFrequency> frequencies)
    {
        throw new NotImplementedException();
    }

    public static CliResult Run(string[] args, Func<string> readStdin, Func<string, string> readFile)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<WordFrequency> Analyze(string? text)
    {
        var words = (text ?? string.Empty)
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => new string(word.Where(char.IsLetterOrDigit).ToArray()))
            .Where(word => !string.IsNullOrWhiteSpace(word));

        return words
            .GroupBy(word => word)
            .Select(group => new WordFrequency(group.Key, group.Count()))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Word, StringComparer.Ordinal)
            .ToList();
    }
}
