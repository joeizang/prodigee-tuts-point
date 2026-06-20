using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProdigeeTutsPoint.Domain.Learning;
using ProdigeeTutsPoint.Infrastructure.Persistence;

namespace ProdigeeTutsPoint.Api.Features.Learning;

public static class LearningEndpoints
{
    private static readonly IReadOnlyList<DiagnosticQuestionResponse> CSharpDiagnosticQuestions =
    [
        new(
            "methods-return-values",
            "What should a small pure method usually expose?",
            "csharp-api-design",
            [
                new("a", "A clear return value with behavior visible through tests."),
                new("b", "Console output so callers can read the result."),
                new("c", "A mutable static field updated by side effect.")
            ],
            "a"),
        new(
            "strings-normalization",
            "For milestone 1, how should mixed-case ASCII input be handled?",
            "csharp-strings",
            [
                new("a", "Keep original casing so output matches input."),
                new("b", "Normalize to lowercase before counting."),
                new("c", "Reject any input with uppercase letters.")
            ],
            "b"),
        new(
            "collections-dictionary",
            "Which structure best models word counts in the first analyzer core?",
            "csharp-dictionaries",
            [
                new("a", "List<string> because words can repeat."),
                new("b", "Dictionary<string, int> because each word maps to its count."),
                new("c", "Queue<string> because words are processed in order.")
            ],
            "b"),
        new(
            "xunit-assertions",
            "What is the strongest visible test for empty input?",
            "csharp-xunit",
            [
                new("a", "Assert the analyzer returns an empty result."),
                new("b", "Assert the method does not print anything."),
                new("c", "Assert the implementation uses a loop.")
            ],
            "a"),
        new(
            "edge-cases",
            "Why should separator-only input have an explicit test?",
            "csharp-tokenization",
            [
                new("a", "It proves the tokenizer does not create empty words."),
                new("b", "It proves Unicode segmentation is complete."),
                new("c", "It proves sorting is faster.")
            ],
            "a"),
        new(
            "deterministic-ordering",
            "How should ties be ordered in milestone 1?",
            "csharp-ordering",
            [
                new("a", "Random order is fine because counts match."),
                new("b", "By original appearance in the input only."),
                new("c", "By word ascending after count descending.")
            ],
            "c")
    ];

    public static RouteGroupBuilder MapLearningEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/learner").WithTags("Learner");

        group.MapGet("/notes", async (
            string profileId,
            string targetType,
            string targetId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var normalizedTargetType = NormalizeTargetType(targetType);
            var note = await db.PersonalNotes
                .AsNoTracking()
                .Where(note =>
                    note.ProfileId == profileId
                    && note.TargetType == normalizedTargetType
                    && note.TargetId == targetId)
                .Select(note => new PersonalNoteResponse(
                    note.Id,
                    note.ProfileId,
                    note.TargetType,
                    note.TargetId,
                    note.Body,
                    note.UpdatedAt))
                .FirstOrDefaultAsync(ct);

            return Results.Ok(note);
        });

        group.MapPut("/notes", async (PersonalNoteUpsertRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var normalizedTargetType = NormalizeTargetType(request.TargetType);
            if (!await TargetExistsAsync(db, normalizedTargetType, request.TargetId, ct))
            {
                return Results.BadRequest(new ProblemResponse("Unknown note target."));
            }

            var now = DateTimeOffset.UtcNow;
            var note = await db.PersonalNotes
                .FirstOrDefaultAsync(note =>
                    note.ProfileId == request.ProfileId
                    && note.TargetType == normalizedTargetType
                    && note.TargetId == request.TargetId,
                    ct);

            if (note is null)
            {
                note = new PersonalNote
                {
                    Id = Guid.NewGuid().ToString("n"),
                    ProfileId = request.ProfileId,
                    TargetType = normalizedTargetType,
                    TargetId = request.TargetId,
                    Body = request.Body,
                    CreatedAt = now,
                    UpdatedAt = now,
                };
                db.PersonalNotes.Add(note);
            }
            else
            {
                note.Body = request.Body;
                note.UpdatedAt = now;
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new PersonalNoteResponse(
                note.Id,
                note.ProfileId,
                note.TargetType,
                note.TargetId,
                note.Body,
                note.UpdatedAt));
        });

