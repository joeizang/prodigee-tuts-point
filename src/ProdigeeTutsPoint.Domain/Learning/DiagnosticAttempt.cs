namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class DiagnosticAttempt
{
    public string Id { get; set; } = Guid.NewGuid().ToString("n");

    public string ProfileId { get; set; } = string.Empty;

    public string TrackId { get; set; } = string.Empty;

    public int Score { get; set; }

    public int MaxScore { get; set; }

    public string RecommendationLevel { get; set; } = string.Empty;

    public string RecommendationTargetId { get; set; } = string.Empty;

    public string RecommendationSummary { get; set; } = string.Empty;

    public string AnswersJson { get; set; } = "[]";

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
}
