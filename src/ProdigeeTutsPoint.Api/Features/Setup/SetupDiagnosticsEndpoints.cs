using System.Diagnostics;
using ProdigeeTutsPoint.Infrastructure.Content;

namespace ProdigeeTutsPoint.Api.Features.Setup;

public static class SetupDiagnosticsEndpoints
{
    public static RouteGroupBuilder MapSetupDiagnosticsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/setup").WithTags("Setup");

        group.MapGet("/diagnostics", async (IWebHostEnvironment environment, CancellationToken ct) =>
        {
            var checks = new List<SetupCheckResponse>
            {
                await CheckDotnetAsync(ct),
                CheckContent(environment),
                CheckFrontendBuild(environment),
                await CheckOllamaAsync(ct),
            };

            return Results.Ok(new SetupDiagnosticsResponse(checks.All(check => check.Status != "error"), checks));
        });

        return group;
    }

    private static async Task<SetupCheckResponse> CheckDotnetAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                ArgumentList = { "--version" },
            });
            if (process is null)
            {
                return new SetupCheckResponse("dotnet-sdk", "error", "Could not start dotnet. Install the .NET SDK and ensure dotnet is on PATH.");
            }
            await process.WaitForExitAsync(cancellationToken);
            var version = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            return process.ExitCode == 0
                ? new SetupCheckResponse("dotnet-sdk", "ok", $".NET SDK detected: {version.Trim()}.")
                : new SetupCheckResponse("dotnet-sdk", "error", "dotnet exists but did not report a usable SDK version.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new SetupCheckResponse("dotnet-sdk", "error", $"Install the .NET SDK. dotnet check failed: {exception.Message}");
        }
    }

    private static SetupCheckResponse CheckContent(IWebHostEnvironment environment)
    {
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "content"));
        var result = new ContentQualityValidator().Validate(root);
        return result.IsValid
            ? new SetupCheckResponse("content", "ok", "Seed content passes the content quality validator.")
            : new SetupCheckResponse("content", "error", $"Content validation failed: {string.Join(" | ", result.Diagnostics.Take(5).Select(diagnostic => diagnostic.Message))}");
    }

    private static SetupCheckResponse CheckFrontendBuild(IWebHostEnvironment environment)
    {
        var indexPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "ProdigeeTutsPoint.Web", "dist", "index.html"));
        return File.Exists(indexPath)
            ? new SetupCheckResponse("frontend-build", "ok", "Built frontend assets are available for ASP.NET hosting.")
            : new SetupCheckResponse("frontend-build", "warning", "Frontend dist assets are missing. Run npm run build for single-server ASP.NET hosting, or run the Vite dev server separately.");
    }

    private static async Task<SetupCheckResponse> CheckOllamaAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        try
        {
            using var response = await client.GetAsync("http://127.0.0.1:11434/api/tags", cancellationToken);
            return response.IsSuccessStatusCode
                ? new SetupCheckResponse("ollama", "ok", "Local Ollama responded on 127.0.0.1:11434.")
                : new SetupCheckResponse("ollama", "warning", $"Ollama endpoint responded with {(int)response.StatusCode}. Local AI review may not work until Ollama is healthy.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new SetupCheckResponse("ollama", "warning", $"Ollama is optional but not reachable: {exception.Message}");
        }
    }
}

public sealed record SetupDiagnosticsResponse(bool IsReady, IReadOnlyCollection<SetupCheckResponse> Checks);

public sealed record SetupCheckResponse(string Id, string Status, string Message);
