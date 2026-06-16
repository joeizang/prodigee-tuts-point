namespace ProdigeeTutsPoint.Api.Features.ExportImport;

public static class ExportImportEndpoints
{
    public static RouteGroupBuilder MapExportImportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/portable-state").WithTags("ExportImport");

        group.MapGet("/export", async (
            string profileId,
            ExportImportService service,
            CancellationToken ct) =>
        {
            return Results.Ok(await service.ExportAsync(profileId, ct));
        });

        group.MapPost("/import", async (
            LearnerStateExportDocument document,
            ExportImportService service,
            CancellationToken ct) =>
        {
            var result = await service.ImportAsync(document, ct);
            return result.Success ? Results.Ok(result) : Results.Conflict(result);
        });

        return group;
    }
}
