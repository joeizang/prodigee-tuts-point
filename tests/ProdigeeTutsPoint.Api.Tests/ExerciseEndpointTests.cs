using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProdigeeTutsPoint.Api.Features.Exercises;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class ExerciseEndpointTests
{
    [Fact]
    public async Task WorkspaceEndpointGeneratesRealDotnetWorkspace()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"workspace-test-{Guid.NewGuid():n}";

        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-to-lowercase/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "ExerciseWorkspace.sln")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "src", "Exercise", "Exercise.csproj")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "tests", "Exercise.Tests", "Exercise.Tests.csproj")));
        var solution = await File.ReadAllTextAsync(
            Path.Combine(workspace.WorkspacePath, "ExerciseWorkspace.sln"),
            TestContext.Current.CancellationToken);
        Assert.Contains(@"src\Exercise\Exercise.csproj", solution);
        Assert.Contains(@"tests\Exercise.Tests\Exercise.Tests.csproj", solution);
        Assert.Contains(workspace.Files, file => file.Path == "src/Exercise/WordFrequencyAnalyzer.cs" && file.Editable);
        Assert.Contains(workspace.Files, file => file.Role == "visible-test" && file.Content is not null);
        Assert.Contains(workspace.Files, file => file.Role == "hidden-test" && file.Content is null);
    }

    [Fact]
    public async Task RunEndpointExecutesVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"runner-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/Exercise/WordFrequencyAnalyzer.cs",
                        """
                        namespace Exercise;

                        public sealed record WordFrequency(string Word, int Count);

                        public static class WordFrequencyAnalyzer
                        {
                            public static string NormalizeToLowercase(string? text)
                            {
                                return text?.ToLowerInvariant() ?? string.Empty;
                            }

                            public static string KeepAsciiLettersAndDigits(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> SplitWordsOnSeparators(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> Tokenize(string? text) => throw new NotImplementedException();
                            public static Dictionary<string, int> CountWords(IEnumerable<string> words) => throw new NotImplementedException();
                            public static void UpdateFrequencyMap(Dictionary<string, int> map, string word) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> SortFrequencies(Dictionary<string, int> frequencies) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> Analyze(string? text) => throw new NotImplementedException();
                        }
                        """)
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Passed", result.Status);
        Assert.True(result.VisiblePassed);
        Assert.True(result.HiddenPassed);
        Assert.DoesNotContain("HiddenTests.cs", result.Output);
        Assert.NotNull(result.StaticAnalysis);

        var assistance = await client.GetFromJsonAsync<ExerciseAssistanceTestResponse>(
            $"/api/exercises/normalize-to-lowercase/assistance?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(assistance);
        Assert.True(assistance.SolutionAvailable);
        Assert.NotNull(assistance.Solution);
        Assert.Contains("ToLowerInvariant", assistance.Solution.Code);
    }

    [Fact]
    public async Task RunEndpointStoresAttemptHistoryAndStaticAnalysis()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"attempt-history-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/Exercise/WordFrequencyAnalyzer.cs",
                        """
                        namespace Exercise;

                        public sealed record WordFrequency(string Word, int Count);

                        public static class WordFrequencyAnalyzer
                        {
                            public static string NormalizeToLowercase(string? text)
                            {
                                return missingSymbol;
                            }

                            public static string KeepAsciiLettersAndDigits(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> SplitWordsOnSeparators(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> Tokenize(string? text) => throw new NotImplementedException();
                            public static Dictionary<string, int> CountWords(IEnumerable<string> words) => throw new NotImplementedException();
                            public static void UpdateFrequencyMap(Dictionary<string, int> map, string word) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> SortFrequencies(Dictionary<string, int> frequencies) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> Analyze(string? text) => throw new NotImplementedException();
                        }
                        """)
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);
        var history = await client.GetFromJsonAsync<IReadOnlyCollection<ExerciseRunHistoryTestResponse>>(
            $"/api/exercises/normalize-to-lowercase/attempts?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.StaticAnalysis);
        Assert.Contains(result.StaticAnalysis, diagnostic => diagnostic.RuleId == "CS0103" && diagnostic.Severity == "error");
        Assert.NotNull(history);
        var latest = Assert.Single(history);
        Assert.Equal(result.Status, latest.Status);
        Assert.True(latest.StaticAnalysisErrorCount > 0);
        Assert.Contains("static analysis", latest.Summary, StringComparison.OrdinalIgnoreCase);

        var storedDiagnostics = await client.GetFromJsonAsync<IReadOnlyCollection<StaticAnalysisDiagnosticTestResponse>>(
            $"/api/exercises/normalize-to-lowercase/static-analysis?profileId={profileId}&runHistoryId={latest.Id}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(storedDiagnostics);
        Assert.Contains(storedDiagnostics, diagnostic => diagnostic.RuleId == "CS0103" && diagnostic.Severity == "error");
    }

    [Fact]
    public async Task RunEndpointPreservesLocationlessStaticAnalysisDiagnostics()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IExerciseRunner>();
                    services.AddScoped<IExerciseRunner>(_ => new FakeExerciseRunner(
                        new CommandResult(
                            1,
                            false,
                            false,
                            false,
                            "error NU1100: Unable to resolve 'Imaginary.Package' for 'net10.0'.",
                            string.Empty),
                        new CommandResult(1, false, false, false, "Visible tests failed.", string.Empty),
                        CommandResult.Skipped("Hidden tests were not run because visible tests failed.")));
                });
            });
        using var client = factory.CreateClient();
        var profileId = $"locationless-analysis-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/run",
            new ExerciseRunTestRequest(profileId, []),
            TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.StaticAnalysis, diagnostic =>
            diagnostic.RuleId == "NU1100"
            && diagnostic.Severity == "error"
            && diagnostic.FilePath == "project"
            && diagnostic.Line is null
            && diagnostic.Column is null);
    }

    [Fact]
    public async Task AssistanceEndpointsTrackHintsUnlocksAndSolutionVisibility()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"assistance-test-{Guid.NewGuid():n}";

        var initial = await client.GetFromJsonAsync<ExerciseAssistanceTestResponse>(
            $"/api/exercises/normalize-to-lowercase/assistance?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(initial);
        Assert.Equal(3, initial.Hints.Count);
        Assert.False(initial.SolutionAvailable);
        Assert.Null(initial.Solution);

        var usedHintResponse = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/hints/conceptual/use",
            new ExerciseHintUseTestRequest(profileId),
            TestContext.Current.CancellationToken);
        var usedHint = await usedHintResponse.Content.ReadFromJsonAsync<ExerciseHintTestResponse>(
            TestContext.Current.CancellationToken);

        var solutionResponse = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/solution/unlock",
            new ExerciseSolutionUnlockTestRequest(profileId, "I want to compare my approach."),
            TestContext.Current.CancellationToken);
        var solution = await solutionResponse.Content.ReadFromJsonAsync<ExerciseSolutionTestResponse>(
            TestContext.Current.CancellationToken);

        var afterUnlock = await client.GetFromJsonAsync<ExerciseAssistanceTestResponse>(
            $"/api/exercises/normalize-to-lowercase/assistance?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(usedHint);
        Assert.True(usedHint.Used);
        Assert.Equal("conceptual", usedHint.Level);
        Assert.NotNull(solution);
        Assert.Contains("ToLowerInvariant", solution.Code);
        Assert.NotNull(afterUnlock);
        Assert.True(afterUnlock.SolutionAvailable);
        Assert.NotNull(afterUnlock.Solution);
        Assert.Contains(afterUnlock.Hints, hint => hint.Id == "conceptual" && hint.Used);

        var mastery = await client.GetFromJsonAsync<IReadOnlyCollection<ConceptMasteryTestResponse>>(
            $"/api/learner/mastery/concepts?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(mastery);
        Assert.Contains(mastery, item => item.EvidenceCount >= 2);
    }

    [Fact]
    public async Task RunEndpointRemovesCopiedRunWorkspace()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"run-cleanup-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/Exercise/WordFrequencyAnalyzer.cs",
                        """
                        namespace Exercise;

                        public sealed record WordFrequency(string Word, int Count);

                        public static class WordFrequencyAnalyzer
                        {
                            public static string NormalizeToLowercase(string? text)
                            {
                                return text?.ToLowerInvariant() ?? string.Empty;
                            }

                            public static string KeepAsciiLettersAndDigits(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> SplitWordsOnSeparators(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> Tokenize(string? text) => throw new NotImplementedException();
                            public static Dictionary<string, int> CountWords(IEnumerable<string> words) => throw new NotImplementedException();
                            public static void UpdateFrequencyMap(Dictionary<string, int> map, string word) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> SortFrequencies(Dictionary<string, int> frequencies) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> Analyze(string? text) => throw new NotImplementedException();
                        }
                        """)
                ]),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-to-lowercase/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);
        Assert.NotNull(workspace);
        var appData = Directory.GetParent(Directory.GetParent(Directory.GetParent(workspace.WorkspacePath)!.FullName)!.FullName)!.FullName;
        var runRoot = Path.Combine(appData, "exercise-runs");
        var leakedRuns = Directory.Exists(runRoot)
            ? Directory.GetDirectories(runRoot, $"*{profileId}*", SearchOption.TopDirectoryOnly)
            : [];

        Assert.Empty(leakedRuns);
    }

    [Fact]
    public async Task WorkspaceEndpointRefreshesGeneratedFilesWithoutOverwritingLearnerSource()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"workspace-refresh-test-{Guid.NewGuid():n}";

        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-to-lowercase/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        var visibleTestsPath = Path.Combine(workspace.WorkspacePath, "tests", "Exercise.Tests", "VisibleTests.cs");
        var learnerSourcePath = Path.Combine(workspace.WorkspacePath, "src", "Exercise", "WordFrequencyAnalyzer.cs");
        await File.WriteAllTextAsync(visibleTestsPath, "// stale generated visible test", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(learnerSourcePath, "// learner source must survive", TestContext.Current.CancellationToken);

        workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-to-lowercase/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        var refreshedVisibleTests = await File.ReadAllTextAsync(visibleTestsPath, TestContext.Current.CancellationToken);
        var learnerSource = await File.ReadAllTextAsync(learnerSourcePath, TestContext.Current.CancellationToken);
        Assert.Contains("VisibleScenario", refreshedVisibleTests);
        Assert.DoesNotContain("stale generated visible test", refreshedVisibleTests);
        Assert.Contains("learner source must survive", learnerSource);
    }

    [Fact]
    public async Task RunEndpointDoesNotLeakHiddenTestDetailsWhenHiddenTestsFail()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"hidden-privacy-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/Exercise/WordFrequencyAnalyzer.cs",
                        """
                        namespace Exercise;

                        public sealed record WordFrequency(string Word, int Count);

                        public static class WordFrequencyAnalyzer
                        {
                            public static string NormalizeToLowercase(string? text)
                            {
                                return text!.ToLowerInvariant();
                            }

                            public static string KeepAsciiLettersAndDigits(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> SplitWordsOnSeparators(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> Tokenize(string? text) => throw new NotImplementedException();
                            public static Dictionary<string, int> CountWords(IEnumerable<string> words) => throw new NotImplementedException();
                            public static void UpdateFrequencyMap(Dictionary<string, int> map, string word) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> SortFrequencies(Dictionary<string, int> frequencies) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> Analyze(string? text) => throw new NotImplementedException();
                        }
                        """)
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("FailedHidden", result.Status);
        Assert.True(result.VisiblePassed);
        Assert.False(result.HiddenPassed);
        Assert.Contains("One or more hidden tests failed", result.Output);
        Assert.DoesNotContain("HiddenScenario", result.Output);
        Assert.DoesNotContain("HiddenTests", result.Output);
        Assert.DoesNotContain("ArgumentNullException", result.Output);
        Assert.DoesNotContain("ArgumentNullException", result.Diagnostics);
    }

    [Fact]
    public async Task RunEndpointMarksHiddenTestsAsNotPassedWhenVisibleTestsFail()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"hidden-skipped-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/Exercise/WordFrequencyAnalyzer.cs",
                        """
                        namespace Exercise;

                        public sealed record WordFrequency(string Word, int Count);

                        public static class WordFrequencyAnalyzer
                        {
                            public static string NormalizeToLowercase(string? text)
                            {
                                return "not normalized";
                            }

                            public static string KeepAsciiLettersAndDigits(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> SplitWordsOnSeparators(string? text) => throw new NotImplementedException();
                            public static IReadOnlyList<string> Tokenize(string? text) => throw new NotImplementedException();
                            public static Dictionary<string, int> CountWords(IEnumerable<string> words) => throw new NotImplementedException();
                            public static void UpdateFrequencyMap(Dictionary<string, int> map, string word) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> SortFrequencies(Dictionary<string, int> frequencies) => throw new NotImplementedException();
                            public static IReadOnlyList<WordFrequency> Analyze(string? text) => throw new NotImplementedException();
                        }
                        """)
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("FailedVisible", result.Status);
        Assert.False(result.VisiblePassed);
        Assert.False(result.HiddenPassed);
        Assert.Contains("Hidden tests were not run because visible tests failed.", result.Output);
    }

    [Fact]
    public async Task DiagnosticsEndpointReturnsRoslynErrorsForCurrentContent()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"diagnostics-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/diagnostics",
            new ExerciseLanguageTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                """
                namespace Exercise;

                public static class WordFrequencyAnalyzer
                {
                    public static string NormalizeToLowercase(string? text)
                    {
                        return missingSymbol;
                    }
                }
                """),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseDiagnosticsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "CS0103");
    }

    [Fact]
    public async Task DiagnosticsEndpointReturnsAnalyzerStyleDiagnostics()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"analyzer-diagnostics-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/diagnostics",
            new ExerciseLanguageTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                """
                namespace Exercise;

                public static class WordFrequencyAnalyzer
                {
                    public static string NormalizeToLowercase(string? text)
                    {
                        var unused = text;
                        return text?.ToLowerInvariant() ?? string.Empty;
                    }
                }
                """),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseDiagnosticsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "PTP0001");
    }

    [Fact]
    public async Task CompletionsEndpointReturnsContextAwareRoslynItems()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"completion-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public static class WordFrequencyAnalyzer
            {
                public static string NormalizeToLowercase(string? text)
                {
                    return text.
                }
            }
            """;

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/completions",
            new ExerciseCompletionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                7,
                21),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCompletionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Label == "ToLowerInvariant");
    }

    [Fact]
    public async Task CompletionsEndpointDoesNotReturnKeywordOrSnippetNoiseForMemberAccess()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"member-access-completion-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public static class WordFrequencyAnalyzer
            {
                public static string NormalizeToLowercase(string? text)
                {
                    return text.
                }
            }
            """;

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/completions",
            new ExerciseCompletionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                7,
                21),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCompletionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.DoesNotContain(result.Items, item => item.Tags.Contains("Keyword"));
        Assert.DoesNotContain(result.Items, item => item.Tags.Contains("Snippet"));
        Assert.DoesNotContain(result.Items, item => item.Label == "for");
    }

    [Fact]
    public async Task CompletionsEndpointReturnsMethodParametersInScope()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"parameter-completion-test-{Guid.NewGuid():n}";
        var content = """
            using System.Collections.Generic;

            namespace Exercise;

            public static class WordFrequencyAnalyzer
            {
                public static IReadOnlyList<string> SplitWordsOnSeparators(string? text)
                {
                    te
                }
            }
            """;
        var position = PositionOf(content, "        te", "        te".Length);

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/completions",
            new ExerciseCompletionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCompletionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Label == "text");
    }

    [Fact]
    public async Task CompletionsEndpointResolvesNullableMethodParameterMemberAccess()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"nullable-member-completion-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public sealed record WordFrequency(string Word, int Count);

            public static class WordFrequencyAnalyzer
            {
                public static string NormalizeToLowercase(string? text)
                {
                    text?.
                    throw new NotImplementedException();
                }
            }
            """;
        var position = PositionOf(content, "        text?.", "        text?.".Length);

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/completions",
            new ExerciseCompletionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCompletionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Label == "ToLowerInvariant");
    }

    [Fact]
    public async Task CompletionsEndpointPrioritizesCurrentMethodParameterInStatementPosition()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"words-parameter-completion-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public sealed record WordFrequency(string Word, int Count);

            public static class WordFrequencyAnalyzer
            {
                public static Dictionary<string, int> CountWords(IEnumerable<string> words)
                {
                    words
                    throw new NotImplementedException();
                }
            }
            """;
        var position = PositionOf(content, "        words", "        words".Length);

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/completions",
            new ExerciseCompletionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCompletionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var first = result.Items.First();
        Assert.Equal("words", first.Label);
        Assert.Contains("IEnumerable<string>", first.Detail);
        Assert.Contains("Parameter", first.Tags);
    }

    [Fact]
    public async Task CompletionsEndpointReturnsAllCurrentMethodParametersInScope()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"all-parameters-completion-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public sealed record WordFrequency(string Word, int Count);

            public static class WordFrequencyAnalyzer
            {
                public static void UpdateFrequencyMap(Dictionary<string, int> map, string word)
                {
                    
                }
            }
            """;
        var position = PositionOf(content, "        ", "        ".Length);

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/completions",
            new ExerciseCompletionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCompletionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Label == "map" && item.Tags.Contains("Parameter"));
        Assert.Contains(result.Items, item => item.Label == "word" && item.Tags.Contains("Parameter"));
    }

    [Fact]
    public async Task HoverEndpointReturnsSemanticFrameworkSymbolInformation()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"hover-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public static class WordFrequencyAnalyzer
            {
                public static string NormalizeToLowercase(string? text)
                {
                    throw new NotImplementedException();
                }
            }
            """;
        var position = PositionOf(content, "NotImplementedException");

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/hover",
            new ExercisePositionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseHoverTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Contents, content => content.Contains("System.NotImplementedException", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Contents, content => content.Contains("C# exercise workspace", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SignatureHelpEndpointReturnsActiveMethodSignature()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"signature-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public static class WordFrequencyAnalyzer
            {
                public static string NormalizeToLowercase(string? text)
                {
                    var normalized = NormalizeToLowercase(text);
                    return normalized;
                }
            }
            """;
        var position = PositionOf(content, "NormalizeToLowercase(text)", "NormalizeToLowercase(".Length);

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/signature-help",
            new ExercisePositionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseSignatureHelpTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Signatures, signature =>
            signature.Label.Contains("NormalizeToLowercase", StringComparison.Ordinal)
            && signature.Parameters.Any(parameter => parameter.Name == "text"));
        Assert.Equal(0, result.ActiveParameter);
    }

    [Fact]
    public async Task CodeActionsEndpointReturnsRealEditableRefactoring()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"code-action-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public static class WordFrequencyAnalyzer
            {
                public static string NormalizeToLowercase(string? text)
                {
                    return text?.ToLowerInvariant() ?? string.Empty;
                }
            }
            """;
        var position = PositionOf(content, "return text");

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/code-actions",
            new ExerciseCodeActionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column,
                position.Line,
                position.Column + 6),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCodeActionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var action = Assert.Single(result.Actions, action => action.Title == "Convert to expression-bodied member");
        Assert.Contains("=> text?.ToLowerInvariant() ?? string.Empty;", action.Edits.Single().Text);
    }

    [Fact]
    public async Task CodeActionsEndpointReturnsMissingUsingQuickFix()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"missing-using-test-{Guid.NewGuid():n}";
        var content = """
            namespace Exercise;

            public static class WordFrequencyAnalyzer
            {
                public static string NormalizeToLowercase(string? text)
                {
                    var builder = new StringBuilder();
                    return builder.ToString();
                }
            }
            """;
        var position = PositionOf(content, "StringBuilder");

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/code-actions",
            new ExerciseCodeActionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column,
                position.Line,
                position.Column + "StringBuilder".Length),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCodeActionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var action = Assert.Single(result.Actions, action => action.Title == "Add using System.Text");
        Assert.Contains("using System.Text;", action.Edits.Single().Text);
    }

    [Fact]
    public async Task FormatEndpointReturnsRoslynFormattedContent()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"format-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/format",
            new ExerciseLanguageTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                """
                namespace Exercise;
                public static class WordFrequencyAnalyzer{
                public static string NormalizeToLowercase(string? text){return text?.ToLowerInvariant() ?? string.Empty;}
                }
                """),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseFormatTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains("public static class WordFrequencyAnalyzer\n{", result.Content);
        Assert.Contains("    public static string NormalizeToLowercase(string? text)", result.Content);
        Assert.Contains("return text?.ToLowerInvariant() ?? string.Empty;", result.Content);
    }

    private sealed record ExerciseWorkspaceTestResponse(
        string ExerciseId,
        string Title,
        string WorkspacePath,
        string LanguageServiceMessage,
        IReadOnlyCollection<ExerciseWorkspaceFileTestResponse> Files,
        string LastStatus,
        string LastOutput,
        string LastDiagnostics);

    private sealed record ExerciseWorkspaceFileTestResponse(
        string Path,
        string Role,
        bool Editable,
        string? Content);

    private sealed record ExerciseRunTestRequest(
        string ProfileId,
        IReadOnlyCollection<ExerciseFileSaveTestRequest> Files);

    private sealed record ExerciseFileSaveTestRequest(string Path, string Content);

    private sealed record ExerciseRunTestResponse(
        string Status,
        bool VisiblePassed,
        bool HiddenPassed,
        bool TimedOut,
        int? ExitCode,
        string Output,
        string Diagnostics,
        IReadOnlyCollection<StaticAnalysisDiagnosticTestResponse> StaticAnalysis);

    private sealed record StaticAnalysisDiagnosticTestResponse(
        string RuleId,
        string Severity,
        string Message,
        string FilePath,
        int? Line,
        int? Column);

    private sealed record ExerciseRunHistoryTestResponse(
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

    private sealed record ExerciseAssistanceTestResponse(
        IReadOnlyCollection<ExerciseHintTestResponse> Hints,
        bool SolutionAvailable,
        ExerciseSolutionTestResponse? Solution);

    private sealed record ExerciseHintTestResponse(
        string Id,
        string Level,
        string Title,
        string Body,
        bool Used);

    private sealed record ExerciseSolutionTestResponse(
        string Title,
        string Body,
        string Code);

    private sealed record ExerciseHintUseTestRequest(string ProfileId);

    private sealed record ExerciseSolutionUnlockTestRequest(string ProfileId, string Reason);

    private sealed record ConceptMasteryTestResponse(
        string ConceptId,
        string Title,
        int Score,
        int MaxScore,
        int EvidenceCount,
        string Status,
        DateTimeOffset? LastActivityAt);

    private sealed record ExerciseLanguageTestRequest(
        string ProfileId,
        string Path,
        string Content);

    private sealed record ExerciseCompletionTestRequest(
        string ProfileId,
        string Path,
        string Content,
        int LineNumber,
        int Column);

    private sealed record ExercisePositionTestRequest(
        string ProfileId,
        string Path,
        string Content,
        int LineNumber,
        int Column);

    private sealed record ExerciseCodeActionTestRequest(
        string ProfileId,
        string Path,
        string Content,
        int StartLineNumber,
        int StartColumn,
        int EndLineNumber,
        int EndColumn);

    private sealed record ExerciseDiagnosticsTestResponse(
        IReadOnlyCollection<ExerciseDiagnosticTestResponse> Diagnostics);

    private sealed record ExerciseDiagnosticTestResponse(
        string Id,
        string Message,
        string Severity,
        int StartLineNumber,
        int StartColumn,
        int EndLineNumber,
        int EndColumn);

    private sealed record ExerciseCompletionsTestResponse(
        IReadOnlyCollection<ExerciseCompletionItemTestResponse> Items);

    private sealed record ExerciseCompletionItemTestResponse(
        string Label,
        string InsertText,
        string FilterText,
        string SortText,
        string Detail,
        IReadOnlyList<string> Tags);

    private sealed record ExerciseHoverTestResponse(
        IReadOnlyCollection<string> Contents,
        ExerciseRangeTestResponse? Range);

    private sealed record ExerciseSignatureHelpTestResponse(
        IReadOnlyCollection<ExerciseSignatureItemTestResponse> Signatures,
        int ActiveSignature,
        int ActiveParameter);

    private sealed record ExerciseSignatureItemTestResponse(
        string Label,
        string Documentation,
        IReadOnlyCollection<ExerciseSignatureParameterTestResponse> Parameters);

    private sealed record ExerciseSignatureParameterTestResponse(
        string Label,
        string Name,
        string Documentation);

    private sealed record ExerciseCodeActionsTestResponse(
        IReadOnlyCollection<ExerciseCodeActionItemTestResponse> Actions);

    private sealed record ExerciseCodeActionItemTestResponse(
        string Title,
        string Kind,
        IReadOnlyCollection<ExerciseTextEditTestResponse> Edits);

    private sealed record ExerciseTextEditTestResponse(
        int StartLineNumber,
        int StartColumn,
        int EndLineNumber,
        int EndColumn,
        string Text);

    private sealed record ExerciseRangeTestResponse(
        int StartLineNumber,
        int StartColumn,
        int EndLineNumber,
        int EndColumn);

    private sealed record ExerciseFormatTestResponse(string Content);

    private static (int Line, int Column) PositionOf(string content, string marker, int offset = 0)
    {
        var absolute = content.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(absolute >= 0, $"Could not find marker '{marker}'.");
        absolute += offset;

        var line = 1;
        var column = 1;
        for (var index = 0; index < absolute; index++)
        {
            if (content[index] == '\n')
            {
                line++;
                column = 1;
                continue;
            }

            column++;
        }

        return (line, column);
    }

    private sealed class FakeExerciseRunner(
        CommandResult analysis,
        CommandResult visible,
        CommandResult hidden) : IExerciseRunner
    {
        public Task<CommandResult> RunStaticAnalysisAsync(string testProject, CancellationToken cancellationToken)
        {
            return Task.FromResult(analysis);
        }

        public Task<CommandResult> RunVisibleTestsAsync(string testProject, CancellationToken cancellationToken)
        {
            return Task.FromResult(visible);
        }

        public Task<CommandResult> RunHiddenTestsAsync(string testProject, CancellationToken cancellationToken)
        {
            return Task.FromResult(hidden);
        }
    }
}
