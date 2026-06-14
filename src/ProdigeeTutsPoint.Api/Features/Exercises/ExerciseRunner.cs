using System.Diagnostics;
using System.Text;

namespace ProdigeeTutsPoint.Api.Features.Exercises;

public interface IExerciseRunner
{
    Task<CommandResult> RunStaticAnalysisAsync(string testProject, CancellationToken cancellationToken);

    Task<CommandResult> RunVisibleTestsAsync(string testProject, CancellationToken cancellationToken);

    Task<CommandResult> RunHiddenTestsAsync(string testProject, CancellationToken cancellationToken);
}

public sealed class DotnetExerciseRunner : IExerciseRunner
{
    private const int OutputLimit = 24_000;
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(20);

    public Task<CommandResult> RunStaticAnalysisAsync(string testProject, CancellationToken cancellationToken)
    {
        return RunDotnetAsync(
            testProject,
            ["build", testProject, "--nologo", "--verbosity:minimal"],
            cancellationToken);
    }

    public Task<CommandResult> RunVisibleTestsAsync(string testProject, CancellationToken cancellationToken)
    {
        return RunDotnetTestAsync(testProject, "FullyQualifiedName~VisibleTests", cancellationToken);
    }

    public Task<CommandResult> RunHiddenTestsAsync(string testProject, CancellationToken cancellationToken)
    {
        return RunDotnetTestAsync(testProject, "FullyQualifiedName~HiddenTests", cancellationToken);
    }

    private static async Task<CommandResult> RunDotnetTestAsync(
        string testProject,
        string filter,
        CancellationToken cancellationToken)
    {
        return await RunDotnetAsync(
            testProject,
            ["test", testProject, "--filter", filter, "--nologo"],
            cancellationToken);
    }

    private static async Task<CommandResult> RunDotnetAsync(
        string testProject,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CommandTimeout);

        var output = new StringBuilder();
        var errors = new StringBuilder();
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Path.GetDirectoryName(testProject)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";
        startInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return CommandResult.RunnerError("Could not start dotnet.");
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(timeout.Token);

            try
            {
                await process.WaitForExitAsync(timeout.Token);
                output.Append(await stdoutTask);
                errors.Append(await stderrTask);
                return new CommandResult(
                    process.ExitCode,
                    false,
                    false,
                    false,
                    Truncate(output.ToString()),
                    Truncate(errors.ToString()));
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                TryKill(process);
                return new CommandResult(-1, true, false, false, Truncate(output.ToString()), "Command timed out.");
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return CommandResult.RunnerError($"Runner error: {exception.Message}");
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static string Truncate(string value)
    {
        return value.Length <= OutputLimit ? value : value[..OutputLimit] + "\n[output truncated]";
    }
}

public sealed record CommandResult(
    int ExitCode,
    bool TimedOut,
    bool WasSkipped,
    bool HasRunnerError,
    string Output,
    string Diagnostics)
{
    public static CommandResult Skipped(string message)
    {
        return new CommandResult(0, false, true, false, message, string.Empty);
    }

    public static CommandResult RunnerError(string message)
    {
        return new CommandResult(-1, false, false, true, string.Empty, message);
    }
}
