namespace ProdigeeTutsPoint.Api.Features.Exercises;

public static class ExerciseEndpoints
{
    public static RouteGroupBuilder MapExerciseEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/exercises").WithTags("Exercises");

        group.MapGet("/{exerciseId}/workspace", async (
            string exerciseId,
            string profileId,
            ExerciseWorkspaceService workspaces,
            CancellationToken ct) =>
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(profileId, exerciseId, ct);
            return workspace is null ? Results.NotFound() : Results.Ok(workspace);
        });

        group.MapPut("/{exerciseId}/workspace/files", async (
            string exerciseId,
            string profileId,
            ExerciseFileSaveRequest request,
            ExerciseWorkspaceService workspaces,
            CancellationToken ct) =>
        {
            try
            {
                var workspace = await workspaces.SaveFileAsync(profileId, exerciseId, request, ct);
                return workspace is null ? Results.NotFound() : Results.Ok(workspace);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        group.MapPost("/{exerciseId}/run", async (
            string exerciseId,
            ExerciseRunRequest request,
            ExerciseWorkspaceService workspaces,
            CancellationToken ct) =>
        {
            var result = await workspaces.RunAsync(request.ProfileId, exerciseId, request.Files, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/{exerciseId}/attempts", async (
            string exerciseId,
            string profileId,
            ExerciseWorkspaceService workspaces,
            CancellationToken ct) =>
        {
            var history = await workspaces.GetAttemptHistoryAsync(profileId, exerciseId, ct);
            return Results.Ok(history);
        });

        group.MapGet("/{exerciseId}/static-analysis", async (
            string exerciseId,
            string profileId,
            string? runHistoryId,
            ExerciseWorkspaceService workspaces,
            CancellationToken ct) =>
        {
            var diagnostics = await workspaces.GetStaticAnalysisHistoryAsync(profileId, exerciseId, runHistoryId, ct);
            return Results.Ok(diagnostics);
        });

        group.MapGet("/{exerciseId}/assistance", async (
            string exerciseId,
            string profileId,
            ExerciseWorkspaceService workspaces,
            CancellationToken ct) =>
        {
            var assistance = await workspaces.GetAssistanceAsync(profileId, exerciseId, ct);
            return assistance is null ? Results.NotFound() : Results.Ok(assistance);
        });

        group.MapPost("/{exerciseId}/hints/{hintId}/use", async (
            string exerciseId,
            string hintId,
            ExerciseHintUseRequest request,
            ExerciseWorkspaceService workspaces,
            CancellationToken ct) =>
        {
            try
            {
                var hint = await workspaces.UseHintAsync(request.ProfileId, exerciseId, hintId, ct);
                return hint is null ? Results.NotFound() : Results.Ok(hint);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        group.MapPost("/{exerciseId}/solution/unlock", async (
            string exerciseId,
            ExerciseSolutionUnlockRequest request,
            ExerciseWorkspaceService workspaces,
            CancellationToken ct) =>
        {
            try
            {
                var solution = await workspaces.UnlockSolutionAsync(request.ProfileId, exerciseId, request.Reason, ct);
                return solution is null ? Results.NotFound() : Results.Ok(solution);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        group.MapPost("/{exerciseId}/language/diagnostics", async (
            string exerciseId,
            ExerciseLanguageRequest request,
            ExerciseLanguageService languageService,
            CancellationToken ct) =>
        {
            try
            {
                var result = await languageService.GetDiagnosticsAsync(exerciseId, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        group.MapPost("/{exerciseId}/language/completions", async (
            string exerciseId,
            ExerciseCompletionRequest request,
            ExerciseLanguageService languageService,
            CancellationToken ct) =>
        {
            try
            {
                var result = await languageService.GetCompletionsAsync(exerciseId, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        group.MapPost("/{exerciseId}/language/hover", async (
            string exerciseId,
            ExercisePositionRequest request,
            ExerciseLanguageService languageService,
            CancellationToken ct) =>
        {
            try
            {
                var result = await languageService.GetHoverAsync(exerciseId, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        group.MapPost("/{exerciseId}/language/signature-help", async (
            string exerciseId,
            ExercisePositionRequest request,
            ExerciseLanguageService languageService,
            CancellationToken ct) =>
        {
            try
            {
                var result = await languageService.GetSignatureHelpAsync(exerciseId, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        group.MapPost("/{exerciseId}/language/code-actions", async (
            string exerciseId,
            ExerciseCodeActionRequest request,
            ExerciseLanguageService languageService,
            CancellationToken ct) =>
        {
            try
            {
                var result = await languageService.GetCodeActionsAsync(exerciseId, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        group.MapPost("/{exerciseId}/language/format", async (
            string exerciseId,
            ExerciseLanguageRequest request,
            ExerciseLanguageService languageService,
            CancellationToken ct) =>
        {
            try
            {
                var result = await languageService.FormatAsync(exerciseId, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new ExerciseProblemResponse(exception.Message));
            }
        });

        return group;
    }
}

public sealed record ExerciseProblemResponse(string Message);

public sealed record ExerciseHintUseRequest(string ProfileId);
