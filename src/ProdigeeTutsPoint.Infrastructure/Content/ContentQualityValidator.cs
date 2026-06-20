using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProdigeeTutsPoint.Infrastructure.Content;

public sealed class ContentQualityValidator
{
    private static readonly Regex MarkdownLinkPattern = new(@"\[[^\]]+\]\((?<target>[^)]+)\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CodeFencePattern = new(@"(?:^|\n)```(?<language>[^\r\n`]*)\r?\n(?<code>[\s\S]*?)\r?\n```", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly HashSet<string> HighlightedCodeFenceLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "c#",
        "csharp",
        "cs",
        "swift",
        "ts",
        "tsx",
        "typescript",
        "js",
        "jsx",
        "javascript",
        "json",
        "bash",
        "sh",
        "shell",
        "console",
        "text",
        "txt",
        "xml",
        "yaml",
        "yml",
        "html",
        "css",
        "sql",
    };
    private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public ContentQualityValidationResult Validate(string rootPath)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        var diagnostics = new List<ContentQualityDiagnostic>();
        var sourceCatalog = ReadYaml<SourceCatalog>(Path.Combine(fullRoot, "sources", "books.yml"), diagnostics);
        var sourceIds = sourceCatalog?.Books.Select(book => book.Id).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var trackFiles = Directory.Exists(Path.Combine(fullRoot, "tracks"))
            ? Directory.GetFiles(Path.Combine(fullRoot, "tracks"), "track.yml", SearchOption.AllDirectories)
            : [];

        foreach (var trackFile in trackFiles)
        {
            var track = ReadYaml<TrackDocument>(trackFile, diagnostics);
            if (track is null)
            {
                continue;
            }

            ValidateTrack(fullRoot, trackFile, track, sourceIds, diagnostics);
        }

        return new ContentQualityValidationResult(diagnostics.Count == 0, diagnostics);
    }

    private void ValidateTrack(
        string rootPath,
        string trackFile,
        TrackDocument track,
        ISet<string> sourceIds,
        List<ContentQualityDiagnostic> diagnostics)
    {
        RequireText(track.Id, trackFile, "track.id", diagnostics);
        RequireText(track.Description, trackFile, "track.description", diagnostics, minWords: 10);

        var conceptIds = track.Concepts.Select(concept => concept.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var lesson in track.Lessons)
        {
            ValidateKnownIds(lesson.Concepts, conceptIds, lesson.Id, "concept", diagnostics);
            ValidateSourceReferences(lesson.SourceReferences, sourceIds, lesson.Id, diagnostics);
            ValidateLesson(rootPath, lesson, diagnostics);
        }

        foreach (var exercise in track.Exercises)
        {
            ValidateKnownIds(exercise.Concepts, conceptIds, exercise.Id, "concept", diagnostics);
            ValidateExercise(rootPath, exercise, diagnostics);
        }

        foreach (var project in track.Projects)
        {
            RequireText(project.Description, trackFile, $"project {project.Id} description", diagnostics, minWords: 12);
            ValidateProjectMilestoneStructure(project, trackFile, diagnostics);
            foreach (var milestone in project.Milestones)
            {
                ValidateSourceReferences(milestone.SourceReferences, sourceIds, milestone.Id, diagnostics);
                ValidateMilestone(rootPath, milestone, diagnostics);
            }
        }
    }

    private static void ValidateLesson(string rootPath, LessonDocument lesson, List<ContentQualityDiagnostic> diagnostics)
    {
        var path = Path.Combine(rootPath, lesson.Markdown);
        if (!File.Exists(path))
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingLessonMarkdown", lesson.Id, $"Lesson markdown is missing: {lesson.Markdown}"));
            return;
        }

        var markdown = File.ReadAllText(path);
        RequireWords(markdown, path, "lesson depth", diagnostics, minWords: 220);
        RequireHeading(markdown, "Learning objectives", path, diagnostics);
        RequireHeading(markdown, "Prerequisites", path, diagnostics);
        RequireHeading(markdown, "Mental model", path, diagnostics);
        RequireHeading(markdown, "Production transfer", path, diagnostics);
        RequireHeading(markdown, "Exercise connection", path, diagnostics);
        RequireHeading(markdown, "Project connection", path, diagnostics);
        RequireHeading(markdown, "Check yourself", path, diagnostics);
        RequireHeading(markdown, "Source reference notes", path, diagnostics);
        RequireCodeFence(markdown, path, diagnostics);
        ValidateCodeFenceLanguages(markdown, path, diagnostics);
        RequireOccurrences(markdown, "**Term:", 2, path, "key term markers", diagnostics);
        RequireOccurrences(markdown, "?", 3, path, "check-yourself prompts", diagnostics);
        ValidateMarkdownLinks(rootPath, path, markdown, diagnostics);
    }

    private void ValidateExercise(string rootPath, ExerciseDocument exercise, List<ContentQualityDiagnostic> diagnostics)
    {
        var path = Path.Combine(rootPath, exercise.Directory, "exercise.yml");
        var definition = ReadYaml<ExerciseDefinition>(path, diagnostics);
        if (definition is null)
        {
            return;
        }

        RequireText(definition.Workspace.Starter, path, $"{exercise.Id}.workspace.starter", diagnostics);
        RequireText(definition.Workspace.VisibleTest, path, $"{exercise.Id}.workspace.visibleTest", diagnostics);
        RequireText(definition.Workspace.HiddenTest, path, $"{exercise.Id}.workspace.hiddenTest", diagnostics);
        if (definition.Hints.Count < 3)
        {
            diagnostics.Add(new ContentQualityDiagnostic("ExerciseHints", exercise.Id, "Exercise must have at least three progressive hints."));
        }

        var levels = definition.Hints.Select(hint => hint.Level).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var level in new[] { "conceptual", "api-approach", "structural" })
        {
            if (!levels.Contains(level))
            {
                diagnostics.Add(new ContentQualityDiagnostic("ExerciseHintLevel", exercise.Id, $"Missing {level} hint."));
            }
        }

        foreach (var hint in definition.Hints)
        {
            RequireText(hint.Title, path, $"{exercise.Id}.hint.title", diagnostics, minWords: 2);
            RequireText(hint.Body, path, $"{exercise.Id}.hint.body", diagnostics, minWords: 10);
        }

        RequireText(definition.Solution.Title, path, $"{exercise.Id}.solution.title", diagnostics, minWords: 2);
        RequireText(definition.Solution.Body, path, $"{exercise.Id}.solution.body", diagnostics, minWords: 10);
        RequireText(definition.Solution.Code, path, $"{exercise.Id}.solution.code", diagnostics, minWords: 8);
        RequireItems(definition.CommonWrongApproaches, path, $"{exercise.Id}.commonWrongApproaches", diagnostics, minItems: 2);
        RequireItems(definition.ExpectedSolutionCharacteristics, path, $"{exercise.Id}.expectedSolutionCharacteristics", diagnostics, minItems: 2);
    }

    private static void ValidateMilestone(string rootPath, MilestoneDocument milestone, List<ContentQualityDiagnostic> diagnostics)
    {
        var path = Path.Combine(rootPath, milestone.Markdown);
        if (!File.Exists(path))
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingMilestoneMarkdown", milestone.Id, $"Milestone markdown is missing: {milestone.Markdown}"));
            return;
        }

        var markdown = File.ReadAllText(path);
        RequireWords(markdown, path, "milestone depth", diagnostics, minWords: 250);
        RequireHeading(markdown, "Rubric", path, diagnostics);
        foreach (var rubricArea in new[] { "Correctness", "Design", "Testing", "Maintainability", "Complexity" })
        {
            if (!markdown.Contains(rubricArea, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(new ContentQualityDiagnostic("MilestoneRubric", milestone.Id, $"Rubric must cover {rubricArea}."));
            }
        }

        ValidateCodeFenceLanguages(markdown, path, diagnostics);
        ValidateMarkdownLinks(rootPath, path, markdown, diagnostics);
    }

    private static void ValidateProjectMilestoneStructure(
        ProjectDocument project,
        string trackFile,
        List<ContentQualityDiagnostic> diagnostics)
    {
        var expectedOrder = 1;
        foreach (var milestone in project.Milestones.OrderBy(milestone => milestone.Order))
        {
            if (milestone.Order != expectedOrder)
            {
                diagnostics.Add(new ContentQualityDiagnostic(
                    "MilestoneOrdering",
                    project.Id,
                    $"Milestones must use contiguous order values starting at 1; expected {expectedOrder} but found {milestone.Order} for {milestone.Id} in {trackFile}."));
            }

            expectedOrder++;
            RequireNonEmptyItems(milestone.Lessons, trackFile, $"milestone {milestone.Id}.lessons", diagnostics, minItems: 1);
            RequireNonEmptyItems(milestone.Exercises, trackFile, $"milestone {milestone.Id}.exercises", diagnostics, minItems: 1);
            RequireNonEmptyItems(milestone.SourceReferences.Select(reference => reference.Book).ToArray(), trackFile, $"milestone {milestone.Id}.sourceReferences", diagnostics, minItems: 1);
        }

        var duplicateExercises = project.Milestones
            .SelectMany(milestone => milestone.Exercises.Select(exerciseId => (milestone.Id, ExerciseId: exerciseId)))
            .GroupBy(item => item.ExerciseId, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .ToList();

        foreach (var duplicate in duplicateExercises)
        {
            diagnostics.Add(new ContentQualityDiagnostic(
                "MilestoneExerciseCoverage",
                project.Id,
                $"Exercise {duplicate.Key} appears in multiple milestones: {string.Join(", ", duplicate.Select(item => item.Id))}."));
        }
    }

    private T? ReadYaml<T>(string path, List<ContentQualityDiagnostic> diagnostics)
    {
        if (!File.Exists(path))
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingYaml", path, $"YAML file is missing: {path}"));
            return default;
        }

        try
        {
            return deserializer.Deserialize<T>(File.ReadAllText(path));
        }
        catch (Exception exception) when (exception is InvalidOperationException or YamlDotNet.Core.YamlException)
        {
            diagnostics.Add(new ContentQualityDiagnostic("YamlParse", path, $"Could not parse YAML: {exception.Message}"));
            return default;
        }
    }

    private static void ValidateKnownIds(IEnumerable<string> ids, ISet<string> knownIds, string owner, string kind, List<ContentQualityDiagnostic> diagnostics)
    {
        foreach (var id in ids)
        {
            if (!knownIds.Contains(id))
            {
                diagnostics.Add(new ContentQualityDiagnostic("UnknownId", owner, $"Unknown {kind} id: {id}"));
            }
        }
    }

    private static void ValidateSourceReferences(
        IEnumerable<SourceReferenceDocument> references,
        ISet<string> sourceIds,
        string owner,
        List<ContentQualityDiagnostic> diagnostics)
    {
        foreach (var reference in references)
        {
            if (!sourceIds.Contains(reference.Book))
            {
                diagnostics.Add(new ContentQualityDiagnostic("SourceReference", owner, $"Unknown source reference id: {reference.Book}"));
            }
        }
    }

    private static void ValidateMarkdownLinks(string rootPath, string markdownPath, string markdown, List<ContentQualityDiagnostic> diagnostics)
    {
        foreach (Match match in MarkdownLinkPattern.Matches(markdown))
        {
            var target = match.Groups["target"].Value;
            if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || target.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || target.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var pathOnly = target.Split('#')[0];
            var fullTarget = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(markdownPath)!, pathOnly));
            if (!fullTarget.StartsWith(rootPath, StringComparison.Ordinal) || !File.Exists(fullTarget))
            {
                diagnostics.Add(new ContentQualityDiagnostic("BrokenLink", markdownPath, $"Broken internal link: {target}"));
            }
        }
    }

    private static void RequireHeading(string markdown, string heading, string path, List<ContentQualityDiagnostic> diagnostics)
    {
        if (!markdown.Contains($"## {heading}", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingHeading", path, $"Missing heading: {heading}"));
        }
    }

    private static void RequireCodeFence(string markdown, string path, List<ContentQualityDiagnostic> diagnostics)
    {
        if (!markdown.Contains("```", StringComparison.Ordinal))
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingExample", path, "Lesson must include at least one code example."));
        }
    }

    private static void ValidateCodeFenceLanguages(string markdown, string path, List<ContentQualityDiagnostic> diagnostics)
    {
        foreach (Match match in CodeFencePattern.Matches(markdown))
        {
            var language = match.Groups["language"].Value.Trim();
            var code = match.Groups["code"].Value.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                diagnostics.Add(new ContentQualityDiagnostic(
                    "MissingCodeFenceLanguage",
                    path,
                    "Code examples must use a supported fenced language so snippets render with syntax highlighting."));
                continue;
            }

            var normalizedLanguage = language.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? language;
            if (!HighlightedCodeFenceLanguages.Contains(normalizedLanguage))
            {
                diagnostics.Add(new ContentQualityDiagnostic(
                    "UnsupportedCodeFenceLanguage",
                    path,
                    $"Unsupported fenced code language '{language}'. Add renderer support before using it in learning content."));
            }
        }
    }

    private static void RequireOccurrences(
        string value,
        string marker,
        int minimum,
        string path,
        string field,
        List<ContentQualityDiagnostic> diagnostics)
    {
        var count = value.Split(marker, StringSplitOptions.None).Length - 1;
        if (count < minimum)
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingPedagogyMarker", path, $"{field} needs at least {minimum} occurrence(s) of '{marker}'; found {count}."));
        }
    }

    private static void RequireItems(
        IReadOnlyCollection<string> values,
        string path,
        string field,
        List<ContentQualityDiagnostic> diagnostics,
        int minItems)
    {
        if (values.Count < minItems)
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingPedagogyMetadata", path, $"{field} needs at least {minItems} item(s)."));
            return;
        }

        foreach (var value in values)
        {
            RequireText(value, path, field, diagnostics, minWords: 4);
        }
    }

    private static void RequireNonEmptyItems(
        IReadOnlyCollection<string> values,
        string path,
        string field,
        List<ContentQualityDiagnostic> diagnostics,
        int minItems)
    {
        if (values.Count < minItems)
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingContentReference", path, $"{field} needs at least {minItems} item(s)."));
            return;
        }

        foreach (var value in values)
        {
            RequireText(value, path, field, diagnostics);
        }
    }

    private static void RequireText(
        string? value,
        string path,
        string field,
        List<ContentQualityDiagnostic> diagnostics,
        int minWords = 1)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(new ContentQualityDiagnostic("MissingText", path, $"Missing {field}."));
            return;
        }

        RequireWords(value, path, field, diagnostics, minWords);
    }

    private static void RequireWords(string value, string path, string field, List<ContentQualityDiagnostic> diagnostics, int minWords)
    {
        var count = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        if (count < minWords)
        {
            diagnostics.Add(new ContentQualityDiagnostic("ShallowText", path, $"{field} needs at least {minWords} words; found {count}."));
        }
    }

    private sealed class SourceCatalog
    {
        public List<SourceBookDocument> Books { get; set; } = [];
    }

    private sealed class SourceBookDocument
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class TrackDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<ConceptDocument> Concepts { get; set; } = [];

        public List<LessonDocument> Lessons { get; set; } = [];

        public List<ExerciseDocument> Exercises { get; set; } = [];

        public List<ProjectDocument> Projects { get; set; } = [];
    }

    private sealed class ConceptDocument
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class LessonDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Markdown { get; set; } = string.Empty;

        public List<string> Concepts { get; set; } = [];

        public List<SourceReferenceDocument> SourceReferences { get; set; } = [];
    }

    private sealed class ExerciseDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Directory { get; set; } = string.Empty;

        public List<string> Concepts { get; set; } = [];
    }

    private sealed class ProjectDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<MilestoneDocument> Milestones { get; set; } = [];
    }

    private sealed class MilestoneDocument
    {
        public string Id { get; set; } = string.Empty;

        public string Markdown { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<string> Lessons { get; set; } = [];

        public List<string> Exercises { get; set; } = [];

        public List<SourceReferenceDocument> SourceReferences { get; set; } = [];
    }

    private sealed class SourceReferenceDocument
    {
        public string Book { get; set; } = string.Empty;
    }

    private sealed class ExerciseDefinition
    {
        public ExerciseWorkspaceDefinition Workspace { get; set; } = new();

        public List<ExerciseHintDefinition> Hints { get; set; } = [];

        public ExerciseSolutionDefinition Solution { get; set; } = new();

        public List<string> CommonWrongApproaches { get; set; } = [];

        public List<string> ExpectedSolutionCharacteristics { get; set; } = [];
    }

    private sealed class ExerciseWorkspaceDefinition
    {
        public string Starter { get; set; } = string.Empty;

        public string VisibleTest { get; set; } = string.Empty;

        public string HiddenTest { get; set; } = string.Empty;
    }

    private sealed class ExerciseHintDefinition
    {
        public string Level { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }

    private sealed class ExerciseSolutionDefinition
    {
        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }
}

public sealed record ContentQualityValidationResult(
    bool IsValid,
    IReadOnlyCollection<ContentQualityDiagnostic> Diagnostics);

public sealed record ContentQualityDiagnostic(
    string Code,
    string Scope,
    string Message);
