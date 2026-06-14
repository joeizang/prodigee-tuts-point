namespace ProdigeeTutsPoint.Domain.Content;

public sealed class Track
{
    public required string Id { get; init; }

    public required string Title { get; set; }

    public required string Slug { get; set; }

    public required string Description { get; set; }

    public required string Language { get; set; }

    public required string ContentVersion { get; set; }

    public List<Module> Modules { get; } = [];

    public List<Project> Projects { get; } = [];
}
