namespace ProdigeeTutsPoint.Domain.Learning;

public sealed class PersonalNote
{
    public string Id { get; set; } = Guid.NewGuid().ToString("n");

    public string ProfileId { get; set; } = string.Empty;

    public string TargetType { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
