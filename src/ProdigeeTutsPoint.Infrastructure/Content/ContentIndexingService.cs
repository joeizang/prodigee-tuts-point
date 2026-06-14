using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProdigeeTutsPoint.Domain.Content;
using ProdigeeTutsPoint.Infrastructure.Persistence;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProdigeeTutsPoint.Infrastructure.Content;

public sealed class ContentIndexingService(
    AppDbContext db,
    IHostEnvironment environment,
    IOptions<ContentOptions> options,
    ILogger<ContentIndexingService> logger)
{
    private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public async Task IndexAsync(CancellationToken cancellationToken)
    {
        var rootPath = ResolveRootPath(environment.ContentRootPath, options.Value.RootPath);
        var diagnostics = new List<string>();

        if (!Directory.Exists(rootPath))
        {
            throw new ContentIndexingException([$"Content root does not exist: {rootPath}"]);
        }

        var sourceCatalog = ReadYaml<SourceCatalog>(Path.Combine(rootPath, "sources", "books.yml"), diagnostics);
        var trackFiles = Directory.GetFiles(Path.Combine(rootPath, "tracks"), "track.yml", SearchOption.AllDirectories);

        if (trackFiles.Length == 0)
        {
            diagnostics.Add("No track.yml files found under content/tracks.");
        }

        var trackDocuments = trackFiles
            .Select(path => (Path: path, Document: ReadYaml<TrackDocument>(path, diagnostics)))
            .Where(item => item.Document is not null)
            .Select(item => (item.Path, Document: item.Document!))
            .ToList();
        var exerciseDefinitions = ReadExerciseDefinitions(rootPath, trackDocuments, diagnostics);

        Validate(rootPath, sourceCatalog, trackDocuments, exerciseDefinitions, diagnostics);

        if (diagnostics.Count > 0)
        {
            throw new ContentIndexingException(diagnostics);
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await ClearIndexedContentAsync(cancellationToken);

        db.SourceBooks.AddRange(sourceCatalog!.Books.Select(book => new SourceBook
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Edition = book.Edition,
            Publisher = book.Publisher,
            OwnershipStatus = book.Ownership,
        }));

        foreach (var (_, document) in trackDocuments)
        {
            AddTrackDocument(document, exerciseDefinitions);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("Indexed {TrackCount} track file(s) from {ContentRoot}", trackDocuments.Count, rootPath);
    }

    private void AddTrackDocument(
        TrackDocument document,
        IReadOnlyDictionary<string, ExerciseContentDefinition> exerciseDefinitions)
    {
        var track = new Track
        {
            Id = document.Id,
            Title = document.Title,
            Slug = document.Slug,
            Description = document.Description,
            Language = document.Language,
            ContentVersion = document.ContentVersion,
        };
        db.Tracks.Add(track);

        db.Concepts.AddRange(document.Concepts.Select(concept => new Concept
        {
            Id = concept.Id,
            TrackId = document.Id,
            Title = concept.Title,
            Description = concept.Description,
        }));

        db.Modules.AddRange(document.Modules.Select(module => new Module
        {
            Id = module.Id,
            TrackId = document.Id,
            Title = module.Title,
            Description = module.Description,
            Order = module.Order,
        }));

        foreach (var lesson in document.Lessons)
        {
            db.Lessons.Add(new Lesson
            {
                Id = lesson.Id,
                TrackId = document.Id,
                ModuleId = lesson.ModuleId,
                Title = lesson.Title,
                Summary = lesson.Summary,
                MarkdownPath = lesson.Markdown,
                ContentVersion = document.ContentVersion,
                Order = lesson.Order,
            });

            db.Set<LessonConcept>().AddRange(lesson.Concepts.Select(conceptId => new LessonConcept
            {
                LessonId = lesson.Id,
                ConceptId = conceptId,
            }));

            AddSourceReferences(lesson.SourceReferences, lesson.Id, projectMilestoneId: null);
        }

        foreach (var exercise in document.Exercises)
        {
            db.Exercises.Add(new Exercise
            {
                Id = exercise.Id,
                TrackId = document.Id,
                Title = exercise.Title,
                Summary = exercise.Summary,
                Language = document.Language,
                Kind = exerciseDefinitions[exercise.Id].Kind,
                DirectoryPath = exercise.Directory,
                ContentVersion = document.ContentVersion,
                Order = exercise.Order,
            });

            db.Set<ExerciseConcept>().AddRange(exercise.Concepts.Select(conceptId => new ExerciseConcept
            {
                ExerciseId = exercise.Id,
                ConceptId = conceptId,
            }));
        }

        foreach (var project in document.Projects)
        {
            db.Projects.Add(new Project
            {
                Id = project.Id,
                TrackId = document.Id,
                Title = project.Title,
                Slug = project.Slug,
                Description = project.Description,
                Language = document.Language,
                ContentVersion = document.ContentVersion,
            });

            foreach (var milestone in project.Milestones)
            {
                db.ProjectMilestones.Add(new ProjectMilestone
                {
                    Id = milestone.Id,
                    ProjectId = project.Id,
                    Title = milestone.Title,
                    Summary = milestone.Summary,
                    MarkdownPath = milestone.Markdown,
                    ContentVersion = document.ContentVersion,
                    Order = milestone.Order,
                });

                db.Set<MilestoneLesson>().AddRange(milestone.Lessons.Select((lessonId, index) => new MilestoneLesson
                {
                    ProjectMilestoneId = milestone.Id,
                    LessonId = lessonId,
                    Order = index + 1,
                }));

                db.Set<MilestoneExercise>().AddRange(milestone.Exercises.Select((exerciseId, index) => new MilestoneExercise
                {
                    ProjectMilestoneId = milestone.Id,
                    ExerciseId = exerciseId,
                    Order = index + 1,
                }));

                AddSourceReferences(milestone.SourceReferences, lessonId: null, milestone.Id);
            }
        }
    }

    private void AddSourceReferences(
        IReadOnlyCollection<SourceReferenceDocument> references,
        string? lessonId,
        string? projectMilestoneId)
    {
        foreach (var reference in references)
        {
            db.SourceReferences.Add(new SourceReference
            {
                Id = $"{lessonId ?? projectMilestoneId}:{reference.Book}:{Slug(reference.Topic)}",
                SourceBookId = reference.Book,
                LessonId = lessonId,
                ProjectMilestoneId = projectMilestoneId,
                Chapter = reference.Chapter,
                Pages = reference.Pages,
                Topic = reference.Topic,
                Usage = reference.Usage,
            });
        }
    }

    private async Task ClearIndexedContentAsync(CancellationToken cancellationToken)
    {
        await db.Set<ExerciseConcept>().ExecuteDeleteAsync(cancellationToken);
        await db.Set<LessonConcept>().ExecuteDeleteAsync(cancellationToken);
        await db.Set<MilestoneExercise>().ExecuteDeleteAsync(cancellationToken);
        await db.Set<MilestoneLesson>().ExecuteDeleteAsync(cancellationToken);
        await db.SourceReferences.ExecuteDeleteAsync(cancellationToken);
        await db.Exercises.ExecuteDeleteAsync(cancellationToken);
        await db.Lessons.ExecuteDeleteAsync(cancellationToken);
        await db.ProjectMilestones.ExecuteDeleteAsync(cancellationToken);
        await db.Projects.ExecuteDeleteAsync(cancellationToken);
        await db.Modules.ExecuteDeleteAsync(cancellationToken);
        await db.Concepts.ExecuteDeleteAsync(cancellationToken);
        await db.Tracks.ExecuteDeleteAsync(cancellationToken);
        await db.SourceBooks.ExecuteDeleteAsync(cancellationToken);
        db.ChangeTracker.Clear();
    }

    private T? ReadYaml<T>(string path, List<string> diagnostics)
    {
        if (!File.Exists(path))
        {
            diagnostics.Add($"Missing content file: {path}");
            return default;
        }

        try
        {
            return deserializer.Deserialize<T>(File.ReadAllText(path));
        }
        catch (Exception exception) when (exception is InvalidOperationException or YamlDotNet.Core.YamlException)
        {
            diagnostics.Add($"Could not parse {path}: {exception.Message}");
            return default;
        }
    }

    private static void Validate(
        string rootPath,
        SourceCatalog? sourceCatalog,
        IReadOnlyCollection<(string Path, TrackDocument Document)> trackDocuments,
        IReadOnlyDictionary<string, ExerciseContentDefinition> exerciseDefinitions,
        List<string> diagnostics)
    {
        var sourceBookIds = sourceCatalog?.Books.Select(book => book.Id).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        foreach (var (path, document) in trackDocuments)
        {
            Require(document.Id, path, "track id", diagnostics);
            Require(document.ContentVersion, path, "contentVersion", diagnostics);

            var conceptIds = document.Concepts.Select(concept => concept.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var lessonIds = document.Lessons.Select(lesson => lesson.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var exerciseIds = document.Exercises.Select(exercise => exercise.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var lesson in document.Lessons)
            {
                ValidateFile(rootPath, lesson.Markdown, $"lesson {lesson.Id}", diagnostics);
                ValidateIds(lesson.Concepts, conceptIds, $"lesson {lesson.Id} concept", diagnostics);
                ValidateSourceReferences(lesson.SourceReferences, sourceBookIds, $"lesson {lesson.Id}", diagnostics);
            }

            foreach (var exercise in document.Exercises)
            {
                ValidateIds(exercise.Concepts, conceptIds, $"exercise {exercise.Id} concept", diagnostics);
                ValidateDirectory(rootPath, exercise.Directory, $"exercise {exercise.Id}", diagnostics);
                if (!exerciseDefinitions.TryGetValue(exercise.Id, out var definition))
                {
                    continue;
                }

                Require(definition.Id, Path.Combine(rootPath, exercise.Directory, "exercise.yml"), "exercise id", diagnostics);
                Require(definition.Kind, Path.Combine(rootPath, exercise.Directory, "exercise.yml"), "exercise kind", diagnostics);
                if (!string.Equals(definition.Id, exercise.Id, StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add($"Exercise definition id '{definition.Id}' does not match track exercise id '{exercise.Id}'.");
                }
            }

            foreach (var project in document.Projects)
            {
                foreach (var milestone in project.Milestones)
                {
                    ValidateFile(rootPath, milestone.Markdown, $"milestone {milestone.Id}", diagnostics);
                    ValidateIds(milestone.Lessons, lessonIds, $"milestone {milestone.Id} lesson", diagnostics);
                    ValidateIds(milestone.Exercises, exerciseIds, $"milestone {milestone.Id} exercise", diagnostics);
                    ValidateSourceReferences(milestone.SourceReferences, sourceBookIds, $"milestone {milestone.Id}", diagnostics);
                }
            }
        }
    }

    private Dictionary<string, ExerciseContentDefinition> ReadExerciseDefinitions(
        string rootPath,
        IReadOnlyCollection<(string Path, TrackDocument Document)> trackDocuments,
        List<string> diagnostics)
    {
        var definitions = new Dictionary<string, ExerciseContentDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (var (_, document) in trackDocuments)
        {
            foreach (var exercise in document.Exercises)
            {
                var exerciseDefinitionPath = Path.Combine(rootPath, exercise.Directory, "exercise.yml");
                var definition = ReadYaml<ExerciseContentDefinition>(exerciseDefinitionPath, diagnostics);
                if (definition is not null)
                {
                    definitions[exercise.Id] = definition;
                }
            }
        }

        return definitions;
    }

    private static void ValidateIds(
        IEnumerable<string> ids,
        ISet<string> knownIds,
        string scope,
        List<string> diagnostics)
    {
        foreach (var id in ids)
        {
            if (!knownIds.Contains(id))
            {
                diagnostics.Add($"Unknown {scope}: {id}");
            }
        }
    }

    private static void ValidateSourceReferences(
        IEnumerable<SourceReferenceDocument> references,
        ISet<string> sourceBookIds,
        string scope,
        List<string> diagnostics)
    {
        foreach (var reference in references)
        {
            if (!sourceBookIds.Contains(reference.Book))
            {
                diagnostics.Add($"Unknown source book '{reference.Book}' in {scope}.");
            }
        }
    }

    private static void ValidateFile(string rootPath, string relativePath, string scope, List<string> diagnostics)
    {
        if (!File.Exists(Path.Combine(rootPath, relativePath)))
        {
            diagnostics.Add($"Missing markdown for {scope}: {relativePath}");
        }
    }

    private static void ValidateDirectory(string rootPath, string relativePath, string scope, List<string> diagnostics)
    {
        if (!Directory.Exists(Path.Combine(rootPath, relativePath)))
        {
            diagnostics.Add($"Missing directory for {scope}: {relativePath}");
        }
    }

    private static void Require(string value, string path, string field, List<string> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add($"Missing {field} in {path}.");
        }
    }

    private static string ResolveRootPath(string contentRootPath, string configuredPath)
    {
        return Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath));
    }

    private static string Slug(string value)
    {
        return string.Join('-', value.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace("<", string.Empty, StringComparison.Ordinal)
            .Replace(">", string.Empty, StringComparison.Ordinal);
    }

    private sealed class SourceCatalog
    {
        public List<SourceBookDocument> Books { get; set; } = [];
    }

    private sealed class SourceBookDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string? Edition { get; set; }

        public string? Publisher { get; set; }

        public string? Ownership { get; set; }
    }

    private sealed class TrackDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        public string ContentVersion { get; set; } = string.Empty;

        public List<ConceptDocument> Concepts { get; set; } = [];

        public List<ModuleDocument> Modules { get; set; } = [];

        public List<LessonDocument> Lessons { get; set; } = [];

        public List<ExerciseDocument> Exercises { get; set; } = [];

        public List<ProjectDocument> Projects { get; set; } = [];
    }

    private sealed class ConceptDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    private sealed class ModuleDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Order { get; set; }
    }

    private sealed class LessonDocument
    {
        public string Id { get; set; } = string.Empty;

        public string ModuleId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Markdown { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<string> Concepts { get; set; } = [];

        public List<SourceReferenceDocument> SourceReferences { get; set; } = [];
    }

    private sealed class ExerciseDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Directory { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<string> Concepts { get; set; } = [];
    }

    private sealed class ExerciseContentDefinition
    {
        public string Id { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;
    }

    private sealed class ProjectDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<MilestoneDocument> Milestones { get; set; } = [];
    }

    private sealed class MilestoneDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Markdown { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<string> Lessons { get; set; } = [];

        public List<string> Exercises { get; set; } = [];

        public List<SourceReferenceDocument> SourceReferences { get; set; } = [];
    }

    private sealed class SourceReferenceDocument
    {
        public string Book { get; set; } = string.Empty;

        public string? Chapter { get; set; }

        public string? Pages { get; set; }

        public string Topic { get; set; } = string.Empty;

        public string Usage { get; set; } = string.Empty;
    }
}
