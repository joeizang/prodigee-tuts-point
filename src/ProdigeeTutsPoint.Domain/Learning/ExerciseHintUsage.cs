namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class ExerciseHintUsage
{
    public required string Id { get; init; }

    public required string ProfileId { get; set; }

    public required string ExerciseId { get; set; }

    public required string HintId { get; set; }

    public required string HintLevel { get; set; }

    public DateTimeOffset UsedAt { get; set; }
}
