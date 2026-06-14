namespace ProdigeeTutsPoint.Domain.Content;

public sealed class MilestoneLesson
{
    public required string ProjectMilestoneId { get; init; }

    public ProjectMilestone? ProjectMilestone { get; init; }

    public required string LessonId { get; init; }

    public Lesson? Lesson { get; init; }

    public int Order { get; init; }
}
