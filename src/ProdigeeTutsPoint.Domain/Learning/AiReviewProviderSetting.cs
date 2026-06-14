namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class AiReviewProviderSetting
{
    public required string Id { get; init; }

    public required string DisplayName { get; set; }

    public required string Preset { get; set; }

    public required string Endpoint { get; set; }

    public required string Model { get; set; }

    public string? SecretName { get; set; }

    public bool IsEnabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
