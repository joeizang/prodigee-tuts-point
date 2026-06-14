namespace ProdigeeTutsPoint.Domain.Content;

public sealed class Lesson
{
    public required string Id { get; init; }

    public required string TrackId { get; set; }

    public Track? Track { get; set; }

    public string? ModuleId { get; set; }

    public Module? Module { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public required string MarkdownPath { get; set; }

    public required string ContentVersion { get; set; }

    public int Order { get; set; }

    public List<LessonConcept> Concepts { get; } = [];

    public List<SourceReference> SourceReferences { get; } = [];
}
