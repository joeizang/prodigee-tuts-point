namespace ProdigeeTutsPoint.Domain.Content;

public sealed class Concept
{
    public required string Id { get; init; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string TrackId { get; set; }

    public Track? Track { get; set; }
}
