namespace ProdigeeTutsPoint.Domain.Content;

public sealed class ProjectMilestone
{
    public required string Id { get; init; }

    public required string ProjectId { get; set; }

    public Project? Project { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public required string MarkdownPath { get; set; }

    public required string ContentVersion { get; set; }

    public int Order { get; set; }

    public List<MilestoneLesson> Lessons { get; } = [];

    public List<MilestoneExercise> Exercises { get; } = [];

    public List<SourceReference> SourceReferences { get; } = [];
}
