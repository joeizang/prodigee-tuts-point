namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class StaticAnalysisDiagnosticRecord
{
    public required string Id { get; init; }

    public required string ProfileId { get; set; }

    public required string ExerciseId { get; set; }

    public string? RunHistoryId { get; set; }

    public required string RuleId { get; set; }

    public required string Severity { get; set; }

    public required string Message { get; set; }

    public required string FilePath { get; set; }

    public int? Line { get; set; }

    public int? Column { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
