namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class StudyTimeEntry
{
    public required string Id { get; init; }

    public required string ProfileId { get; set; }

    public required string TargetType { get; set; }

    public required string TargetId { get; set; }

    public int ActiveSeconds { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset EndedAt { get; set; }
}
