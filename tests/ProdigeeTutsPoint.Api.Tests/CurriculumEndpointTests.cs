using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class CurriculumEndpointTests
{
    [Fact]
    public async Task TracksEndpointReturnsIndexedTracks()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var tracks = await client.GetFromJsonAsync<List<TrackSummaryTestResponse>>(
            "/api/curriculum/tracks",
            TestContext.Current.CancellationToken);

        Assert.NotNull(tracks);
        Assert.Contains(tracks, track => track.Id == "csharp" && track.Title == "C# Language");
        Assert.Contains(tracks, track => track.Id == "typescript" && track.Title == "TypeScript and Node.js Servers");
        Assert.Contains(tracks, track => track.Id == "swift" && track.Title == "Swift and Server-Side Swift");
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
    public async Task CliMilestoneEndpointReturnsLessonsExercisesAndSources()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var milestone = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/wordfreq-csharp/milestones/cli-and-file-io",
            TestContext.Current.CancellationToken);

        Assert.NotNull(milestone);
        Assert.Equal("CLI and File I/O", milestone.Title);
        Assert.Equal(3, milestone.Lessons.Count);
        Assert.Equal(5, milestone.Exercises.Count);
        Assert.Contains(milestone.Lessons, lesson => lesson.Id == "cli-contracts-and-exit-codes");
        Assert.Contains(milestone.Exercises, exercise => exercise.Id == "run-wordfreq-cli-request");
        Assert.NotEmpty(milestone.Sources);
        Assert.Contains("exit codes", milestone.Markdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TheoryClusterEndpointReturnsLessonStudyLinksWithSourceAnchors()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var cluster = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/wordfreq-csharp/milestones/pure-word-counting-core/theory-cluster",
            TestContext.Current.CancellationToken);

        Assert.NotNull(cluster);
        Assert.Equal("wordfreq-csharp", cluster.ProjectId);
        Assert.Equal("pure-word-counting-core", cluster.MilestoneId);
        Assert.Equal(6, cluster.Items.Count);

        var first = cluster.Items.First();
        Assert.Equal("text-as-data-csharp", first.LessonId);
        Assert.Equal("Text as Data in C#", first.Title);
        Assert.Contains(first.Sources, source =>
            source.Id == "text-as-data-csharp:csharp-12-in-a-nutshell:string-char-immutability-indexing"
            && source.BookTitle == "C# 12 in a Nutshell");
    }

    [Fact]
    public async Task TheoryClusterEndpointReturnsCliMilestoneStudyLinks()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var cluster = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/wordfreq-csharp/milestones/cli-and-file-io/theory-cluster",
            TestContext.Current.CancellationToken);

        Assert.NotNull(cluster);
        Assert.Equal("cli-and-file-io", cluster.MilestoneId);
        Assert.Equal(3, cluster.Items.Count);
        Assert.Equal("cli-contracts-and-exit-codes", cluster.Items.First().LessonId);
        Assert.All(cluster.Items, item => Assert.NotEmpty(item.Sources));
    }

    [Fact]
    public async Task ProjectEndpointReturnsSecondMilestone()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var project = await client.GetFromJsonAsync<ProjectDetailTestResponse>(
            "/api/curriculum/projects/wordfreq-csharp",
            TestContext.Current.CancellationToken);

        Assert.NotNull(project);
        Assert.Equal(3, project.Milestones.Count);
        Assert.Contains(project.Milestones, milestone => milestone.Id == "cli-and-file-io");
        Assert.Contains(project.Milestones, milestone => milestone.Id == "streaming-and-scale");
    }

    [Fact]
    public async Task StreamingMilestoneEndpointReturnsLessonsExercisesAndSources()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var milestone = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/wordfreq-csharp/milestones/streaming-and-scale",
            TestContext.Current.CancellationToken);

        Assert.NotNull(milestone);
        Assert.Equal("Streaming and Scale", milestone.Title);
        Assert.Equal(3, milestone.Lessons.Count);
        Assert.Equal(4, milestone.Exercises.Count);
        Assert.Contains(milestone.Lessons, lesson => lesson.Id == "streaming-large-text-input");
        Assert.Contains(milestone.Exercises, exercise => exercise.Id == "run-streaming-wordfreq");
        Assert.NotEmpty(milestone.Sources);
        Assert.Contains("top-N", milestone.Markdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LogqueryProjectEndpointReturnsFirstProjectMilestone()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var project = await client.GetFromJsonAsync<ProjectDetailTestResponse>(
            "/api/curriculum/projects/logquery-csharp",
            TestContext.Current.CancellationToken);

        Assert.NotNull(project);
        Assert.Equal("logquery-csharp", project.Id);
        var milestone = Assert.Single(project.Milestones);
        Assert.Equal("parse-filter-summarize", milestone.Id);
    }

    [Fact]
    public async Task TypeScriptProjectEndpointReturnsFileIoAndStreamingMilestones()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var project = await client.GetFromJsonAsync<ProjectDetailTestResponse>(
            "/api/curriculum/projects/logprobe-typescript",
            TestContext.Current.CancellationToken);

        Assert.NotNull(project);
        Assert.Equal("logprobe-typescript", project.Id);
        Assert.Equal(7, project.Milestones.Count);
        Assert.Contains(project.Milestones, milestone => milestone.Id == "logprobe-file-io");
        Assert.Contains(project.Milestones, milestone => milestone.Id == "logprobe-streaming-scale");
    }

    [Fact]
    public async Task TypeScriptFileIoAndStreamingMilestonesReturnLessonsExercisesAndSources()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var fileIo = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-file-io",
            TestContext.Current.CancellationToken);
        var streaming = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-streaming-scale",
            TestContext.Current.CancellationToken);

        Assert.NotNull(fileIo);
        Assert.Equal("File I/O boundaries", fileIo.Title);
        Assert.Equal(2, fileIo.Lessons.Count);
        Assert.Equal(3, fileIo.Exercises.Count);
        Assert.Contains(fileIo.Lessons, lesson => lesson.Id == "file-io-boundaries-node");
        Assert.Contains(fileIo.Exercises, exercise => exercise.Id == "resolve-input-source-ts");
        Assert.NotEmpty(fileIo.Sources);

        Assert.NotNull(streaming);
        Assert.Equal("Streaming and scale", streaming.Title);
        Assert.Equal(2, streaming.Lessons.Count);
        Assert.Equal(4, streaming.Exercises.Count);
        Assert.Contains(streaming.Lessons, lesson => lesson.Id == "async-iterables-for-lines");
        Assert.Contains(streaming.Exercises, exercise => exercise.Id == "run-streaming-logprobe-ts");
        Assert.NotEmpty(streaming.Sources);
    }

    [Fact]
    public async Task TypeScriptHttpAdapterMilestoneReturnsLessonsExercisesAndSources()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var project = await client.GetFromJsonAsync<ProjectDetailTestResponse>(
            "/api/curriculum/projects/logprobe-typescript",
            TestContext.Current.CancellationToken);
        var milestone = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-http-adapter",
            TestContext.Current.CancellationToken);
        var cluster = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-http-adapter/theory-cluster",
            TestContext.Current.CancellationToken);

        Assert.NotNull(project);
        Assert.Equal(7, project.Milestones.Count);
        Assert.Contains(project.Milestones, item => item.Id == "logprobe-http-adapter");

        Assert.NotNull(milestone);
        Assert.Equal("HTTP adapter", milestone.Title);
        Assert.Equal(2, milestone.Lessons.Count);
        Assert.Equal(4, milestone.Exercises.Count);
        Assert.Contains(milestone.Lessons, lesson => lesson.Id == "http-boundaries-node");
        Assert.Contains(milestone.Lessons, lesson => lesson.Id == "response-contracts-and-errors");
        Assert.Contains(milestone.Exercises, exercise => exercise.Id == "handle-logprobe-request-ts");
        Assert.NotEmpty(milestone.Sources);
        Assert.Contains("JSON response", milestone.Markdown, StringComparison.OrdinalIgnoreCase);

        Assert.NotNull(cluster);
        Assert.Equal(2, cluster.Items.Count);
        Assert.All(cluster.Items, item => Assert.NotEmpty(item.Sources));
    }

    [Fact]
    public async Task TypeScriptNativeRuntimeAndHardeningMilestonesReturnLessonsExercisesAndSources()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var runtime = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-node-runtime",
            TestContext.Current.CancellationToken);
        var hardening = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-server-hardening",
            TestContext.Current.CancellationToken);
        var hardeningCluster = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-server-hardening/theory-cluster",
            TestContext.Current.CancellationToken);

        Assert.NotNull(runtime);
        Assert.Equal("Native Node runtime", runtime.Title);
        Assert.Equal(2, runtime.Lessons.Count);
        Assert.Equal(4, runtime.Exercises.Count);
        Assert.Contains(runtime.Lessons, lesson => lesson.Id == "native-node-http-runtime");
        Assert.Contains(runtime.Exercises, exercise => exercise.Id == "compose-node-server-handler-ts");
        Assert.NotEmpty(runtime.Sources);

        Assert.NotNull(hardening);
        Assert.Equal("Server hardening", hardening.Title);
        Assert.Equal(2, hardening.Lessons.Count);
        Assert.Equal(4, hardening.Exercises.Count);
        Assert.Contains(hardening.Lessons, lesson => lesson.Id == "error-boundaries-timeouts");
        Assert.Contains(hardening.Exercises, exercise => exercise.Id == "record-handler-telemetry-ts");
        Assert.NotEmpty(hardening.Sources);

        Assert.NotNull(hardeningCluster);
        Assert.Equal(2, hardeningCluster.Items.Count);
        Assert.All(hardeningCluster.Items, item => Assert.NotEmpty(item.Sources));
    }

    [Fact]
    public async Task SwiftFoundationMilestoneReturnsLessonsExercisesAndSources()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var project = await client.GetFromJsonAsync<ProjectDetailTestResponse>(
            "/api/curriculum/projects/logprobe-swift",
            TestContext.Current.CancellationToken);
        var milestone = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/logprobe-swift/milestones/swiftpm-command-boundary",
            TestContext.Current.CancellationToken);
        var cluster = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/logprobe-swift/milestones/swiftpm-command-boundary/theory-cluster",
            TestContext.Current.CancellationToken);

        Assert.NotNull(project);
        Assert.Equal("logprobe-swift", project.Id);
        Assert.Equal(7, project.Milestones.Count);
        Assert.Contains(project.Milestones, item => item.Id == "swiftpm-command-boundary");
        Assert.Contains(project.Milestones, item => item.Id == "swift-cli-contracts");
        Assert.Contains(project.Milestones, item => item.Id == "swift-file-input-boundaries");
        Assert.Contains(project.Milestones, item => item.Id == "swift-streaming-scale");
        Assert.Contains(project.Milestones, item => item.Id == "swift-cli-composition");
        Assert.Contains(project.Milestones, item => item.Id == "swift-vapor-request-adapter");
        Assert.Contains(project.Milestones, item => item.Id == "swift-server-hardening");

        Assert.NotNull(milestone);
        Assert.Equal("SwiftPM command boundary", milestone.Title);
        var lesson = Assert.Single(milestone.Lessons);
        Assert.Equal("swiftpm-command-boundaries", lesson.Id);
        var exercise = Assert.Single(milestone.Exercises);
        Assert.Equal("parse-command-request-swift", exercise.Id);
        Assert.Equal("Swift", exercise.Language);
        Assert.NotEmpty(milestone.Sources);
        Assert.Contains("SwiftPM package", milestone.Markdown, StringComparison.OrdinalIgnoreCase);

        Assert.NotNull(cluster);
        var clusterItem = Assert.Single(cluster.Items);
        Assert.Equal("swiftpm-command-boundaries", clusterItem.LessonId);
        Assert.NotEmpty(clusterItem.Sources);
    }

    [Theory]
    [InlineData(
        "swift-cli-contracts",
        "Swift CLI contracts",
        "swift-cli-output-contracts",
        "parse-output-format-swift")]
    [InlineData(
        "swift-file-input-boundaries",
        "Swift file input boundaries",
        "swift-file-input-boundaries",
        "resolve-input-source-swift")]
    [InlineData(
        "swift-streaming-scale",
        "Swift streaming and scale",
        "swift-async-line-streams",
        "count-levels-swift")]
    [InlineData(
        "swift-cli-composition",
        "Swift CLI composition",
        "swift-cli-composition",
        "run-logprobe-command-swift")]
    [InlineData(
        "swift-vapor-request-adapter",
        "Swift Vapor request adapter",
        "swift-vapor-request-adapters",
        "handle-logprobe-request-swift")]
    [InlineData(
        "swift-server-hardening",
        "Swift server hardening",
        "swift-server-hardening",
        "handle-hardened-logprobe-request-swift")]
    public async Task SwiftLogprobeMilestonesReturnLessonsExercisesAndSourceBackedTheory(
        string milestoneId,
        string title,
        string lessonId,
        string exerciseId)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var milestone = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            $"/api/curriculum/projects/logprobe-swift/milestones/{milestoneId}",
            TestContext.Current.CancellationToken);
        var cluster = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            $"/api/curriculum/projects/logprobe-swift/milestones/{milestoneId}/theory-cluster",
            TestContext.Current.CancellationToken);

        Assert.NotNull(milestone);
        Assert.Equal(title, milestone.Title);
        var lesson = Assert.Single(milestone.Lessons);
        Assert.Equal(lessonId, lesson.Id);
        var exercise = Assert.Single(milestone.Exercises);
        Assert.Equal(exerciseId, exercise.Id);
        Assert.Equal("Swift", exercise.Language);
        Assert.NotEmpty(milestone.Sources);

        Assert.NotNull(cluster);
        var clusterItem = Assert.Single(cluster.Items);
        Assert.Equal(lessonId, clusterItem.LessonId);
        Assert.NotEmpty(clusterItem.Sources);
    }

    [Fact]
    public async Task TheoryClusterEndpointReturnsTypeScriptFileIoAndStreamingStudyLinks()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var fileIo = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-file-io/theory-cluster",
            TestContext.Current.CancellationToken);
        var streaming = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/logprobe-typescript/milestones/logprobe-streaming-scale/theory-cluster",
            TestContext.Current.CancellationToken);

        Assert.NotNull(fileIo);
        Assert.Equal(2, fileIo.Items.Count);
        Assert.All(fileIo.Items, item => Assert.NotEmpty(item.Sources));
        Assert.NotNull(streaming);
        Assert.Equal(2, streaming.Items.Count);
        Assert.All(streaming.Items, item => Assert.NotEmpty(item.Sources));
    }

    [Fact]
    public async Task LogqueryMilestoneEndpointReturnsLessonsExercisesAndSources()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var milestone = await client.GetFromJsonAsync<MilestoneDetailTestResponse>(
            "/api/curriculum/projects/logquery-csharp/milestones/parse-filter-summarize",
            TestContext.Current.CancellationToken);

        Assert.NotNull(milestone);
        Assert.Equal("Parse, Filter, Summarize", milestone.Title);
        Assert.Equal(2, milestone.Lessons.Count);
        Assert.Equal(5, milestone.Exercises.Count);
        Assert.Contains(milestone.Lessons, lesson => lesson.Id == "log-lines-as-records");
        Assert.Contains(milestone.Exercises, exercise => exercise.Id == "parse-many-log-lines");
        Assert.Contains(milestone.Exercises, exercise => exercise.Id == "run-logquery-summary");
        Assert.NotEmpty(milestone.Sources);
        Assert.Contains("malformed input", milestone.Markdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TheoryClusterEndpointReturnsStreamingAndLogqueryStudyLinks()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var streaming = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/wordfreq-csharp/milestones/streaming-and-scale/theory-cluster",
            TestContext.Current.CancellationToken);
        var logquery = await client.GetFromJsonAsync<TheoryClusterTestResponse>(
            "/api/curriculum/projects/logquery-csharp/milestones/parse-filter-summarize/theory-cluster",
            TestContext.Current.CancellationToken);

        Assert.NotNull(streaming);
        Assert.Equal(3, streaming.Items.Count);
        Assert.All(streaming.Items, item => Assert.NotEmpty(item.Sources));
        Assert.NotNull(logquery);
        Assert.Equal(2, logquery.Items.Count);
        Assert.All(logquery.Items, item => Assert.NotEmpty(item.Sources));
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
        Assert.Contains(items, item => item.Kind == "Project" && item.Path == "/projects/logquery-csharp");
        Assert.Contains(items, item => item.Kind == "Lesson" && item.Path == "/lessons/text-as-data-csharp");
        Assert.Contains(items, item => item.Kind == "Exercise" && item.Path == "/exercises/normalize-to-lowercase");
        Assert.Contains(items, item => item.Kind == "Exercise" && item.Path == "/exercises/run-logquery-summary");
        Assert.Contains(items, item => item.Kind == "Concept" && item.Path == "/concepts/csharp-dictionaries");
    }

    private sealed record TrackSummaryTestResponse(
        string Id,
        string Title,
        string Slug,
        string Description,
        string Language);

    private sealed record ProjectDetailTestResponse(
        string Id,
        string TrackId,
        string Title,
        string Slug,
        string Description,
        string Language,
        IReadOnlyCollection<MilestoneSummaryTestResponse> Milestones);

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

    private sealed record MilestoneSummaryTestResponse(string Id, string Title, string Summary);

    private sealed record TheoryClusterTestResponse(
        string ProjectId,
        string MilestoneId,
        string Title,
        string Summary,
        IReadOnlyCollection<TheoryClusterItemTestResponse> Items);

    private sealed record TheoryClusterItemTestResponse(
        string LessonId,
        string Title,
        string Summary,
        IReadOnlyCollection<SourceReferenceWithIdTestResponse> Sources);

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
