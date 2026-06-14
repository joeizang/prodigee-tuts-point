using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdigeeTutsPoint.Domain.Learning;

namespace ProdigeeTutsPoint.Infrastructure.Persistence;

public sealed class PersonalNoteConfiguration : IEntityTypeConfiguration<PersonalNote>
{
    public void Configure(EntityTypeBuilder<PersonalNote> builder)
    {
        builder.HasKey(note => note.Id);
        builder.Property(note => note.Id).HasMaxLength(80);
        builder.Property(note => note.ProfileId).HasMaxLength(120);
        builder.Property(note => note.TargetType).HasMaxLength(40);
        builder.Property(note => note.TargetId).HasMaxLength(180);
        builder.Property(note => note.Body).HasMaxLength(12000);
        builder.HasIndex(note => new { note.ProfileId, note.TargetType, note.TargetId }).IsUnique();
    }
}

public sealed class DiagnosticAttemptConfiguration : IEntityTypeConfiguration<DiagnosticAttempt>
{
    public void Configure(EntityTypeBuilder<DiagnosticAttempt> builder)
    {
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.Id).HasMaxLength(80);
        builder.Property(attempt => attempt.ProfileId).HasMaxLength(120);
        builder.Property(attempt => attempt.TrackId).HasMaxLength(120);
        builder.Property(attempt => attempt.RecommendationLevel).HasMaxLength(80);
        builder.Property(attempt => attempt.RecommendationTargetId).HasMaxLength(120);
        builder.Property(attempt => attempt.RecommendationSummary).HasMaxLength(500);
        builder.Property(attempt => attempt.AnswersJson).HasMaxLength(12000);
        builder.HasIndex(attempt => new { attempt.ProfileId, attempt.TrackId, attempt.SubmittedAt });
    }
}

public sealed class ConceptMasteryEvidenceConfiguration : IEntityTypeConfiguration<ConceptMasteryEvidence>
{
    public void Configure(EntityTypeBuilder<ConceptMasteryEvidence> builder)
    {
        builder.HasKey(evidence => evidence.Id);
        builder.Property(evidence => evidence.Id).HasMaxLength(80);
        builder.Property(evidence => evidence.ProfileId).HasMaxLength(120);
        builder.Property(evidence => evidence.ConceptId).HasMaxLength(120);
        builder.Property(evidence => evidence.SourceType).HasMaxLength(80);
        builder.Property(evidence => evidence.SourceId).HasMaxLength(120);
        builder.Property(evidence => evidence.Summary).HasMaxLength(500);
        builder.HasIndex(evidence => new { evidence.ProfileId, evidence.ConceptId, evidence.CreatedAt });
    }
}

public sealed class ExerciseAttemptConfiguration : IEntityTypeConfiguration<ExerciseAttempt>
{
    public void Configure(EntityTypeBuilder<ExerciseAttempt> builder)
    {
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.Id).HasMaxLength(180);
        builder.Property(attempt => attempt.ProfileId).HasMaxLength(120);
        builder.Property(attempt => attempt.ExerciseId).HasMaxLength(120);
        builder.Property(attempt => attempt.WorkspacePath).HasMaxLength(600);
        builder.Property(attempt => attempt.Status).HasMaxLength(80);
        builder.Property(attempt => attempt.Output).HasMaxLength(24000);
        builder.Property(attempt => attempt.Diagnostics).HasMaxLength(12000);
        builder.HasIndex(attempt => new { attempt.ProfileId, attempt.ExerciseId }).IsUnique();
    }
}

public sealed class ExerciseRunHistoryConfiguration : IEntityTypeConfiguration<ExerciseRunHistory>
{
    public void Configure(EntityTypeBuilder<ExerciseRunHistory> builder)
    {
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).HasMaxLength(80);
        builder.Property(history => history.ProfileId).HasMaxLength(120);
        builder.Property(history => history.ExerciseId).HasMaxLength(120);
        builder.Property(history => history.Status).HasMaxLength(80);
        builder.Property(history => history.Summary).HasMaxLength(500);
        builder.Property(history => history.Output).HasMaxLength(24000);
        builder.Property(history => history.Diagnostics).HasMaxLength(12000);
        builder.HasIndex(history => new { history.ProfileId, history.ExerciseId, history.CreatedAt });
    }
}

