using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdigeeTutsPoint.Domain.Content;

namespace ProdigeeTutsPoint.Infrastructure.Persistence;

public sealed class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.HasKey(track => track.Id);
        builder.Property(track => track.Id).HasMaxLength(120);
        builder.Property(track => track.Title).HasMaxLength(200);
        builder.Property(track => track.Slug).HasMaxLength(120);
        builder.Property(track => track.Language).HasMaxLength(80);
        builder.Property(track => track.ContentVersion).HasMaxLength(80);
    }
}

public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(module => module.Id);
        builder.Property(module => module.Id).HasMaxLength(120);
        builder.Property(module => module.TrackId).HasMaxLength(120);
        builder.Property(module => module.Title).HasMaxLength(200);
        builder.HasIndex(module => new { module.TrackId, module.Order });
    }
}

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.HasKey(lesson => lesson.Id);
        builder.Property(lesson => lesson.Id).HasMaxLength(120);
        builder.Property(lesson => lesson.TrackId).HasMaxLength(120);
        builder.Property(lesson => lesson.ModuleId).HasMaxLength(120);
        builder.Property(lesson => lesson.Title).HasMaxLength(200);
        builder.Property(lesson => lesson.MarkdownPath).HasMaxLength(500);
        builder.Property(lesson => lesson.ContentVersion).HasMaxLength(80);
        builder.HasIndex(lesson => new { lesson.TrackId, lesson.Order });
    }
}

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Id).HasMaxLength(120);
        builder.Property(project => project.TrackId).HasMaxLength(120);
        builder.Property(project => project.Title).HasMaxLength(200);
        builder.Property(project => project.Slug).HasMaxLength(120);
        builder.Property(project => project.Language).HasMaxLength(80);
        builder.Property(project => project.ContentVersion).HasMaxLength(80);
    }
}

public sealed class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
    {
        builder.HasKey(milestone => milestone.Id);
        builder.Property(milestone => milestone.Id).HasMaxLength(120);
        builder.Property(milestone => milestone.ProjectId).HasMaxLength(120);
        builder.Property(milestone => milestone.Title).HasMaxLength(200);
        builder.Property(milestone => milestone.MarkdownPath).HasMaxLength(500);
        builder.Property(milestone => milestone.ContentVersion).HasMaxLength(80);
        builder.HasIndex(milestone => new { milestone.ProjectId, milestone.Order });
    }
}

public sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasKey(exercise => exercise.Id);
        builder.Property(exercise => exercise.Id).HasMaxLength(120);
        builder.Property(exercise => exercise.TrackId).HasMaxLength(120);
        builder.Property(exercise => exercise.Title).HasMaxLength(200);
        builder.Property(exercise => exercise.Language).HasMaxLength(80);
        builder.Property(exercise => exercise.Kind).HasMaxLength(80);
        builder.Property(exercise => exercise.DirectoryPath).HasMaxLength(500);
        builder.Property(exercise => exercise.ContentVersion).HasMaxLength(80);
        builder.HasIndex(exercise => new { exercise.TrackId, exercise.Order });
    }
}

public sealed class ConceptConfiguration : IEntityTypeConfiguration<Concept>
{
    public void Configure(EntityTypeBuilder<Concept> builder)
    {
        builder.HasKey(concept => concept.Id);
        builder.Property(concept => concept.Id).HasMaxLength(120);
        builder.Property(concept => concept.TrackId).HasMaxLength(120);
        builder.Property(concept => concept.Title).HasMaxLength(200);
    }
}

public sealed class SourceBookConfiguration : IEntityTypeConfiguration<SourceBook>
{
    public void Configure(EntityTypeBuilder<SourceBook> builder)
    {
        builder.HasKey(book => book.Id);
        builder.Property(book => book.Id).HasMaxLength(120);
        builder.Property(book => book.Title).HasMaxLength(240);
        builder.Property(book => book.Author).HasMaxLength(200);
        builder.Property(book => book.Edition).HasMaxLength(80);
        builder.Property(book => book.Publisher).HasMaxLength(120);
        builder.Property(book => book.OwnershipStatus).HasMaxLength(80);
    }
}

public sealed class SourceReferenceConfiguration : IEntityTypeConfiguration<SourceReference>
{
    public void Configure(EntityTypeBuilder<SourceReference> builder)
    {
        builder.HasKey(reference => reference.Id);
        builder.Property(reference => reference.Id).HasMaxLength(180);
        builder.Property(reference => reference.SourceBookId).HasMaxLength(120);
        builder.Property(reference => reference.LessonId).HasMaxLength(120);
        builder.Property(reference => reference.ProjectMilestoneId).HasMaxLength(120);
        builder.Property(reference => reference.Chapter).HasMaxLength(200);
        builder.Property(reference => reference.Pages).HasMaxLength(60);
        builder.Property(reference => reference.Topic).HasMaxLength(400);
        builder.Property(reference => reference.Usage).HasMaxLength(80);
    }
}

public sealed class LessonConceptConfiguration : IEntityTypeConfiguration<LessonConcept>
{
    public void Configure(EntityTypeBuilder<LessonConcept> builder)
    {
        builder.HasKey(link => new { link.LessonId, link.ConceptId });
        builder.Property(link => link.LessonId).HasMaxLength(120);
        builder.Property(link => link.ConceptId).HasMaxLength(120);
    }
}

public sealed class ExerciseConceptConfiguration : IEntityTypeConfiguration<ExerciseConcept>
{
    public void Configure(EntityTypeBuilder<ExerciseConcept> builder)
    {
        builder.HasKey(link => new { link.ExerciseId, link.ConceptId });
        builder.Property(link => link.ExerciseId).HasMaxLength(120);
        builder.Property(link => link.ConceptId).HasMaxLength(120);
    }
}

public sealed class MilestoneLessonConfiguration : IEntityTypeConfiguration<MilestoneLesson>
{
    public void Configure(EntityTypeBuilder<MilestoneLesson> builder)
    {
        builder.HasKey(link => new { link.ProjectMilestoneId, link.LessonId });
        builder.Property(link => link.ProjectMilestoneId).HasMaxLength(120);
        builder.Property(link => link.LessonId).HasMaxLength(120);
    }
}

public sealed class MilestoneExerciseConfiguration : IEntityTypeConfiguration<MilestoneExercise>
{
    public void Configure(EntityTypeBuilder<MilestoneExercise> builder)
    {
        builder.HasKey(link => new { link.ProjectMilestoneId, link.ExerciseId });
        builder.Property(link => link.ProjectMilestoneId).HasMaxLength(120);
        builder.Property(link => link.ExerciseId).HasMaxLength(120);
    }
}
