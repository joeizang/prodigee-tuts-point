using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
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
        try
        {
            await db.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "Exercises" ADD COLUMN "Kind" TEXT NOT NULL DEFAULT 'focused';""",
                cancellationToken);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 1
            && exception.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
        {
            // Local single-user SQLite has no migrations yet; this keeps existing databases usable.
        }
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

        await db.SaveChangesAsync(cancellationToken);
    }
}
