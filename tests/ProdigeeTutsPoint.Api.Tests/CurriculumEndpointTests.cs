using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class CurriculumEndpointTests
{
    [Fact]
    public async Task TracksEndpointReturnsIndexedCSharpTrack()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var tracks = await client.GetFromJsonAsync<List<TrackSummaryTestResponse>>(
            "/api/curriculum/tracks",
            TestContext.Current.CancellationToken);

        var track = Assert.Single(tracks ?? []);
        Assert.Equal("csharp", track.Id);
        Assert.Equal("C# Language", track.Title);
    }

    [Fact]
    public async Task MilestoneEndpointReturnsLessonsExercisesAndSources()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var milestone = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/wordfreq-csharp/milestones/pure-word-counting-core",
            TestContext.Current.CancellationToken);

        Assert.NotNull(milestone);
        Assert.Equal("Pure Word Counting Core", milestone.Title);
        Assert.Equal(6, milestone.Lessons.Count);
        Assert.Equal(10, milestone.Exercises.Count);
        Assert.NotEmpty(milestone.Sources);
        Assert.Contains("Normalize text with the milestone ASCII-first rule", milestone.Markdown);
    }

    [Fact]
    public async Task SearchEndpointReturnsDeepLinksAcrossCurriculum()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var results = await client.GetFromJsonAsync<List<SearchResultTestResponse>>(
            "/api/curriculum/search?q=dictionary",
            TestContext.Current.CancellationToken);

        Assert.NotNull(results);
        Assert.Contains(results, result => result.Kind == "Lesson" && result.Path == "/lessons/dictionaries-as-frequency-maps");
        Assert.Contains(results, result => result.Kind == "SourceReference" && result.Path.StartsWith("/sources#", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SearchEndpointReturnsConceptDeepLinks()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var results = await client.GetFromJsonAsync<List<SearchResultTestResponse>>(
            "/api/curriculum/search?q=frequency%20maps",
            TestContext.Current.CancellationToken);

        Assert.NotNull(results);
        Assert.Contains(results, result => result.Kind == "Concept" && result.Path == "/concepts/csharp-dictionaries");
    }

    [Fact]
    public async Task SearchEndpointSearchesLessonMarkdownBody()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var results = await client.GetFromJsonAsync<List<SearchResultTestResponse>>(
            "/api/curriculum/search?q=compliance-sensitive",
            TestContext.Current.CancellationToken);

        Assert.NotNull(results);
        Assert.Contains(results, result =>
            result.Kind == "Lesson"
            && result.Path == "/lessons/text-as-data-csharp"
            && result.Metadata == "Markdown body");
    }

    [Fact]
    public async Task ConceptEndpointReturnsConceptForNotesUi()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var concept = await client.GetFromJsonAsync<ConceptDetailTestResponse>(
            "/api/curriculum/concepts/csharp-dictionaries",
            TestContext.Current.CancellationToken);

        Assert.NotNull(concept);
        Assert.Equal("csharp-dictionaries", concept.Id);
        Assert.Equal("csharp", concept.TrackId);
        Assert.Equal("Dictionaries and Frequency Maps", concept.Title);
    }

    [Fact]
    public async Task SoftLocksAreDataDrivenFromCurriculumOrder()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var firstLesson = await client.GetFromJsonAsync<LessonDetailTestResponse>(
            "/api/curriculum/lessons/text-as-data-csharp",
            TestContext.Current.CancellationToken);
        var secondLesson = await client.GetFromJsonAsync<LessonDetailTestResponse>(
            "/api/curriculum/lessons/normalization-and-tokenization",
            TestContext.Current.CancellationToken);
        var firstExercise = await client.GetFromJsonAsync<ExerciseDetailTestResponse>(
            "/api/curriculum/exercises/normalize-to-lowercase",
            TestContext.Current.CancellationToken);
        var secondExercise = await client.GetFromJsonAsync<ExerciseDetailTestResponse>(
            "/api/curriculum/exercises/keep-ascii-letters-and-digits",
            TestContext.Current.CancellationToken);

        Assert.NotNull(firstLesson);
        Assert.NotNull(secondLesson);
        Assert.NotNull(firstExercise);
        Assert.NotNull(secondExercise);
        Assert.Equal("focused", firstExercise.Kind);
        Assert.Empty(firstLesson.SoftLocks);
        Assert.Contains(secondLesson.SoftLocks, softLock => softLock.TargetId == "text-as-data-csharp");
        Assert.Contains(firstExercise.SoftLocks, softLock => softLock.TargetId == "text-as-data-csharp");
        Assert.Contains(secondExercise.SoftLocks, softLock => softLock.TargetId == "normalization-and-tokenization");
    }

    [Fact]
    public async Task SourcesEndpointReturnsBooksWithMetadataReferences()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var books = await client.GetFromJsonAsync<List<SourceBookTestResponse>>(
            "/api/curriculum/sources",
            TestContext.Current.CancellationToken);

        Assert.NotNull(books);
        Assert.Contains(books, book => book.Id == "csharp-12-in-a-nutshell");
        Assert.All(books.SelectMany(book => book.References), reference =>
        {
            Assert.NotEmpty(reference.Id);
            Assert.NotEmpty(reference.Topic);
            Assert.DoesNotContain("public sealed", reference.Topic);
        });
    }

    [Fact]
    public async Task NavigationEndpointReturnsContentBackedCommandItems()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var items = await client.GetFromJsonAsync<List<NavigationItemTestResponse>>(
            "/api/curriculum/navigation",
            TestContext.Current.CancellationToken);

        Assert.NotNull(items);
        Assert.Contains(items, item => item.Kind == "Track" && item.Path == "/tracks/csharp");
        Assert.Contains(items, item => item.Kind == "Project" && item.Path == "/projects/wordfreq-csharp");
        Assert.Contains(items, item => item.Kind == "Lesson" && item.Path == "/lessons/text-as-data-csharp");
        Assert.Contains(items, item => item.Kind == "Exercise" && item.Path == "/exercises/normalize-to-lowercase");
        Assert.Contains(items, item => item.Kind == "Concept" && item.Path == "/concepts/csharp-dictionaries");
    }

    private sealed record TrackSummaryTestResponse(
        string Id,
        string Title,
        string Slug,
        string Description,
        string Language);

    private sealed record MilestoneDetailTestResponse(
        string Id,
        string ProjectId,
        string Title,
        string Summary,
        string Markdown,
        IReadOnlyCollection<LessonSummaryTestResponse> Lessons,
        IReadOnlyCollection<ExerciseSummaryTestResponse> Exercises,
        IReadOnlyCollection<SourceReferenceTestResponse> Sources,
        IReadOnlyCollection<SoftLockTestResponse> SoftLocks);

    private sealed record LessonDetailTestResponse(
        string Id,
        string TrackId,
        string Title,
        string Summary,
        string Markdown,
        IReadOnlyCollection<SourceReferenceTestResponse> Sources,
        IReadOnlyCollection<SoftLockTestResponse> SoftLocks);

    private sealed record ExerciseDetailTestResponse(
        string Id,
        string TrackId,
        string Title,
        string Summary,
        string Language,
        string Kind,
        string DirectoryPath,
        IReadOnlyCollection<SoftLockTestResponse> SoftLocks);

    private sealed record LessonSummaryTestResponse(string Id, string Title, string Summary);

    private sealed record ExerciseSummaryTestResponse(string Id, string Title, string Summary, string Language);

    private sealed record SourceReferenceTestResponse(
        string BookId,
        string BookTitle,
        string? Chapter,
        string? Pages,
        string Topic,
        string Usage);

    private sealed record SearchResultTestResponse(
        string Kind,
        string Id,
        string Title,
        string Summary,
        string Path,
        string? Metadata);

    private sealed record NavigationItemTestResponse(
        string Kind,
        string Label,
        string Path,
        string Summary);

    private sealed record SourceBookTestResponse(
        string Id,
        string Title,
        string Author,
        string? Edition,
        string? Publisher,
        string OwnershipStatus,
        IReadOnlyCollection<SourceReferenceWithIdTestResponse> References);

    private sealed record SourceReferenceWithIdTestResponse(
        string Id,
        string BookId,
        string BookTitle,
        string? Chapter,
        string? Pages,
        string Topic,
        string Usage);

    private sealed record ConceptDetailTestResponse(
        string Id,
        string TrackId,
        string Title,
        string Description);

    private sealed record SoftLockTestResponse(
        string TargetType,
        string TargetId,
        string Title,
        string Reason);
}
