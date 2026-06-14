namespace ProdigeeTutsPoint.Domain.Content;

public sealed class Exercise
{
    public required string Id { get; init; }

    public required string TrackId { get; set; }

    public Track? Track { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public required string Language { get; set; }

    public required string Kind { get; set; }

    public required string DirectoryPath { get; set; }

    public required string ContentVersion { get; set; }

    public int Order { get; set; }

    public List<ExerciseConcept> Concepts { get; } = [];
}
