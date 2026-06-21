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

public interface IPythonExerciseRunner
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
        timeout.CancelAfter(SwiftCommandTimeout(workspacePath));

        var swiftScratchPath = Path.Combine(workspacePath, ".build");
        var clangModuleCachePath = Path.Combine(workspacePath, ".clang-module-cache");
        Directory.CreateDirectory(swiftScratchPath);
        Directory.CreateDirectory(clangModuleCachePath);

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
        var swiftArguments = arguments.ToList();
        if (swiftArguments.Count > 0
            && (swiftArguments[0].Equals("build", StringComparison.Ordinal)
                || swiftArguments[0].Equals("test", StringComparison.Ordinal)))
        {
            swiftArguments.InsertRange(
                1,
                [
                    "--scratch-path",
                    swiftScratchPath,
                    "--disable-sandbox"
                ]);
        }

        foreach (var argument in swiftArguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.Environment["NO_COLOR"] = "1";
        startInfo.Environment["CI"] = "1";
        startInfo.Environment["CLANG_MODULE_CACHE_PATH"] = clangModuleCachePath;

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

    private static TimeSpan SwiftCommandTimeout(string workspacePath)
    {
        var packagePath = Path.Combine(workspacePath, "Package.swift");
        if (File.Exists(packagePath)
            && File.ReadAllText(packagePath).Contains("github.com/vapor/vapor.git", StringComparison.OrdinalIgnoreCase))
        {
            return TimeSpan.FromMinutes(3);
        }

        return CommandTimeout;
    }
}

public sealed class PythonExerciseRunner(IWebHostEnvironment environment) : IPythonExerciseRunner
{
    private const int OutputLimit = 24_000;
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(20);

    public async Task<CommandResult> RunStaticAnalysisAsync(string workspacePath, CancellationToken cancellationToken)
    {
        var commands = new (string Label, IReadOnlyCollection<string> Arguments)[]
        {
            ("Ruff lint", ["ruff", "check", "--output-format", "concise", "src"]),
            ("BasedPyright", ["basedpyright", "src"]),
        };

        var output = new StringBuilder();
        var diagnostics = new StringBuilder();
        var exitCode = 0;
        var timedOut = false;
        var hasRunnerError = false;

        foreach (var command in commands)
        {
            var result = await RunPythonAsync(workspacePath, command.Arguments, cancellationToken);
            output.AppendLine($"{command.Label}:");
            output.AppendLine(result.Output);
            diagnostics.AppendLine($"{command.Label}:");
            diagnostics.AppendLine(result.Diagnostics);

            if (result.ExitCode != 0 && exitCode == 0)
            {
                exitCode = result.ExitCode;
            }

            timedOut = timedOut || result.TimedOut;
            hasRunnerError = hasRunnerError || result.HasRunnerError;
        }

        return new CommandResult(
            exitCode,
            timedOut,
            false,
            hasRunnerError,
            Truncate(output.ToString()),
            Truncate(diagnostics.ToString()));
    }

    public Task<CommandResult> RunVisibleTestsAsync(string workspacePath, CancellationToken cancellationToken)
    {
        return RunPythonAsync(
            workspacePath,
            PythonTestArguments(workspacePath, "*_visible.py"),
            cancellationToken);
    }

    public Task<CommandResult> RunHiddenTestsAsync(string workspacePath, CancellationToken cancellationToken)
    {
        return RunPythonAsync(
            workspacePath,
            PythonTestArguments(workspacePath, "*_hidden.py"),
            cancellationToken);
    }

    private static IReadOnlyCollection<string> PythonTestArguments(string workspacePath, string pattern)
    {
        var testsRoot = Path.Combine(workspacePath, "tests");
        var testFiles = Directory.Exists(testsRoot)
            ? Directory.GetFiles(testsRoot, pattern)
                .Select(path => Path.GetRelativePath(workspacePath, path))
                .Order(StringComparer.Ordinal)
                .ToArray()
            : [];

        return ["pytest", "-q", .. testFiles];
    }

    private async Task<CommandResult> RunPythonAsync(
        string workspacePath,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(Path.Combine(workspacePath, "pyproject.toml")))
        {
            return CommandResult.RunnerError("Python project manifest is missing.");
        }

        var repositoryRoot = GetRepositoryRoot();
        var repositoryPythonProject = Path.Combine(repositoryRoot, "pyproject.toml");
        if (!File.Exists(repositoryPythonProject))
        {
            return CommandResult.RunnerError("Repository Python tool project is missing pyproject.toml.");
        }

        if (!CommandExists("uv"))
        {
            return CommandResult.RunnerError("uv is required for Python exercises. Install uv, then run `uv sync` from the repository root.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CommandTimeout);

        var output = new StringBuilder();
        var errors = new StringBuilder();
        var startInfo = new ProcessStartInfo
        {
            FileName = "uv",
            WorkingDirectory = workspacePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(repositoryRoot);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.Environment["NO_COLOR"] = "1";
        startInfo.Environment["PYTHONPATH"] = Path.Combine(workspacePath, "src");
        startInfo.Environment["PYTHONDONTWRITEBYTECODE"] = "1";
        startInfo.Environment["UV_CACHE_DIR"] = Path.Combine(repositoryRoot, ".uv-cache");

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return CommandResult.RunnerError("Could not start uv.");
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

    private string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", ".."));
    }

    private static bool CommandExists(string command)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(directory, command);
            if (File.Exists(candidate))
            {
                return true;
            }
        }

        return false;
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
