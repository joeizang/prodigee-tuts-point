namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class ReviewCard
{
    public required string Id { get; init; }

    public required string ConceptId { get; set; }

    public required string Prompt { get; set; }

    public required string Answer { get; set; }

    public required string SourceType { get; set; }

    public required string SourceId { get; set; }

    public int Order { get; set; }

    public bool IsActive { get; set; } = true;
}
