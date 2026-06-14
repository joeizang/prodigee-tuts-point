namespace ProdigeeTutsPoint.Infrastructure.Content;

public sealed class ContentIndexingException(IReadOnlyCollection<string> diagnostics)
    : Exception($"Content indexing failed: {string.Join("; ", diagnostics)}")
{
    public IReadOnlyCollection<string> Diagnostics { get; } = diagnostics;
}
