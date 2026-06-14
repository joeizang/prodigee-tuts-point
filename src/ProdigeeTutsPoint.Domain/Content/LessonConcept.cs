namespace ProdigeeTutsPoint.Domain.Content;

public sealed class LessonConcept
{
    public required string LessonId { get; init; }

    public Lesson? Lesson { get; init; }

    public required string ConceptId { get; init; }

    public Concept? Concept { get; init; }
}
