using Microsoft.EntityFrameworkCore;
using ProdigeeTutsPoint.Domain.Learning;
using ProdigeeTutsPoint.Infrastructure.Persistence;

namespace ProdigeeTutsPoint.Api.Features.ExportImport;

public sealed class ExportImportService(AppDbContext db)
{
    private const int ExportFormatVersion = 1;

    public async Task<LearnerStateExportDocument> ExportAsync(string profileId, CancellationToken cancellationToken)
    {
        var content = new ContentIndexMetadata(
            await db.Tracks.AsNoTracking().Select(track => new ContentItemVersion("track", track.Id, track.ContentVersion)).ToListAsync(cancellationToken),
            await db.Projects.AsNoTracking().Select(project => new ContentItemVersion("project", project.Id, project.ContentVersion)).ToListAsync(cancellationToken),
            await db.ProjectMilestones.AsNoTracking().Select(milestone => new ContentItemVersion("milestone", milestone.Id, milestone.ContentVersion)).ToListAsync(cancellationToken),
            await db.Lessons.AsNoTracking().Select(lesson => new ContentItemVersion("lesson", lesson.Id, lesson.ContentVersion)).ToListAsync(cancellationToken),
            await db.Exercises.AsNoTracking().Select(exercise => new ContentItemVersion("exercise", exercise.Id, exercise.ContentVersion)).ToListAsync(cancellationToken));

        var sourceMetadata = new SourceMetadata(
            await db.SourceBooks.AsNoTracking()
                .Select(book => new SourceBookExport(book.Id, book.Title, book.Author, book.Edition, book.Publisher, book.OwnershipStatus))
                .ToListAsync(cancellationToken),
            await db.SourceReferences.AsNoTracking()
                .Select(reference => new SourceReferenceExport(reference.Id, reference.SourceBookId, reference.LessonId, reference.ProjectMilestoneId, reference.Chapter, reference.Pages, reference.Topic, reference.Usage))
                .ToListAsync(cancellationToken));

        var providerSettings = await db.AiReviewProviderSettings.AsNoTracking()
            .Select(provider => new AiProviderSettingExport(
                provider.Id,
                provider.DisplayName,
                provider.Preset,
                provider.Endpoint,
                provider.Model,
                provider.SecretName,
                provider.IsEnabled,
                provider.CreatedAt,
                provider.UpdatedAt))
            .ToListAsync(cancellationToken);

        var learnerState = new LearnerStateExport(
            await db.PersonalNotes.AsNoTracking().Where(note => note.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.DiagnosticAttempts.AsNoTracking().Where(attempt => attempt.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.ConceptMasteryEvidence.AsNoTracking().Where(evidence => evidence.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.ExerciseAttempts.AsNoTracking().Where(attempt => attempt.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.ExerciseRunHistory.AsNoTracking().Where(history => history.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.StaticAnalysisDiagnostics.AsNoTracking().Where(diagnostic => diagnostic.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.ExerciseHintUsages.AsNoTracking().Where(usage => usage.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.ExerciseSolutionUnlocks.AsNoTracking().Where(unlock => unlock.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.ReviewCards.AsNoTracking().ToListAsync(cancellationToken),
            await db.ReviewCardAttempts.AsNoTracking().Where(attempt => attempt.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.StudyTimeEntries.AsNoTracking().Where(entry => entry.ProfileId == profileId).ToListAsync(cancellationToken),
            await db.AiReviewResults.AsNoTracking().Where(review => review.ProfileId == profileId).ToListAsync(cancellationToken));

        return new LearnerStateExportDocument(
            ExportFormatVersion,
            "prodigee-tuts-point",
            profileId,
            DateTimeOffset.UtcNow,
            content,
            sourceMetadata,
            providerSettings,
            learnerState);
    }

    public async Task<ImportResult> ImportAsync(LearnerStateExportDocument document, CancellationToken cancellationToken)
    {
        var conflicts = await ValidateImportAsync(document, cancellationToken);
        if (conflicts.Count > 0)
        {
            return new ImportResult(false, conflicts, 0);
        }

        var profileId = document.ProfileId;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await db.PersonalNotes.Where(note => note.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.DiagnosticAttempts.Where(attempt => attempt.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.ConceptMasteryEvidence.Where(evidence => evidence.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.ExerciseAttempts.Where(attempt => attempt.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.ExerciseRunHistory.Where(history => history.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.StaticAnalysisDiagnostics.Where(diagnostic => diagnostic.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.ExerciseHintUsages.Where(usage => usage.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.ExerciseSolutionUnlocks.Where(unlock => unlock.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.ReviewCardAttempts.Where(attempt => attempt.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.StudyTimeEntries.Where(entry => entry.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);
        await db.AiReviewResults.Where(review => review.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken);

        db.PersonalNotes.AddRange(document.LearnerState.PersonalNotes.Select(note => { note.ProfileId = profileId; return note; }));
        db.DiagnosticAttempts.AddRange(document.LearnerState.DiagnosticAttempts.Select(attempt => { attempt.ProfileId = profileId; return attempt; }));
        db.ConceptMasteryEvidence.AddRange(document.LearnerState.ConceptMasteryEvidence.Select(evidence => { evidence.ProfileId = profileId; return evidence; }));
        db.ExerciseAttempts.AddRange(document.LearnerState.ExerciseAttempts.Select(attempt => { attempt.ProfileId = profileId; return attempt; }));
        db.ExerciseRunHistory.AddRange(document.LearnerState.ExerciseRunHistory.Select(history => { history.ProfileId = profileId; return history; }));
        db.StaticAnalysisDiagnostics.AddRange(document.LearnerState.StaticAnalysisDiagnostics.Select(diagnostic => { diagnostic.ProfileId = profileId; return diagnostic; }));
        db.ExerciseHintUsages.AddRange(document.LearnerState.ExerciseHintUsages.Select(usage => { usage.ProfileId = profileId; return usage; }));
        db.ExerciseSolutionUnlocks.AddRange(document.LearnerState.ExerciseSolutionUnlocks.Select(unlock => { unlock.ProfileId = profileId; return unlock; }));
        db.ReviewCardAttempts.AddRange(document.LearnerState.ReviewCardAttempts.Select(attempt => { attempt.ProfileId = profileId; return attempt; }));
        db.StudyTimeEntries.AddRange(document.LearnerState.StudyTimeEntries.Select(entry => { entry.ProfileId = profileId; return entry; }));
        db.AiReviewResults.AddRange(document.LearnerState.AiReviewResults.Select(review => { review.ProfileId = profileId; return review; }));

        foreach (var provider in document.ProviderSettings)
        {
            var existing = await db.AiReviewProviderSettings.FirstOrDefaultAsync(item => item.Id == provider.Id, cancellationToken);
            if (existing is null)
            {
                db.AiReviewProviderSettings.Add(provider.ToEntity());
                continue;
            }

            existing.DisplayName = provider.DisplayName;
            existing.Preset = provider.Preset;
            existing.Endpoint = provider.Endpoint;
            existing.Model = provider.Model;
            existing.SecretName = provider.SecretName;
            existing.IsEnabled = provider.IsEnabled && string.IsNullOrWhiteSpace(provider.SecretName);
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ImportResult(true, [], CountLearnerRows(document.LearnerState));
    }

    private async Task<List<ImportConflict>> ValidateImportAsync(LearnerStateExportDocument document, CancellationToken cancellationToken)
    {
        var conflicts = new List<ImportConflict>();
        if (document.FormatVersion != ExportFormatVersion)
        {
            conflicts.Add(new ImportConflict("FormatVersion", $"Unsupported export format version {document.FormatVersion}."));
        }

        if (!string.Equals(document.AppId, "prodigee-tuts-point", StringComparison.Ordinal))
        {
            conflicts.Add(new ImportConflict("AppId", $"Unsupported app id '{document.AppId}'."));
        }

        await CheckVersionsAsync("track", document.Content.Tracks, db.Tracks.AsNoTracking().Select(track => new ContentItemVersion("track", track.Id, track.ContentVersion)), conflicts, cancellationToken);
        await CheckVersionsAsync("project", document.Content.Projects, db.Projects.AsNoTracking().Select(project => new ContentItemVersion("project", project.Id, project.ContentVersion)), conflicts, cancellationToken);
        await CheckVersionsAsync("milestone", document.Content.Milestones, db.ProjectMilestones.AsNoTracking().Select(milestone => new ContentItemVersion("milestone", milestone.Id, milestone.ContentVersion)), conflicts, cancellationToken);
        await CheckVersionsAsync("lesson", document.Content.Lessons, db.Lessons.AsNoTracking().Select(lesson => new ContentItemVersion("lesson", lesson.Id, lesson.ContentVersion)), conflicts, cancellationToken);
        await CheckVersionsAsync("exercise", document.Content.Exercises, db.Exercises.AsNoTracking().Select(exercise => new ContentItemVersion("exercise", exercise.Id, exercise.ContentVersion)), conflicts, cancellationToken);

        if (document.ProviderSettings.Any(provider => provider.SecretValuePresent))
        {
            conflicts.Add(new ImportConflict("ProviderSecrets", "Export payload must not contain provider secret values."));
        }

        return conflicts;
    }

    private static async Task CheckVersionsAsync(
        string kind,
        IReadOnlyCollection<ContentItemVersion> exported,
        IQueryable<ContentItemVersion> currentQuery,
        List<ImportConflict> conflicts,
        CancellationToken cancellationToken)
    {
        var current = await currentQuery.ToDictionaryAsync(item => item.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        foreach (var item in exported)
        {
            if (!current.TryGetValue(item.Id, out var currentItem))
            {
                conflicts.Add(new ImportConflict("MissingContent", $"Export references missing {kind} '{item.Id}'."));
                continue;
            }

            if (!string.Equals(currentItem.ContentVersion, item.ContentVersion, StringComparison.Ordinal))
            {
                conflicts.Add(new ImportConflict("ContentVersion", $"{kind} '{item.Id}' has version '{currentItem.ContentVersion}', export requires '{item.ContentVersion}'."));
            }
        }
    }

    private static int CountLearnerRows(LearnerStateExport state)
    {
        return state.PersonalNotes.Count
            + state.DiagnosticAttempts.Count
            + state.ConceptMasteryEvidence.Count
            + state.ExerciseAttempts.Count
            + state.ExerciseRunHistory.Count
            + state.StaticAnalysisDiagnostics.Count
            + state.ExerciseHintUsages.Count
            + state.ExerciseSolutionUnlocks.Count
            + state.ReviewCardAttempts.Count
            + state.StudyTimeEntries.Count
            + state.AiReviewResults.Count;
    }
}

public sealed record LearnerStateExportDocument(
    int FormatVersion,
    string AppId,
    string ProfileId,
    DateTimeOffset ExportedAt,
    ContentIndexMetadata Content,
    SourceMetadata SourceMetadata,
    IReadOnlyCollection<AiProviderSettingExport> ProviderSettings,
    LearnerStateExport LearnerState);

public sealed record ContentIndexMetadata(
    IReadOnlyCollection<ContentItemVersion> Tracks,
    IReadOnlyCollection<ContentItemVersion> Projects,
    IReadOnlyCollection<ContentItemVersion> Milestones,
    IReadOnlyCollection<ContentItemVersion> Lessons,
    IReadOnlyCollection<ContentItemVersion> Exercises);

public sealed record ContentItemVersion(string Kind, string Id, string ContentVersion);

public sealed record SourceMetadata(
    IReadOnlyCollection<SourceBookExport> Books,
    IReadOnlyCollection<SourceReferenceExport> References);

public sealed record SourceBookExport(
    string Id,
    string Title,
    string Author,
    string? Edition,
    string? Publisher,
    string? OwnershipStatus);

public sealed record SourceReferenceExport(
    string Id,
    string SourceBookId,
    string? LessonId,
    string? ProjectMilestoneId,
    string? Chapter,
    string? Pages,
    string Topic,
    string Usage);

public sealed record AiProviderSettingExport(
    string Id,
    string DisplayName,
    string Preset,
    string Endpoint,
    string Model,
    string? SecretName,
    bool IsEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool SecretValuePresent = false)
{
    public AiReviewProviderSetting ToEntity()
    {
        return new AiReviewProviderSetting
        {
            Id = Id,
            DisplayName = DisplayName,
            Preset = Preset,
            Endpoint = Endpoint,
            Model = Model,
            SecretName = SecretName,
            IsEnabled = IsEnabled && string.IsNullOrWhiteSpace(SecretName),
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
        };
    }
}

public sealed record LearnerStateExport(
    IReadOnlyCollection<PersonalNote> PersonalNotes,
    IReadOnlyCollection<DiagnosticAttempt> DiagnosticAttempts,
    IReadOnlyCollection<ConceptMasteryEvidence> ConceptMasteryEvidence,
    IReadOnlyCollection<ExerciseAttempt> ExerciseAttempts,
    IReadOnlyCollection<ExerciseRunHistory> ExerciseRunHistory,
    IReadOnlyCollection<StaticAnalysisDiagnosticRecord> StaticAnalysisDiagnostics,
    IReadOnlyCollection<ExerciseHintUsage> ExerciseHintUsages,
    IReadOnlyCollection<ExerciseSolutionUnlock> ExerciseSolutionUnlocks,
    IReadOnlyCollection<ReviewCard> ReviewCards,
    IReadOnlyCollection<ReviewCardAttempt> ReviewCardAttempts,
    IReadOnlyCollection<StudyTimeEntry> StudyTimeEntries,
    IReadOnlyCollection<AiReviewResult> AiReviewResults);

public sealed record ImportResult(
    bool Success,
    IReadOnlyCollection<ImportConflict> Conflicts,
    int ImportedRowCount);

public sealed record ImportConflict(string Code, string Message);
