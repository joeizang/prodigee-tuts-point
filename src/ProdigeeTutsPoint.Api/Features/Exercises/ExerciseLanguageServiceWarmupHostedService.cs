namespace ProdigeeTutsPoint.Api.Features.Exercises;

public sealed class ExerciseLanguageServiceWarmupHostedService(
    ILogger<ExerciseLanguageServiceWarmupHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var startedAt = TimeProvider.System.GetTimestamp();
            await ExerciseLanguageService.WarmUpAsync(cancellationToken);
            var elapsed = TimeProvider.System.GetElapsedTime(startedAt);
            logger.LogInformation("Warmed C# exercise language service in {ElapsedMilliseconds}ms.", elapsed.TotalMilliseconds);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "C# exercise language service warmup failed; first editor request will initialize it on demand.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
