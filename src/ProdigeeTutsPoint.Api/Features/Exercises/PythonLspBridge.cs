using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProdigeeTutsPoint.Api.Features.Exercises;

public sealed class PythonLspBridge(IWebHostEnvironment environment) : IDisposable
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(14);
    private readonly ConcurrentDictionary<string, PythonLspSession> sessions = new(StringComparer.Ordinal);
    private bool disposed;

    public async Task<ExerciseDiagnosticsResponse> GetDiagnosticsAsync(
        string workspacePath,
        string relativePath,
        string content,
        CancellationToken cancellationToken)
    {
        try
        {
            return await GetSession(workspacePath).GetDiagnosticsAsync(relativePath, content, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ExerciseDiagnosticsResponse([], [SetupMessage(exception)]);
        }
    }

    public async Task<ExerciseCompletionsResponse> GetCompletionsAsync(
        string workspacePath,
        string relativePath,
        string content,
        int lineNumber,
        int column,
        CancellationToken cancellationToken)
    {
        try
        {
            return await GetSession(workspacePath).GetCompletionsAsync(relativePath, content, lineNumber, column, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ExerciseCompletionsResponse([], [SetupMessage(exception)]);
        }
    }

    public async Task<ExerciseHoverResponse> GetHoverAsync(
        string workspacePath,
        string relativePath,
        string content,
        int lineNumber,
        int column,
        CancellationToken cancellationToken)
    {
        try
        {
            return await GetSession(workspacePath).GetHoverAsync(relativePath, content, lineNumber, column, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ExerciseHoverResponse([], null, [SetupMessage(exception)]);
        }
    }

    public async Task<ExerciseSignatureHelpResponse> GetSignatureHelpAsync(
        string workspacePath,
        string relativePath,
        string content,
        int lineNumber,
        int column,
        CancellationToken cancellationToken)
    {
        try
        {
            return await GetSession(workspacePath).GetSignatureHelpAsync(relativePath, content, lineNumber, column, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ExerciseSignatureHelpResponse([], 0, 0, [SetupMessage(exception)]);
        }
    }

    public async Task<ExerciseCodeActionsResponse> GetCodeActionsAsync(
        string workspacePath,
        string relativePath,
        string content,
        int startLineNumber,
        int startColumn,
        int endLineNumber,
        int endColumn,
        CancellationToken cancellationToken)
    {
        try
        {
            return await GetSession(workspacePath).GetCodeActionsAsync(
                relativePath,
                content,
                startLineNumber,
                startColumn,
                endLineNumber,
                endColumn,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ExerciseCodeActionsResponse([], [SetupMessage(exception)]);
        }
    }

    public async Task<ExerciseFormatResponse> FormatAsync(
        string workspacePath,
        string relativePath,
        string content,
        CancellationToken cancellationToken)
    {
        try
        {
            return await PythonFormatter.FormatAsync(workspacePath, GetRepositoryRoot(), relativePath, content, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ExerciseFormatResponse(content, [SetupMessage(exception)]);
        }
    }

    private PythonLspSession GetSession(string workspacePath)
    {
        var fullWorkspacePath = Path.GetFullPath(workspacePath);
        return sessions.GetOrAdd(fullWorkspacePath, path => new PythonLspSession(path, GetRepositoryRoot()));
    }

    private static string SetupMessage(Exception exception)
    {
        return $"Python language service unavailable: {exception.Message}. Install uv, run `uv sync` from the repository root, and retry to enable full Python/FastAPI IntelliSense.";
    }

    private string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", ".."));
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        foreach (var session in sessions.Values)
        {
            session.Dispose();
        }
    }

    private sealed class PythonLspSession : IDisposable
    {
        private readonly string workspacePath;
        private readonly string repositoryRoot;
        private readonly string languageServerCommand;
        private readonly SemaphoreSlim gate = new(1, 1);
        private readonly SemaphoreSlim writeGate = new(1, 1);
        private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonNode?>> pending = new();
        private readonly ConcurrentDictionary<string, IReadOnlyCollection<ExerciseDiagnosticResponse>> latestDiagnostics = new(StringComparer.Ordinal);
        private readonly HashSet<string> openedDocuments = new(StringComparer.Ordinal);
        private readonly Process process;
        private readonly Stream stdout;
        private readonly Task readerTask;
        private int requestId;
        private int documentVersion;
        private bool initialized;
        private bool disposed;

        public PythonLspSession(string workspacePath, string repositoryRoot)
        {
            this.workspacePath = workspacePath;
            this.repositoryRoot = repositoryRoot;
            languageServerCommand = ResolveLanguageServerCommand(repositoryRoot);
            process = StartServer();
            stdout = process.StandardOutput.BaseStream;
            readerTask = Task.Run(ReadLoopAsync);
        }

        public async Task<ExerciseDiagnosticsResponse> GetDiagnosticsAsync(
            string relativePath,
            string content,
            CancellationToken cancellationToken)
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var documentUri = await SyncDocumentAsync(relativePath, content, settleDelayMs: 450, cancellationToken);
                return new ExerciseDiagnosticsResponse(latestDiagnostics.GetValueOrDefault(documentUri, []), []);
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<ExerciseCompletionsResponse> GetCompletionsAsync(
            string relativePath,
            string content,
            int lineNumber,
            int column,
            CancellationToken cancellationToken)
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var documentUri = await SyncDocumentAsync(relativePath, content, settleDelayMs: 100, cancellationToken);
                var response = await SendRequestAsync("textDocument/completion", new JsonObject
                {
                    ["textDocument"] = new JsonObject { ["uri"] = documentUri },
                    ["position"] = ToLspPosition(lineNumber, column),
                    ["context"] = new JsonObject { ["triggerKind"] = 1 },
                }, cancellationToken);

                return new ExerciseCompletionsResponse(ToCompletions(response), []);
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<ExerciseHoverResponse> GetHoverAsync(
            string relativePath,
            string content,
            int lineNumber,
            int column,
            CancellationToken cancellationToken)
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var documentUri = await SyncDocumentAsync(relativePath, content, settleDelayMs: 100, cancellationToken);
                var response = await SendRequestAsync("textDocument/hover", new JsonObject
                {
                    ["textDocument"] = new JsonObject { ["uri"] = documentUri },
                    ["position"] = ToLspPosition(lineNumber, column),
                }, cancellationToken);

                return ToHover(response);
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<ExerciseSignatureHelpResponse> GetSignatureHelpAsync(
            string relativePath,
            string content,
            int lineNumber,
            int column,
            CancellationToken cancellationToken)
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var documentUri = await SyncDocumentAsync(relativePath, content, settleDelayMs: 100, cancellationToken);
                var response = await SendRequestAsync("textDocument/signatureHelp", new JsonObject
                {
                    ["textDocument"] = new JsonObject { ["uri"] = documentUri },
                    ["position"] = ToLspPosition(lineNumber, column),
                    ["context"] = new JsonObject { ["triggerKind"] = 1 },
                }, cancellationToken);

                return ToSignatureHelp(response);
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<ExerciseCodeActionsResponse> GetCodeActionsAsync(
            string relativePath,
            string content,
            int startLineNumber,
            int startColumn,
            int endLineNumber,
            int endColumn,
            CancellationToken cancellationToken)
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var documentUri = await SyncDocumentAsync(relativePath, content, settleDelayMs: 300, cancellationToken);
                var response = await SendRequestAsync("textDocument/codeAction", new JsonObject
                {
                    ["textDocument"] = new JsonObject { ["uri"] = documentUri },
                    ["range"] = new JsonObject
                    {
                        ["start"] = ToLspPosition(startLineNumber, startColumn),
                        ["end"] = ToLspPosition(endLineNumber, endColumn),
                    },
                    ["context"] = new JsonObject { ["diagnostics"] = new JsonArray() },
                }, cancellationToken);

                return new ExerciseCodeActionsResponse(ToCodeActions(response, documentUri), []);
            }
            finally
            {
                gate.Release();
            }
        }

        private async Task<string> SyncDocumentAsync(
            string relativePath,
            string content,
            int settleDelayMs,
            CancellationToken cancellationToken)
        {
            ThrowIfExited();
            await EnsureInitializedAsync(cancellationToken);
            var documentPath = Path.GetFullPath(Path.Combine(workspacePath, relativePath));
            var documentUri = ToFileUri(documentPath);
            var version = Interlocked.Increment(ref documentVersion);

            if (openedDocuments.Add(documentUri))
            {
                await NotifyAsync("textDocument/didOpen", new JsonObject
                {
                    ["textDocument"] = new JsonObject
                    {
                        ["uri"] = documentUri,
                        ["languageId"] = "python",
                        ["version"] = version,
                        ["text"] = content,
                    },
                }, cancellationToken);
            }
            else
            {
                await NotifyAsync("textDocument/didChange", new JsonObject
                {
                    ["textDocument"] = new JsonObject
                    {
                        ["uri"] = documentUri,
                        ["version"] = version,
                    },
                    ["contentChanges"] = new JsonArray
                    {
                        new JsonObject { ["text"] = content },
                    },
                }, cancellationToken);
            }

            if (settleDelayMs > 0)
            {
                await Task.Delay(settleDelayMs, cancellationToken);
            }

            return documentUri;
        }

        private Process StartServer()
        {
            var startInfo = new ProcessStartInfo("uv")
            {
                WorkingDirectory = workspacePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("run");
            startInfo.ArgumentList.Add("--project");
            startInfo.ArgumentList.Add(repositoryRoot);
            startInfo.ArgumentList.Add(languageServerCommand);
            startInfo.ArgumentList.Add("--stdio");
            startInfo.Environment["UV_CACHE_DIR"] = Path.Combine(repositoryRoot, ".uv-cache");
            var started = Process.Start(startInfo)
                ?? throw new InvalidOperationException($"Could not start {languageServerCommand}.");
            _ = Task.Run(async () =>
            {
                while (await started.StandardError.ReadLineAsync() is not null)
                {
                }
            });
            return started;
        }

        private static string ResolveLanguageServerCommand(string repositoryRoot)
        {
            if (!File.Exists(Path.Combine(repositoryRoot, "pyproject.toml")))
            {
                throw new InvalidOperationException("Repository Python tool project is missing pyproject.toml");
            }

            if (!CommandExists("uv"))
            {
                throw new InvalidOperationException("uv is required for Python language services");
            }

            return "basedpyright-langserver";
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (initialized)
            {
                return;
            }

            var rootUri = ToFileUri(workspacePath);
            await SendRequestAsync("initialize", new JsonObject
            {
                ["processId"] = Environment.ProcessId,
                ["rootUri"] = rootUri,
                ["capabilities"] = new JsonObject
                {
                    ["textDocument"] = new JsonObject
                    {
                        ["completion"] = new JsonObject
                        {
                            ["completionItem"] = new JsonObject { ["snippetSupport"] = false },
                        },
                        ["hover"] = new JsonObject(),
                        ["signatureHelp"] = new JsonObject(),
                        ["publishDiagnostics"] = new JsonObject(),
                        ["codeAction"] = new JsonObject(),
                    },
                    ["workspace"] = new JsonObject(),
                },
                ["workspaceFolders"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["uri"] = rootUri,
                        ["name"] = Path.GetFileName(workspacePath),
                    },
                },
                ["initializationOptions"] = new JsonObject
                {
                    ["settings"] = new JsonObject
                    {
                        ["python"] = new JsonObject
                        {
                            ["analysis"] = new JsonObject
                            {
                                ["typeCheckingMode"] = "standard",
                                ["autoImportCompletions"] = true,
                                ["diagnosticMode"] = "workspace",
                            },
                        },
                    },
                },
            }, cancellationToken);
            await NotifyAsync("initialized", new JsonObject(), cancellationToken);
            initialized = true;
        }

        private async Task<JsonNode?> SendRequestAsync(string method, JsonObject parameters, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(RequestTimeout);
            var id = Interlocked.Increment(ref requestId);
            var completion = new TaskCompletionSource<JsonNode?>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!pending.TryAdd(id, completion))
            {
                throw new InvalidOperationException("Could not register Python LSP request.");
            }

            await WriteMessageAsync(new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id,
                ["method"] = method,
                ["params"] = parameters,
            }, timeout.Token);

            using var timeoutRegistration = timeout.Token.Register(() => completion.TrySetCanceled(timeout.Token));
            try
            {
                return await completion.Task.WaitAsync(timeout.Token);
            }
            finally
            {
                pending.TryRemove(id, out _);
            }
        }

        private Task NotifyAsync(string method, JsonObject parameters, CancellationToken cancellationToken)
        {
            return WriteMessageAsync(new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["method"] = method,
                ["params"] = parameters,
            }, cancellationToken);
        }

        private async Task WriteMessageAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var body = message.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            var bytes = Encoding.UTF8.GetBytes(body);
            await writeGate.WaitAsync(cancellationToken);
            try
            {
                var header = Encoding.ASCII.GetBytes($"Content-Length: {bytes.Length}\r\n\r\n");
                await process.StandardInput.BaseStream.WriteAsync(header, cancellationToken);
                await process.StandardInput.BaseStream.WriteAsync(bytes, cancellationToken);
                await process.StandardInput.BaseStream.FlushAsync(cancellationToken);
            }
            finally
            {
                writeGate.Release();
            }
        }

        private async Task ReadLoopAsync()
        {
            try
            {
                while (!disposed)
                {
                    var contentLength = 0;
                    while (true)
                    {
                        var header = await ReadAsciiLineAsync();
                        if (header is null)
                        {
                            return;
                        }

                        if (header.Length == 0)
                        {
                            break;
                        }

                        if (header.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                        {
                            contentLength = int.Parse(header["Content-Length:".Length..].Trim());
                        }
                    }

                    if (contentLength <= 0)
                    {
                        continue;
                    }

                    var buffer = new byte[contentLength];
                    var read = 0;
                    while (read < contentLength)
                    {
                        var current = await stdout.ReadAsync(buffer.AsMemory(read, contentLength - read));
                        if (current == 0)
                        {
                            return;
                        }

                        read += current;
                    }

                    DispatchMessage(JsonNode.Parse(Encoding.UTF8.GetString(buffer)));
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                foreach (var request in pending.Values)
                {
                    request.TrySetException(exception);
                }
            }
        }

        private async Task<string?> ReadAsciiLineAsync()
        {
            var bytes = new List<byte>(64);
            var buffer = new byte[1];
            while (true)
            {
                var read = await stdout.ReadAsync(buffer.AsMemory(0, 1));
                if (read == 0)
                {
                    return bytes.Count == 0 ? null : Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r');
                }

                if (buffer[0] == (byte)'\n')
                {
                    return Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r');
                }

                bytes.Add(buffer[0]);
            }
        }

        private void DispatchMessage(JsonNode? message)
        {
            if (message?["method"]?.GetValue<string>() == "textDocument/publishDiagnostics")
            {
                var uri = message["params"]?["uri"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    latestDiagnostics[uri] = ToDiagnostics(message["params"]?["diagnostics"]);
                }

                return;
            }

            if (message?["id"]?.GetValue<int?>() is not { } id)
            {
                return;
            }

            if (!pending.TryRemove(id, out var completion))
            {
                return;
            }

            if (message["error"] is { } error)
            {
                completion.TrySetException(new InvalidOperationException(error.ToJsonString()));
                return;
            }

            completion.TrySetResult(message["result"]);
        }

        private void ThrowIfExited()
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"{languageServerCommand} exited with code {process.ExitCode}.");
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
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
            catch (IOException)
            {
            }
        }
    }

    private static class PythonFormatter
    {
        public static async Task<ExerciseFormatResponse> FormatAsync(
            string workspacePath,
            string repositoryRoot,
            string relativePath,
            string content,
            CancellationToken cancellationToken)
        {
            var repositoryPythonProject = Path.Combine(repositoryRoot, "pyproject.toml");
            if (!File.Exists(repositoryPythonProject))
            {
                return new ExerciseFormatResponse(content, ["Python formatter unavailable: repository pyproject.toml is missing."]);
            }

            if (!CommandExists("uv"))
            {
                return new ExerciseFormatResponse(content, ["Python formatter unavailable: uv is not installed on PATH."]);
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(8));
            var startInfo = new ProcessStartInfo("uv")
            {
                WorkingDirectory = workspacePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("run");
            startInfo.ArgumentList.Add("--project");
            startInfo.ArgumentList.Add(repositoryRoot);
            startInfo.ArgumentList.Add("ruff");
            startInfo.ArgumentList.Add("format");
            startInfo.ArgumentList.Add("--stdin-filename");
            startInfo.ArgumentList.Add(Path.Combine(workspacePath, relativePath));
            startInfo.ArgumentList.Add("-");
            startInfo.Environment["UV_CACHE_DIR"] = Path.Combine(repositoryRoot, ".uv-cache");

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start Ruff.");
            await process.StandardInput.WriteAsync(content);
            process.StandardInput.Close();
            var outputTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var errorTask = process.StandardError.ReadToEndAsync(timeout.Token);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
                var output = await outputTask;
                var error = await errorTask;
                return process.ExitCode == 0
                    ? new ExerciseFormatResponse(output, [])
                    : new ExerciseFormatResponse(content, [error]);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                TryKill(process);
                return new ExerciseFormatResponse(content, ["Python formatting timed out."]);
            }
        }
    }

    private static IReadOnlyCollection<ExerciseDiagnosticResponse> ToDiagnostics(JsonNode? diagnostics)
    {
        if (diagnostics is not JsonArray array)
        {
            return [];
        }

        return array
            .OfType<JsonObject>()
            .Select(diagnostic =>
            {
                var range = diagnostic["range"]!;
                var start = range["start"]!;
                var end = range["end"]!;
                return new ExerciseDiagnosticResponse(
                    diagnostic["code"]?.ToString() ?? "python",
                    diagnostic["message"]?.GetValue<string>() ?? "Python diagnostic",
                    SeverityName(diagnostic["severity"]?.GetValue<int>() ?? 3),
                    start["line"]!.GetValue<int>() + 1,
                    start["character"]!.GetValue<int>() + 1,
                    end["line"]!.GetValue<int>() + 1,
                    Math.Max(end["character"]!.GetValue<int>() + 1, start["character"]!.GetValue<int>() + 2));
            })
            .ToArray();
    }

    private static IReadOnlyCollection<ExerciseCompletionItemResponse> ToCompletions(JsonNode? response)
    {
        var items = response switch
        {
            JsonObject obj when obj["items"] is JsonArray array => array,
            JsonArray array => array,
            _ => null,
        };
        if (items is null)
        {
            return [];
        }

        return items
            .OfType<JsonObject>()
            .Take(250)
            .Select(item => new ExerciseCompletionItemResponse(
                item["label"]?.GetValue<string>() ?? string.Empty,
                item["insertText"]?.GetValue<string>()
                    ?? item["textEdit"]?["newText"]?.GetValue<string>()
                    ?? item["label"]?.GetValue<string>()
                    ?? string.Empty,
                item["filterText"]?.GetValue<string>() ?? item["label"]?.GetValue<string>() ?? string.Empty,
                item["sortText"]?.GetValue<string>() ?? item["label"]?.GetValue<string>() ?? string.Empty,
                DetailForCompletion(item),
                TagsForCompletion(item["kind"]?.GetValue<int>())))
            .Where(item => !string.IsNullOrWhiteSpace(item.Label))
            .DistinctBy(item => item.Label)
            .ToArray();
    }

    private static ExerciseHoverResponse ToHover(JsonNode? response)
    {
        if (response is not JsonObject obj)
        {
            return new ExerciseHoverResponse([], null, []);
        }

        var contents = obj["contents"] switch
        {
            JsonObject contentObject when contentObject["value"] is { } value => [value.GetValue<string>()],
            JsonArray values => values.Where(value => value is not null).Select(value => value!.ToString()).ToArray(),
            JsonValue value => [value.GetValue<string>()],
            _ => Array.Empty<string>(),
        };

        return new ExerciseHoverResponse(contents, ToRange(obj["range"]), []);
    }

    private static ExerciseSignatureHelpResponse ToSignatureHelp(JsonNode? response)
    {
        if (response is not JsonObject obj || obj["signatures"] is not JsonArray signatures)
        {
            return new ExerciseSignatureHelpResponse([], 0, 0, []);
        }

        return new ExerciseSignatureHelpResponse(
            signatures
                .OfType<JsonObject>()
                .Select(signature => new ExerciseSignatureItemResponse(
                    signature["label"]?.GetValue<string>() ?? string.Empty,
                    signature["documentation"]?.ToString() ?? string.Empty,
                    signature["parameters"] is JsonArray parameters
                        ? parameters
                            .OfType<JsonObject>()
                            .Select(parameter => new ExerciseSignatureParameterResponse(
                                parameter["label"]?.ToString() ?? string.Empty,
                                parameter["label"]?.ToString() ?? string.Empty,
                                parameter["documentation"]?.ToString() ?? string.Empty))
                            .ToArray()
                        : []))
                .ToArray(),
            obj["activeSignature"]?.GetValue<int>() ?? 0,
            obj["activeParameter"]?.GetValue<int>() ?? 0,
            []);
    }

    private static IReadOnlyCollection<ExerciseCodeActionItemResponse> ToCodeActions(JsonNode? response, string documentUri)
    {
        if (response is not JsonArray actions)
        {
            return [];
        }

        return actions
            .OfType<JsonObject>()
            .Select(action =>
            {
                var title = action["title"]?.GetValue<string>();
                var edits = ExtractWorkspaceEdits(action["edit"], documentUri);
                return string.IsNullOrWhiteSpace(title) || edits.Count == 0
                    ? null
                    : new ExerciseCodeActionItemResponse(title, action["kind"]?.GetValue<string>() ?? "quickfix", edits);
            })
            .Where(action => action is not null)
            .Cast<ExerciseCodeActionItemResponse>()
            .ToArray();
    }

    private static IReadOnlyCollection<ExerciseTextEditResponse> ExtractWorkspaceEdits(JsonNode? edit, string documentUri)
    {
        if (edit is not JsonObject editObject)
        {
            return [];
        }

        var edits = new List<ExerciseTextEditResponse>();
        if (editObject["changes"] is JsonObject changes && changes[documentUri] is { } uriEdits)
        {
            AddTextEdits(edits, uriEdits);
        }

        if (editObject["documentChanges"] is JsonArray documentChanges)
        {
            foreach (var change in documentChanges.OfType<JsonObject>())
            {
                if (change["textDocument"]?["uri"]?.GetValue<string>() == documentUri)
                {
                    AddTextEdits(edits, change["edits"]);
                }
            }
        }

        return edits;
    }

    private static void AddTextEdits(List<ExerciseTextEditResponse> target, JsonNode? edits)
    {
        if (edits is not JsonArray array)
        {
            return;
        }

        foreach (var edit in array.OfType<JsonObject>())
        {
            var range = edit["range"];
            var start = range?["start"];
            var end = range?["end"];
            if (start is null || end is null)
            {
                continue;
            }

            target.Add(new ExerciseTextEditResponse(
                start["line"]!.GetValue<int>() + 1,
                start["character"]!.GetValue<int>() + 1,
                end["line"]!.GetValue<int>() + 1,
                end["character"]!.GetValue<int>() + 1,
                edit["newText"]?.GetValue<string>() ?? string.Empty));
        }
    }

    private static ExerciseRangeResponse? ToRange(JsonNode? range)
    {
        var start = range?["start"];
        var end = range?["end"];
        if (start is null || end is null)
        {
            return null;
        }

        return new ExerciseRangeResponse(
            start["line"]!.GetValue<int>() + 1,
            start["character"]!.GetValue<int>() + 1,
            end["line"]!.GetValue<int>() + 1,
            Math.Max(end["character"]!.GetValue<int>() + 1, start["character"]!.GetValue<int>() + 2));
    }

    private static JsonObject ToLspPosition(int lineNumber, int column)
    {
        return new JsonObject
        {
            ["line"] = Math.Max(0, lineNumber - 1),
            ["character"] = Math.Max(0, column - 1),
        };
    }

    private static string DetailForCompletion(JsonObject item)
    {
        var detail = item["detail"]?.GetValue<string>() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(detail))
        {
            return detail;
        }

        return item["documentation"]?["value"]?.GetValue<string>() ?? string.Empty;
    }

    private static IReadOnlyList<string> TagsForCompletion(int? kind)
    {
        return kind switch
        {
            2 => ["Method"],
            3 => ["Function"],
            5 => ["Field"],
            6 => ["Variable"],
            7 => ["Class"],
            8 => ["Interface"],
            12 => ["Property"],
            13 => ["Variable"],
            14 => ["Keyword"],
            21 => ["Constant"],
            22 => ["Struct"],
            _ => [],
        };
    }

    private static string SeverityName(int severity)
    {
        return severity switch
        {
            1 => "Error",
            2 => "Warning",
            3 => "Info",
            _ => "Hidden",
        };
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

    private static string ToFileUri(string path)
    {
        return new Uri(Path.GetFullPath(path)).AbsoluteUri;
    }
}
