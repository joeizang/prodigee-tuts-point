using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using ProdigeeTutsPoint.Domain.Learning;
using ProdigeeTutsPoint.Infrastructure.Content;
using ProdigeeTutsPoint.Infrastructure.Persistence;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProdigeeTutsPoint.Api.Features.Exercises;

public sealed class ExerciseWorkspaceService(
    IWebHostEnvironment environment,
    IOptions<ContentOptions> contentOptions,
    AppDbContext db,
    IExerciseRunner runner)
{
    private const int OutputLimit = 24_000;
    private static readonly Regex DotnetLocationDiagnosticPattern = new(
        @"^(?<file>.+?)\((?<line>\d+),(?<column>\d+)\): (?<severity>error|warning) (?<rule>[A-Z]+[A-Z0-9]*\d+): (?<message>.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex DotnetLocationlessDiagnosticPattern = new(
        @"^(?<severity>error|warning) (?<rule>[A-Z]+[A-Z0-9]*\d+): (?<message>.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public async Task<ExerciseWorkspaceResponse?> EnsureWorkspaceAsync(
        string profileId,
        string exerciseId,
        CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises
            .Where(exercise => exercise.Id == exerciseId)
            .Select(exercise => new { exercise.Id, exercise.Title, exercise.Summary, exercise.DirectoryPath })
            .FirstOrDefaultAsync(cancellationToken);

        if (exercise is null)
        {
            return null;
        }

        await CleanupOldWorkspacesAsync(cancellationToken);

        var workspacePath = GetWorkspacePath(profileId, exerciseId);
        var definition = await ReadExerciseDefinitionAsync(exercise.DirectoryPath, cancellationToken);
        Directory.CreateDirectory(workspacePath);
        Directory.CreateDirectory(Path.Combine(workspacePath, "src", "Exercise"));
        Directory.CreateDirectory(Path.Combine(workspacePath, "tests", "Exercise.Tests"));

        WriteGeneratedFile(Path.Combine(workspacePath, "ExerciseWorkspace.sln"), SolutionFile());
        WriteGeneratedFile(Path.Combine(workspacePath, ".editorconfig"), EditorConfigFile());
        WriteGeneratedFile(Path.Combine(workspacePath, "src", "Exercise", "Exercise.csproj"), ExerciseProjectFile());
        WriteFileIfMissing(
            Path.Combine(workspacePath, "src", "Exercise", "WordFrequencyAnalyzer.cs"),
            await ReadContentTextAsync(definition.Workspace.Starter, cancellationToken));
        WriteGeneratedFile(Path.Combine(workspacePath, "tests", "Exercise.Tests", "Exercise.Tests.csproj"), TestProjectFile());
        WriteGeneratedFile(Path.Combine(workspacePath, "tests", "Exercise.Tests", "VisibleTests.cs"), VisibleTests(definition.Workspace.VisibleTest));
        WriteGeneratedFile(Path.Combine(workspacePath, "tests", "Exercise.Tests", "HiddenTests.cs"), HiddenTests(definition.Workspace.HiddenTest));

        var attempt = await db.ExerciseAttempts
            .FirstOrDefaultAsync(attempt => attempt.ProfileId == profileId && attempt.ExerciseId == exerciseId, cancellationToken);
        if (attempt is null)
        {
            var now = DateTimeOffset.UtcNow;
            attempt = new ExerciseAttempt
            {
                Id = $"{profileId}:{exerciseId}",
                ProfileId = profileId,
                ExerciseId = exerciseId,
                WorkspacePath = workspacePath,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.ExerciseAttempts.Add(attempt);
            await db.SaveChangesAsync(cancellationToken);
        }

        return await BuildResponseAsync(exercise.Id, exercise.Title, workspacePath, attempt, cancellationToken);
    }

    public async Task<ExerciseWorkspaceResponse?> SaveFileAsync(
        string profileId,
        string exerciseId,
        ExerciseFileSaveRequest request,
        CancellationToken cancellationToken)
    {
        var workspace = await EnsureWorkspaceAsync(profileId, exerciseId, cancellationToken);
        if (workspace is null)
        {
            return null;
        }

        var file = workspace.Files.FirstOrDefault(file => file.Path == request.Path);
        if (file is null || file.Role != "editable")
        {
            throw new InvalidOperationException("Only editable exercise files can be saved.");
        }

        var fullPath = ResolveWorkspaceFilePath(workspace.WorkspacePath, request.Path);
        await File.WriteAllTextAsync(fullPath, request.Content, cancellationToken);

        var attempt = await db.ExerciseAttempts
            .FirstAsync(attempt => attempt.ProfileId == profileId && attempt.ExerciseId == exerciseId, cancellationToken);
        attempt.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return await BuildResponseAsync(workspace.ExerciseId, workspace.Title, workspace.WorkspacePath, attempt, cancellationToken);
    }

    public async Task<ExerciseRunResponse?> RunAsync(
        string profileId,
        string exerciseId,
        IReadOnlyCollection<ExerciseFileSaveRequest> files,
        CancellationToken cancellationToken)
    {
        var workspace = await EnsureWorkspaceAsync(profileId, exerciseId, cancellationToken);
        if (workspace is null)
        {
            return null;
        }

        foreach (var file in files)
        {
            if (workspace.Files.Any(existing => existing.Path == file.Path && existing.Role == "editable"))
            {
                await File.WriteAllTextAsync(ResolveWorkspaceFilePath(workspace.WorkspacePath, file.Path), file.Content, cancellationToken);
            }
        }

        var runWorkspacePath = CreateRunWorkspace(workspace.WorkspacePath, profileId, exerciseId);
        CommandResult visible;
        CommandResult hidden;
        CommandResult analysis;
        IReadOnlyCollection<StaticAnalysisDiagnosticResponse> analysisDiagnostics;
        try
        {
            var testProject = Path.Combine(runWorkspacePath, "tests", "Exercise.Tests", "Exercise.Tests.csproj");
            analysis = await runner.RunStaticAnalysisAsync(testProject, cancellationToken);
            analysisDiagnostics = ParseStaticAnalysisDiagnostics(runWorkspacePath, analysis);
            visible = await runner.RunVisibleTestsAsync(testProject, cancellationToken);
            hidden = visible.ExitCode == 0 && !visible.TimedOut && !visible.HasRunnerError
                ? await runner.RunHiddenTestsAsync(testProject, cancellationToken)
                : CommandResult.Skipped("Hidden tests were not run because visible tests failed.");
        }
        finally
        {
            DeleteDirectoryBestEffort(runWorkspacePath);
        }

        var status = BuildStatus(visible, hidden);
        var combinedOutput = Truncate($"""
        Visible tests:
        {visible.Output}

        Hidden tests:
        {HiddenOutputForLearner(hidden)}
        """);
        var diagnostics = Truncate($"{visible.Diagnostics}\n{HiddenDiagnosticsForLearner(hidden)}".Trim());
        var attempt = await db.ExerciseAttempts
            .FirstAsync(attempt => attempt.ProfileId == profileId && attempt.ExerciseId == exerciseId, cancellationToken);
        var runHistory = new ExerciseRunHistory
        {
            Id = Guid.NewGuid().ToString("n"),
            ProfileId = profileId,
            ExerciseId = exerciseId,
            Status = status,
            VisiblePassed = visible.ExitCode == 0 && !visible.TimedOut && !visible.HasRunnerError,
            HiddenPassed = !hidden.WasSkipped && hidden.ExitCode == 0 && !hidden.TimedOut && !hidden.HasRunnerError,
            TimedOut = visible.TimedOut || hidden.TimedOut || analysis.TimedOut,
            ExitCode = hidden.WasSkipped ? visible.ExitCode : hidden.ExitCode,
            Summary = RunSummary(status, analysisDiagnostics),
            Output = combinedOutput,
            Diagnostics = diagnostics,
            StaticAnalysisErrorCount = analysisDiagnostics.Count(diagnostic => diagnostic.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)),
            StaticAnalysisWarningCount = analysisDiagnostics.Count(diagnostic => diagnostic.Severity.Equals("warning", StringComparison.OrdinalIgnoreCase)),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        attempt.Status = status;
        attempt.VisiblePassed = runHistory.VisiblePassed;
        attempt.HiddenPassed = runHistory.HiddenPassed;
        attempt.TimedOut = runHistory.TimedOut;
        attempt.ExitCode = runHistory.ExitCode;
        attempt.Output = combinedOutput;
        attempt.Diagnostics = diagnostics;
        attempt.UpdatedAt = runHistory.CreatedAt;
        db.ExerciseRunHistory.Add(runHistory);
        db.StaticAnalysisDiagnostics.AddRange(analysisDiagnostics.Select(diagnostic => new StaticAnalysisDiagnosticRecord
        {
            Id = Guid.NewGuid().ToString("n"),
            ProfileId = profileId,
            ExerciseId = exerciseId,
            RunHistoryId = runHistory.Id,
            RuleId = diagnostic.RuleId,
            Severity = diagnostic.Severity,
            Message = diagnostic.Message,
            FilePath = diagnostic.FilePath,
            Line = diagnostic.Line,
            Column = diagnostic.Column,
            CreatedAt = runHistory.CreatedAt,
        }));
        await db.SaveChangesAsync(cancellationToken);

        return new ExerciseRunResponse(
            attempt.Status,
            attempt.VisiblePassed,
            attempt.HiddenPassed,
            attempt.TimedOut,
            attempt.ExitCode,
            attempt.Output,
            attempt.Diagnostics,
            analysisDiagnostics);
    }

    public async Task<IReadOnlyCollection<ExerciseRunHistoryResponse>> GetAttemptHistoryAsync(
        string profileId,
        string exerciseId,
        CancellationToken cancellationToken)
    {
        var history = await db.ExerciseRunHistory
            .Where(history => history.ProfileId == profileId && history.ExerciseId == exerciseId)
            .Select(history => new ExerciseRunHistoryResponse(
                history.Id,
                history.Status,
                history.VisiblePassed,
                history.HiddenPassed,
                history.TimedOut,
                history.ExitCode,
                history.Summary,
                history.StaticAnalysisErrorCount,
                history.StaticAnalysisWarningCount,
                history.CreatedAt))
            .ToListAsync(cancellationToken);

        return history
            .OrderByDescending(history => history.CreatedAt)
            .Take(20)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<StaticAnalysisDiagnosticResponse>> GetStaticAnalysisHistoryAsync(
        string profileId,
        string exerciseId,
        string? runHistoryId,
        CancellationToken cancellationToken)
    {
        var query = db.StaticAnalysisDiagnostics
            .Where(diagnostic => diagnostic.ProfileId == profileId && diagnostic.ExerciseId == exerciseId);
        if (!string.IsNullOrWhiteSpace(runHistoryId))
        {
            query = query.Where(diagnostic => diagnostic.RunHistoryId == runHistoryId);
        }

        var diagnostics = await query
            .Select(diagnostic => new
            {
                diagnostic.RuleId,
                diagnostic.Severity,
                diagnostic.Message,
                diagnostic.FilePath,
                diagnostic.Line,
                diagnostic.Column,
                diagnostic.CreatedAt,
            })
            .ToListAsync(cancellationToken);
        return diagnostics
            .OrderByDescending(diagnostic => diagnostic.CreatedAt)
            .Take(100)
            .Select(diagnostic => new StaticAnalysisDiagnosticResponse(
                diagnostic.RuleId,
                diagnostic.Severity,
                diagnostic.Message,
                diagnostic.FilePath,
                diagnostic.Line,
                diagnostic.Column))
            .ToArray();
    }

    public async Task<ExerciseAssistanceResponse?> GetAssistanceAsync(
        string profileId,
        string exerciseId,
        CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises
            .Where(exercise => exercise.Id == exerciseId)
            .Select(exercise => new { exercise.Id, exercise.DirectoryPath })
            .FirstOrDefaultAsync(cancellationToken);
        if (exercise is null)
        {
            return null;
        }

        var definition = await ReadExerciseDefinitionAsync(exercise.DirectoryPath, cancellationToken);
        var usedHintIds = await db.ExerciseHintUsages
            .Where(usage => usage.ProfileId == profileId && usage.ExerciseId == exerciseId)
            .Select(usage => usage.HintId)
            .ToListAsync(cancellationToken);
        var unlocked = await IsSolutionAvailableAsync(profileId, exerciseId, cancellationToken);
        var solution = unlocked ? definition.Solution : null;

        return new ExerciseAssistanceResponse(
            definition.Hints
                .Select(hint => new ExerciseHintResponse(hint.Id, hint.Level, hint.Title, hint.Body, usedHintIds.Contains(hint.Id)))
                .ToArray(),
            unlocked,
            solution is null ? null : new ExerciseSolutionResponse(solution.Title, solution.Body, solution.Code));
    }

    public async Task<ExerciseHintResponse?> UseHintAsync(
        string profileId,
        string exerciseId,
        string hintId,
        CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises
            .Where(exercise => exercise.Id == exerciseId)
            .Select(exercise => new { exercise.Id, exercise.DirectoryPath })
            .FirstOrDefaultAsync(cancellationToken);
        if (exercise is null)
        {
            return null;
        }

        var definition = await ReadExerciseDefinitionAsync(exercise.DirectoryPath, cancellationToken);
        var hint = definition.Hints.FirstOrDefault(hint => hint.Id == hintId);
        if (hint is null)
        {
            throw new InvalidOperationException("Unknown exercise hint.");
        }

        var now = DateTimeOffset.UtcNow;
        if (!await db.ExerciseHintUsages.AnyAsync(
                usage => usage.ProfileId == profileId && usage.ExerciseId == exerciseId && usage.HintId == hintId,
                cancellationToken))
        {
            db.ExerciseHintUsages.Add(new ExerciseHintUsage
            {
                Id = Guid.NewGuid().ToString("n"),
                ProfileId = profileId,
                ExerciseId = exerciseId,
                HintId = hint.Id,
                HintLevel = hint.Level,
                UsedAt = now,
            });
            await AddExerciseConceptEvidenceAsync(profileId, exerciseId, "Hint", hint.Id, 0, 1, $"Used {hint.Level} hint.", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new ExerciseHintResponse(hint.Id, hint.Level, hint.Title, hint.Body, true);
    }

    public async Task<ExerciseSolutionResponse?> UnlockSolutionAsync(
        string profileId,
        string exerciseId,
        string reason,
        CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises
            .Where(exercise => exercise.Id == exerciseId)
            .Select(exercise => new { exercise.Id, exercise.DirectoryPath })
            .FirstOrDefaultAsync(cancellationToken);
        if (exercise is null)
        {
            return null;
        }

        var definition = await ReadExerciseDefinitionAsync(exercise.DirectoryPath, cancellationToken);
        if (string.IsNullOrWhiteSpace(definition.Solution.Code))
        {
            throw new InvalidOperationException("This exercise does not have an authored model solution yet.");
        }

        var now = DateTimeOffset.UtcNow;
        if (!await db.ExerciseSolutionUnlocks.AnyAsync(
                unlock => unlock.ProfileId == profileId && unlock.ExerciseId == exerciseId,
                cancellationToken))
        {
            db.ExerciseSolutionUnlocks.Add(new ExerciseSolutionUnlock
            {
                Id = Guid.NewGuid().ToString("n"),
                ProfileId = profileId,
                ExerciseId = exerciseId,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Intentional unlock" : reason,
                UnlockedAt = now,
            });
            await AddExerciseConceptEvidenceAsync(profileId, exerciseId, "SolutionUnlock", exerciseId, 0, 1, "Unlocked model solution.", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new ExerciseSolutionResponse(definition.Solution.Title, definition.Solution.Body, definition.Solution.Code);
    }

    private async Task<ExerciseWorkspaceResponse> BuildResponseAsync(
        string exerciseId,
        string title,
        string workspacePath,
        ExerciseAttempt attempt,
        CancellationToken cancellationToken)
    {
        var files = new List<ExerciseWorkspaceFileResponse>
        {
            await FileResponseAsync(workspacePath, "src/Exercise/WordFrequencyAnalyzer.cs", "editable", true, cancellationToken),
            await FileResponseAsync(workspacePath, "tests/Exercise.Tests/VisibleTests.cs", "visible-test", false, cancellationToken),
            new("tests/Exercise.Tests/HiddenTests.cs", "hidden-test", false, null),
            await FileResponseAsync(workspacePath, "src/Exercise/Exercise.csproj", "readonly", false, cancellationToken),
            await FileResponseAsync(workspacePath, "tests/Exercise.Tests/Exercise.Tests.csproj", "readonly", false, cancellationToken),
        };

        return new ExerciseWorkspaceResponse(
            exerciseId,
            title,
            workspacePath,
            "Project-aware Roslyn services are active for editable C# files: diagnostics, completions, hover, signature help, formatting, and code actions.",
            files,
            attempt.Status,
            attempt.Output,
            attempt.Diagnostics);
    }

    private static async Task<ExerciseWorkspaceFileResponse> FileResponseAsync(
        string workspacePath,
        string relativePath,
        string role,
        bool editable,
        CancellationToken cancellationToken)
    {
        var content = await File.ReadAllTextAsync(ResolveWorkspaceFilePath(workspacePath, relativePath), cancellationToken);
        return new ExerciseWorkspaceFileResponse(relativePath, role, editable, content);
    }

    private async Task CleanupOldWorkspacesAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-14);
        await CleanupOldDirectoriesAsync(GetWorkspaceRoot(), cutoff, cancellationToken);
        await CleanupOldDirectoriesAsync(GetRunWorkspaceRoot(), cutoff, cancellationToken);
    }

    private static Task CleanupOldDirectoriesAsync(
        string root,
        DateTimeOffset cutoff,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(root))
        {
            return Task.CompletedTask;
        }

        var fullRoot = Path.GetFullPath(root);
        foreach (var directory in Directory.GetDirectories(fullRoot))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var info = new DirectoryInfo(directory);
            if (info.FullName.StartsWith(fullRoot, StringComparison.Ordinal)
                && info.LastWriteTimeUtc < cutoff.UtcDateTime)
            {
                DeleteDirectoryBestEffort(info.FullName);
            }
        }

        return Task.CompletedTask;
    }

    private string GetWorkspacePath(string profileId, string exerciseId)
    {
        return Path.Combine(GetWorkspaceRoot(), SafeSegment(profileId), SafeSegment(exerciseId));
    }

    private string GetWorkspaceRoot()
    {
        return Path.Combine(environment.ContentRootPath, "App_Data", "exercise-workspaces");
    }

    private string GetRunWorkspaceRoot()
    {
        return Path.Combine(environment.ContentRootPath, "App_Data", "exercise-runs");
    }

    private string GetContentRoot()
    {
        return Path.GetFullPath(Path.IsPathRooted(contentOptions.Value.RootPath)
            ? contentOptions.Value.RootPath
            : Path.Combine(environment.ContentRootPath, contentOptions.Value.RootPath));
    }

    private async Task<ExerciseContentDefinition> ReadExerciseDefinitionAsync(
        string directoryPath,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(GetContentRoot(), directoryPath, "exercise.yml");
        var yaml = await File.ReadAllTextAsync(path, cancellationToken);
        var definition = deserializer.Deserialize<ExerciseContentDefinition>(yaml);
        if (string.IsNullOrWhiteSpace(definition.Workspace.Starter)
            || string.IsNullOrWhiteSpace(definition.Workspace.VisibleTest)
            || string.IsNullOrWhiteSpace(definition.Workspace.HiddenTest))
        {
            throw new InvalidOperationException($"Exercise workspace metadata is incomplete: {path}");
        }

        return definition;
    }

    private async Task<bool> IsSolutionAvailableAsync(
        string profileId,
        string exerciseId,
        CancellationToken cancellationToken)
    {
        var passed = await db.ExerciseRunHistory.AnyAsync(
            history => history.ProfileId == profileId && history.ExerciseId == exerciseId && history.Status == "Passed",
            cancellationToken);
        if (passed)
        {
            return true;
        }

        return await db.ExerciseSolutionUnlocks.AnyAsync(
            unlock => unlock.ProfileId == profileId && unlock.ExerciseId == exerciseId,
            cancellationToken);
    }

    private async Task AddExerciseConceptEvidenceAsync(
        string profileId,
        string exerciseId,
        string sourceType,
        string sourceId,
        int score,
        int maxScore,
        string summary,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        var conceptIds = await db.Set<ProdigeeTutsPoint.Domain.Content.ExerciseConcept>()
            .Where(link => link.ExerciseId == exerciseId)
            .Select(link => link.ConceptId)
            .ToListAsync(cancellationToken);

        foreach (var conceptId in conceptIds)
        {
            db.ConceptMasteryEvidence.Add(new ConceptMasteryEvidence
            {
                Id = Guid.NewGuid().ToString("n"),
                ProfileId = profileId,
                ConceptId = conceptId,
                SourceType = sourceType,
                SourceId = sourceId,
                Score = score,
                MaxScore = maxScore,
                Summary = summary,
                CreatedAt = createdAt,
            });
        }
    }

    private async Task<string> ReadContentTextAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(Path.Combine(GetContentRoot(), relativePath));
        if (!fullPath.StartsWith(GetContentRoot(), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Exercise content path escapes the content root.");
        }

        return await File.ReadAllTextAsync(fullPath, cancellationToken);
    }

    private string CreateRunWorkspace(string workspacePath, string profileId, string exerciseId)
    {
        var runRoot = GetRunWorkspaceRoot();
        Directory.CreateDirectory(runRoot);
        var runPath = Path.Combine(
            runRoot,
            $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{SafeSegment(profileId)}-{SafeSegment(exerciseId)}-{Guid.NewGuid():n}");
        CopyDirectory(workspacePath, runPath);
        return runPath;
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(IsGeneratedBuildDirectory))
            {
                continue;
            }

            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(IsGeneratedBuildDirectory))
            {
                continue;
            }

            var destination = Path.Combine(destinationDirectory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private static bool IsGeneratedBuildDirectory(string segment)
    {
        return segment is "bin" or "obj";
    }

    private static string SafeSegment(string value)
    {
        var chars = value.Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-');
        return string.Concat(chars);
    }

    private static string ResolveWorkspaceFilePath(string workspacePath, string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(workspacePath, relativePath));
        if (!fullPath.StartsWith(Path.GetFullPath(workspacePath), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Path escapes the exercise workspace.");
        }

        return fullPath;
    }

    private static void WriteFileIfMissing(string path, string content)
    {
        if (File.Exists(path))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static void WriteGeneratedFile(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (File.Exists(path) && string.Equals(File.ReadAllText(path), content, StringComparison.Ordinal))
        {
            return;
        }

        File.WriteAllText(path, content);
    }

    private static void DeleteDirectoryBestEffort(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
            // Active or locked generated workspaces should not block learner progress.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup only; stale generated dirs are retried on later requests.
        }
    }

    private static string BuildStatus(CommandResult visible, CommandResult hidden)
    {
        if (visible.HasRunnerError || hidden.HasRunnerError)
        {
            return "RunnerError";
        }

        if (visible.TimedOut || hidden.TimedOut)
        {
            return "TimedOut";
        }

        if (visible.ExitCode != 0)
        {
            return "FailedVisible";
        }

        if (!hidden.WasSkipped && hidden.ExitCode != 0)
        {
            return "FailedHidden";
        }

        return "Passed";
    }

    private static string HiddenOutputForLearner(CommandResult hidden)
    {
        if (hidden.WasSkipped)
        {
            return hidden.Output;
        }

        if (hidden.TimedOut)
        {
            return "Hidden tests timed out.";
        }

        if (hidden.HasRunnerError)
        {
            return "Hidden test runner failed before results were available.";
        }

        return hidden.ExitCode == 0
            ? "Hidden tests passed."
            : "One or more hidden tests failed. Review the visible tests and edge-case requirements.";
    }

    private static string HiddenDiagnosticsForLearner(CommandResult hidden)
    {
        if (hidden.WasSkipped)
        {
            return string.Empty;
        }

        if (hidden.TimedOut)
        {
            return "Hidden tests timed out.";
        }

        if (hidden.HasRunnerError)
        {
            return hidden.Diagnostics;
        }

        return hidden.ExitCode == 0 ? string.Empty : "Hidden tests failed.";
    }

    private static string RunSummary(
        string status,
        IReadOnlyCollection<StaticAnalysisDiagnosticResponse> staticAnalysisDiagnostics)
    {
        var errors = staticAnalysisDiagnostics.Count(diagnostic => diagnostic.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
        var warnings = staticAnalysisDiagnostics.Count(diagnostic => diagnostic.Severity.Equals("warning", StringComparison.OrdinalIgnoreCase));
        return $"{status}: static analysis found {errors} error(s) and {warnings} warning(s).";
    }

    private static IReadOnlyCollection<StaticAnalysisDiagnosticResponse> ParseStaticAnalysisDiagnostics(
        string workspacePath,
        CommandResult analysis)
    {
        var lines = $"{analysis.Output}\n{analysis.Diagnostics}".Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        return lines
            .Select(line => ParseStaticAnalysisDiagnostic(workspacePath, line.Trim()))
            .Where(diagnostic => diagnostic is not null)
            .Cast<StaticAnalysisDiagnosticResponse>()
            .DistinctBy(diagnostic => (diagnostic.RuleId, diagnostic.FilePath, diagnostic.Line, diagnostic.Column, diagnostic.Message))
            .ToArray();
    }

    private static StaticAnalysisDiagnosticResponse? ParseStaticAnalysisDiagnostic(string workspacePath, string line)
    {
        var locationMatch = DotnetLocationDiagnosticPattern.Match(line);
        if (locationMatch.Success)
        {
            var filePath = locationMatch.Groups["file"].Value;
            var relativePath = Path.IsPathRooted(filePath)
                ? Path.GetRelativePath(workspacePath, filePath)
                : filePath;
            return new StaticAnalysisDiagnosticResponse(
                locationMatch.Groups["rule"].Value,
                locationMatch.Groups["severity"].Value.ToLowerInvariant(),
                locationMatch.Groups["message"].Value,
                relativePath,
                int.Parse(locationMatch.Groups["line"].Value),
                int.Parse(locationMatch.Groups["column"].Value));
        }

        var locationlessMatch = DotnetLocationlessDiagnosticPattern.Match(line);
        if (!locationlessMatch.Success)
        {
            return null;
        }

        return new StaticAnalysisDiagnosticResponse(
            locationlessMatch.Groups["rule"].Value,
            locationlessMatch.Groups["severity"].Value.ToLowerInvariant(),
            locationlessMatch.Groups["message"].Value,
            "project",
            null,
            null);
    }

    private static string Truncate(string value)
    {
        return value.Length <= OutputLimit ? value : value[..OutputLimit] + "\n[output truncated]";
    }

    private static string SolutionFile()
    {
        return """
        Microsoft Visual Studio Solution File, Format Version 12.00
        # Visual Studio Version 17
        VisualStudioVersion = 17.0.31903.59
        MinimumVisualStudioVersion = 10.0.40219.1
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Exercise", "src\Exercise\Exercise.csproj", "{11111111-1111-1111-1111-111111111111}"
        EndProject
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Exercise.Tests", "tests\Exercise.Tests\Exercise.Tests.csproj", "{22222222-2222-2222-2222-222222222222}"
        EndProject
        Global
        EndGlobal
        """;
    }

    private static string ExerciseProjectFile()
    {
        return """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <LangVersion>14.0</LangVersion>
            <EnableNETAnalyzers>true</EnableNETAnalyzers>
            <AnalysisLevel>latest</AnalysisLevel>
            <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
          </PropertyGroup>
        </Project>
        """;
    }

    private static string EditorConfigFile()
    {
        return """
        root = true

        [*.cs]
        dotnet_diagnostic.PTP0001.severity = warning
        dotnet_diagnostic.PTP0002.severity = warning
        """;
    }

    private static string TestProjectFile()
    {
        return """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <LangVersion>14.0</LangVersion>
            <IsPackable>false</IsPackable>
            <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
            <PackageReference Include="xunit.v3" Version="1.0.1" />
            <PackageReference Include="xunit.runner.visualstudio" Version="3.0.1" />
          </ItemGroup>
          <ItemGroup>
            <ProjectReference Include="../../src/Exercise/Exercise.csproj" />
          </ItemGroup>
        </Project>
        """;
    }

    private static string VisibleTests(string assertion)
    {
        return $$"""
        using Exercise;
        using Xunit;

        namespace Exercise.Tests;

        public sealed class VisibleTests
        {
            [Fact]
            public void VisibleScenario()
            {
        {{Indent(assertion, 8)}}
            }
        }
        """;
    }

    private static string HiddenTests(string assertion)
    {
        return $$"""
        using Exercise;
        using Xunit;

        namespace Exercise.Tests;

        public sealed class HiddenTests
        {
            [Fact]
            public void HiddenScenario()
            {
        {{Indent(assertion, 8)}}
            }
        }
        """;
    }

    private static string Indent(string value, int spaces)
    {
        var padding = new string(' ', spaces);
        return string.Join(
            Environment.NewLine,
            value.Trim().Split(["\r\n", "\n"], StringSplitOptions.None).Select(line => padding + line));
    }
}

public sealed class ExerciseContentDefinition
{
    public ExerciseWorkspaceDefinition Workspace { get; set; } = new();

    public List<ExerciseHintDefinition> Hints { get; set; } = [];

    public ExerciseSolutionDefinition Solution { get; set; } = new();
}

public sealed class ExerciseWorkspaceDefinition
{
    public string Starter { get; set; } = string.Empty;

    public string VisibleTest { get; set; } = string.Empty;

    public string HiddenTest { get; set; } = string.Empty;
}

public sealed class ExerciseHintDefinition
{
    public string Id { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;
}

public sealed class ExerciseSolutionDefinition
{
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}

public sealed record ExerciseAssistanceResponse(
    IReadOnlyCollection<ExerciseHintResponse> Hints,
    bool SolutionAvailable,
    ExerciseSolutionResponse? Solution);

public sealed record ExerciseHintResponse(
    string Id,
    string Level,
    string Title,
    string Body,
    bool Used);

public sealed record ExerciseSolutionResponse(
    string Title,
    string Body,
    string Code);

public sealed record ExerciseSolutionUnlockRequest(
    string ProfileId,
    string Reason);

public sealed record ExerciseWorkspaceResponse(
    string ExerciseId,
    string Title,
    string WorkspacePath,
    string LanguageServiceMessage,
    IReadOnlyCollection<ExerciseWorkspaceFileResponse> Files,
    string LastStatus,
    string LastOutput,
    string LastDiagnostics);

public sealed record ExerciseWorkspaceFileResponse(
    string Path,
    string Role,
    bool Editable,
    string? Content);

public sealed record ExerciseFileSaveRequest(string Path, string Content);

public sealed record ExerciseRunRequest(string ProfileId, IReadOnlyCollection<ExerciseFileSaveRequest> Files);

public sealed record ExerciseRunResponse(
    string Status,
    bool VisiblePassed,
    bool HiddenPassed,
    bool TimedOut,
    int? ExitCode,
    string Output,
    string Diagnostics,
    IReadOnlyCollection<StaticAnalysisDiagnosticResponse> StaticAnalysis);

public sealed record StaticAnalysisDiagnosticResponse(
    string RuleId,
    string Severity,
    string Message,
    string FilePath,
    int? Line,
    int? Column);

public sealed record ExerciseRunHistoryResponse(
    string Id,
    string Status,
    bool VisiblePassed,
    bool HiddenPassed,
    bool TimedOut,
    int? ExitCode,
    string Summary,
    int StaticAnalysisErrorCount,
    int StaticAnalysisWarningCount,
    DateTimeOffset CreatedAt);