        group.MapGet("/diagnostics/csharp", () => Results.Ok(new DiagnosticResponse(
            "csharp",
            "C# Readiness Diagnostic",
            "Checks methods, strings, collections, dictionaries, xUnit assertions, and edge-case reasoning.",
            CSharpDiagnosticQuestions.Select(question => question.WithoutCorrectAnswer()).ToList())));

        group.MapGet("/diagnostics/csharp/latest", async (string profileId, AppDbContext db, CancellationToken ct) =>
        {
            var attempts = await db.DiagnosticAttempts
                .AsNoTracking()
                .Where(attempt => attempt.ProfileId == profileId && attempt.TrackId == "csharp")
                .Select(attempt => new DiagnosticAttemptResponse(
                    attempt.Id,
                    attempt.TrackId,
                    attempt.Score,
                    attempt.MaxScore,
                    attempt.RecommendationLevel,
                    attempt.RecommendationTargetId,
                    attempt.RecommendationSummary,
                    attempt.SubmittedAt))
                .ToListAsync(ct);
            var attempt = attempts
                .OrderByDescending(attempt => attempt.SubmittedAt)
                .FirstOrDefault();

            return Results.Ok(attempt);
        });

        group.MapPost("/diagnostics/csharp/attempts", async (
            DiagnosticAttemptRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var submittedAnswers = request.Answers.ToDictionary(answer => answer.QuestionId, answer => answer.AnswerId);
            var score = CSharpDiagnosticQuestions.Count(question =>
                submittedAnswers.TryGetValue(question.Id, out var answerId) && answerId == question.CorrectAnswerId);
            var maxScore = CSharpDiagnosticQuestions.Count;
            var recommendation = BuildDiagnosticRecommendation(score, maxScore);
            var attempt = new DiagnosticAttempt
            {
                Id = Guid.NewGuid().ToString("n"),
                ProfileId = request.ProfileId,
                TrackId = "csharp",
                Score = score,
                MaxScore = maxScore,
                RecommendationLevel = recommendation.Level,
                RecommendationTargetId = recommendation.TargetId,
                RecommendationSummary = recommendation.Summary,
                AnswersJson = JsonSerializer.Serialize(request.Answers),
                SubmittedAt = DateTimeOffset.UtcNow,
            };

            db.DiagnosticAttempts.Add(attempt);

            foreach (var question in CSharpDiagnosticQuestions)
            {
                var correct = submittedAnswers.TryGetValue(question.Id, out var answerId)
                    && answerId == question.CorrectAnswerId;
                db.ConceptMasteryEvidence.Add(new ConceptMasteryEvidence
                {
                    Id = Guid.NewGuid().ToString("n"),
                    ProfileId = request.ProfileId,
                    ConceptId = question.ConceptId,
                    SourceType = "Diagnostic",
                    SourceId = attempt.Id,
                    Score = correct ? 1 : 0,
                    MaxScore = 1,
                    Summary = correct ? "Diagnostic answer correct." : "Diagnostic answer missed.",
                    CreatedAt = attempt.SubmittedAt,
                });
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new DiagnosticAttemptResponse(
                attempt.Id,
                attempt.TrackId,
                attempt.Score,
                attempt.MaxScore,
                attempt.RecommendationLevel,
                attempt.RecommendationTargetId,
                attempt.RecommendationSummary,
                attempt.SubmittedAt));
        });

