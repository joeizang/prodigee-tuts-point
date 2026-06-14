using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProdigeeTutsPoint.Infrastructure.Content;
using ProdigeeTutsPoint.Infrastructure.Persistence;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class ContentIndexingServiceTests
{
    [Fact]
    public async Task IndexAsyncPersistsExerciseKindAndIsIdempotent()
    {
        await using var fixture = await ContentIndexingFixture.CreateAsync(TestContext.Current.CancellationToken);
        WriteMinimalContent(fixture.ContentRoot, """
        id: normalize-to-lowercase
        title: NormalizeToLowercase
        kind: focused
        workspace:
          starter: tracks/csharp/exercises/_shared/WordFrequencyAnalyzer.cs
          visibleTest: |
            Assert.True(true);
          hiddenTest: |
            Assert.True(true);
        """);
        var indexer = fixture.CreateIndexer();

        await indexer.IndexAsync(TestContext.Current.CancellationToken);
        await indexer.IndexAsync(TestContext.Current.CancellationToken);

        var exercise = await fixture.Db.Exercises.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("focused", exercise.Kind);
        Assert.Equal(1, await fixture.Db.Exercises.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await fixture.Db.Tracks.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await fixture.Db.SourceBooks.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task IndexAsyncReportsBrokenExerciseContent()
    {
        await using var fixture = await ContentIndexingFixture.CreateAsync(TestContext.Current.CancellationToken);
        WriteMinimalContent(fixture.ContentRoot, exerciseYaml: null);
        var indexer = fixture.CreateIndexer();

        var exception = await Assert.ThrowsAsync<ContentIndexingException>(
            () => indexer.IndexAsync(TestContext.Current.CancellationToken));

        Assert.Contains(exception.Diagnostics, diagnostic => diagnostic.Contains("Missing content file", StringComparison.Ordinal));
        Assert.Contains(exception.Diagnostics, diagnostic => diagnostic.Contains("exercise.yml", StringComparison.Ordinal));
    }

    private static void WriteMinimalContent(string root, string? exerciseYaml)
    {
        Write(Path.Combine(root, "sources", "books.yml"), """
        books:
          - id: csharp-book
            title: C# Book
            author: Test Author
            ownership: Owned
        """);
        Write(Path.Combine(root, "tracks", "csharp", "track.yml"), """
        id: csharp
        title: C# Language
        slug: csharp
        description: C# track
        language: CSharp
        contentVersion: test
        concepts:
          - id: csharp-strings
            title: Strings
            description: String handling.
        modules:
          - id: csharp-core
            title: Core
            description: Core module.
            order: 1
        lessons:
          - id: text-as-data-csharp
            moduleId: csharp-core
            title: Text as Data
            summary: Strings as data.
            markdown: tracks/csharp/lessons/text-as-data-csharp.md
            order: 1
            concepts:
              - csharp-strings
            sourceReferences:
              - book: csharp-book
                chapter: Strings
                topic: string basics
                usage: QualityAnchor
        exercises:
          - id: normalize-to-lowercase
            title: NormalizeToLowercase
            summary: Lowercase text.
            directory: tracks/csharp/exercises/normalize-to-lowercase
            order: 1
            concepts:
              - csharp-strings
        projects:
          - id: wordfreq-csharp
            title: wordfreq-csharp
            slug: wordfreq-csharp
            description: Word frequency.
            milestones:
              - id: pure-word-counting-core
                title: Pure Word Counting Core
                summary: Core milestone.
                markdown: tracks/csharp/projects/wordfreq/milestones/pure-word-counting-core.md
                order: 1
                lessons:
                  - text-as-data-csharp
                exercises:
                  - normalize-to-lowercase
                sourceReferences:
                  - book: csharp-book
                    chapter: Collections
                    topic: dictionaries
                    usage: QualityAnchor
        """);
        Write(Path.Combine(root, "tracks", "csharp", "lessons", "text-as-data-csharp.md"), "# Text as Data");
        Write(Path.Combine(root, "tracks", "csharp", "projects", "wordfreq", "milestones", "pure-word-counting-core.md"), "# Milestone");
        Write(Path.Combine(root, "tracks", "csharp", "exercises", "_shared", "WordFrequencyAnalyzer.cs"), "namespace Exercise; public static class WordFrequencyAnalyzer {}");

        if (exerciseYaml is not null)
        {
            Write(Path.Combine(root, "tracks", "csharp", "exercises", "normalize-to-lowercase", "exercise.yml"), exerciseYaml);
        }
        else
        {
            Directory.CreateDirectory(Path.Combine(root, "tracks", "csharp", "exercises", "normalize-to-lowercase"));
        }
    }

    private static void Write(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private sealed class ContentIndexingFixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private readonly string root;

        private ContentIndexingFixture(SqliteConnection connection, AppDbContext db, string root)
        {
            this.connection = connection;
            Db = db;
            this.root = root;
            ContentRoot = Path.Combine(root, "content");
        }

        public AppDbContext Db { get; }

        public string ContentRoot { get; }

        public static async Task<ContentIndexingFixture> CreateAsync(CancellationToken cancellationToken)
        {
            var root = Path.Combine(Path.GetTempPath(), $"prodigee-content-indexing-{Guid.NewGuid():n}");
            Directory.CreateDirectory(root);
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync(cancellationToken);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;
            var db = new AppDbContext(options);
            await db.Database.EnsureCreatedAsync(cancellationToken);
            return new ContentIndexingFixture(connection, db, root);
        }

        public ContentIndexingService CreateIndexer()
        {
            return new ContentIndexingService(
                Db,
                new TestHostEnvironment(root),
                Options.Create(new ContentOptions { RootPath = "content" }),
                NullLogger<ContentIndexingService>.Instance);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await connection.DisposeAsync();
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "ProdigeeTutsPoint.Tests";

        public string ContentRootPath { get; set; } = contentRootPath;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
