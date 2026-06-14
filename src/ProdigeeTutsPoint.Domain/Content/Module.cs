namespace ProdigeeTutsPoint.Domain.Content;

public sealed class Module
{
    public required string Id { get; init; }

    public required string TrackId { get; set; }

    public Track? Track { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public int Order { get; set; }

    public List<Lesson> Lessons { get; } = [];
}
