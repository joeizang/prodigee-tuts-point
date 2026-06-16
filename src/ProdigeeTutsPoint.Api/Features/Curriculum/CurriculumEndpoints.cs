using Microsoft.EntityFrameworkCore;
using ProdigeeTutsPoint.Infrastructure.Content;
using ProdigeeTutsPoint.Infrastructure.Persistence;

namespace ProdigeeTutsPoint.Api.Features.Curriculum;

public static class CurriculumEndpoints
{
    public static RouteGroupBuilder MapCurriculumEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/curriculum").WithTags("Curriculum");

        group.MapGet("/tracks", async (AppDbContext db, CancellationToken ct) =>
        {
            var tracks = await db.Tracks
                .OrderBy(track => track.Title)
                .Select(track => new TrackSummaryResponse(
                    track.Id,
                    track.Title,
                    track.Slug,
                    track.Description,
                    track.Language))
                .ToListAsync(ct);

            return Results.Ok(tracks);
        });

        group.MapGet("/tracks/{trackId}", async (string trackId, AppDbContext db, CancellationToken ct) =>
        {
            var track = await db.Tracks
                .Where(track => track.Id == trackId)
                .Select(track => new TrackDetailResponse(
                    track.Id,
                    track.Title,
                    track.Slug,
                    track.Description,
                    track.Language,
                    track.Modules
                        .OrderBy(module => module.Order)
                        .Select(module => new ModuleResponse(module.Id, module.Title, module.Description))
                        .ToList(),
                    track.Projects
                        .OrderBy(project => project.Title)
                        .Select(project => new ProjectSummaryResponse(
                            project.Id,
                            project.Title,
                            project.Slug,
                            project.Description,
                            project.Language))
                        .ToList()))
                .FirstOrDefaultAsync(ct);

            return track is null ? Results.NotFound() : Results.Ok(track);
        });

        group.MapGet("/sources", async (AppDbContext db, CancellationToken ct) =>
        {
            var books = await db.SourceBooks
                .Where(book => book.OwnershipStatus == "Owned")
                .OrderBy(book => book.Title)
                .Select(book => new SourceBookResponse(
                    book.Id,
                    book.Title,
                    book.Author,
                    book.Edition,
                    book.Publisher,
                    book.OwnershipStatus ?? "Unspecified",
                    book.References
                        .OrderBy(reference => reference.Topic)
                        .Select(reference => new SourceReferenceResponse(
                            reference.Id,
                            reference.SourceBookId,
                            reference.SourceBook!.Title,
                            reference.Chapter,
                            reference.Pages,
                            reference.Topic,
                            reference.Usage))
                        .ToList()))
                .ToListAsync(ct);

            return Results.Ok(books);
        });

        group.MapGet("/navigation", async (AppDbContext db, CancellationToken ct) =>
        {
            var staticItems = new[]
            {
                new NavigationItemResponse("Dashboard", "Dashboard", "/", "Study overview"),
                new NavigationItemResponse("Tracks", "Tracks", "/tracks", "All curriculum tracks"),
                new NavigationItemResponse("Review", "Review", "/review", "Review queue"),
                new NavigationItemResponse("Search", "Search", "/search", "Search curriculum"),
                new NavigationItemResponse("Sources", "Sources", "/sources", "Source library"),
                new NavigationItemResponse("Settings", "Settings", "/settings", "Local settings"),
            };

            var tracks = await db.Tracks
                .OrderBy(track => track.Title)
                .Select(track => new NavigationItemResponse(
                    "Track",
                    track.Title,
                    $"/tracks/{track.Id}",
                    track.Description))
                .ToListAsync(ct);

            var projects = await db.Projects
                .OrderBy(project => project.Title)
                .Select(project => new NavigationItemResponse(
                    "Project",
                    project.Title,
                    $"/projects/{project.Id}",
                    project.Description))
                .ToListAsync(ct);

            var milestones = await db.ProjectMilestones
                .OrderBy(milestone => milestone.Order)
                .Select(milestone => new NavigationItemResponse(
                    "Milestone",
                    milestone.Title,
                    $"/projects/{milestone.ProjectId}/milestones/{milestone.Id}",
                    milestone.Summary))
                .ToListAsync(ct);

            var lessons = await db.Lessons
                .OrderBy(lesson => lesson.Order)
                .Select(lesson => new NavigationItemResponse(
                    "Lesson",
                    lesson.Title,
                    $"/lessons/{lesson.Id}",
                    lesson.Summary))
                .ToListAsync(ct);

            var exercises = await db.Exercises
                .OrderBy(exercise => exercise.Order)
                .Select(exercise => new NavigationItemResponse(
                    "Exercise",
                    exercise.Title,
                    $"/exercises/{exercise.Id}",
                    exercise.Summary))
                .ToListAsync(ct);

            var concepts = await db.Concepts
                .OrderBy(concept => concept.Title)
                .Select(concept => new NavigationItemResponse(
                    "Concept",
                    concept.Title,
                    $"/concepts/{concept.Id}",
                    concept.Description))
                .ToListAsync(ct);

            return Results.Ok(staticItems
                .Concat(tracks)
                .Concat(projects)
                .Concat(milestones)
                .Concat(lessons)
                .Concat(exercises)
                .Concat(concepts)
                .ToList());
        });

