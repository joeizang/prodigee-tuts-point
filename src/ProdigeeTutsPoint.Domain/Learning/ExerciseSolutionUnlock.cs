namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class ExerciseSolutionUnlock
{
    public required string Id { get; init; }

    public required string ProfileId { get; set; }

    public required string ExerciseId { get; set; }

    public required string Reason { get; set; }

    public DateTimeOffset UnlockedAt { get; set; }
}
