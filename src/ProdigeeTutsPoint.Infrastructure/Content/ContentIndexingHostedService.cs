using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProdigeeTutsPoint.Infrastructure.Persistence;

namespace ProdigeeTutsPoint.Infrastructure.Content;

public sealed class ContentIndexingHostedService(
    IServiceProvider serviceProvider,
    ILogger<ContentIndexingHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureContentSchemaAsync(db, cancellationToken);
        await EnsureLearnerStateTablesAsync(db, cancellationToken);

        var indexer = scope.ServiceProvider.GetRequiredService<ContentIndexingService>();

        try
        {
            await indexer.IndexAsync(cancellationToken);
        }
        catch (ContentIndexingException exception)
        {
            foreach (var diagnostic in exception.Diagnostics)
            {
                logger.LogError("Content diagnostic: {Diagnostic}", diagnostic);
            }

            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task EnsureContentSchemaAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (!await ColumnExistsAsync(db, "Exercises", "Kind", cancellationToken))
        {
            await db.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "Exercises" ADD COLUMN "Kind" TEXT NOT NULL DEFAULT 'focused';""",
                cancellationToken);
        }
    }

    private static async Task<bool> ColumnExistsAsync(
        AppDbContext db,
        string tableName,
        string columnName,
        CancellationToken cancellationToken)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"""PRAGMA table_info("{tableName.Replace("\"", "\"\"", StringComparison.Ordinal)}");""";

        if (command.Connection?.State != System.Data.ConnectionState.Open)
        {
            await db.Database.OpenConnectionAsync(cancellationToken);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task EnsureLearnerStateTablesAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "PersonalNotes" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PersonalNotes" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "TargetType" TEXT NOT NULL,
                "TargetId" TEXT NOT NULL,
                "Body" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_PersonalNotes_ProfileId_TargetType_TargetId"
            ON "PersonalNotes" ("ProfileId", "TargetType", "TargetId");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "DiagnosticAttempts" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_DiagnosticAttempts" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "TrackId" TEXT NOT NULL,
                "Score" INTEGER NOT NULL,
                "MaxScore" INTEGER NOT NULL,
                "RecommendationLevel" TEXT NOT NULL,
                "RecommendationTargetId" TEXT NOT NULL,
                "RecommendationSummary" TEXT NOT NULL,
                "AnswersJson" TEXT NOT NULL,
                "SubmittedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_DiagnosticAttempts_ProfileId_TrackId_SubmittedAt"
            ON "DiagnosticAttempts" ("ProfileId", "TrackId", "SubmittedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ConceptMasteryEvidence" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ConceptMasteryEvidence" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "ConceptId" TEXT NOT NULL,
                "SourceType" TEXT NOT NULL,
                "SourceId" TEXT NOT NULL,
                "Score" INTEGER NOT NULL,
                "MaxScore" INTEGER NOT NULL,
                "Summary" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_ConceptMasteryEvidence_ProfileId_ConceptId_CreatedAt"
            ON "ConceptMasteryEvidence" ("ProfileId", "ConceptId", "CreatedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ExerciseAttempts" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ExerciseAttempts" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "ExerciseId" TEXT NOT NULL,
                "WorkspacePath" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "VisiblePassed" INTEGER NOT NULL,
                "HiddenPassed" INTEGER NOT NULL,
                "TimedOut" INTEGER NOT NULL,
                "ExitCode" INTEGER NULL,
                "Output" TEXT NOT NULL,
                "Diagnostics" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ExerciseAttempts_ProfileId_ExerciseId"
            ON "ExerciseAttempts" ("ProfileId", "ExerciseId");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ExerciseRunHistory" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ExerciseRunHistory" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "ExerciseId" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "VisiblePassed" INTEGER NOT NULL,
                "HiddenPassed" INTEGER NOT NULL,
                "TimedOut" INTEGER NOT NULL,
                "ExitCode" INTEGER NULL,
                "Summary" TEXT NOT NULL,
                "Output" TEXT NOT NULL,
                "Diagnostics" TEXT NOT NULL,
                "StaticAnalysisErrorCount" INTEGER NOT NULL,
                "StaticAnalysisWarningCount" INTEGER NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_ExerciseRunHistory_ProfileId_ExerciseId_CreatedAt"
            ON "ExerciseRunHistory" ("ProfileId", "ExerciseId", "CreatedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "StaticAnalysisDiagnostics" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_StaticAnalysisDiagnostics" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "ExerciseId" TEXT NOT NULL,
                "RunHistoryId" TEXT NULL,
                "RuleId" TEXT NOT NULL,
                "Severity" TEXT NOT NULL,
                "Message" TEXT NOT NULL,
                "FilePath" TEXT NOT NULL,
                "Line" INTEGER NULL,
                "Column" INTEGER NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_StaticAnalysisDiagnostics_ProfileId_ExerciseId_CreatedAt"
            ON "StaticAnalysisDiagnostics" ("ProfileId", "ExerciseId", "CreatedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_StaticAnalysisDiagnostics_RunHistoryId"
            ON "StaticAnalysisDiagnostics" ("RunHistoryId");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ExerciseHintUsages" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ExerciseHintUsages" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "ExerciseId" TEXT NOT NULL,
                "HintId" TEXT NOT NULL,
                "HintLevel" TEXT NOT NULL,
                "UsedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_ExerciseHintUsages_ProfileId_ExerciseId_UsedAt"
            ON "ExerciseHintUsages" ("ProfileId", "ExerciseId", "UsedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ExerciseSolutionUnlocks" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ExerciseSolutionUnlocks" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "ExerciseId" TEXT NOT NULL,
                "Reason" TEXT NOT NULL,
                "UnlockedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ExerciseSolutionUnlocks_ProfileId_ExerciseId"
            ON "ExerciseSolutionUnlocks" ("ProfileId", "ExerciseId");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "AiReviewProviderSettings" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AiReviewProviderSettings" PRIMARY KEY,
                "DisplayName" TEXT NOT NULL,
                "Preset" TEXT NOT NULL,
                "Endpoint" TEXT NOT NULL,
                "Model" TEXT NOT NULL,
                "SecretName" TEXT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "AiReviewResults" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AiReviewResults" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "ProjectId" TEXT NOT NULL,
                "MilestoneId" TEXT NOT NULL,
                "ProviderId" TEXT NOT NULL,
                "ProviderPreset" TEXT NOT NULL,
                "Model" TEXT NOT NULL,
                "PromptVersion" TEXT NOT NULL,
                "RubricVersion" TEXT NOT NULL,
                "Policy" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "Score" INTEGER NOT NULL,
                "MaxScore" INTEGER NOT NULL,
                "Summary" TEXT NOT NULL,
                "StrengthsJson" TEXT NOT NULL,
                "RisksJson" TEXT NOT NULL,
                "NextStepsJson" TEXT NOT NULL,
                "RawOutput" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_AiReviewResults_ProfileId_ProjectId_MilestoneId_CreatedAt"
            ON "AiReviewResults" ("ProfileId", "ProjectId", "MilestoneId", "CreatedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ReviewCards" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ReviewCards" PRIMARY KEY,
                "ConceptId" TEXT NOT NULL,
                "Prompt" TEXT NOT NULL,
                "Answer" TEXT NOT NULL,
                "SourceType" TEXT NOT NULL,
                "SourceId" TEXT NOT NULL,
                "Order" INTEGER NOT NULL,
                "IsActive" INTEGER NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_ReviewCards_ConceptId_Order"
            ON "ReviewCards" ("ConceptId", "Order");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ReviewCardAttempts" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ReviewCardAttempts" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "ReviewCardId" TEXT NOT NULL,
                "ConceptId" TEXT NOT NULL,
                "Rating" TEXT NOT NULL,
                "Score" INTEGER NOT NULL,
                "MaxScore" INTEGER NOT NULL,
                "ReviewedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_ReviewCardAttempts_ProfileId_ReviewCardId_ReviewedAt"
            ON "ReviewCardAttempts" ("ProfileId", "ReviewCardId", "ReviewedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_ReviewCardAttempts_ProfileId_ConceptId_ReviewedAt"
            ON "ReviewCardAttempts" ("ProfileId", "ConceptId", "ReviewedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "StudyTimeEntries" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_StudyTimeEntries" PRIMARY KEY,
                "ProfileId" TEXT NOT NULL,
                "TargetType" TEXT NOT NULL,
                "TargetId" TEXT NOT NULL,
                "ActiveSeconds" INTEGER NOT NULL,
                "StartedAt" TEXT NOT NULL,
                "EndedAt" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_StudyTimeEntries_ProfileId_TargetType_TargetId_StartedAt"
            ON "StudyTimeEntries" ("ProfileId", "TargetType", "TargetId", "StartedAt");
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_StudyTimeEntries_ProfileId_StartedAt"
            ON "StudyTimeEntries" ("ProfileId", "StartedAt");
            """,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (!await db.AiReviewProviderSettings.AnyAsync(provider => provider.Id == "hosted-openai", cancellationToken))
        {
            db.AiReviewProviderSettings.Add(new ProdigeeTutsPoint.Domain.Learning.AiReviewProviderSetting
            {
                Id = "hosted-openai",
                DisplayName = "Hosted OpenAI",
                Preset = "HostedOpenAI",
                Endpoint = "https://api.openai.com/v1",
                Model = "gpt-4.1-mini",
                SecretName = "OPENAI_API_KEY",
                IsEnabled = false,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        if (!await db.AiReviewProviderSettings.AnyAsync(provider => provider.Id == "local-ollama", cancellationToken))
        {
            db.AiReviewProviderSettings.Add(new ProdigeeTutsPoint.Domain.Learning.AiReviewProviderSetting
            {
                Id = "local-ollama",
                DisplayName = "Local Ollama",
                Preset = "LocalOllama",
                Endpoint = "http://127.0.0.1:11434/v1",
                Model = "llama3.1",
                SecretName = null,
                IsEnabled = false,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        var reviewCards = new[]
        {
            new { Id = "csharp-strings-ascii-contract", ConceptId = "csharp-strings", Prompt = "What is the milestone 1 text-processing contract?", Answer = "Use an ASCII-first contract: lowercase ASCII text deterministically, preserve ASCII letters and digits as word characters, and document that Unicode segmentation is a later extension.", SourceType = "Lesson", SourceId = "text-as-data-csharp", Order = 1 },
            new { Id = "csharp-tokenization-empty-tokens", ConceptId = "csharp-tokenization", Prompt = "Why must tokenization avoid empty strings?", Answer = "Empty tokens make counts noisy, hide separator bugs, and create unstable tests. Separators should create boundaries, not words.", SourceType = "Lesson", SourceId = "normalization-and-tokenization", Order = 1 },
            new { Id = "csharp-dictionaries-update-pattern", ConceptId = "csharp-dictionaries", Prompt = "What is the safe dictionary update pattern for word counts?", Answer = "Use TryGetValue to read the current count; increment existing words and initialize missing words to one.", SourceType = "Lesson", SourceId = "dictionaries-as-frequency-maps", Order = 1 },
            new { Id = "csharp-ordering-tie-breaker", ConceptId = "csharp-ordering", Prompt = "What is the deterministic ordering rule for word frequencies?", Answer = "Sort by count descending, then by word ascending so ties are stable and testable.", SourceType = "Lesson", SourceId = "deterministic-ordering", Order = 1 },
            new { Id = "csharp-xunit-pure-functions", ConceptId = "csharp-xunit", Prompt = "Why are pure functions the right first test target?", Answer = "They make behavior observable through inputs and return values without console, file-system, or clock effects.", SourceType = "Lesson", SourceId = "testing-pure-functions-xunit", Order = 1 },
            new { Id = "csharp-api-design-core-boundary", ConceptId = "csharp-api-design", Prompt = "Why should the analyzer core avoid reading files directly?", Answer = "A pure core is easier to test, reuse, benchmark, and wrap later with CLI or file input without changing the parsing logic.", SourceType = "Lesson", SourceId = "designing-small-core-api", Order = 1 },
        };
        foreach (var card in reviewCards)
        {
            if (!await db.ReviewCards.AnyAsync(existing => existing.Id == card.Id, cancellationToken))
            {
                db.ReviewCards.Add(new ProdigeeTutsPoint.Domain.Learning.ReviewCard
                {
                    Id = card.Id,
                    ConceptId = card.ConceptId,
                    Prompt = card.Prompt,
                    Answer = card.Answer,
                    SourceType = card.SourceType,
                    SourceId = card.SourceId,
                    Order = card.Order,
                    IsActive = true,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