        group.MapGet("/mastery/concepts", async (string profileId, string? trackId, AppDbContext db, CancellationToken ct) =>
        {
            var conceptQuery = db.Concepts.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(trackId))
            {
                conceptQuery = conceptQuery.Where(concept => concept.TrackId == trackId);
            }

            var conceptIds = await conceptQuery
                .Select(concept => concept.Id)
                .ToListAsync(ct);
            var conceptIdSet = conceptIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var evidenceRows = await db.ConceptMasteryEvidence
                .AsNoTracking()
                .Where(evidence => evidence.ProfileId == profileId && conceptIdSet.Contains(evidence.ConceptId))
                .ToListAsync(ct);
            var concepts = await conceptQuery
                .Select(concept => new { concept.Id, concept.Title })
                .ToListAsync(ct);
            var evidenceByConcept = evidenceRows.ToLookup(evidence => evidence.ConceptId);
            var evidence = concepts
                .Select(concept =>
                {
                    var rows = evidenceByConcept[concept.Id].ToArray();
                    var score = rows.Sum(row => row.Score);
                    var maxScore = rows.Sum(row => row.MaxScore);
                    DateTimeOffset? lastActivityAt = rows.Length == 0 ? null : rows.Max(row => row.CreatedAt);
                    return new ConceptMasterySummaryResponse(
                        concept.Id,
                        concept.Title,
                        score,
                        maxScore,
                        rows.Length,
                        MasteryStatus(score, maxScore, rows.Length, lastActivityAt),
                        lastActivityAt);
                })
                .OrderBy(concept => concept.ConceptId)
                .ToArray();

            return Results.Ok(evidence);
        });

        group.MapGet("/summary", async (string profileId, string? trackId, AppDbContext db, CancellationToken ct) =>
        {
            var conceptQuery = db.Concepts.AsNoTracking();
            var exerciseQuery = db.Exercises.AsNoTracking();
            var milestoneQuery = db.ProjectMilestones.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(trackId))
            {
                conceptQuery = conceptQuery.Where(concept => concept.TrackId == trackId);
                exerciseQuery = exerciseQuery.Where(exercise => exercise.TrackId == trackId);
                milestoneQuery = milestoneQuery.Where(milestone => milestone.Project!.TrackId == trackId);
            }

            var conceptIds = await conceptQuery
                .Select(concept => concept.Id)
                .ToListAsync(ct);
            var conceptIdSet = conceptIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var masteryRows = await db.ConceptMasteryEvidence
                .AsNoTracking()
                .Where(evidence => evidence.ProfileId == profileId && conceptIdSet.Contains(evidence.ConceptId))
                .ToListAsync(ct);
            var conceptCount = conceptIds.Count;
            var mastery = masteryRows
                .GroupBy(evidence => evidence.ConceptId)
                .Select(group =>
                {
                    var score = group.Sum(evidence => evidence.Score);
                    var maxScore = group.Sum(evidence => evidence.MaxScore);
                    var lastActivityAt = group.Max(evidence => evidence.CreatedAt);
                    return MasteryStatus(score, maxScore, group.Count(), lastActivityAt);
                })
                .ToArray();
            var reliableConcepts = mastery.Count(status => status == "Reliable");
            var reviewDueCount = await ReviewDueCountAsync(profileId, db, ct);
            var exerciseIds = await exerciseQuery
                .Select(exercise => exercise.Id)
                .ToListAsync(ct);
            var exerciseTotal = exerciseIds.Count;
            var exercisePassed = await db.ExerciseRunHistory
                .AsNoTracking()
                .Where(history =>
                    history.ProfileId == profileId
                    && history.Status == "Passed"
                    && exerciseIds.Contains(history.ExerciseId))
                .Select(history => history.ExerciseId)
                .Distinct()
                .CountAsync(ct);
            var milestoneTotal = await milestoneQuery.CountAsync(ct);
            var milestoneCompleted = exerciseTotal > 0 && exercisePassed >= exerciseTotal ? 1 : 0;
            var studyRows = await db.StudyTimeEntries
                .AsNoTracking()
                .Where(entry => entry.ProfileId == profileId)
                .Select(entry => new { entry.StartedAt, entry.ActiveSeconds })
                .ToListAsync(ct);
            var totalStudySeconds = studyRows.Sum(entry => entry.ActiveSeconds);
            var streakDays = StudyStreakDays(studyRows.Select(entry => entry.StartedAt));

            return Results.Ok(new LearnerSummaryResponse(
                reviewDueCount,
                streakDays,
                totalStudySeconds,
                milestoneCompleted,
                milestoneTotal,
                exercisePassed,
                exerciseTotal,
                reliableConcepts,
                conceptCount,
                "Private personal progress only. No social comparison is recorded or displayed."));
        });

        group.MapGet("/review/cards", async (string profileId, AppDbContext db, CancellationToken ct) =>
        {
            var cards = await db.ReviewCards
                .AsNoTracking()
                .Where(card => card.IsActive)
                .OrderBy(card => card.Order)
                .ToListAsync(ct);
            var attempts = await db.ReviewCardAttempts
                .AsNoTracking()
                .Where(attempt => attempt.ProfileId == profileId)
                .ToListAsync(ct);
            var attemptsByCard = attempts.ToLookup(attempt => attempt.ReviewCardId);
            var now = DateTimeOffset.UtcNow;
            return Results.Ok(cards.Select(card =>
            {
                var cardAttempts = attemptsByCard[card.Id].OrderByDescending(attempt => attempt.ReviewedAt).ToArray();
                var last = cardAttempts.FirstOrDefault();
                var dueAt = NextReviewAt(last);
                return new ReviewCardResponse(
                    card.Id,
                    card.ConceptId,
                    card.Prompt,
                    card.Answer,
                    card.SourceType,
                    card.SourceId,
                    last?.ReviewedAt,
                    dueAt,
                    dueAt <= now);
            }).ToArray());
        });

        group.MapPost("/review/cards/{cardId}/attempts", async (
            string cardId,
            ReviewCardAttemptRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var card = await db.ReviewCards
                .AsNoTracking()
                .FirstOrDefaultAsync(card => card.Id == cardId && card.IsActive, ct);
            if (card is null)
            {
                return Results.NotFound();
            }

            var now = DateTimeOffset.UtcNow;
            var score = request.Rating.Trim().ToLowerInvariant() switch
            {
                "again" => 0,
                "hard" => 1,
                "good" => 2,
                "easy" => 3,
                _ => 1,
            };
            var attempt = new ReviewCardAttempt
            {
                Id = Guid.NewGuid().ToString("n"),
                ProfileId = request.ProfileId,
                ReviewCardId = card.Id,
                ConceptId = card.ConceptId,
                Rating = request.Rating,
                Score = score,
                MaxScore = 3,
                ReviewedAt = now,
            };
            db.ReviewCardAttempts.Add(attempt);
            db.ConceptMasteryEvidence.Add(new ConceptMasteryEvidence
            {
                Id = Guid.NewGuid().ToString("n"),
                ProfileId = request.ProfileId,
                ConceptId = card.ConceptId,
                SourceType = "ReviewRecall",
                SourceId = attempt.Id,
                Score = score,
                MaxScore = 3,
                Summary = $"Review card rated {request.Rating}.",
                CreatedAt = now,
            });
            await db.SaveChangesAsync(ct);

            return Results.Ok(new ReviewCardAttemptResponse(attempt.Id, attempt.ReviewCardId, attempt.ConceptId, attempt.Rating, attempt.Score, attempt.MaxScore, attempt.ReviewedAt));
        });

        group.MapPost("/study-time", async (StudyTimeRecordRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var targetType = NormalizeTargetType(request.TargetType);
            if (!await TargetExistsAsync(db, targetType, request.TargetId, ct))
            {
                return Results.BadRequest(new ProblemResponse("Unknown study-time target."));
            }

            var activeSeconds = Math.Clamp(request.ActiveSeconds, 1, 15 * 60);
            var endedAt = request.EndedAt ?? DateTimeOffset.UtcNow;
            var startedAt = request.StartedAt ?? endedAt.AddSeconds(-activeSeconds);
            var entry = new StudyTimeEntry
            {
                Id = Guid.NewGuid().ToString("n"),
                ProfileId = request.ProfileId,
                TargetType = targetType,
                TargetId = request.TargetId,
                ActiveSeconds = activeSeconds,
                StartedAt = startedAt,
                EndedAt = endedAt,
            };
            db.StudyTimeEntries.Add(entry);

            if (activeSeconds >= 30)
            {
                foreach (var conceptId in await ConceptIdsForTargetAsync(db, targetType, request.TargetId, ct))
                {
                    db.ConceptMasteryEvidence.Add(new ConceptMasteryEvidence
                    {
                        Id = Guid.NewGuid().ToString("n"),
                        ProfileId = request.ProfileId,
                        ConceptId = conceptId,
                        SourceType = "StudyTime",
                        SourceId = entry.Id,
                        Score = targetType is "project" or "milestone" ? 2 : 1,
                        MaxScore = 3,
                        Summary = $"Studied {targetType} for {activeSeconds} active second(s).",
                        CreatedAt = endedAt,
                    });
                }
            }

            await db.SaveChangesAsync(ct);
            return Results.Ok(new StudyTimeRecordResponse(entry.Id, entry.TargetType, entry.TargetId, entry.ActiveSeconds, entry.StartedAt, entry.EndedAt));
        });

        return group;
    }

    private static async Task<int> ReviewDueCountAsync(string profileId, AppDbContext db, CancellationToken ct)
    {
        var cards = await db.ReviewCards.AsNoTracking().Where(card => card.IsActive).Select(card => card.Id).ToListAsync(ct);
        var attempts = await db.ReviewCardAttempts
            .AsNoTracking()
            .Where(attempt => attempt.ProfileId == profileId)
            .ToListAsync(ct);
        var attemptsByCard = attempts.ToLookup(attempt => attempt.ReviewCardId);
        var now = DateTimeOffset.UtcNow;
        return cards.Count(cardId => NextReviewAt(attemptsByCard[cardId].OrderByDescending(attempt => attempt.ReviewedAt).FirstOrDefault()) <= now);
    }

    private static DateTimeOffset NextReviewAt(ReviewCardAttempt? latest)
    {
        if (latest is null)
        {
            return DateTimeOffset.MinValue;
        }

        var interval = latest.Rating.Trim().ToLowerInvariant() switch
        {
            "easy" => TimeSpan.FromDays(7),
            "good" => TimeSpan.FromDays(3),
            "hard" => TimeSpan.FromDays(1),
            _ => TimeSpan.Zero,
        };
        return latest.ReviewedAt.Add(interval);
    }

    private static string MasteryStatus(int score, int maxScore, int evidenceCount, DateTimeOffset? lastActivityAt)
    {
        if (evidenceCount == 0 || maxScore == 0)
        {
            return "Not Started";
        }

        if (lastActivityAt is not null && lastActivityAt.Value < DateTimeOffset.UtcNow.AddDays(-14))
        {
            return "Needs Review";
        }

        var ratio = (double)score / maxScore;
        return (evidenceCount, ratio) switch
        {
            (_, < 0.5) => "Needs Review",
            (1, _) => "Introduced",
            (>= 2 and < 4, < 0.85) => "Practiced",
            (>= 2 and < 4, _) => "Applied",
            (>= 4, >= 0.85) => "Reliable",
            _ => "Applied",
        };
    }

    private static int StudyStreakDays(IEnumerable<DateTimeOffset> timestamps)
    {
        var days = timestamps.Select(timestamp => timestamp.UtcDateTime.Date).Distinct().ToHashSet();
        var current = DateTimeOffset.UtcNow.UtcDateTime.Date;
        var streak = 0;
        while (days.Contains(current))
        {
            streak++;
            current = current.AddDays(-1);
        }

        return streak;
    }

    private static async Task<IReadOnlyCollection<string>> ConceptIdsForTargetAsync(
        AppDbContext db,
        string targetType,
        string targetId,
        CancellationToken ct)
    {
        return targetType switch
        {
            "lesson" => await db.Set<ProdigeeTutsPoint.Domain.Content.LessonConcept>()
                .AsNoTracking()
                .Where(link => link.LessonId == targetId)
                .Select(link => link.ConceptId)
                .ToListAsync(ct),
            "exercise" => await db.Set<ProdigeeTutsPoint.Domain.Content.ExerciseConcept>()
                .AsNoTracking()
                .Where(link => link.ExerciseId == targetId)
                .Select(link => link.ConceptId)
                .ToListAsync(ct),
            "project" => await db.ProjectMilestones
                .AsNoTracking()
                .Where(milestone => milestone.ProjectId == targetId)
                .SelectMany(milestone => milestone.Exercises)
                .SelectMany(link => link.Exercise!.Concepts)
                .Select(link => link.ConceptId)
                .Distinct()
                .ToListAsync(ct),
            "milestone" => await db.ProjectMilestones
                .AsNoTracking()
                .Where(milestone => milestone.Id == targetId)
                .SelectMany(milestone => milestone.Exercises)
                .SelectMany(link => link.Exercise!.Concepts)
                .Select(link => link.ConceptId)
                .Distinct()
                .ToListAsync(ct),
            "review" => await db.Concepts.AsNoTracking().Select(concept => concept.Id).ToListAsync(ct),
            _ => [],
        };
    }

    private static DiagnosticRecommendation BuildDiagnosticRecommendation(int score, int maxScore)
    {
        var ratio = (double)score / maxScore;
        return ratio switch
        {
            < 0.5 => new DiagnosticRecommendation(
                "Primer",
                "text-as-data-csharp",
                "Start with the C# text and collections primers before attempting the milestone."),
            < 0.85 => new DiagnosticRecommendation(
                "Drills",
                "normalize-to-lowercase",
                "Work through the focused drills, then return to the milestone."),
            _ => new DiagnosticRecommendation(
                "Milestone",
                "pure-word-counting-core",
                "You are ready to start wordfreq-csharp milestone 1."),
        };
    }

    private static string NormalizeTargetType(string targetType)
    {
        return targetType.Trim().ToLowerInvariant();
    }

    private static Task<bool> TargetExistsAsync(
        AppDbContext db,
        string targetType,
        string targetId,
        CancellationToken ct)
    {
        return targetType switch
        {
            "track" => db.Tracks.AsNoTracking().AnyAsync(track => track.Id == targetId, ct),
            "project" => db.Projects.AsNoTracking().AnyAsync(project => project.Id == targetId, ct),
            "milestone" => db.ProjectMilestones.AsNoTracking().AnyAsync(milestone => milestone.Id == targetId, ct),
            "lesson" => db.Lessons.AsNoTracking().AnyAsync(lesson => lesson.Id == targetId, ct),
            "exercise" => db.Exercises.AsNoTracking().AnyAsync(exercise => exercise.Id == targetId, ct),
            "concept" => db.Concepts.AsNoTracking().AnyAsync(concept => concept.Id == targetId, ct),
            "sourcereference" => db.SourceReferences.AsNoTracking().AnyAsync(reference => reference.Id == targetId, ct),
            "review" => Task.FromResult(targetId == "csharp"),
            _ => Task.FromResult(false),
        };
    }

    private sealed record DiagnosticRecommendation(string Level, string TargetId, string Summary);

    private sealed record DiagnosticQuestionResponse(
        string Id,
        string Prompt,
        string ConceptId,
        IReadOnlyCollection<DiagnosticAnswerResponse> Answers,
        string CorrectAnswerId)
    {
        public DiagnosticQuestionPublicResponse WithoutCorrectAnswer()
        {
            return new DiagnosticQuestionPublicResponse(Id, Prompt, ConceptId, Answers);
        }
    }
}

