using System.Diagnostics;
using System.Text;

namespace ProdigeeTutsPoint.Api.Features.Exercises;

public interface IExerciseRunner
{
    Task<CommandResult> RunStaticAnalysisAsync(string testProject, CancellationToken cancellationToken);

    Task<CommandResult> RunVisibleTestsAsync(string testProject, CancellationToken cancellationToken);

    Task<CommandResult> RunHiddenTestsAsync(string testProject, CancellationToken cancellationToken);
}

public interface ITypeScriptExerciseRunner
{
    Task<CommandResult> RunStaticAnalysisAsync(string workspacePath, CancellationToken cancellationToken);

    Task<CommandResult> RunVisibleTestsAsync(string workspacePath, CancellationToken cancellationToken);

    Task<CommandResult> RunHiddenTestsAsync(string workspacePath, CancellationToken cancellationToken);
}

public interface ISwiftExerciseRunner
{
    Task<CommandResult> RunStaticAnalysisAsync(string workspacePath, CancellationToken cancellationToken);

    Task<CommandResult> RunVisibleTestsAsync(string workspacePath, CancellationToken cancellationToken);

    Task<CommandResult> RunHiddenTestsAsync(string workspacePath, CancellationToken cancellationToken);
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

public sealed class TypeScriptExerciseRunner(IWebHostEnvironment environment) : ITypeScriptExerciseRunner
{
    private const int OutputLimit = 24_000;
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(20);

    public Task<CommandResult> RunStaticAnalysisAsync(string workspacePath, CancellationToken cancellationToken)
    {
        return RunNodeToolAsync(
            workspacePath,
            TypeScriptBinPath(),
            ["--noEmit", "--pretty", "false", "-p", "tsconfig.json"],
            cancellationToken);
    }

    public Task<CommandResult> RunVisibleTestsAsync(string workspacePath, CancellationToken cancellationToken)
    {
        return RunNodeToolAsync(
            workspacePath,
            VitestBinPath(),
            ["run", "tests/visible.test.ts", "--root", workspacePath, "--reporter", "verbose"],
            cancellationToken);
    }

    public Task<CommandResult> RunHiddenTestsAsync(string workspacePath, CancellationToken cancellationToken)
    {
        return RunNodeToolAsync(
            workspacePath,
            VitestBinPath(),
            ["run", "tests/hidden.test.ts", "--root", workspacePath, "--reporter", "verbose"],
            cancellationToken);
    }

    private string TypeScriptBinPath()
    {
        return Path.Combine(WebNodeModulesPath(), "typescript", "bin", "tsc");
    }

    private string VitestBinPath()
    {
        return Path.Combine(WebNodeModulesPath(), "vitest", "vitest.mjs");
    }

    private string WebNodeModulesPath()
    {
        return Path.GetFullPath(Path.Combine(
            environment.ContentRootPath,
            "..",
            "ProdigeeTutsPoint.Web",
            "node_modules"));
    }

    private static async Task<CommandResult> RunNodeToolAsync(
        string workspacePath,
        string toolPath,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(toolPath))
        {
            return CommandResult.RunnerError($"Node tool is missing: {toolPath}");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CommandTimeout);

        var output = new StringBuilder();
        var errors = new StringBuilder();
        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            WorkingDirectory = workspacePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add(toolPath);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.Environment["NO_COLOR"] = "1";
        startInfo.Environment["CI"] = "1";

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return CommandResult.RunnerError("Could not start node.");
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

public sealed class SwiftExerciseRunner : ISwiftExerciseRunner
{
    private const int OutputLimit = 24_000;
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(30);

    public Task<CommandResult> RunStaticAnalysisAsync(string workspacePath, CancellationToken cancellationToken)
    {
        return RunSwiftAsync(
            workspacePath,
            ["build", "--build-tests"],
            cancellationToken);
    }

    public Task<CommandResult> RunVisibleTestsAsync(string workspacePath, CancellationToken cancellationToken)
    {
        return RunSwiftAsync(
            workspacePath,
            ["test", "--filter", "ExerciseVisibleTests"],
            cancellationToken);
    }

    public Task<CommandResult> RunHiddenTestsAsync(string workspacePath, CancellationToken cancellationToken)
    {
        return RunSwiftAsync(
            workspacePath,
            ["test", "--filter", "ExerciseHiddenTests"],
            cancellationToken);
    }

    private static async Task<CommandResult> RunSwiftAsync(
        string workspacePath,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(Path.Combine(workspacePath, "Package.swift")))
        {
            return CommandResult.RunnerError("Swift package manifest is missing.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CommandTimeout);

        var output = new StringBuilder();
        var errors = new StringBuilder();
        var startInfo = new ProcessStartInfo
        {
            FileName = "swift",
            WorkingDirectory = workspacePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.Environment["NO_COLOR"] = "1";
        startInfo.Environment["CI"] = "1";

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return CommandResult.RunnerError("Could not start swift.");
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(timeout.Token);

            try
            {
                await process.WaitForExitAsync(timeout.Token);
                output.Append(await stdoutTask);
                errors.Append(await stderrTask);
                var successfulOutput = process.ExitCode == 0
                    ? $"{output}{errors}"
                    : output.ToString();
                var diagnostics = process.ExitCode == 0 ? string.Empty : errors.ToString();
                return new CommandResult(
                    process.ExitCode,
                    false,
                    false,
                    false,
                    Truncate(successfulOutput),
                    Truncate(diagnostics));
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
