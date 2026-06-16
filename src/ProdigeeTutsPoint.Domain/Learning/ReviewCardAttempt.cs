namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class ReviewCardAttempt
{
    public required string Id { get; init; }

    public required string ProfileId { get; set; }

    public required string ReviewCardId { get; set; }

    public required string ConceptId { get; set; }

    public required string Rating { get; set; }

    public int Score { get; set; }

    public int MaxScore { get; set; } = 1;

    public DateTimeOffset ReviewedAt { get; set; }
}