        group.MapGet("/search", async (string? q, AppDbContext db, ContentFileReader contentFiles, CancellationToken ct) =>
        {
            var query = q?.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.Ok(Array.Empty<SearchResultResponse>());
            }

            var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var pattern = $"%{string.Join('%', terms.Select(EscapeLikePattern))}%";

            var tracks = await db.Tracks
                .Where(track => EF.Functions.Like(track.Title, pattern) || EF.Functions.Like(track.Description, pattern))
                .Select(track => new SearchResultResponse(
                    "Track",
                    track.Id,
                    track.Title,
                    track.Description,
                    $"/tracks/{track.Id}",
                    null))
                .ToListAsync(ct);

            var projects = await db.Projects
                .Where(project => EF.Functions.Like(project.Title, pattern) || EF.Functions.Like(project.Description, pattern))
                .Select(project => new SearchResultResponse(
                    "Project",
                    project.Id,
                    project.Title,
                    project.Description,
                    $"/projects/{project.Id}",
                    null))
                .ToListAsync(ct);

            var lessons = await db.Lessons
                .Where(lesson => EF.Functions.Like(lesson.Title, pattern) || EF.Functions.Like(lesson.Summary, pattern))
                .Select(lesson => new SearchResultResponse(
                    "Lesson",
                    lesson.Id,
                    lesson.Title,
                    lesson.Summary,
                    $"/lessons/{lesson.Id}",
                    null))
                .ToListAsync(ct);
            var lessonIds = lessons.Select(lesson => lesson.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var lessonBodies = await db.Lessons
                .Where(lesson => !lessonIds.Contains(lesson.Id))
                .Select(lesson => new { lesson.Id, lesson.Title, lesson.Summary, lesson.MarkdownPath })
                .ToListAsync(ct);

            foreach (var lesson in lessonBodies)
            {
                var markdown = await contentFiles.ReadMarkdownAsync(lesson.MarkdownPath, ct);
                if (ContainsAllTerms(markdown, terms))
                {
                    lessons.Add(new SearchResultResponse(
                        "Lesson",
                        lesson.Id,
                        lesson.Title,
                        lesson.Summary,
                        $"/lessons/{lesson.Id}",
                        "Markdown body"));
                }
            }

            var exercises = await db.Exercises
                .Where(exercise => EF.Functions.Like(exercise.Title, pattern) || EF.Functions.Like(exercise.Summary, pattern))
                .Select(exercise => new SearchResultResponse(
                    "Exercise",
                    exercise.Id,
                    exercise.Title,
                    exercise.Summary,
                    $"/exercises/{exercise.Id}",
                    null))
                .ToListAsync(ct);

            var concepts = await db.Concepts
                .Where(concept => EF.Functions.Like(concept.Title, pattern) || EF.Functions.Like(concept.Description, pattern))
                .Select(concept => new SearchResultResponse(
                    "Concept",
                    concept.Id,
                    concept.Title,
                    concept.Description,
                    $"/concepts/{concept.Id}",
                    null))
                .ToListAsync(ct);

            var sources = await db.SourceReferences
                .Where(reference =>
                    EF.Functions.Like(reference.Topic, pattern)
                    || EF.Functions.Like(reference.Usage, pattern)
                    || reference.Chapter != null && EF.Functions.Like(reference.Chapter, pattern))
                .Select(reference => new SearchResultResponse(
                    "SourceReference",
                    reference.Id,
                    reference.SourceBook!.Title,
                    reference.Topic,
                    $"/sources#{reference.Id}",
                    reference.Chapter))
                .ToListAsync(ct);

            var results = tracks
                .Concat(projects)
                .Concat(lessons)
                .Concat(exercises)
                .Concat(concepts)
                .Concat(sources)
                .OrderBy(result => result.Kind)
                .ThenBy(result => result.Title)
                .Take(40)
                .ToList();

            return Results.Ok(results);
        });

