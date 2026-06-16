using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ProdigeeTutsPoint.Api.Features.ExportImport;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class ExportImportEndpointTests
{
    [Fact]
    public async Task ExportExcludesSecretsAndImportRestoresProfileState()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"portable-state-test-{Guid.NewGuid():n}";

        var noteResponse = await client.PutAsJsonAsync(
            "/api/learner/notes",
            new NoteUpsertTestRequest(profileId, "lesson", "text-as-data-csharp", "Original export note."),
            TestContext.Current.CancellationToken);
        noteResponse.EnsureSuccessStatusCode();
        var studyTimeResponse = await client.PostAsJsonAsync(
            "/api/learner/study-time",
            new StudyTimeRecordTestRequest(profileId, "lesson", "text-as-data-csharp", 60, null, null),
            TestContext.Current.CancellationToken);
        studyTimeResponse.EnsureSuccessStatusCode();

        var exported = await client.GetFromJsonAsync<LearnerStateExportDocument>(
            $"/api/portable-state/export?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(exported);
        Assert.Equal(profileId, exported.ProfileId);
        Assert.NotEmpty(exported.Content.Tracks);
        Assert.NotEmpty(exported.SourceMetadata.Books);
        Assert.NotEmpty(exported.ProviderSettings);
        Assert.All(exported.ProviderSettings, provider => Assert.False(provider.SecretValuePresent));
        Assert.Contains(exported.ProviderSettings, provider =>
            provider.Id == "hosted-openai"
            && provider.SecretName == "OPENAI_API_KEY");
        Assert.DoesNotContain("sk-", System.Text.Json.JsonSerializer.Serialize(exported), StringComparison.OrdinalIgnoreCase);

        var changedNoteResponse = await client.PutAsJsonAsync(
            "/api/learner/notes",
            new NoteUpsertTestRequest(profileId, "lesson", "text-as-data-csharp", "Changed local note."),
            TestContext.Current.CancellationToken);
        changedNoteResponse.EnsureSuccessStatusCode();

        var importResponse = await client.PostAsJsonAsync(
            "/api/portable-state/import",
            exported,
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, importResponse.StatusCode);
        var importResult = await importResponse.Content.ReadFromJsonAsync<ImportResult>(TestContext.Current.CancellationToken);
        Assert.NotNull(importResult);
        Assert.True(importResult.Success);
        Assert.True(importResult.ImportedRowCount >= 2);

        var restoredNote = await client.GetFromJsonAsync<NoteTestResponse>(
            $"/api/learner/notes?profileId={profileId}&targetType=lesson&targetId=text-as-data-csharp",
            TestContext.Current.CancellationToken);
        var summary = await client.GetFromJsonAsync<LearnerSummaryTestResponse>(
            $"/api/learner/summary?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(restoredNote);
        Assert.Equal("Original export note.", restoredNote.Body);
        Assert.NotNull(summary);
        Assert.True(summary.TotalStudySeconds >= 60);
    }

    [Fact]
    public async Task ImportReportsContentVersionConflicts()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"portable-conflict-test-{Guid.NewGuid():n}";
        var exported = await client.GetFromJsonAsync<LearnerStateExportDocument>(
            $"/api/portable-state/export?profileId={profileId}",
            TestContext.Current.CancellationToken);
        Assert.NotNull(exported);

        var incompatible = exported with
        {
            Content = exported.Content with
            {
                Tracks = exported.Content.Tracks
                    .Select(track => track.Id == "csharp" ? track with { ContentVersion = "incompatible-version" } : track)
                    .ToArray(),
            },
        };

        var response = await client.PostAsJsonAsync(
            "/api/portable-state/import",
            incompatible,
            TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ImportResult>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains(result.Conflicts, conflict => conflict.Code == "ContentVersion" && conflict.Message.Contains("csharp", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record NoteUpsertTestRequest(string ProfileId, string TargetType, string TargetId, string Body);

    private sealed record NoteTestResponse(
        string Id,
        string ProfileId,
        string TargetType,
        string TargetId,
        string Body,
        DateTimeOffset UpdatedAt);

    private sealed record StudyTimeRecordTestRequest(
        string ProfileId,
        string TargetType,
        string TargetId,
        int ActiveSeconds,
        DateTimeOffset? StartedAt,
        DateTimeOffset? EndedAt);

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
}
