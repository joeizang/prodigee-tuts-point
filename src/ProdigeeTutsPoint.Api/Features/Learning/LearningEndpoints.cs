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
            var attempt = await db.DiagnosticAttempts
                .Where(attempt => attempt.ProfileId == profileId && attempt.TrackId == "csharp")
                .OrderByDescending(attempt => attempt.SubmittedAt)
                .Select(attempt => new DiagnosticAttemptResponse(
                    attempt.Id,
                    attempt.TrackId,
                    attempt.Score,
                    attempt.MaxScore,
                    attempt.RecommendationLevel,
                    attempt.RecommendationTargetId,
                    attempt.RecommendationSummary,
                    attempt.SubmittedAt))
                .FirstOrDefaultAsync(ct);

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

        group.MapGet("/mastery/concepts", async (string profileId, AppDbContext db, CancellationToken ct) =>
        {
            var evidence = await db.ConceptMasteryEvidence
                .Where(evidence => evidence.ProfileId == profileId)
                .GroupBy(evidence => evidence.ConceptId)
                .Select(group => new ConceptMasterySummaryResponse(
                    group.Key,
                    group.Sum(evidence => evidence.Score),
                    group.Sum(evidence => evidence.MaxScore),
                    group.Count()))
                .ToListAsync(ct);

            return Results.Ok(evidence);
        });

        return group;
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
            "track" => db.Tracks.AnyAsync(track => track.Id == targetId, ct),
            "project" => db.Projects.AnyAsync(project => project.Id == targetId, ct),
            "milestone" => db.ProjectMilestones.AnyAsync(milestone => milestone.Id == targetId, ct),
            "lesson" => db.Lessons.AnyAsync(lesson => lesson.Id == targetId, ct),
            "exercise" => db.Exercises.AnyAsync(exercise => exercise.Id == targetId, ct),
            "concept" => db.Concepts.AnyAsync(concept => concept.Id == targetId, ct),
            "sourcereference" => db.SourceReferences.AnyAsync(reference => reference.Id == targetId, ct),
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
    int Score,
    int MaxScore,
    int EvidenceCount);

public sealed record ProblemResponse(string Message);
