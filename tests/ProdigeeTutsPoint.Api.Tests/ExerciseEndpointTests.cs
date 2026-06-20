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
    public async Task WorkspaceEndpointGeneratesStreamingAndLogqueryWorkspaces()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var streaming = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/run-streaming-wordfreq/workspace?profileId=streaming-{Guid.NewGuid():n}",
            TestContext.Current.CancellationToken);
        var logquery = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/run-logquery-summary/workspace?profileId=logquery-{Guid.NewGuid():n}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(streaming);
        Assert.Contains(streaming.Files, file =>
            file.Path == "src/Exercise/WordFrequencyAnalyzer.cs"
            && file.Editable
            && file.Content is not null
            && file.Content.Contains("WordFrequencyStreaming", StringComparison.Ordinal));
        Assert.Contains(streaming.Files, file =>
            file.Role == "visible-test"
            && file.Content is not null
            && file.Content.Contains("WordFrequencyStreaming.Run", StringComparison.Ordinal));

        Assert.NotNull(logquery);
        Assert.Contains(logquery.Files, file =>
            file.Path == "src/Exercise/WordFrequencyAnalyzer.cs"
            && file.Editable
            && file.Content is not null
            && file.Content.Contains("LogQuery", StringComparison.Ordinal));
        Assert.Contains(logquery.Files, file =>
            file.Role == "visible-test"
            && file.Content is not null
            && file.Content.Contains("LogQuery.Run", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WorkspaceEndpointGeneratesTypeScriptNodeWorkspace()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/parse-command-request-ts/workspace?profileId=typescript-{Guid.NewGuid():n}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        Assert.Equal("TypeScript", workspace.Language);
        Assert.Equal("node-typescript", workspace.Runtime);
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "package.json")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "tsconfig.json")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "src", "exercise.ts")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "tests", "visible.test.ts")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "tests", "hidden.test.ts")));
        Assert.Contains(workspace.Files, file => file.Path == "src/exercise.ts" && file.Editable);
        Assert.Contains(workspace.Files, file => file.Path == "tests/visible.test.ts" && file.Role == "visible-test" && file.Content is not null);
        Assert.Contains(workspace.Files, file => file.Path == "tests/hidden.test.ts" && file.Role == "hidden-test" && file.Content is null);
    }

    [Fact]
    public async Task WorkspaceEndpointGeneratesTypeScriptWorkspaceWithExerciseSpecificEntryPoint()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/parse-output-format-ts/workspace?profileId=typescript-entrypoint-{Guid.NewGuid():n}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        Assert.Equal("TypeScript", workspace.Language);
        Assert.Contains(workspace.Files, file =>
            file.Path == "tests/visible.test.ts"
            && file.Content is not null
            && file.Content.Contains("import { parseOutputFormat } from '../src/exercise'", StringComparison.Ordinal));
        Assert.Contains(workspace.Files, file =>
            file.Path == "tests/visible.test.ts"
            && file.Content is not null
            && file.Content.Contains("async () =>", StringComparison.Ordinal));
        Assert.Contains(workspace.Files, file =>
            file.Path == "src/exercise.ts"
            && file.Editable
            && file.Content is not null
            && file.Content.Contains("export function parseOutputFormat", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WorkspaceEndpointGeneratesSwiftPackageWorkspace()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-workspace-{Guid.NewGuid():n}";

        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/parse-command-request-swift/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        Assert.Equal("Swift", workspace.Language);
        Assert.Equal("swiftpm", workspace.Runtime);
        Assert.Contains("swift test execution are active", workspace.LanguageServiceMessage);
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "Package.swift")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "Sources", "Exercise", "Exercise.swift")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "Tests", "ExerciseVisibleTests", "VisibleTests.swift")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "Tests", "ExerciseHiddenTests", "HiddenTests.swift")));
        Assert.Contains(workspace.Files, file =>
            file.Path == "Sources/Exercise/Exercise.swift"
            && file.Editable
            && file.Content is not null
            && file.Content.Contains("parseCommandRequest", StringComparison.Ordinal));
        Assert.Contains(workspace.Files, file =>
            file.Path == "Tests/ExerciseVisibleTests/VisibleTests.swift"
            && file.Role == "visible-test"
            && file.Content is not null
            && file.Content.Contains("XCTAssertEqual", StringComparison.Ordinal));
        Assert.Contains(workspace.Files, file =>
            file.Path == "Tests/ExerciseHiddenTests/HiddenTests.swift"
            && file.Role == "hidden-test"
            && file.Content is null);
        Assert.Contains(workspace.Files, file =>
            file.Path == "Package.swift"
            && file.Role == "readonly"
            && file.Content is not null
            && file.Content.Contains(".testTarget", StringComparison.Ordinal));

        var sourcePath = Path.Combine(workspace.WorkspacePath, "Sources", "Exercise", "Exercise.swift");
        await File.WriteAllTextAsync(sourcePath, "public func learnerEdit() {}", TestContext.Current.CancellationToken);

        workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/parse-command-request-swift/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        Assert.Contains(workspace.Files, file =>
            file.Path == "Sources/Exercise/Exercise.swift"
            && file.Content == "public func learnerEdit() {}");
    }

    [Fact]
    public async Task WorkspaceEndpointGeneratesPythonPytestWorkspace()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"python-workspace-{Guid.NewGuid():n}";

        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-note-title-py/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        Assert.Equal("Python", workspace.Language);
        Assert.Equal("python-pytest", workspace.Runtime);
        Assert.Contains("Pyright", workspace.LanguageServiceMessage);
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "pyproject.toml")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "src", "note_titles.py")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "tests", "test_note_titles_visible.py")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspacePath, "tests", "test_note_titles_hidden.py")));
        Assert.Contains(workspace.Files, file =>
            file.Path == "src/note_titles.py"
            && file.Editable
            && file.Content is not null
            && file.Content.Contains("def normalize_title", StringComparison.Ordinal));
        Assert.Contains(workspace.Files, file =>
            file.Path == "tests/test_note_titles_visible.py"
            && file.Role == "visible-test"
            && file.Content is not null
            && file.Content.Contains("from note_titles import normalize_title", StringComparison.Ordinal));
        Assert.Contains(workspace.Files, file =>
            file.Path == "tests/test_note_titles_hidden.py"
            && file.Role == "hidden-test"
            && file.Content is null);
        Assert.Contains(workspace.Files, file =>
            file.Path == "pyproject.toml"
            && file.Role == "readonly"
            && file.Content is not null
            && file.Content.Contains("basedpyright", StringComparison.Ordinal)
            && file.Content.Contains("uv sync", StringComparison.Ordinal));

        var sourcePath = Path.Combine(workspace.WorkspacePath, "src", "note_titles.py");
        await File.WriteAllTextAsync(sourcePath, "def normalize_title(raw_title: str) -> str:\n    return raw_title\n", TestContext.Current.CancellationToken);

        workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-note-title-py/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        Assert.Contains(workspace.Files, file =>
            file.Path == "src/note_titles.py"
            && file.Content is not null
            && file.Content.Contains("return raw_title", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WorkspaceEndpointMigratesLegacyPythonWorkspaceGeneratedAsCSharpFallback()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"python-legacy-workspace-{Guid.NewGuid():n}";

        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-note-title-py/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        var legacySourceDirectory = Path.Combine(workspace.WorkspacePath, "src", "Exercise");
        Directory.CreateDirectory(legacySourceDirectory);
        await File.WriteAllTextAsync(
            Path.Combine(legacySourceDirectory, "WordFrequencyAnalyzer.cs"),
            """
            def normalize_title(raw_title: str) -> str:
                return raw_title.strip().lower()
            """,
            TestContext.Current.CancellationToken);
        File.Delete(Path.Combine(workspace.WorkspacePath, "src", "note_titles.py"));

        workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-note-title-py/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        Assert.Equal("python-pytest", workspace.Runtime);
        Assert.Contains("Pyright", workspace.LanguageServiceMessage);
        Assert.DoesNotContain(workspace.Files, file => file.Path.EndsWith(".cs", StringComparison.Ordinal));
        Assert.Contains(workspace.Files, file =>
            file.Path == "src/note_titles.py"
            && file.Editable
            && file.Content is not null
            && file.Content.Contains("return raw_title.strip().lower()", StringComparison.Ordinal));
        Assert.False(File.Exists(Path.Combine(workspace.WorkspacePath, "src", "Exercise", "WordFrequencyAnalyzer.cs")));
        Assert.False(File.Exists(Path.Combine(workspace.WorkspacePath, "ExerciseWorkspace.sln")));
    }

    [Fact]
    public async Task RunEndpointExecutesPythonVisibleAndHiddenTestsThroughUv()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"python-runner-missing-uv-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-note-title-py/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/note_titles.py",
                        """
                        def normalize_title(raw_title: str) -> str:
                            words = raw_title.split()
                            if not words:
                                raise ValueError("title must contain at least one non-space character")

                            return " ".join(words).lower()
                        """)
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Passed", result.Status);
        Assert.True(result.VisiblePassed);
        Assert.True(result.HiddenPassed);
        Assert.DoesNotContain("uv is required", result.Diagnostics, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunEndpointExecutesTypeScriptVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"typescript-runner-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-ts/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/exercise.ts",
                        """
                        export type LogLevel = 'debug' | 'info' | 'warn' | 'error'

                        export type CommandRequest = {
                          readonly level: LogLevel
                          readonly limit: number
                          readonly includeArchived: boolean
                        }

                        const levels = ['debug', 'info', 'warn', 'error'] as const

                        export function parseCommandRequest(args: readonly string[]): CommandRequest {
                          let level: LogLevel = 'info'
                          let limit = 100
                          let includeArchived = false

                          for (let index = 0; index < args.length; index++) {
                            const option = args[index]

                            if (option === '--archived') {
                              includeArchived = true
                              continue
                            }

                            if (option === '--level') {
                              const value = args[++index]
                              if (!isLogLevel(value)) {
                                throw new Error(`Unsupported level: ${value ?? '<missing>'}`)
                              }

                              level = value
                              continue
                            }

                            if (option === '--limit') {
                              const value = args[++index]
                              const parsed = Number(value)
                              if (!Number.isInteger(parsed) || parsed < 1) {
                                throw new Error(`Invalid limit: ${value ?? '<missing>'}`)
                              }

                              limit = parsed
                              continue
                            }

                            throw new Error(`Unknown option: ${option}`)
                          }

                          return { level, limit, includeArchived }
                        }

                        function isLogLevel(value: string | undefined): value is LogLevel {
                          return levels.some((level) => level === value)
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
        Assert.DoesNotContain("hidden.test.ts", result.Output);
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointExecutesTypeScriptCliContractsVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"typescript-cli-contracts-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-output-format-ts/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/exercise.ts",
                        """
                        export type OutputFormat = 'table' | 'json'

                        export function parseOutputFormat(value: string | undefined): OutputFormat {
                          if (value === undefined) {
                            return 'table'
                          }

                          if (value === 'table' || value === 'json') {
                            return value
                          }

                          throw new Error(`Unsupported format: ${value}`)
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
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointExecutesAsyncTypeScriptFileIoVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"typescript-file-io-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/resolve-input-source-ts/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/exercise.ts",
                        """
                        export type FileReadResult =
                          | { readonly ok: true; readonly text: string }
                          | { readonly ok: false; readonly message: string }

                        export type InputSource =
                          | { readonly kind: 'stdin' }
                          | { readonly kind: 'file'; readonly path: string }

                        export type TextFileReader = (path: string) => Promise<string>

                        export async function resolveInputSource(
                          source: InputSource,
                          readStdin: () => Promise<string>,
                          readFile: TextFileReader,
                        ): Promise<FileReadResult> {
                          if (source.kind === 'stdin') {
                            return { ok: true, text: await readStdin() }
                          }

                          try {
                            return { ok: true, text: await readFile(source.path) }
                          } catch {
                            return { ok: false, message: `Could not read input file: ${source.path}` }
                          }
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
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointExecutesTypeScriptStreamingVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"typescript-streaming-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/run-streaming-logprobe-ts/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/exercise.ts",
                        """
                        export type StreamingRunResult = {
                          readonly exitCode: number
                          readonly stdout: string
                          readonly stderr: string
                        }

                        export async function runStreamingLogprobe(
                          lines: AsyncIterable<string>,
                          limit: number,
                        ): Promise<StreamingRunResult> {
                          const counts = new Map<string, number>()

                          for await (const line of lines) {
                            const level = line.trim().split(/\s+/)[1]?.toUpperCase()
                            if (level === undefined) {
                              continue
                            }

                            counts.set(level, (counts.get(level) ?? 0) + 1)
                          }

                          const stdout = [...counts.entries()]
                            .map(([key, count]) => ({ key, count }))
                            .sort((left, right) => right.count - left.count || left.key.localeCompare(right.key))
                            .slice(0, Math.max(0, limit))
                            .map((row) => `${row.key}\t${row.count}`)
                            .join('\n')

                          return { exitCode: 0, stdout, stderr: '' }
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
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointExecutesTypeScriptHttpAdapterVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"typescript-http-adapter-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/handle-logprobe-request-ts/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/exercise.ts",
                        """
                        export type LogQueryHttpRequest = {
                          readonly method: string
                          readonly url: string
                        }

                        export type LogQueryRequest = {
                          readonly level?: string
                          readonly limit: number
                        }

                        export type HttpResponse = {
                          readonly status: number
                          readonly headers: Readonly<Record<string, string>>
                          readonly body: string
                        }

                        export type LogSummaryReader = (
                          request: LogQueryRequest,
                        ) => Promise<readonly { readonly level: string; readonly count: number }[]>

                        export async function handleLogprobeRequest(
                          request: LogQueryHttpRequest,
                          readSummary: LogSummaryReader,
                        ): Promise<HttpResponse> {
                          const headers = { 'content-type': 'application/json' }
                          if (request.method.toUpperCase() !== 'GET') {
                            return { status: 405, headers, body: JSON.stringify({ error: 'Method not allowed' }) }
                          }

                          const parsed = new URL(request.url, 'http://localhost')
                          if (parsed.pathname !== '/logs') {
                            return { status: 404, headers, body: JSON.stringify({ error: 'Route not found' }) }
                          }

                          const rawLimit = parsed.searchParams.get('limit')
                          const limit = rawLimit === null ? 100 : Number(rawLimit)
                          if (!Number.isInteger(limit) || limit < 1) {
                            return { status: 400, headers, body: JSON.stringify({ error: 'Invalid limit' }) }
                          }

                          const level = parsed.searchParams.get('level') ?? undefined
                          const query = level === undefined ? { limit } : { level, limit }
                          const data = await readSummary(query)
                          return { status: 200, headers, body: JSON.stringify({ data }) }
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
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointExecutesTypeScriptNodeRuntimeVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"typescript-node-runtime-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/adapt-node-request-ts/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/exercise.ts",
                        """
                        export type RuntimeRequest = {
                          readonly method: string
                          readonly url: string
                          readonly headers: Readonly<Record<string, string>>
                          readonly body: string
                        }

                        export async function adaptNodeRequest(input: {
                          readonly method?: string
                          readonly url?: string
                          readonly headers: Readonly<Record<string, string | readonly string[] | undefined>>
                          readonly bodyChunks: AsyncIterable<string | Buffer>
                        }): Promise<RuntimeRequest> {
                          const headers: Record<string, string> = {}
                          for (const [name, value] of Object.entries(input.headers)) {
                            if (value === undefined) {
                              continue
                            }

                            headers[name.toLowerCase()] = typeof value === 'string' ? value : value.join(', ')
                          }

                          let body = ''
                          for await (const chunk of input.bodyChunks) {
                            body += typeof chunk === 'string' ? chunk : chunk.toString('utf8')
                          }

                          return {
                            method: input.method ?? 'GET',
                            url: input.url ?? '/',
                            headers,
                            body,
                          }
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
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointExecutesTypeScriptServerHardeningVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"typescript-server-hardening-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/record-handler-telemetry-ts/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/exercise.ts",
                        """
                        export type RuntimeRequest = {
                          readonly method: string
                          readonly url: string
                          readonly headers: Readonly<Record<string, string>>
                          readonly body: string
                        }

                        export type RuntimeResponse = {
                          readonly status: number
                          readonly headers: Readonly<Record<string, string>>
                          readonly body: string
                        }

                        export type RequestContext = {
                          readonly requestId: string
                          readonly startedAtMs: number
                        }

                        export type TelemetryEvent = {
                          readonly requestId: string
                          readonly method: string
                          readonly url: string
                          readonly status: number
                          readonly durationMs: number
                        }

                        export type TelemetrySink = (event: TelemetryEvent) => void

                        export async function recordHandlerTelemetry(
                          request: RuntimeRequest,
                          context: RequestContext,
                          handle: () => Promise<RuntimeResponse>,
                          nowMs: () => number,
                          sink: TelemetrySink,
                        ): Promise<RuntimeResponse> {
                          const response = await handle()
                          sink({
                            requestId: context.requestId,
                            method: request.method,
                            url: request.url,
                            status: response.status,
                            durationMs: nowMs() - context.startedAtMs,
                          })
                          return response
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
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointExecutesSwiftVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-runner-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-swift/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "Sources/Exercise/Exercise.swift",
                        SwiftParserSolution())
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Passed", result.Status);
        Assert.True(result.VisiblePassed);
        Assert.True(result.HiddenPassed);
        Assert.DoesNotContain("ExerciseHiddenTests", result.Output, StringComparison.Ordinal);
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointSkipsSwiftHiddenTestsWhenVisibleTestsFail()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-visible-fail-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-swift/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "Sources/Exercise/Exercise.swift",
                        SwiftParserSolution().Replace("var limit = 100", "var limit = 1", StringComparison.Ordinal))
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
    public async Task RunEndpointMasksSwiftHiddenTestDetailsWhenHiddenTestsFail()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-hidden-fail-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-swift/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "Sources/Exercise/Exercise.swift",
                        SwiftParserSolution().Replace(
                            "throw CommandRequestError.invalidLevel(value)",
                            "throw CommandRequestError.unknownOption(value)",
                            StringComparison.Ordinal))
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("FailedHidden", result.Status);
        Assert.True(result.VisiblePassed);
        Assert.False(result.HiddenPassed);
        Assert.Contains("One or more hidden tests failed.", result.Output);
        Assert.DoesNotContain("trace", result.Output, StringComparison.Ordinal);
        Assert.Equal("Hidden tests failed.", result.Diagnostics);
    }

    [Fact]
    public async Task RunEndpointReturnsSwiftStaticAnalysisDiagnostics()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-analysis-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-swift/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "Sources/Exercise/Exercise.swift",
                        SwiftParserSolution().Replace("return CommandRequest(", "return MissingType(", StringComparison.Ordinal))
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("FailedVisible", result.Status);
        Assert.Contains(result.StaticAnalysis, diagnostic =>
            diagnostic.RuleId == "SWIFT"
            && diagnostic.Severity == "error"
            && diagnostic.FilePath == "Sources/Exercise/Exercise.swift"
            && diagnostic.Line is not null
            && diagnostic.Column is not null);
    }

    [Theory]
    [InlineData("parse-output-format-swift")]
    [InlineData("resolve-input-source-swift")]
    [InlineData("count-levels-swift")]
    [InlineData("run-logprobe-command-swift")]
    [InlineData("handle-logprobe-request-swift")]
    [InlineData("handle-hardened-logprobe-request-swift")]
    public async Task RunEndpointExecutesSwiftLogprobeMilestoneExercises(string exerciseId)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-logprobe-{exerciseId}-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            $"/api/exercises/{exerciseId}/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "Sources/Exercise/Exercise.swift",
                        SwiftLogprobeCoreSolution())
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Passed", result.Status);
        Assert.True(result.VisiblePassed);
        Assert.True(result.HiddenPassed);
        Assert.Empty(result.StaticAnalysis);
    }

    [Fact]
    public async Task RunEndpointReturnsTypeScriptStaticAnalysisDiagnostics()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"typescript-analysis-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-ts/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/exercise.ts",
                        """
                        export type LogLevel = 'debug' | 'info' | 'warn' | 'error'

                        export type CommandRequest = {
                          readonly level: LogLevel
                          readonly limit: number
                          readonly includeArchived: boolean
                        }

                        export function parseCommandRequest(): CommandRequest {
                          return {
                            level: 'trace',
                            limit: 'many',
                            includeArchived: false,
                          }
                        }
                        """)
                ]),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseRunTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("FailedVisible", result.Status);
        Assert.Contains(result.StaticAnalysis, diagnostic =>
            diagnostic.RuleId.StartsWith("TS", StringComparison.Ordinal)
            && diagnostic.FilePath == "src/exercise.ts");
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
    public async Task RunEndpointExecutesLogqueryCapstoneVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"logquery-capstone-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/run-logquery-summary/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/Exercise/WordFrequencyAnalyzer.cs",
                        """
                        namespace Exercise;

                        public sealed record LogEvent(DateTimeOffset Timestamp, string Level, string Message);

                        public sealed record LogQueryOptions(string? Level, string? Contains);

                        public sealed record LogQueryResult(int ExitCode, string Output, string Error);

                        public static class LogQuery
                        {
                            public static bool TryParseLine(string line, out LogEvent? logEvent)
                            {
                                logEvent = null;
                                var parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length < 3 || !DateTimeOffset.TryParse(parts[0], out var timestamp))
                                {
                                    return false;
                                }

                                logEvent = new LogEvent(timestamp, parts[1].ToUpperInvariant(), parts[2]);
                                return true;
                            }

                            public static IReadOnlyList<LogEvent> ParseMany(IEnumerable<string> lines, out int malformedCount)
                            {
                                malformedCount = 0;
                                var events = new List<LogEvent>();

                                foreach (var line in lines)
                                {
                                    if (TryParseLine(line, out var logEvent) && logEvent is not null)
                                    {
                                        events.Add(logEvent);
                                    }
                                    else
                                    {
                                        malformedCount++;
                                    }
                                }

                                return events;
                            }

                            public static IEnumerable<LogEvent> Filter(IEnumerable<LogEvent> events, LogQueryOptions options)
                            {
                                var query = events;
                                if (!string.IsNullOrWhiteSpace(options.Level))
                                {
                                    var level = options.Level.ToUpperInvariant();
                                    query = query.Where(item => item.Level == level);
                                }

                                if (!string.IsNullOrWhiteSpace(options.Contains))
                                {
                                    query = query.Where(item => item.Message.Contains(options.Contains, StringComparison.OrdinalIgnoreCase));
                                }

                                return query;
                            }

                            public static IReadOnlyList<KeyValuePair<string, int>> CountByLevel(IEnumerable<LogEvent> events)
                            {
                                return events
                                    .GroupBy(item => item.Level)
                                    .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                                    .OrderBy(item => item.Key, StringComparer.Ordinal)
                                    .ToList();
                            }

                            public static LogQueryResult Run(IEnumerable<string> lines, LogQueryOptions options)
                            {
                                var events = ParseMany(lines, out var malformedCount);
                                if (malformedCount > 0)
                                {
                                    return new LogQueryResult(2, string.Empty, $"Input contains {malformedCount} malformed log line(s).");
                                }

                                var filtered = Filter(events, options);
                                var summary = CountByLevel(filtered);
                                var output = string.Join("\n", summary.Select(item => $"{item.Key}\t{item.Value}"));
                                return new LogQueryResult(0, output, string.Empty);
                            }
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
    }

    [Fact]
    public async Task RunEndpointExecutesWordfreqCliCapstoneVisibleAndHiddenTests()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"wordfreq-cli-capstone-test-{Guid.NewGuid():n}";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/run-wordfreq-cli-request/run",
            new ExerciseRunTestRequest(
                profileId,
                [
                    new(
                        "src/Exercise/WordFrequencyAnalyzer.cs",
                        """
                        namespace Exercise;

                        public enum CliInputMode
                        {
                            Stdin,
                            Text,
                            File,
                        }

                        public sealed record CliOptions(CliInputMode Mode, string? Value);

                        public sealed record CliResult(int ExitCode, string Output, string Error);

                        public sealed record WordFrequency(string Word, int Count);

                        public static class WordFrequencyCli
                        {
                            public static CliOptions ParseOptions(string[] args)
                            {
                                return args switch
                                {
                                    [] => new CliOptions(CliInputMode.Stdin, null),
                                    ["--text", var value] when !string.IsNullOrWhiteSpace(value) => new CliOptions(CliInputMode.Text, value),
                                    ["--file", var path] when !string.IsNullOrWhiteSpace(path) => new CliOptions(CliInputMode.File, path),
                                    _ => throw new ArgumentException("Usage: wordfreq [--text VALUE|--file PATH]"),
                                };
                            }

                            public static string ReadInputFile(string path)
                            {
                                return File.ReadAllText(path);
                            }

                            public static CliResult TryReadInputFile(string path)
                            {
                                if (!File.Exists(path))
                                {
                                    return new CliResult(2, string.Empty, $"Input file not found: {path}");
                                }

                                return new CliResult(0, File.ReadAllText(path), string.Empty);
                            }

                            public static string FormatTable(IEnumerable<WordFrequency> frequencies)
                            {
                                return string.Join(
                                    Environment.NewLine,
                                    frequencies.Select(item => $"{item.Word}\t{item.Count}"));
                            }

                            public static CliResult Run(string[] args, Func<string> readStdin, Func<string, string> readFile)
                            {
                                try
                                {
                                    var options = ParseOptions(args);
                                    var text = options.Mode switch
                                    {
                                        CliInputMode.Stdin => readStdin(),
                                        CliInputMode.Text => options.Value ?? string.Empty,
                                        CliInputMode.File => readFile(options.Value ?? string.Empty),
                                        _ => string.Empty,
                                    };

                                    var output = FormatTable(Analyze(text));
                                    return new CliResult(0, output, string.Empty);
                                }
                                catch (ArgumentException exception)
                                {
                                    return new CliResult(2, string.Empty, exception.Message);
                                }
                                catch (FileNotFoundException exception)
                                {
                                    return new CliResult(2, string.Empty, exception.Message);
                                }
                            }

                            public static IReadOnlyList<WordFrequency> Analyze(string? text)
                            {
                                var words = (text ?? string.Empty)
                                    .ToLowerInvariant()
                                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(word => new string(word.Where(char.IsLetterOrDigit).ToArray()))
                                    .Where(word => !string.IsNullOrWhiteSpace(word));

                                return words
                                    .GroupBy(word => word)
                                    .Select(group => new WordFrequency(group.Key, group.Count()))
                                    .OrderByDescending(item => item.Count)
                                    .ThenBy(item => item.Word, StringComparer.Ordinal)
                                    .ToList();
                            }
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
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "PTP0001" && diagnostic.Severity == "Error");
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
    public async Task CodeActionsEndpointReturnsRoslynLspProviderRefactoring()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"lsp-refactoring-test-{Guid.NewGuid():n}";
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
        var start = PositionOf(content, "text?.ToLowerInvariant()");
        var end = PositionOf(content, "text?.ToLowerInvariant()", "text?.ToLowerInvariant()".Length);

        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/code-actions",
            new ExerciseCodeActionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                start.Line,
                start.Column,
                end.Line,
                end.Column),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ExerciseCodeActionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.DoesNotContain(result.SetupMessages, message => message.Contains("Roslyn LSP code actions were unavailable", StringComparison.Ordinal));
        Assert.Contains(result.Actions, action =>
            action.Title.Equals("Extract method", StringComparison.Ordinal)
            || action.Title.Equals("Extract local function", StringComparison.Ordinal));
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

    [Fact]
    public async Task SwiftDiagnosticsEndpointReturnsSourceKitErrorsForCurrentContent()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-diagnostics-test-{Guid.NewGuid():n}";
        var starter = await GetSwiftExerciseContentAsync(client, profileId);
        var content = starter + "\npublic let =\n";

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-swift/language/diagnostics",
            new ExerciseLanguageTestRequest(
                profileId,
                "Sources/Exercise/Exercise.swift",
                content),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExerciseDiagnosticsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Severity == "Error");
    }

    [Fact]
    public async Task SwiftCompletionsEndpointReturnsSourceKitEnumCases()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-completion-test-{Guid.NewGuid():n}";
        var content = await GetSwiftExerciseContentAsync(client, profileId) + """

            public func chooseLogLevel() -> LogLevel {
                return LogLevel.
            }
            """;
        var position = PositionOf(content, "    return LogLevel.", "    return LogLevel.".Length);

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-swift/language/completions",
            new ExerciseCompletionTestRequest(
                profileId,
                "Sources/Exercise/Exercise.swift",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExerciseCompletionsTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Label.Contains("debug", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Items, item => item.Label.Contains("info", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SwiftHoverEndpointReturnsSourceKitSymbolInformation()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"swift-hover-test-{Guid.NewGuid():n}";
        var content = await GetSwiftExerciseContentAsync(client, profileId);
        var position = PositionOf(content, "LogLevel");

        var response = await client.PostAsJsonAsync(
            "/api/exercises/parse-command-request-swift/language/hover",
            new ExercisePositionTestRequest(
                profileId,
                "Sources/Exercise/Exercise.swift",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExerciseHoverTestResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Contents, content => content.Contains("enum LogLevel", StringComparison.Ordinal));
    }

    [Fact]
    public async Task LanguageServiceReusesCachedProjectSnapshotForRepeatedRequests()
    {
        ExerciseLanguageService.ResetProjectSnapshotCacheForTests();
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"snapshot-cache-test-{Guid.NewGuid():n}";
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
        var position = PositionOf(content, "text.", "text.".Length);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var result = await RequestCompletionsAsync(client, profileId, content, position);
            Assert.Contains(result.Items, item => item.Label == "ToLowerInvariant");
        }

        Assert.Equal(1, ExerciseLanguageService.CachedProjectSnapshotBuildCount);
    }

    [Fact]
    public async Task LanguageServiceCachesSiblingSourcesAndInvalidatesWhenTheyChange()
    {
        ExerciseLanguageService.ResetProjectSnapshotCacheForTests();
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var profileId = $"sibling-cache-test-{Guid.NewGuid():n}";
        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/normalize-to-lowercase/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);
        Assert.NotNull(workspace);

        var siblingPath = Path.Combine(workspace.WorkspacePath, "src", "Exercise", "ExtraAnalyzer.cs");
        await File.WriteAllTextAsync(
            siblingPath,
            """
            namespace Exercise;

            public static class ExtraAnalyzer
            {
                public static string NormalizeFromSibling(string? value) => value ?? string.Empty;
            }
            """,
            TestContext.Current.CancellationToken);

        var content = """
            namespace Exercise;

            public static class WordFrequencyAnalyzer
            {
                public static string NormalizeToLowercase(string? text)
                {
                    return ExtraAnalyzer.
                }
            }
            """;
        var position = PositionOf(content, "ExtraAnalyzer.", "ExtraAnalyzer.".Length);
        var first = await RequestCompletionsAsync(client, profileId, content, position);
        Assert.Contains(first.Items, item => item.Label == "NormalizeFromSibling");
        Assert.Equal(1, ExerciseLanguageService.CachedProjectSnapshotBuildCount);

        await File.WriteAllTextAsync(
            siblingPath,
            """
            namespace Exercise;

            public static class ExtraAnalyzer
            {
                public static string NormalizeFromSibling(string? value) => value ?? string.Empty;
                public static string NormalizeAfterInvalidation(string? value) => value?.Trim() ?? string.Empty;
            }
            """,
            TestContext.Current.CancellationToken);

        var second = await RequestCompletionsAsync(client, profileId, content, position);
        Assert.Contains(second.Items, item => item.Label == "NormalizeAfterInvalidation");
        Assert.Equal(2, ExerciseLanguageService.CachedProjectSnapshotBuildCount);
    }

    private sealed record ExerciseWorkspaceTestResponse(
        string ExerciseId,
        string Title,
        string Language,
        string Runtime,
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
        IReadOnlyCollection<ExerciseCodeActionItemTestResponse> Actions,
        IReadOnlyCollection<string> SetupMessages);

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

    private static async Task<string> GetSwiftExerciseContentAsync(HttpClient client, string profileId)
    {
        var workspace = await client.GetFromJsonAsync<ExerciseWorkspaceTestResponse>(
            $"/api/exercises/parse-command-request-swift/workspace?profileId={profileId}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(workspace);
        var editableFile = Assert.Single(workspace.Files, file => file.Path == "Sources/Exercise/Exercise.swift");
        Assert.NotNull(editableFile.Content);
        return editableFile.Content;
    }

    private static string SwiftParserSolution()
    {
        return """
        public enum LogLevel: String, Equatable {
            case debug
            case info
            case warn
            case error
        }

        public struct CommandRequest: Equatable {
            public let level: LogLevel
            public let limit: Int
            public let includeArchived: Bool

            public init(level: LogLevel, limit: Int, includeArchived: Bool) {
                self.level = level
                self.limit = limit
                self.includeArchived = includeArchived
            }
        }

        public enum CommandRequestError: Error, Equatable {
            case unknownOption(String)
            case missingValue(String)
            case invalidLevel(String)
            case invalidLimit(String)
        }

        public func parseCommandRequest(_ args: [String]) throws -> CommandRequest {
            var level = LogLevel.info
            var limit = 100
            var includeArchived = false
            var index = 0

            while index < args.count {
                let option = args[index]
                switch option {
                case "--archived":
                    includeArchived = true
                    index += 1
                case "--level":
                    guard index + 1 < args.count else { throw CommandRequestError.missingValue(option) }
                    let value = args[index + 1]
                    guard let parsed = LogLevel(rawValue: value) else { throw CommandRequestError.invalidLevel(value) }
                    level = parsed
                    index += 2
                case "--limit":
                    guard index + 1 < args.count else { throw CommandRequestError.missingValue(option) }
                    let value = args[index + 1]
                    guard let parsed = Int(value), parsed > 0 else { throw CommandRequestError.invalidLimit(value) }
                    limit = parsed
                    index += 2
                case "--unknown":
                    throw CommandRequestError.unknownOption(option)
                default:
                    throw CommandRequestError.unknownOption(option)
                }
            }

            return CommandRequest(level: level, limit: limit, includeArchived: includeArchived)
        }
        """;
    }

    private static string SwiftLogprobeCoreSolution()
    {
        return """
        public enum LogLevel: String, Equatable {
            case debug
            case info
            case warn
            case error
        }

        public enum OutputFormat: String, Equatable {
            case table
            case json
        }

        public enum CliParseError: Error, Equatable {
            case unsupportedFormat(String)
            case missingPath
        }

        public enum FileReadResult: Equatable {
            case success(String)
            case failure(String)
        }

        public enum InputSource: Equatable {
            case stdin
            case file(String)
        }

        public struct LevelCount: Equatable {
            public let level: String
            public let count: Int

            public init(level: String, count: Int) {
                self.level = level
                self.count = count
            }
        }

        public struct LogprobeCommandRequest: Equatable {
            public let source: InputSource
            public let format: OutputFormat
            public let limit: Int

            public init(source: InputSource, format: OutputFormat, limit: Int) {
                self.source = source
                self.format = format
                self.limit = limit
            }
        }

        public enum LogprobeCommandResult: Equatable {
            case rendered(String)
            case failed(String)
        }

        public struct HttpLogprobeRequest: Equatable {
            public let query: [String: String]
            public let body: String?

            public init(query: [String: String], body: String?) {
                self.query = query
                self.body = body
            }
        }

        public struct HttpLogprobeResponse: Equatable {
            public let status: Int
            public let contentType: String
            public let body: String

            public init(status: Int, contentType: String, body: String) {
                self.status = status
                self.contentType = contentType
                self.body = body
            }
        }

        public struct LogprobeTelemetry: Equatable {
            public let requestId: String
            public let status: Int
            public let durationMs: Int
            public let outcome: String

            public init(requestId: String, status: Int, durationMs: Int, outcome: String) {
                self.requestId = requestId
                self.status = status
                self.durationMs = durationMs
                self.outcome = outcome
            }
        }

        public struct HardenedLogprobeResponse: Equatable {
            public let response: HttpLogprobeResponse
            public let telemetry: LogprobeTelemetry

            public init(response: HttpLogprobeResponse, telemetry: LogprobeTelemetry) {
                self.response = response
                self.telemetry = telemetry
            }
        }

        public func parseOutputFormat(_ value: String?) throws -> OutputFormat {
            switch value {
            case nil:
                return .table
            case "table":
                return .table
            case "json":
                return .json
            case let unsupported?:
                throw CliParseError.unsupportedFormat(unsupported)
            }
        }

        public func resolveInputSource(
            _ source: InputSource,
            readStdin: () async -> String,
            readFile: (String) async throws -> String
        ) async -> FileReadResult {
            switch source {
            case .stdin:
                return .success(await readStdin())
            case .file(let path):
                do {
                    return .success(try await readFile(path))
                } catch {
                    return .failure("Could not read input file: \(path)")
                }
            }
        }

        public func countLevels<S: AsyncSequence>(
            from lines: S,
            limit: Int
        ) async throws -> [LevelCount] where S.Element == String {
            var counts: [String: Int] = [:]
            let supported = Set(["DEBUG", "INFO", "WARN", "ERROR"])

            for try await line in lines {
                for token in line.uppercased().split(whereSeparator: { !$0.isLetter }) {
                    let level = String(token)
                    if supported.contains(level) {
                        counts[level, default: 0] += 1
                        break
                    }
                }
            }

            return counts
                .map { LevelCount(level: $0.key, count: $0.value) }
                .sorted { left, right in
                    left.count == right.count ? left.level < right.level : left.count > right.count
                }
                .prefix(max(0, limit))
                .map { $0 }
        }

        public func renderLevelCounts(_ counts: [LevelCount], format: OutputFormat) -> String {
            switch format {
            case .table:
                return counts.map { "\($0.level) \($0.count)" }.joined(separator: "\n")
            case .json:
                let objects = counts.map { "{\"level\":\"\($0.level)\",\"count\":\($0.count)}" }
                return "[\(objects.joined(separator: ","))]"
            }
        }

        public func runLogprobeCommand(
            _ request: LogprobeCommandRequest,
            readStdin: () async -> String,
            readFile: (String) async throws -> String
        ) async throws -> LogprobeCommandResult {
            let input = await resolveInputSource(request.source, readStdin: readStdin, readFile: readFile)
            switch input {
            case .failure(let message):
                return .failed(message)
            case .success(let text):
                let lines = AsyncStream<String> { continuation in
                    for line in text.split(whereSeparator: \.isNewline) {
                        continuation.yield(String(line))
                    }
                    continuation.finish()
                }
                let counts = try await countLevels(from: lines, limit: request.limit)
                return .rendered(renderLevelCounts(counts, format: request.format))
            }
        }

        public func handleLogprobeRequest(_ request: HttpLogprobeRequest) async -> HttpLogprobeResponse {
            let format: OutputFormat
            do {
                format = try parseOutputFormat(request.query["format"])
            } catch CliParseError.unsupportedFormat(let value) {
                return HttpLogprobeResponse(status: 400, contentType: "text/plain", body: "unsupported format: \(value)")
            } catch {
                return HttpLogprobeResponse(status: 400, contentType: "text/plain", body: "invalid request")
            }

            let limitText = request.query["limit"] ?? "10"
            guard let limit = Int(limitText), limit > 0 else {
                return HttpLogprobeResponse(status: 400, contentType: "text/plain", body: "limit must be a positive integer")
            }

            do {
                let result = try await runLogprobeCommand(
                    LogprobeCommandRequest(source: .stdin, format: format, limit: limit),
                    readStdin: { request.body ?? "" },
                    readFile: { _ in throw CliParseError.missingPath }
                )

                switch result {
                case .rendered(let body):
                    return HttpLogprobeResponse(
                        status: 200,
                        contentType: format == .json ? "application/json" : "text/plain",
                        body: body
                    )
                case .failed(let message):
                    return HttpLogprobeResponse(status: 400, contentType: "text/plain", body: message)
                }
            } catch {
                return HttpLogprobeResponse(status: 500, contentType: "text/plain", body: "internal server error")
            }
        }

        public func handleHardenedLogprobeRequest(
            _ request: HttpLogprobeRequest,
            requestId: String,
            startedAtMs: Int,
            deadlineMs: Int,
            maxBodyBytes: Int,
            nowMs: () async -> Int
        ) async -> HardenedLogprobeResponse {
            let now = await nowMs()
            let duration = max(0, now - startedAtMs)

            func envelope(_ response: HttpLogprobeResponse, outcome: String) -> HardenedLogprobeResponse {
                HardenedLogprobeResponse(
                    response: response,
                    telemetry: LogprobeTelemetry(
                        requestId: requestId,
                        status: response.status,
                        durationMs: duration,
                        outcome: outcome
                    )
                )
            }

            if duration > deadlineMs {
                return envelope(
                    HttpLogprobeResponse(status: 504, contentType: "text/plain", body: "request timed out"),
                    outcome: "timed-out"
                )
            }

            if let body = request.body, body.utf8.count > maxBodyBytes {
                return envelope(
                    HttpLogprobeResponse(status: 413, contentType: "text/plain", body: "request body too large"),
                    outcome: "rejected"
                )
            }

            let response = await handleLogprobeRequest(request)
            let outcome: String
            switch response.status {
            case 200..<300:
                outcome = "ok"
            case 400..<500:
                outcome = "validation-failed"
            default:
                outcome = "failed"
            }

            return envelope(response, outcome: outcome)
        }
        """;
    }

    private static async Task<ExerciseCompletionsTestResponse> RequestCompletionsAsync(
        HttpClient client,
        string profileId,
        string content,
        (int Line, int Column) position)
    {
        var response = await client.PostAsJsonAsync(
            "/api/exercises/normalize-to-lowercase/language/completions",
            new ExerciseCompletionTestRequest(
                profileId,
                "src/Exercise/WordFrequencyAnalyzer.cs",
                content,
                position.Line,
                position.Column),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExerciseCompletionsTestResponse>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        return result;
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
