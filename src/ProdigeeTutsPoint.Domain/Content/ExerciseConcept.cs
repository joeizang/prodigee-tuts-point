namespace ProdigeeTutsPoint.Domain.Content;

public sealed class ExerciseConcept
{
    public required string ExerciseId { get; init; }

    public Exercise? Exercise { get; init; }

    public required string ConceptId { get; init; }

    public Concept? Concept { get; init; }
}
