namespace ProdigeeTutsPoint.Domain.Content;

public sealed class MilestoneExercise
{
    public required string ProjectMilestoneId { get; init; }

    public ProjectMilestone? ProjectMilestone { get; init; }

    public required string ExerciseId { get; init; }

    public Exercise? Exercise { get; init; }

    public int Order { get; init; }
}
