namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class ConceptMasteryEvidence
{
    public string Id { get; set; } = Guid.NewGuid().ToString("n");

    public string ProfileId { get; set; } = string.Empty;

    public string ConceptId { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string SourceId { get; set; } = string.Empty;

    public int Score { get; set; }

    public int MaxScore { get; set; }

    public string Summary { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
