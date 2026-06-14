namespace Exercise;

public sealed record WordFrequency(string Word, int Count);

public static class WordFrequencyAnalyzer
{
    public static string NormalizeToLowercase(string? text)
    {
        throw new NotImplementedException();
    }

    public static string KeepAsciiLettersAndDigits(string? text)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<string> SplitWordsOnSeparators(string? text)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<string> Tokenize(string? text)
    {
        throw new NotImplementedException();
    }

    public static Dictionary<string, int> CountWords(IEnumerable<string> words)
    {
        throw new NotImplementedException();
    }

    public static void UpdateFrequencyMap(Dictionary<string, int> map, string word)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<WordFrequency> SortFrequencies(Dictionary<string, int> frequencies)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<WordFrequency> Analyze(string? text)
    {
        throw new NotImplementedException();
    }
}
