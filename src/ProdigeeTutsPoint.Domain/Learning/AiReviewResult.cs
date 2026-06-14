namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class AiReviewResult
{
    public required string Id { get; init; }

    public required string ProfileId { get; set; }

    public required string ProjectId { get; set; }

    public required string MilestoneId { get; set; }

    public required string ProviderId { get; set; }

    public required string ProviderPreset { get; set; }

    public required string Model { get; set; }

    public required string PromptVersion { get; set; }

    public required string RubricVersion { get; set; }

    public required string Policy { get; set; }

    public required string Status { get; set; }

    public int Score { get; set; }

    public int MaxScore { get; set; }

    public required string Summary { get; set; }

    public required string StrengthsJson { get; set; }

    public required string RisksJson { get; set; }

    public required string NextStepsJson { get; set; }

    public required string RawOutput { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
