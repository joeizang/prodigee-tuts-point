namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class ExerciseAttempt
{
    public string Id { get; set; } = string.Empty;

    public string ProfileId { get; set; } = string.Empty;

    public string ExerciseId { get; set; } = string.Empty;

    public string WorkspacePath { get; set; } = string.Empty;

    public string Status { get; set; } = "NotRun";

    public bool VisiblePassed { get; set; }

    public bool HiddenPassed { get; set; }

    public bool TimedOut { get; set; }

    public int? ExitCode { get; set; }

    public string Output { get; set; } = string.Empty;

    public string Diagnostics { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
