using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProdigeeTutsPoint.Api.Features.Exercises;

public sealed class CSharpLspBridge(IWebHostEnvironment environment) : IDisposable
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(12);
    private readonly ConcurrentDictionary<string, CSharpLspSession> sessions = new(StringComparer.Ordinal);
    private bool disposed;

    public async Task<IReadOnlyCollection<ExerciseCodeActionItemResponse>> GetCodeActionsAsync(
        string workspacePath,
        string relativePath,
        string content,
        int startLineNumber,
        int startColumn,
        int endLineNumber,
        int endColumn,
        List<string> setupMessages,
        CancellationToken cancellationToken)
    {
        try
        {
            var fullWorkspacePath = Path.GetFullPath(workspacePath);
            var session = sessions.GetOrAdd(fullWorkspacePath, path => new CSharpLspSession(path, GetRepositoryRoot()));
            return await session.GetCodeActionsAsync(
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
            setupMessages.Add($"Roslyn LSP code actions were unavailable: {exception.Message}");
            return [];
        }
    }

    private string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(environment.ContentRootPath);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "dotnet-tools.json")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not find dotnet-tools.json for the C# LSP tool manifest.");
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

    private sealed class CSharpLspSession : IDisposable
    {
        private readonly string workspacePath;
        private readonly string repositoryRoot;
        private readonly SemaphoreSlim gate = new(1, 1);
        private readonly SemaphoreSlim writeGate = new(1, 1);
        private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonNode?>> pending = new();
        private readonly HashSet<string> openedDocuments = new(StringComparer.Ordinal);
        private readonly Process process;
        private readonly StreamReader stdout;
        private readonly Task readerTask;
        private int requestId;
        private int documentVersion;
        private bool initialized;
        private bool disposed;

        public CSharpLspSession(string workspacePath, string repositoryRoot)
        {
            this.workspacePath = workspacePath;
            this.repositoryRoot = repositoryRoot;
            process = StartServer();
            stdout = new StreamReader(process.StandardOutput.BaseStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: false);
            readerTask = Task.Run(ReadLoopAsync);
        }

        public async Task<IReadOnlyCollection<ExerciseCodeActionItemResponse>> GetCodeActionsAsync(
            string relativePath,
            string content,
            int startLineNumber,
            int startColumn,
            int endLineNumber,
            int endColumn,
            CancellationToken cancellationToken)
        {
            ThrowIfExited();
            await gate.WaitAsync(cancellationToken);
            try
            {
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
                            ["languageId"] = "csharp",
                            ["version"] = version,
                            ["text"] = content,
                        },
                    }, cancellationToken);
                    await Task.Delay(700, cancellationToken);
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
                    await Task.Delay(100, cancellationToken);
                }

                var response = await SendRequestAsync("textDocument/codeAction", new JsonObject
                {
                    ["textDocument"] = new JsonObject { ["uri"] = documentUri },
                    ["range"] = new JsonObject
                    {
                        ["start"] = ToLspPosition(startLineNumber, startColumn),
                        ["end"] = ToLspPosition(endLineNumber, endColumn),
                    },
                    ["context"] = new JsonObject
                    {
                        ["diagnostics"] = new JsonArray(),
                    },
                }, cancellationToken);

                return ToCodeActions(response, documentUri);
            }
            finally
            {
                gate.Release();
            }
        }

        private Process StartServer()
        {
            var startInfo = new ProcessStartInfo("dotnet", "tool run csharp-ls")
            {
                WorkingDirectory = repositoryRoot,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var started = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start csharp-ls.");
            _ = Task.Run(async () =>
            {
                while (await started.StandardError.ReadLineAsync() is not null)
                {
                }
            });
            return started;
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
                        ["codeAction"] = new JsonObject
                        {
                            ["dynamicRegistration"] = false,
                            ["codeActionLiteralSupport"] = new JsonObject
                            {
                                ["codeActionKind"] = new JsonObject
                                {
                                    ["valueSet"] = new JsonArray("quickfix", "refactor", "refactor.extract", "refactor.inline", "refactor.rewrite", "source"),
                                },
                            },
                        },
                    },
                },
                ["workspaceFolders"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["uri"] = rootUri,
                        ["name"] = Path.GetFileName(workspacePath),
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
                throw new InvalidOperationException("Could not register LSP request.");
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
                        var header = await stdout.ReadLineAsync();
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

                    var buffer = new char[contentLength];
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

                    DispatchMessage(JsonNode.Parse(new string(buffer)));
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

        private void DispatchMessage(JsonNode? message)
        {
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
                throw new InvalidOperationException($"csharp-ls exited with code {process.ExitCode}.");
            }
        }

        private static JsonObject ToLspPosition(int lineNumber, int column)
        {
            return new JsonObject
            {
                ["line"] = Math.Max(0, lineNumber - 1),
                ["character"] = Math.Max(0, column - 1),
            };
        }

        private static IReadOnlyCollection<ExerciseCodeActionItemResponse> ToCodeActions(JsonNode? response, string documentUri)
        {
            if (response is not JsonArray actions)
            {
                return [];
            }

            return actions
                .OfType<JsonObject>()
                .Select(action => ToCodeAction(action, documentUri))
                .Where(action => action is not null)
                .Cast<ExerciseCodeActionItemResponse>()
                .ToArray();
        }

        private static ExerciseCodeActionItemResponse? ToCodeAction(JsonObject action, string documentUri)
        {
            var title = action["title"]?.GetValue<string>();
            var edits = ExtractEdits(action["edit"], documentUri);
            if (string.IsNullOrWhiteSpace(title) || edits.Count == 0)
            {
                return null;
            }

            return new ExerciseCodeActionItemResponse(
                title,
                action["kind"]?.GetValue<string>() ?? InferKind(title),
                edits);
        }

        private static IReadOnlyCollection<ExerciseTextEditResponse> ExtractEdits(JsonNode? edit, string documentUri)
        {
            if (edit is not JsonObject editObject)
            {
                return [];
            }

            var edits = new List<ExerciseTextEditResponse>();
            if (editObject["documentChanges"] is JsonArray documentChanges)
            {
                foreach (var change in documentChanges.OfType<JsonObject>())
                {
                    if (change["textDocument"]?["uri"]?.GetValue<string>() != documentUri)
                    {
                        continue;
                    }

                    AddTextEdits(edits, change["edits"]);
                }
            }

            if (editObject["changes"] is JsonObject changes && changes[documentUri] is { } uriEdits)
            {
                AddTextEdits(edits, uriEdits);
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

        private static string InferKind(string title)
        {
            return title.Contains("Extract", StringComparison.OrdinalIgnoreCase)
                || title.Contains("Convert", StringComparison.OrdinalIgnoreCase)
                || title.Contains("Inline", StringComparison.OrdinalIgnoreCase)
                ? "refactor"
                : "quickfix";
        }

        private static string ToFileUri(string path)
        {
            return new Uri(Path.GetFullPath(path)).AbsoluteUri;
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
}
