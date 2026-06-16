using Microsoft.EntityFrameworkCore;
using ProdigeeTutsPoint.Domain.Content;
using ProdigeeTutsPoint.Domain.Learning;

namespace ProdigeeTutsPoint.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AiReviewProviderSetting> AiReviewProviderSettings => Set<AiReviewProviderSetting>();

    public DbSet<AiReviewResult> AiReviewResults => Set<AiReviewResult>();

    public DbSet<Concept> Concepts => Set<Concept>();

    public DbSet<ConceptMasteryEvidence> ConceptMasteryEvidence => Set<ConceptMasteryEvidence>();

    public DbSet<DiagnosticAttempt> DiagnosticAttempts => Set<DiagnosticAttempt>();

    public DbSet<ExerciseAttempt> ExerciseAttempts => Set<ExerciseAttempt>();

    public DbSet<ExerciseHintUsage> ExerciseHintUsages => Set<ExerciseHintUsage>();

    public DbSet<ExerciseRunHistory> ExerciseRunHistory => Set<ExerciseRunHistory>();

    public DbSet<ExerciseSolutionUnlock> ExerciseSolutionUnlocks => Set<ExerciseSolutionUnlock>();

    public DbSet<Exercise> Exercises => Set<Exercise>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<Module> Modules => Set<Module>();

    public DbSet<PersonalNote> PersonalNotes => Set<PersonalNote>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();

    public DbSet<ReviewCard> ReviewCards => Set<ReviewCard>();

    public DbSet<ReviewCardAttempt> ReviewCardAttempts => Set<ReviewCardAttempt>();

    public DbSet<SourceBook> SourceBooks => Set<SourceBook>();

    public DbSet<SourceReference> SourceReferences => Set<SourceReference>();

    public DbSet<StaticAnalysisDiagnosticRecord> StaticAnalysisDiagnostics => Set<StaticAnalysisDiagnosticRecord>();

    public DbSet<StudyTimeEntry> StudyTimeEntries => Set<StudyTimeEntry>();

    public DbSet<Track> Tracks => Set<Track>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
