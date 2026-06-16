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
}
