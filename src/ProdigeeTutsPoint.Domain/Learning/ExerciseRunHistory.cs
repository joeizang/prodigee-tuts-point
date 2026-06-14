namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class ExerciseRunHistory
{
    public required string Id { get; init; }

    public required string ProfileId { get; set; }

    public required string ExerciseId { get; set; }

    public required string Status { get; set; }

    public bool VisiblePassed { get; set; }

    public bool HiddenPassed { get; set; }

    public bool TimedOut { get; set; }

    public int? ExitCode { get; set; }

    public required string Summary { get; set; }

    public required string Output { get; set; }

    public required string Diagnostics { get; set; }

    public int StaticAnalysisErrorCount { get; set; }

    public int StaticAnalysisWarningCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
