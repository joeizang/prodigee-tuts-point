namespace ProdigeeTutsPoint.Domain.Content;

public sealed class SourceBook
{
    public required string Id { get; init; }

    public required string Title { get; set; }

    public required string Author { get; set; }

    public string? Edition { get; set; }

    public string? Publisher { get; set; }

    public string? OwnershipStatus { get; set; }

    public List<SourceReference> References { get; } = [];
}
