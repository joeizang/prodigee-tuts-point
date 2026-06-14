namespace ProdigeeTutsPoint.Domain.Content;

public sealed class SourceReference
{
    public required string Id { get; init; }

    public required string SourceBookId { get; set; }

    public SourceBook? SourceBook { get; set; }

    public string? LessonId { get; set; }

    public Lesson? Lesson { get; set; }

    public string? ProjectMilestoneId { get; set; }

    public ProjectMilestone? ProjectMilestone { get; set; }

    public string? Chapter { get; set; }

    public string? Pages { get; set; }

    public required string Topic { get; set; }

    public required string Usage { get; set; }
}
