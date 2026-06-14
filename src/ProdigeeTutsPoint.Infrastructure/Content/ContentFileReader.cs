using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ProdigeeTutsPoint.Infrastructure.Content;

public sealed class ContentFileReader(IHostEnvironment environment, IOptions<ContentOptions> options)
{
    public async Task<string> ReadMarkdownAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(relativePath);

        return File.Exists(fullPath)
            ? await File.ReadAllTextAsync(fullPath, cancellationToken)
            : string.Empty;
    }

    private string ResolvePath(string relativePath)
    {
        var rootPath = Path.GetFullPath(Path.IsPathRooted(options.Value.RootPath)
            ? options.Value.RootPath
            : Path.Combine(environment.ContentRootPath, options.Value.RootPath));

        var fullPath = Path.GetFullPath(Path.Combine(rootPath, relativePath));
        if (!fullPath.StartsWith(rootPath, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Content path escapes the content root.");
        }

        return fullPath;
    }
}