        group.MapGet("/concepts/{conceptId}", async (string conceptId, AppDbContext db, CancellationToken ct) =>
        {
            var concept = await db.Concepts
                .Where(concept => concept.Id == conceptId)
                .Select(concept => new ConceptDetailResponse(
                    concept.Id,
                    concept.TrackId,
                    concept.Title,
                    concept.Description))
                .FirstOrDefaultAsync(ct);

            return concept is null ? Results.NotFound() : Results.Ok(concept);
        });

        group.MapGet("/projects/{projectId}", async (string projectId, AppDbContext db, CancellationToken ct) =>
        {
            var project = await db.Projects
                .Where(project => project.Id == projectId)
                .Select(project => new ProjectDetailResponse(
                    project.Id,
                    project.TrackId,
                    project.Title,
                    project.Slug,
                    project.Description,
                    project.Language,
                    project.Milestones
                        .OrderBy(milestone => milestone.Order)
                        .Select(milestone => new MilestoneSummaryResponse(
                            milestone.Id,
                            milestone.Title,
                            milestone.Summary))
                        .ToList()))
                .FirstOrDefaultAsync(ct);

            return project is null ? Results.NotFound() : Results.Ok(project);
        });

        group.MapGet("/projects/{projectId}/milestones/{milestoneId}", async (
            string projectId,
            string milestoneId,
            AppDbContext db,
            ContentFileReader contentFiles,
            CancellationToken ct) =>
        {
            var milestone = await db.ProjectMilestones
                .Where(milestone => milestone.ProjectId == projectId && milestone.Id == milestoneId)
                .Select(milestone => new
                {
                    milestone.Id,
                    milestone.ProjectId,
                    milestone.Title,
                    milestone.Summary,
                    milestone.MarkdownPath,
                    Lessons = milestone.Lessons
                        .OrderBy(link => link.Order)
                        .Select(link => new LessonSummaryResponse(
                            link.LessonId,
                            link.Lesson!.Title,
                            link.Lesson.Summary))
                        .ToList(),
                    Exercises = milestone.Exercises
                        .OrderBy(link => link.Order)
                        .Select(link => new ExerciseSummaryResponse(
                            link.ExerciseId,
                            link.Exercise!.Title,
                            link.Exercise.Summary,
                            link.Exercise.Language))
                        .ToList(),
                    Sources = milestone.SourceReferences
                        .Select(reference => new SourceReferenceResponse(
                            reference.Id,
                            reference.SourceBookId,
                            reference.SourceBook!.Title,
                            reference.Chapter,
                            reference.Pages,
                            reference.Topic,
                            reference.Usage))
                        .ToList(),
                })
                .FirstOrDefaultAsync(ct);

            if (milestone is null)
            {
                return Results.NotFound();
            }

            var markdown = await contentFiles.ReadMarkdownAsync(milestone.MarkdownPath, ct);

            return Results.Ok(new MilestoneDetailResponse(
                milestone.Id,
                milestone.ProjectId,
                milestone.Title,
                milestone.Summary,
                markdown,
                milestone.Lessons,
                milestone.Exercises,
                milestone.Sources,
                await BuildMilestoneSoftLocksAsync(db, milestone.ProjectId, milestone.Id, ct)));
        });

        group.MapGet("/projects/{projectId}/milestones/{milestoneId}/theory-cluster", async (
            string projectId,
            string milestoneId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var cluster = await db.ProjectMilestones
                .Where(milestone => milestone.ProjectId == projectId && milestone.Id == milestoneId)
                .Select(milestone => new TheoryClusterResponse(
                    milestone.ProjectId,
                    milestone.Id,
                    milestone.Title,
                    milestone.Summary,
                    milestone.Lessons
                        .OrderBy(link => link.Order)
                        .Select(link => new TheoryClusterItemResponse(
                            link.LessonId,
                            link.Lesson!.Title,
                            link.Lesson.Summary,
                            link.Lesson.SourceReferences
                                .OrderBy(reference => reference.SourceBook!.Title)
                                .ThenBy(reference => reference.Chapter)
                                .ThenBy(reference => reference.Topic)
                                .Select(reference => new SourceReferenceResponse(
                                    reference.Id,
                                    reference.SourceBookId,
                                    reference.SourceBook!.Title,
                                    reference.Chapter,
                                    reference.Pages,
                                    reference.Topic,
                                    reference.Usage))
                                .ToList()))
                        .ToList()))
                .FirstOrDefaultAsync(ct);

            return cluster is null ? Results.NotFound() : Results.Ok(cluster);
        });