public sealed class StaticAnalysisDiagnosticRecordConfiguration : IEntityTypeConfiguration<StaticAnalysisDiagnosticRecord>
{
    public void Configure(EntityTypeBuilder<StaticAnalysisDiagnosticRecord> builder)
    {
        builder.HasKey(diagnostic => diagnostic.Id);
        builder.Property(diagnostic => diagnostic.Id).HasMaxLength(80);
        builder.Property(diagnostic => diagnostic.ProfileId).HasMaxLength(120);
        builder.Property(diagnostic => diagnostic.ExerciseId).HasMaxLength(120);
        builder.Property(diagnostic => diagnostic.RunHistoryId).HasMaxLength(80);
        builder.Property(diagnostic => diagnostic.RuleId).HasMaxLength(80);
        builder.Property(diagnostic => diagnostic.Severity).HasMaxLength(40);
        builder.Property(diagnostic => diagnostic.Message).HasMaxLength(1200);
        builder.Property(diagnostic => diagnostic.FilePath).HasMaxLength(600);
        builder.HasIndex(diagnostic => new { diagnostic.ProfileId, diagnostic.ExerciseId, diagnostic.CreatedAt });
        builder.HasIndex(diagnostic => diagnostic.RunHistoryId);
    }
}

public sealed class ExerciseHintUsageConfiguration : IEntityTypeConfiguration<ExerciseHintUsage>
{
    public void Configure(EntityTypeBuilder<ExerciseHintUsage> builder)
    {
        builder.HasKey(usage => usage.Id);
        builder.Property(usage => usage.Id).HasMaxLength(80);
        builder.Property(usage => usage.ProfileId).HasMaxLength(120);
        builder.Property(usage => usage.ExerciseId).HasMaxLength(120);
        builder.Property(usage => usage.HintId).HasMaxLength(120);
        builder.Property(usage => usage.HintLevel).HasMaxLength(80);
        builder.HasIndex(usage => new { usage.ProfileId, usage.ExerciseId, usage.UsedAt });
    }
}

public sealed class ExerciseSolutionUnlockConfiguration : IEntityTypeConfiguration<ExerciseSolutionUnlock>
{
    public void Configure(EntityTypeBuilder<ExerciseSolutionUnlock> builder)
    {
        builder.HasKey(unlock => unlock.Id);
        builder.Property(unlock => unlock.Id).HasMaxLength(80);
        builder.Property(unlock => unlock.ProfileId).HasMaxLength(120);
        builder.Property(unlock => unlock.ExerciseId).HasMaxLength(120);
        builder.Property(unlock => unlock.Reason).HasMaxLength(400);
        builder.HasIndex(unlock => new { unlock.ProfileId, unlock.ExerciseId }).IsUnique();
    }
}

public sealed class AiReviewProviderSettingConfiguration : IEntityTypeConfiguration<AiReviewProviderSetting>
{
    public void Configure(EntityTypeBuilder<AiReviewProviderSetting> builder)
    {
        builder.HasKey(provider => provider.Id);
        builder.Property(provider => provider.Id).HasMaxLength(80);
        builder.Property(provider => provider.DisplayName).HasMaxLength(160);
        builder.Property(provider => provider.Preset).HasMaxLength(80);
        builder.Property(provider => provider.Endpoint).HasMaxLength(500);
        builder.Property(provider => provider.Model).HasMaxLength(160);
        builder.Property(provider => provider.SecretName).HasMaxLength(160);
    }
}

public sealed class AiReviewResultConfiguration : IEntityTypeConfiguration<AiReviewResult>
{
    public void Configure(EntityTypeBuilder<AiReviewResult> builder)
    {
        builder.HasKey(review => review.Id);
        builder.Property(review => review.Id).HasMaxLength(80);
        builder.Property(review => review.ProfileId).HasMaxLength(120);
        builder.Property(review => review.ProjectId).HasMaxLength(120);
        builder.Property(review => review.MilestoneId).HasMaxLength(120);
        builder.Property(review => review.ProviderId).HasMaxLength(80);
        builder.Property(review => review.ProviderPreset).HasMaxLength(80);
        builder.Property(review => review.Model).HasMaxLength(160);
        builder.Property(review => review.PromptVersion).HasMaxLength(80);
        builder.Property(review => review.RubricVersion).HasMaxLength(80);
        builder.Property(review => review.Policy).HasMaxLength(80);
        builder.Property(review => review.Status).HasMaxLength(80);
        builder.Property(review => review.Summary).HasMaxLength(2000);
        builder.Property(review => review.StrengthsJson).HasMaxLength(8000);
        builder.Property(review => review.RisksJson).HasMaxLength(8000);
        builder.Property(review => review.NextStepsJson).HasMaxLength(8000);
        builder.Property(review => review.RawOutput).HasMaxLength(24000);
        builder.HasIndex(review => new { review.ProfileId, review.ProjectId, review.MilestoneId, review.CreatedAt });
    }
}
