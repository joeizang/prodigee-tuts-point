using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProdigeeTutsPoint.Infrastructure.Content;
using ProdigeeTutsPoint.Infrastructure.Persistence;

if (args is ["--index", var contentRootArg, var databasePathArg])
{
    var contentRootPath = Path.GetFullPath(contentRootArg);
    var databasePath = Path.GetFullPath(databasePathArg);
    Directory.CreateDirectory(Path.GetDirectoryName(databasePath) ?? ".");

    var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite($"Data Source={databasePath}")
        .Options;

    await using var db = new AppDbContext(dbOptions);
    await db.Database.EnsureCreatedAsync();

    var indexer = new ContentIndexingService(
        db,
        new ToolHostEnvironment(Directory.GetCurrentDirectory()),
        Options.Create(new ContentOptions { RootPath = contentRootPath }),
        NullLogger<ContentIndexingService>.Instance);

    await indexer.IndexAsync(CancellationToken.None);
    Console.WriteLine($"Content indexed: {contentRootPath} -> {databasePath}");
    return 0;
}

var contentRoot = args.Length > 0
    ? args[0]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "content"));

var validator = new ContentQualityValidator();
var result = validator.Validate(contentRoot);

if (result.IsValid)
{
    Console.WriteLine($"Content validation passed: {contentRoot}");
    return 0;
}

Console.Error.WriteLine($"Content validation failed: {contentRoot}");
foreach (var diagnostic in result.Diagnostics)
{
    Console.Error.WriteLine($"{diagnostic.Code} [{diagnostic.Scope}] {diagnostic.Message}");
}

return 1;

internal sealed class ToolHostEnvironment(string contentRootPath) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Production;

    public string ApplicationName { get; set; } = "ProdigeeTutsPoint.ContentValidation";

    public string ContentRootPath { get; set; } = contentRootPath;

    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
        new Microsoft.Extensions.FileProviders.NullFileProvider();
}