        group.MapGet("/lessons/{lessonId}", async (
            string lessonId,
            AppDbContext db,
            ContentFileReader contentFiles,
            CancellationToken ct) =>
        {
            var lesson = await db.Lessons
                .Where(lesson => lesson.Id == lessonId)
                .Select(lesson => new
                {
                    lesson.Id,
                    lesson.TrackId,
                    lesson.Title,
                    lesson.Summary,
                    lesson.MarkdownPath,
                    Sources = lesson.SourceReferences
                        .Select(reference => new SourceReferenceResponse(
                            reference.Id,
                            reference.SourceBookId,
                            reference.SourceBook!.Title,
                            reference.Chapter,
                            reference.Pages,
                            reference.Topic,
                            reference.Usage))
                        .ToList(),
                })
                .FirstOrDefaultAsync(ct);

            if (lesson is null)
            {
                return Results.NotFound();
            }

            var markdown = await contentFiles.ReadMarkdownAsync(lesson.MarkdownPath, ct);

            return Results.Ok(new LessonDetailResponse(
                lesson.Id,
                lesson.TrackId,
                lesson.Title,
                lesson.Summary,
                markdown,
                lesson.Sources,
                await BuildLessonSoftLocksAsync(db, lesson.Id, ct)));
        });

        group.MapGet("/exercises/{exerciseId}", async (string exerciseId, AppDbContext db, CancellationToken ct) =>
        {
            var exercise = await db.Exercises
                .Where(exercise => exercise.Id == exerciseId)
                .Select(exercise => new
                {
                    exercise.Id,
                    exercise.TrackId,
                    exercise.Title,
                    exercise.Summary,
                    exercise.Language,
                    exercise.Kind,
                    exercise.DirectoryPath,
                    exercise.Order,
                })
                .FirstOrDefaultAsync(ct);

            if (exercise is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new ExerciseDetailResponse(
                exercise.Id,
                exercise.TrackId,
                exercise.Title,
                exercise.Summary,
                exercise.Language,
                exercise.Kind,
                exercise.DirectoryPath,
                await BuildExerciseSoftLocksAsync(db, exercise.Id, ct)));
        });

