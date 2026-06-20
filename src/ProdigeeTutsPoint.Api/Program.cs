using Microsoft.Extensions.FileProviders;
using ProdigeeTutsPoint.Api.Features.AiReview;
using ProdigeeTutsPoint.Api.Features.Curriculum;
using ProdigeeTutsPoint.Api.Features.Exercises;
using ProdigeeTutsPoint.Api.Features.ExportImport;
using ProdigeeTutsPoint.Api.Features.Learning;
using ProdigeeTutsPoint.Api.Features.Setup;
using ProdigeeTutsPoint.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IExerciseRunner, DotnetExerciseRunner>();
builder.Services.AddScoped<ITypeScriptExerciseRunner, TypeScriptExerciseRunner>();
builder.Services.AddScoped<ISwiftExerciseRunner, SwiftExerciseRunner>();
builder.Services.AddScoped<IPythonExerciseRunner, PythonExerciseRunner>();
builder.Services.AddScoped<ExerciseWorkspaceService>();
builder.Services.AddSingleton<CSharpLspBridge>();
builder.Services.AddSingleton<SwiftLspBridge>();
builder.Services.AddSingleton<PythonLspBridge>();
builder.Services.AddScoped<ExerciseLanguageService>();
builder.Services.AddScoped<ExportImportService>();
builder.Services.AddSingleton<IAiChatClientFactory, AiChatClientFactory>();
builder.Services.AddHostedService<ExerciseLanguageServiceWarmupHostedService>();
builder.Services.AddScoped<AiReviewService>();

var app = builder.Build();
var webDistPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "ProdigeeTutsPoint.Web", "dist"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

if (Directory.Exists(webDistPath))
{
    var webFileProvider = new PhysicalFileProvider(webDistPath);

    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = webFileProvider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = webFileProvider });
}

app.MapGet("/api/health", () => Results.Ok(new HealthResponse("ok", DateTimeOffset.UtcNow)))
    .WithName("GetHealth");
app.MapCurriculumEndpoints();
app.MapExerciseEndpoints();
app.MapLearningEndpoints();
app.MapAiReviewEndpoints();
app.MapExportImportEndpoints();
app.MapSetupDiagnosticsEndpoints();

app.MapFallback(async context =>
{
    var indexPath = Path.Combine(webDistPath, "index.html");
    if (!File.Exists(indexPath))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Frontend assets have not been built.");
        return;
    }

    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(indexPath);
});

app.Run();

public partial class Program;

internal sealed record HealthResponse(string Status, DateTimeOffset CheckedAt);
