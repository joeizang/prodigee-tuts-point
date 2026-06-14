namespace ProdigeeTutsPoint.Api.Features.AiReview;

public static class AiReviewEndpoints
{
    public static RouteGroupBuilder MapAiReviewEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/ai").WithTags("AI Review");

        group.MapGet("/providers", async (AiReviewService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetProvidersAsync(ct));
        });

        group.MapPost("/providers/{providerId}/test", async (
            string providerId,
            AiReviewService service,
            CancellationToken ct) =>
        {
            var result = await service.TestProviderAsync(providerId, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/reviews", async (
            AiReviewRequest request,
            AiReviewService service,
            CancellationToken ct) =>
        {
            try
            {
                var result = await service.RunReviewAsync(request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new AiReviewProblemResponse(exception.Message));
            }
        });

        group.MapGet("/reviews", async (
            string profileId,
            string projectId,
            string milestoneId,
            AiReviewService service,
            CancellationToken ct) =>
        {
            return Results.Ok(await service.GetReviewsAsync(profileId, projectId, milestoneId, ct));
        });

        return group;
    }
}

public sealed record AiReviewProblemResponse(string Message);