        return group;
    }

    private static string EscapeLikePattern(string term)
    {
        return term
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
    }

    private static bool ContainsAllTerms(string value, IReadOnlyCollection<string> terms)
    {
        return terms.All(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<IReadOnlyCollection<SoftLockResponse>> BuildLessonSoftLocksAsync(
        AppDbContext db,
        string lessonId,
        CancellationToken ct)
    {
        var lesson = await db.Lessons
            .Where(lesson => lesson.Id == lessonId)
            .Select(lesson => new { lesson.ModuleId, lesson.Order })
            .FirstOrDefaultAsync(ct);

        if (lesson?.ModuleId is null)
        {
            return [];
        }

        return await db.Lessons
            .Where(candidate => candidate.ModuleId == lesson.ModuleId && candidate.Order < lesson.Order)
            .OrderBy(candidate => candidate.Order)
            .Select(candidate => new SoftLockResponse(
                "lesson",
                candidate.Id,
                candidate.Title,
                "Recommended prerequisite before this lesson."))
            .ToListAsync(ct);
    }

    private static async Task<IReadOnlyCollection<SoftLockResponse>> BuildMilestoneSoftLocksAsync(
        AppDbContext db,
        string projectId,
        string milestoneId,
        CancellationToken ct)
    {
        var milestone = await db.ProjectMilestones
            .Where(milestone => milestone.ProjectId == projectId && milestone.Id == milestoneId)
            .Select(milestone => new { milestone.Order })
            .FirstOrDefaultAsync(ct);

        if (milestone is null)
        {
            return [];
        }

        return await db.ProjectMilestones
            .Where(candidate => candidate.ProjectId == projectId && candidate.Order < milestone.Order)
            .OrderBy(candidate => candidate.Order)
            .Select(candidate => new SoftLockResponse(
                "milestone",
                candidate.Id,
                candidate.Title,
                "Recommended prerequisite milestone."))
            .ToListAsync(ct);
    }

    private static async Task<IReadOnlyCollection<SoftLockResponse>> BuildExerciseSoftLocksAsync(
        AppDbContext db,
        string exerciseId,
        CancellationToken ct)
    {
        var milestoneLink = await db.Set<ProdigeeTutsPoint.Domain.Content.MilestoneExercise>()
            .Where(link => link.ExerciseId == exerciseId)
            .OrderBy(link => link.Order)
            .Select(link => new { link.ProjectMilestoneId, link.Order })
            .FirstOrDefaultAsync(ct);

        if (milestoneLink is null)
        {
            return [];
        }

        return await db.Set<ProdigeeTutsPoint.Domain.Content.MilestoneLesson>()
            .Where(link => link.ProjectMilestoneId == milestoneLink.ProjectMilestoneId && link.Order <= milestoneLink.Order)
            .OrderBy(link => link.Order)
            .Select(link => new SoftLockResponse(
                "lesson",
                link.LessonId,
                link.Lesson!.Title,
                "Recommended prerequisite before this exercise."))
            .ToListAsync(ct);
    }
}

public sealed record TrackSummaryResponse(
    string Id,
    string Title,
    string Slug,
    string Description,
    string Language);

public sealed record TrackDetailResponse(
    string Id,
    string Title,
    string Slug,
    string Description,
    string Language,
    IReadOnlyCollection<ModuleResponse> Modules,
    IReadOnlyCollection<ProjectSummaryResponse> Projects);

public sealed record ModuleResponse(string Id, string Title, string Description);

public sealed record ProjectSummaryResponse(
    string Id,
    string Title,
    string Slug,
    string Description,
    string Language);

public sealed record ProjectDetailResponse(
    string Id,
    string TrackId,
    string Title,
    string Slug,
    string Description,
    string Language,
    IReadOnlyCollection<MilestoneSummaryResponse> Milestones);

public sealed record MilestoneSummaryResponse(string Id, string Title, string Summary);

public sealed record MilestoneDetailResponse(
    string Id,
    string ProjectId,
    string Title,
    string Summary,
    string Markdown,
    IReadOnlyCollection<LessonSummaryResponse> Lessons,
    IReadOnlyCollection<ExerciseSummaryResponse> Exercises,
    IReadOnlyCollection<SourceReferenceResponse> Sources,
    IReadOnlyCollection<SoftLockResponse> SoftLocks);

public sealed record LessonSummaryResponse(string Id, string Title, string Summary);

public sealed record TheoryClusterResponse(
    string ProjectId,
    string MilestoneId,
    string Title,
    string Summary,
    IReadOnlyCollection<TheoryClusterItemResponse> Items);

public sealed record TheoryClusterItemResponse(
    string LessonId,
    string Title,
    string Summary,
    IReadOnlyCollection<SourceReferenceResponse> Sources);

public sealed record LessonDetailResponse(
    string Id,
    string TrackId,
    string Title,
    string Summary,
    string Markdown,
    IReadOnlyCollection<SourceReferenceResponse> Sources,
    IReadOnlyCollection<SoftLockResponse> SoftLocks);

public sealed record ExerciseSummaryResponse(string Id, string Title, string Summary, string Language);

public sealed record ExerciseDetailResponse(
    string Id,
    string TrackId,
    string Title,
    string Summary,
    string Language,
    string Kind,
    string DirectoryPath,
    IReadOnlyCollection<SoftLockResponse> SoftLocks);

public sealed record ConceptDetailResponse(
    string Id,
    string TrackId,
    string Title,
    string Description);

public sealed record SourceReferenceResponse(
    string Id,
    string BookId,
    string BookTitle,
    string? Chapter,
    string? Pages,
    string Topic,
    string Usage);

public sealed record SourceBookResponse(
    string Id,
    string Title,
    string Author,
    string? Edition,
    string? Publisher,
    string OwnershipStatus,
    IReadOnlyCollection<SourceReferenceResponse> References);

public sealed record SearchResultResponse(
    string Kind,
    string Id,
    string Title,
    string Summary,
    string Path,
    string? Metadata);

public sealed record NavigationItemResponse(
    string Kind,
    string Label,
    string Path,
    string Summary);

public sealed record SoftLockResponse(
    string TargetType,
    string TargetId,
    string Title,
    string Reason);