public sealed record PersonalNoteUpsertRequest(
    string ProfileId,
    string TargetType,
    string TargetId,
    string Body);

public sealed record PersonalNoteResponse(
    string Id,
    string ProfileId,
    string TargetType,
    string TargetId,
    string Body,
    DateTimeOffset UpdatedAt);

public sealed record DiagnosticResponse(
    string TrackId,
    string Title,
    string Summary,
    IReadOnlyCollection<DiagnosticQuestionPublicResponse> Questions);

public sealed record DiagnosticQuestionPublicResponse(
    string Id,
    string Prompt,
    string ConceptId,
    IReadOnlyCollection<DiagnosticAnswerResponse> Answers);

public sealed record DiagnosticAnswerResponse(string Id, string Text);

public sealed record DiagnosticAttemptRequest(
    string ProfileId,
    IReadOnlyCollection<DiagnosticAnswerSubmission> Answers);

public sealed record DiagnosticAnswerSubmission(string QuestionId, string AnswerId);

public sealed record DiagnosticAttemptResponse(
    string Id,
    string TrackId,
    int Score,
    int MaxScore,
    string RecommendationLevel,
    string RecommendationTargetId,
    string RecommendationSummary,
    DateTimeOffset SubmittedAt);

public sealed record ConceptMasterySummaryResponse(
    string ConceptId,
    string Title,
    int Score,
    int MaxScore,
    int EvidenceCount,
    string Status,
    DateTimeOffset? LastActivityAt);

public sealed record LearnerSummaryResponse(
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

public sealed record ReviewCardResponse(
    string Id,
    string ConceptId,
    string Prompt,
    string Answer,
    string SourceType,
    string SourceId,
    DateTimeOffset? LastReviewedAt,
    DateTimeOffset DueAt,
    bool IsDue);

public sealed record ReviewCardAttemptRequest(
    string ProfileId,
    string Rating);

public sealed record ReviewCardAttemptResponse(
    string Id,
    string ReviewCardId,
    string ConceptId,
    string Rating,
    int Score,
    int MaxScore,
    DateTimeOffset ReviewedAt);

public sealed record StudyTimeRecordRequest(
    string ProfileId,
    string TargetType,
    string TargetId,
    int ActiveSeconds,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt);

public sealed record StudyTimeRecordResponse(
    string Id,
    string TargetType,
    string TargetId,
    int ActiveSeconds,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt);

public sealed record ProblemResponse(string Message);
