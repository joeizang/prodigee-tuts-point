using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProdigeeTutsPoint.Api.Features.Exercises;

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
        Assert.Contains(mastery, concept => concept.ConceptId == "csharp-dictionaries" && concept.Score == 1 && concept.Status == "Introduced");

        var csharpMastery = await client.GetFromJsonAsync<List<ConceptMasteryTestResponse>>(
            $"/api/learner/mastery/concepts?profileId={profileId}&trackId=csharp",
            TestContext.Current.CancellationToken);
        var typeScriptMastery = await client.GetFromJsonAsync<List<ConceptMasteryTestResponse>>(
            $"/api/learner/mastery/concepts?profileId={profileId}&trackId=typescript",
            TestContext.Current.CancellationToken);

        Assert.NotNull(csharpMastery);
        Assert.NotNull(typeScriptMastery);
        Assert.Contains(csharpMastery, concept => concept.ConceptId == "csharp-dictionaries");
        Assert.DoesNotContain(csharpMastery, concept => concept.ConceptId == "ts-type-boundaries");
        Assert.Contains(typeScriptMastery, concept => concept.ConceptId == "ts-type-boundaries" && concept.Score == 0);
        Assert.DoesNotContain(typeScriptMastery, concept => concept.ConceptId == "csharp-dictionaries");

        var latest = await client.GetFromJsonAsync<DiagnosticAttemptTestResponse>(
            $"/api/learner/diagnostics/csharp/latest?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(latest);
        Assert.Equal(attempt.Id, latest.Id);
    }

    [Fact]
    public async Task ReviewCardsStudyTimeAndSummaryAreTrackedPrivately()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"mastery-loop-test-{Guid.NewGuid():n}";

        var initialSummary = await client.GetFromJsonAsync<LearnerSummaryTestResponse>(
            $"/api/learner/summary?profileId={profileId}",
            TestContext.Current.CancellationToken);
        var cards = await client.GetFromJsonAsync<IReadOnlyCollection<ReviewCardTestResponse>>(
            $"/api/learner/review/cards?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(initialSummary);
        Assert.Contains("Private", initialSummary.GamificationPolicy);
        Assert.NotNull(cards);
        Assert.NotEmpty(cards);
        Assert.All(cards, card => Assert.True(card.IsDue));

        var firstCard = cards.First();
        var reviewResponse = await client.PostAsJsonAsync(
            $"/api/learner/review/cards/{firstCard.Id}/attempts",
            new ReviewCardAttemptTestRequest(profileId, "good"),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);

        var timeResponse = await client.PostAsJsonAsync(
            "/api/learner/study-time",
            new StudyTimeRecordTestRequest(
                profileId,
                "lesson",
                "text-as-data-csharp",
                90,
                DateTimeOffset.UtcNow.AddSeconds(-90),
                DateTimeOffset.UtcNow),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, timeResponse.StatusCode);

        var summary = await client.GetFromJsonAsync<LearnerSummaryTestResponse>(
            $"/api/learner/summary?profileId={profileId}",
            TestContext.Current.CancellationToken);
        var mastery = await client.GetFromJsonAsync<IReadOnlyCollection<ConceptMasteryTestResponse>>(
            $"/api/learner/mastery/concepts?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(summary);
        Assert.True(summary.TotalStudySeconds >= 90);
        Assert.True(summary.StudyStreakDays >= 1);
        Assert.True(summary.ReviewDueCount < cards.Count);
        Assert.NotNull(mastery);
        Assert.Contains(mastery, concept => concept.Status is "Introduced" or "Practiced" or "Applied" or "Reliable");

        var csharpSummary = await client.GetFromJsonAsync<LearnerSummaryTestResponse>(
            $"/api/learner/summary?profileId={profileId}&trackId=csharp",
            TestContext.Current.CancellationToken);
        var typeScriptSummary = await client.GetFromJsonAsync<LearnerSummaryTestResponse>(
            $"/api/learner/summary?profileId={profileId}&trackId=typescript",
            TestContext.Current.CancellationToken);

        Assert.NotNull(csharpSummary);
        Assert.NotNull(typeScriptSummary);
        Assert.Equal(14, csharpSummary.ConceptCount);
        Assert.Equal(20, typeScriptSummary.ConceptCount);
    }

    [Fact]
    public async Task PythonSummaryCountsCompletedMilestonesIndependentlyFromPassedExercises()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"python-summary-{Guid.NewGuid():n}";

        var initialSummary = await client.GetFromJsonAsync<LearnerSummaryTestResponse>(
            $"/api/learner/summary?profileId={profileId}&trackId=python",
            TestContext.Current.CancellationToken);

        Assert.NotNull(initialSummary);
        Assert.Equal(0, initialSummary.MilestonesCompleted);
        Assert.True(initialSummary.MilestoneCount > 1);

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-note-title-py/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/note_titles.py",
                        """
                        def normalize_title(raw_title: str) -> str:
                            words = raw_title.split()
                            if not words:
                                raise ValueError("title must contain at least one non-space character")

                            return " ".join(words).lower()
                        """)
                ]),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var summary = await client.GetFromJsonAsync<LearnerSummaryTestResponse>(
            $"/api/learner/summary?profileId={profileId}&trackId=python",
            TestContext.Current.CancellationToken);

        Assert.NotNull(summary);
        Assert.Equal(1, summary.ExercisesPassed);
        Assert.Equal(1, summary.MilestonesCompleted);
        Assert.True(summary.MilestoneCount > summary.MilestonesCompleted);
    }

    [Fact]
    public async Task PythonSummaryDoesNotAdvanceWhenExerciseFailsStaticAnalysis()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPythonExerciseRunner>();
                    services.AddScoped<IPythonExerciseRunner>(_ => new FakePythonExerciseRunner(
                        new CommandResult(
                            1,
                            false,
                            false,
                            false,
                            "src/note_titles.py:4:12 - error: Type \"int\" is not assignable to return type \"str\" (reportReturnType)",
                            string.Empty),
                        new CommandResult(0, false, false, false, "1 passed", string.Empty),
                        new CommandResult(0, false, false, false, "1 passed", string.Empty)));
                });
            });
        using var client = factory.CreateClient();
        var profileId = $"python-static-summary-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-note-title-py/run",
            new ExerciseRunTestRequest(profileId, []),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var summary = await client.GetFromJsonAsync<LearnerSummaryTestResponse>(
            $"/api/learner/summary?profileId={profileId}&trackId=python",
            TestContext.Current.CancellationToken);

        Assert.NotNull(summary);
        Assert.Equal(0, summary.ExercisesPassed);
        Assert.Equal(0, summary.MilestonesCompleted);
        Assert.True(summary.ExerciseCount > 0);
        Assert.True(summary.MilestoneCount > 0);
    }

    [Fact]
    public async Task ReviewCardsCanBeFilteredByTrack()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"review-track-test-{Guid.NewGuid():n}";

        var csharpCards = await client.GetFromJsonAsync<IReadOnlyCollection<ReviewCardTestResponse>>(
            $"/api/learner/review/cards?profileId={profileId}&trackId=csharp",
            TestContext.Current.CancellationToken);
        var pythonCards = await client.GetFromJsonAsync<IReadOnlyCollection<ReviewCardTestResponse>>(
            $"/api/learner/review/cards?profileId={profileId}&trackId=python",
            TestContext.Current.CancellationToken);

        Assert.NotNull(csharpCards);
        Assert.NotNull(pythonCards);
        Assert.NotEmpty(csharpCards);
        Assert.All(csharpCards, card => Assert.StartsWith("csharp-", card.ConceptId, StringComparison.Ordinal));
        Assert.DoesNotContain(pythonCards, card => card.ConceptId.StartsWith("csharp-", StringComparison.Ordinal));
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
        string Title,
        int Score,
        int MaxScore,
        int EvidenceCount,
        string Status,
        DateTimeOffset? LastActivityAt);

    private sealed record LearnerSummaryTestResponse(
        int ReviewDueCount,
        int StudyStreakDays,
        int TotalStudySeconds,
        int MilestonesCompleted,
        int MilestoneCount,
        int ExercisesPassed,
        int ExerciseCount,
        int ReliableConcepts,
        int ConceptCount,
        string GamificationPolicy);

    private sealed record ReviewCardTestResponse(
        string Id,
        string ConceptId,
        string Prompt,
        string Answer,
        string SourceType,
        string SourceId,
        DateTimeOffset? LastReviewedAt,
        DateTimeOffset DueAt,
        bool IsDue);

    private sealed record ReviewCardAttemptTestRequest(string ProfileId, string Rating);

    private sealed record StudyTimeRecordTestRequest(
        string ProfileId,
        string TargetType,
        string TargetId,
        int ActiveSeconds,
        DateTimeOffset? StartedAt,
        DateTimeOffset? EndedAt);

    private sealed record ExerciseRunTestRequest(
        string ProfileId,
        IReadOnlyCollection<ExerciseFileSaveTestRequest> Files);

    private sealed record ExerciseFileSaveTestRequest(string Path, string Content);

    private sealed class FakePythonExerciseRunner(
        CommandResult analysis,
        CommandResult visible,
        CommandResult hidden) : IPythonExerciseRunner
    {
        public Task<CommandResult> RunStaticAnalysisAsync(string workspacePath, CancellationToken cancellationToken)
        {
            return Task.FromResult(analysis);
        }

        public Task<CommandResult> RunVisibleTestsAsync(string workspacePath, CancellationToken cancellationToken)
        {
            return Task.FromResult(visible);
        }

        public Task<CommandResult> RunHiddenTestsAsync(string workspacePath, CancellationToken cancellationToken)
        {
            return Task.FromResult(hidden);
        }
    }
}
