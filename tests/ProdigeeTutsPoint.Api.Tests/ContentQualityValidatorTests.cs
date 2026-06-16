using ProdigeeTutsPoint.Infrastructure.Content;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class ContentQualityValidatorTests
{
    [Fact]
    public void WordfreqCsharpSeedContentPassesQualityValidator()
    {
        var root = FindContentRoot();
        var result = new ContentQualityValidator().Validate(root);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic =>
            $"{diagnostic.Code} [{diagnostic.Scope}] {diagnostic.Message}")));
    }

    [Fact]
    public void ValidatorRejectsNonContiguousMilestoneOrdering()
    {
        var sourceRoot = FindContentRoot();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"prodigee-content-quality-{Guid.NewGuid():n}");
        CopyDirectory(sourceRoot, tempRoot);
        try
        {
            var trackPath = Path.Combine(tempRoot, "tracks", "csharp", "track.yml");
            var track = File.ReadAllText(trackPath);
            track = track.Replace(
                """
                      - id: cli-and-file-io
                        title: CLI and File I/O
                        summary: Wrap the pure analyzer with argument parsing, input reading, deterministic output, and user-facing failures.
                        markdown: tracks/csharp/projects/wordfreq/milestones/cli-and-file-io.md
                        order: 2
                """,
                """
                      - id: cli-and-file-io
                        title: CLI and File I/O
                        summary: Wrap the pure analyzer with argument parsing, input reading, deterministic output, and user-facing failures.
                        markdown: tracks/csharp/projects/wordfreq/milestones/cli-and-file-io.md
                        order: 4
                """,
                StringComparison.Ordinal);
            File.WriteAllText(trackPath, track);

            var result = new ContentQualityValidator().Validate(tempRoot);

            Assert.False(result.IsValid);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "MilestoneOrdering");
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static string FindContentRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var content = Path.Combine(directory.FullName, "content");
            if (Directory.Exists(content))
            {
                return content;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate content root.");
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(directory.Replace(source, destination, StringComparison.Ordinal));
        }

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, file.Replace(source, destination, StringComparison.Ordinal));
        }
    }
}
