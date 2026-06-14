using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class LearningEndpointTests
{
    [Fact]
    public async Task NotesCanBeAttachedToLessons()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"notes-test-{Guid.NewGuid():n}";

        var putResponse = await client.PutAsJsonAsync(
            "/api/learner/notes",
            new NoteUpsertTestRequest(
                profileId,
                "lesson",
                "text-as-data-csharp",
                "Remember the ASCII-first milestone boundary."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var note = await client.GetFromJsonAsync<NoteTestResponse>(
            $"/api/learner/notes?profileId={profileId}&targetType=lesson&targetId=text-as-data-csharp",
            TestContext.Current.CancellationToken);

        Assert.NotNull(note);
        Assert.Equal("lesson", note.TargetType);
        Assert.Equal("Remember the ASCII-first milestone boundary.", note.Body);
    }

    [Theory]
    [InlineData("concept", "csharp-dictionaries")]
    [InlineData("sourcereference", "dictionaries-as-frequency-maps:csharp-12-in-a-nutshell:dictionarytkeytvalue-lookup-and-update-patterns")]
    public async Task NotesCanBeAttachedToSourceAnchorsAndConcepts(string targetType, string targetId)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"notes-{targetType}-test-{Guid.NewGuid():n}";

        var putResponse = await client.PutAsJsonAsync(
            "/api/learner/notes",
            new NoteUpsertTestRequest(
                profileId,
                targetType,
                targetId,
                "Personal synthesis note."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var note = await client.GetFromJsonAsync<NoteTestResponse>(
            $"/api/learner/notes?profileId={profileId}&targetType={targetType}&targetId={targetId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(note);
        Assert.Equal(targetType, note.TargetType);
        Assert.Equal(targetId, note.TargetId);
        Assert.Equal("Personal synthesis note.", note.Body);
    }

    [Fact]
    public async Task DiagnosticAttemptStoresResultAndMasteryEvidence()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"diagnostic-test-{Guid.NewGuid():n}";

        var diagnostic = await client.GetFromJsonAsync<DiagnosticTestResponse>(
            "/api/learner/diagnostics/csharp",
            TestContext.Current.CancellationToken);

        Assert.NotNull(diagnostic);
        Assert.Equal(6, diagnostic.Questions.Count);
        Assert.All(diagnostic.Questions, question => Assert.NotEmpty(question.Answers));

        var attemptResponse = await client.PostAsJsonAsync(
            "/api/learner/diagnostics/csharp/attempts",
            new DiagnosticAttemptTestRequest(
                profileId,
                [
                    new("methods-return-values", "a"),
                    new("strings-normalization", "b"),
                    new("collections-dictionary", "b"),
                    new("xunit-assertions", "a"),
                    new("edge-cases", "a"),
                    new("deterministic-ordering", "c"),
                ]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, attemptResponse.StatusCode);
        var attempt = await attemptResponse.Content.ReadFromJsonAsync<DiagnosticAttemptTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(attempt);
        Assert.Equal(6, attempt.Score);
        Assert.Equal("Milestone", attempt.RecommendationLevel);

        var mastery = await client.GetFromJsonAsync<List<ConceptMasteryTestResponse>>(
            $"/api/learner/mastery/concepts?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(mastery);
        Assert.Contains(mastery, concept => concept.ConceptId == "csharp-dictionaries" && concept.Score == 1);
    }

    private sealed record NoteUpsertTestRequest(
        string ProfileId,
        string TargetType,
        string TargetId,
        string Body);

    private sealed record NoteTestResponse(
        string Id,
        string ProfileId,
        string TargetType,
        string TargetId,
        string Body,
        DateTimeOffset UpdatedAt);

    private sealed record DiagnosticTestResponse(
        string TrackId,
        string Title,
        string Summary,
        IReadOnlyCollection<DiagnosticQuestionTestResponse> Questions);

    private sealed record DiagnosticQuestionTestResponse(
        string Id,
        string Prompt,
        string ConceptId,
        IReadOnlyCollection<DiagnosticAnswerTestResponse> Answers);

    private sealed record DiagnosticAnswerTestResponse(string Id, string Text);

    private sealed record DiagnosticAttemptTestRequest(
        string ProfileId,
        IReadOnlyCollection<DiagnosticAnswerSubmissionTestRequest> Answers);

    private sealed record DiagnosticAnswerSubmissionTestRequest(string QuestionId, string AnswerId);

    private sealed record DiagnosticAttemptTestResponse(
        string Id,
        string TrackId,
        int Score,
        int MaxScore,
        string RecommendationLevel,
        string RecommendationTargetId,
        string RecommendationSummary,
        DateTimeOffset SubmittedAt);

    private sealed record ConceptMasteryTestResponse(
        string ConceptId,
        int Score,
        int MaxScore,
        int EvidenceCount);
}
