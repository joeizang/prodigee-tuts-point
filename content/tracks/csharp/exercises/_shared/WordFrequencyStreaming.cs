using System.Text;

namespace Exercise;

public sealed record WordFrequency(string Word, int Count);

public static class WordFrequencyStreaming
{
    public static Dictionary<string, int> CountLines(IEnumerable<string> lines)
    {
        throw new NotImplementedException();
    }

    public static void MergeInto(Dictionary<string, int> destination, IReadOnlyDictionary<string, int> source)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<WordFrequency> Top(IReadOnlyDictionary<string, int> counts, int limit)
    {
        throw new NotImplementedException();
    }

    public static string Run(IEnumerable<string> lines, int limit)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<string> Tokenize(string? text)
    {
        var builder = new StringBuilder();
        foreach (var character in (text ?? string.Empty).ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (builder.Length > 0)
            {
                yield return builder.ToString();
                builder.Clear();
            }
        }

        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }
}
