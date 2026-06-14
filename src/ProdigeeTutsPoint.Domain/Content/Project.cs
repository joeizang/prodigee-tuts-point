namespace ProdigeeTutsPoint.Domain.Content;

public sealed class Project
{
    public required string Id { get; init; }

    public required string TrackId { get; set; }

    public Track? Track { get; set; }

    public required string Title { get; set; }

    public required string Slug { get; set; }

    public required string Description { get; set; }

    public required string Language { get; set; }

    public required string ContentVersion { get; set; }

    public List<ProjectMilestone> Milestones { get; } = [];
}
