using ProdigeeTutsPoint.Infrastructure.Content;

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
