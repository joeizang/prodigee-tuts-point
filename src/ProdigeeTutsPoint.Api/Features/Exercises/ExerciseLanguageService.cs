using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace ProdigeeTutsPoint.Api.Features.Exercises;

public sealed partial class ExerciseLanguageService(
    ExerciseWorkspaceService workspaces,
    CSharpLspBridge lspBridge,
    SwiftLspBridge swiftLspBridge,
    PythonLspBridge pythonLspBridge)
{
    private const int CompletionLimit = 250;
    private const int CodeActionLimit = 20;
    private static readonly Lazy<MefHostServices> SharedRoslynHostServices = new(CreateRoslynHostServices);
    private static readonly Lazy<IReadOnlyCollection<MetadataReference>> TrustedPlatformReferences = new(CreateTrustedPlatformReferences);
    private static readonly ImmutableArray<DiagnosticAnalyzer> ExerciseAnalyzers = [new UnusedLocalVariableAnalyzer()];
    private static readonly ConcurrentDictionary<string, CachedRoslynProject> ProjectSnapshots = new(StringComparer.Ordinal);
    private static readonly SemaphoreSlim ProjectSnapshotLock = new(1, 1);
    private static int ProjectSnapshotBuildCount;

    internal static int CachedProjectSnapshotBuildCount => Volatile.Read(ref ProjectSnapshotBuildCount);

    internal static void ResetProjectSnapshotCacheForTests()
    {
        foreach (var snapshot in ProjectSnapshots.Values)
        {
            snapshot.Workspace.Dispose();
        }

        ProjectSnapshots.Clear();
        Interlocked.Exchange(ref ProjectSnapshotBuildCount, 0);
    }

    public async Task<ExerciseDiagnosticsResponse?> GetDiagnosticsAsync(
        string exerciseId,
        ExerciseLanguageRequest request,
        CancellationToken cancellationToken)
    {
        if (IsSwiftFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await swiftLspBridge.GetDiagnosticsAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                cancellationToken);
        }

        if (IsPythonFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await pythonLspBridge.GetDiagnosticsAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                cancellationToken);
        }

        using var context = await CreateContextAsync(exerciseId, request.ProfileId, request.Path, request.Content, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var syntaxTree = await context.Document.GetSyntaxTreeAsync(cancellationToken)
            ?? throw new InvalidOperationException("Could not load the C# syntax tree.");
        var compilation = await context.Document.Project.GetCompilationAsync(cancellationToken)
            ?? throw new InvalidOperationException("Could not compile the C# exercise project.");

        var diagnostics = compilation
            .GetDiagnostics(cancellationToken)
            .Concat(await GetAnalyzerDiagnosticsAsync(context.Document, compilation, syntaxTree, context.SetupMessages, cancellationToken))
            .Where(diagnostic => diagnostic.Location.SourceTree == syntaxTree)
            .Select(ToResponse)
            .Where(diagnostic => diagnostic is not null)
            .Cast<ExerciseDiagnosticResponse>()
            .DistinctBy(diagnostic => (diagnostic.Id, diagnostic.StartLineNumber, diagnostic.StartColumn, diagnostic.Message))
            .OrderBy(diagnostic => diagnostic.StartLineNumber)
            .ThenBy(diagnostic => diagnostic.StartColumn)
            .ToArray();

        return new ExerciseDiagnosticsResponse(diagnostics, context.SetupMessages);
    }

    public static async Task WarmUpAsync(CancellationToken cancellationToken)
    {
        using var workspace = new AdhocWorkspace(SharedRoslynHostServices.Value);
        var projectId = ProjectId.CreateNewId("Warmup");
        var documentId = DocumentId.CreateNewId(projectId, "Warmup.cs");
        const string warmupSource = """
        namespace Exercise;

        public static class Warmup
        {
            public static void Run()
            {
                var text = "";
                text.
            }
        }
        """;
        var sourceText = SourceText.From(warmupSource);
        var completionPosition = warmupSource.IndexOf("text.", StringComparison.Ordinal) + "text.".Length;
        var solution = workspace.CurrentSolution
            .AddProject(ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "Warmup",
                "Warmup",
                LanguageNames.CSharp,
                parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithNullableContextOptions(NullableContextOptions.Enable)
                    .WithOptimizationLevel(OptimizationLevel.Debug)))
            .AddMetadataReferences(projectId, TrustedPlatformReferences.Value)
            .AddDocument(documentId, "Warmup.cs", sourceText);

        if (!workspace.TryApplyChanges(solution))
        {
            return;
        }

        var document = workspace.CurrentSolution.GetDocument(documentId);
        if (document is null)
        {
            return;
        }

        var completionService = CompletionService.GetService(document);
        if (completionService is not null)
        {
            _ = await completionService.GetCompletionsAsync(document, completionPosition, cancellationToken: cancellationToken);
        }

        var compilation = await document.Project.GetCompilationAsync(cancellationToken);
        _ = compilation?.GetDiagnostics(cancellationToken);
    }

    public async Task<ExerciseCompletionsResponse?> GetCompletionsAsync(
        string exerciseId,
        ExerciseCompletionRequest request,
        CancellationToken cancellationToken)
    {
        if (IsSwiftFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await swiftLspBridge.GetCompletionsAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                request.LineNumber,
                request.Column,
                cancellationToken);
        }

        if (IsPythonFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await pythonLspBridge.GetCompletionsAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                request.LineNumber,
                request.Column,
                cancellationToken);
        }

        using var context = await CreateContextAsync(exerciseId, request.ProfileId, request.Path, request.Content, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var position = GetAbsolutePosition(context.SourceText, request.LineNumber, request.Column);
        var completionService = CompletionService.GetService(context.Document)
            ?? throw new InvalidOperationException("C# completion service is unavailable.");
        var completions = await completionService.GetCompletionsAsync(context.Document, position, cancellationToken: cancellationToken);

        if (completions is null)
        {
            return new ExerciseCompletionsResponse([], context.SetupMessages);
        }

        var isMemberAccess = IsMemberAccessCompletion(context.SourceText, position);
        var typedPrefix = GetTypedCompletionPrefix(context.SourceText, position);
        var items = new List<ExerciseCompletionItemResponse>();
        if (!isMemberAccess)
        {
            items.AddRange(await GetInScopeSymbolCompletionsAsync(
                context.Document,
                position,
                typedPrefix,
                cancellationToken));
        }

        foreach (var item in completions.ItemsList
            .Where(item => ShouldIncludeCompletion(item, isMemberAccess))
            .Where(item => MatchesTypedPrefix(item, typedPrefix))
            .Where(item => items.All(existing => !string.Equals(existing.Label, item.DisplayText, StringComparison.Ordinal)))
            .Take(CompletionLimit))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var description = await completionService.GetDescriptionAsync(context.Document, item, cancellationToken);
            items.Add(new ExerciseCompletionItemResponse(
                item.DisplayText,
                item.Properties.TryGetValue("InsertionText", out var insertionText)
                    ? insertionText
                    : item.DisplayText,
                item.FilterText,
                item.SortText,
                description?.Text ?? string.Empty,
                item.Tags));
        }

        return new ExerciseCompletionsResponse(items, context.SetupMessages);
    }

    private static async Task<IReadOnlyCollection<ExerciseCompletionItemResponse>> GetInScopeSymbolCompletionsAsync(
        Document document,
        int position,
        string typedPrefix,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        if (semanticModel is null)
        {
            return [];
        }

        return semanticModel.LookupSymbols(position)
            .Where(symbol => symbol is IParameterSymbol or ILocalSymbol)
            .Where(symbol => !symbol.IsImplicitlyDeclared)
            .Where(symbol => MatchesTypedPrefix(symbol.Name, typedPrefix))
            .GroupBy(symbol => symbol.Name, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(symbol => symbol.Name, StringComparer.Ordinal)
            .Select(symbol => new ExerciseCompletionItemResponse(
                symbol.Name,
                symbol.Name,
                symbol.Name,
                $"0000_{symbol.Name}",
                SymbolCompletionDetail(symbol),
                SymbolCompletionTags(symbol)))
            .ToArray();
    }

    public async Task<ExerciseHoverResponse?> GetHoverAsync(
        string exerciseId,
        ExercisePositionRequest request,
        CancellationToken cancellationToken)
    {
        if (IsSwiftFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await swiftLspBridge.GetHoverAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                request.LineNumber,
                request.Column,
                cancellationToken);
        }

        if (IsPythonFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await pythonLspBridge.GetHoverAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                request.LineNumber,
                request.Column,
                cancellationToken);
        }

        using var context = await CreateContextAsync(exerciseId, request.ProfileId, request.Path, request.Content, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var position = GetAbsolutePosition(context.SourceText, request.LineNumber, request.Column);
        var root = await context.Document.GetSyntaxRootAsync(cancellationToken)
            ?? throw new InvalidOperationException("Could not load the C# syntax root.");
        var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken)
            ?? throw new InvalidOperationException("Could not load the C# semantic model.");

        var node = FindBestSemanticNode(root, position);
        if (node is null)
        {
            return new ExerciseHoverResponse([], null, context.SetupMessages);
        }

        var symbol = ResolveSymbol(semanticModel, node, cancellationToken);
        if (symbol is null)
        {
            return new ExerciseHoverResponse([], ToRange(context.SourceText, node.Span), context.SetupMessages);
        }

        var contents = BuildHoverContents(symbol);
        return new ExerciseHoverResponse(contents, ToRange(context.SourceText, node.Span), context.SetupMessages);
    }

    public async Task<ExerciseSignatureHelpResponse?> GetSignatureHelpAsync(
        string exerciseId,
        ExercisePositionRequest request,
        CancellationToken cancellationToken)
    {
        if (IsSwiftFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await swiftLspBridge.GetSignatureHelpAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                request.LineNumber,
                request.Column,
                cancellationToken);
        }

        if (IsPythonFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await pythonLspBridge.GetSignatureHelpAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                request.LineNumber,
                request.Column,
                cancellationToken);
        }

        using var context = await CreateContextAsync(exerciseId, request.ProfileId, request.Path, request.Content, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var position = GetAbsolutePosition(context.SourceText, request.LineNumber, request.Column);
        var root = await context.Document.GetSyntaxRootAsync(cancellationToken)
            ?? throw new InvalidOperationException("Could not load the C# syntax root.");
        var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken)
            ?? throw new InvalidOperationException("Could not load the C# semantic model.");

        var signatureContext = FindSignatureContext(root, position);
        if (signatureContext is null)
        {
            return new ExerciseSignatureHelpResponse([], 0, 0, context.SetupMessages);
        }

        var symbols = ResolveCallableSymbols(semanticModel, signatureContext.Node, cancellationToken)
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<IMethodSymbol>()
            .Where(symbol => symbol.MethodKind is not MethodKind.PropertyGet and not MethodKind.PropertySet)
            .Take(12)
            .ToArray();

        var signatures = symbols
            .Select(symbol => new ExerciseSignatureItemResponse(
                symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                symbol.GetDocumentationCommentSummary(),
                symbol.Parameters
                    .Select(parameter => new ExerciseSignatureParameterResponse(
                        parameter.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                        parameter.Name,
                        parameter.GetDocumentationCommentSummary()))
                    .ToArray()))
            .ToArray();

        return new ExerciseSignatureHelpResponse(signatures, 0, signatureContext.ActiveParameter, context.SetupMessages);
    }

    public async Task<ExerciseCodeActionsResponse?> GetCodeActionsAsync(
        string exerciseId,
        ExerciseCodeActionRequest request,
        CancellationToken cancellationToken)
    {
        if (IsSwiftFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await swiftLspBridge.GetCodeActionsAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                request.StartLineNumber,
                request.StartColumn,
                request.EndLineNumber,
                request.EndColumn,
                cancellationToken);
        }

        if (IsPythonFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await pythonLspBridge.GetCodeActionsAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                request.StartLineNumber,
                request.StartColumn,
                request.EndLineNumber,
                request.EndColumn,
                cancellationToken);
        }

        using var context = await CreateContextAsync(exerciseId, request.ProfileId, request.Path, request.Content, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var root = await context.Document.GetSyntaxRootAsync(cancellationToken)
            ?? throw new InvalidOperationException("Could not load the C# syntax root.");
        var sourceText = context.SourceText;
        var span = ToTextSpan(sourceText, request.StartLineNumber, request.StartColumn, request.EndLineNumber, request.EndColumn);
        var actions = new List<ExerciseCodeActionItemResponse>();

        actions.AddRange(await lspBridge.GetCodeActionsAsync(
            context.WorkspacePath,
            request.Path,
            request.Content,
            request.StartLineNumber,
            request.StartColumn,
            request.EndLineNumber,
            request.EndColumn,
            context.SetupMessages,
            cancellationToken));
        await AddExpressionBodyRefactoringAsync(context.Document, root, span, actions, cancellationToken);
        await AddThrowCompletionActionsAsync(context.Document, root, span, actions, cancellationToken);
        await AddMissingUsingActionsAsync(context.Document, root, sourceText, actions, cancellationToken);

        return new ExerciseCodeActionsResponse(
            actions
                .DistinctBy(action => (action.Title, action.Kind, string.Join('|', action.Edits.Select(edit => $"{edit.StartLineNumber}:{edit.StartColumn}:{edit.EndLineNumber}:{edit.EndColumn}:{edit.Text}"))))
                .Take(CodeActionLimit)
                .ToArray(),
            context.SetupMessages);
    }

    public async Task<ExerciseFormatResponse?> FormatAsync(
        string exerciseId,
        ExerciseLanguageRequest request,
        CancellationToken cancellationToken)
    {
        if (IsSwiftFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await swiftLspBridge.FormatAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                cancellationToken);
        }

        if (IsPythonFile(request.Path))
        {
            var workspace = await workspaces.EnsureWorkspaceAsync(request.ProfileId, exerciseId, cancellationToken);
            if (workspace is null)
            {
                return null;
            }

            EnsureEditableFile(workspace, request.Path);
            return await pythonLspBridge.FormatAsync(
                workspace.WorkspacePath,
                request.Path,
                request.Content,
                cancellationToken);
        }

        using var context = await CreateContextAsync(exerciseId, request.ProfileId, request.Path, request.Content, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var formattedDocument = await Formatter.FormatAsync(context.Document, cancellationToken: cancellationToken);
        var formattedText = await formattedDocument.GetTextAsync(cancellationToken);
        return new ExerciseFormatResponse(formattedText.ToString(), context.SetupMessages);
    }

    private async Task<RoslynExerciseContext?> CreateContextAsync(
        string exerciseId,
        string profileId,
        string path,
        string content,
        CancellationToken cancellationToken)
    {
        var workspace = await workspaces.EnsureWorkspaceAsync(profileId, exerciseId, cancellationToken);
        if (workspace is null)
        {
            return null;
        }

        EnsureEditableFile(workspace, path);

        var sourceText = SourceText.From(content);
        var documentPath = Path.GetFullPath(Path.Combine(workspace.WorkspacePath, path));
        var snapshot = await GetOrCreateProjectSnapshotAsync(workspace.WorkspacePath, documentPath, cancellationToken);
        var solution = snapshot.BaseSolution.WithDocumentText(snapshot.EditableDocumentId, sourceText);
        var document = solution.GetDocument(snapshot.EditableDocumentId)
            ?? throw new InvalidOperationException("Could not load the C# document snapshot.");

        return new RoslynExerciseContext(workspace.WorkspacePath, document, sourceText, []);
    }

    private static async Task<CachedRoslynProject> GetOrCreateProjectSnapshotAsync(
        string workspacePath,
        string editableDocumentPath,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{Path.GetFullPath(workspacePath)}::{Path.GetFullPath(editableDocumentPath)}";
        var stamp = BuildProjectSnapshotStamp(workspacePath, editableDocumentPath);
        if (ProjectSnapshots.TryGetValue(cacheKey, out var cached) && cached.Stamp == stamp)
        {
            return cached;
        }

        await ProjectSnapshotLock.WaitAsync(cancellationToken);
        try
        {
            if (ProjectSnapshots.TryGetValue(cacheKey, out cached) && cached.Stamp == stamp)
            {
                return cached;
            }

            var snapshot = await BuildProjectSnapshotAsync(workspacePath, editableDocumentPath, stamp, cancellationToken);
            ProjectSnapshots.AddOrUpdate(
                cacheKey,
                snapshot,
                (_, _) => snapshot);
            Interlocked.Increment(ref ProjectSnapshotBuildCount);
            return snapshot;
        }
        finally
        {
            ProjectSnapshotLock.Release();
        }
    }

    private static async Task<CachedRoslynProject> BuildProjectSnapshotAsync(
        string workspacePath,
        string editableDocumentPath,
        string stamp,
        CancellationToken cancellationToken)
    {
        var roslynWorkspace = new AdhocWorkspace(SharedRoslynHostServices.Value);
        var projectId = ProjectId.CreateNewId("Exercise");
        var editableDocumentId = DocumentId.CreateNewId(projectId, Path.GetFileName(editableDocumentPath));
        var globalsId = DocumentId.CreateNewId(projectId, "GlobalUsings.g.cs");
        var projectPath = Path.Combine(workspacePath, "src", "Exercise", "Exercise.csproj");
        var solution = roslynWorkspace.CurrentSolution
            .AddProject(ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "Exercise",
                "Exercise",
                LanguageNames.CSharp,
                filePath: projectPath,
                parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithNullableContextOptions(NullableContextOptions.Enable)
                    .WithOptimizationLevel(OptimizationLevel.Debug)))
            .AddMetadataReferences(projectId, TrustedPlatformReferences.Value)
            .AddDocument(
                globalsId,
                "GlobalUsings.g.cs",
                SourceText.From("""
                global using System;
                global using System.Collections.Generic;
                global using System.Linq;
                """),
                filePath: Path.Combine(workspacePath, "src", "Exercise", "obj", "GlobalUsings.g.cs"))
            .AddDocument(
                editableDocumentId,
                Path.GetFileName(editableDocumentPath),
                SourceText.From(await File.ReadAllTextAsync(editableDocumentPath, cancellationToken)),
                filePath: editableDocumentPath);

        foreach (var siblingPath in GetSiblingSourceFiles(workspacePath, editableDocumentPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            solution = solution.AddDocument(
                DocumentId.CreateNewId(projectId, Path.GetFileName(siblingPath)),
                Path.GetFileName(siblingPath),
                SourceText.From(await File.ReadAllTextAsync(siblingPath, cancellationToken)),
                filePath: siblingPath);
        }

        solution = await AddAnalyzerConfigDocumentAsync(
            solution,
            projectId,
            Path.Combine(workspacePath, ".editorconfig"),
            cancellationToken);

        if (!roslynWorkspace.TryApplyChanges(solution))
        {
            roslynWorkspace.Dispose();
            throw new InvalidOperationException("Could not create the C# language workspace.");
        }

        return new CachedRoslynProject(
            roslynWorkspace,
            roslynWorkspace.CurrentSolution,
            editableDocumentId,
            stamp);
    }

    private static IEnumerable<string> GetSiblingSourceFiles(string workspacePath, string editableDocumentPath)
    {
        var sourceRoot = Path.Combine(workspacePath, "src", "Exercise");
        if (!Directory.Exists(sourceRoot))
        {
            return [];
        }

        var normalizedEditablePath = Path.GetFullPath(editableDocumentPath);
        return Directory
            .EnumerateFiles(sourceRoot, "*.cs", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFullPath)
            .Where(path => !string.Equals(path, normalizedEditablePath, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);
    }

    private static string BuildProjectSnapshotStamp(string workspacePath, string editableDocumentPath)
    {
        var relevantFiles = new List<string>
        {
            Path.Combine(workspacePath, "src", "Exercise", "Exercise.csproj"),
            Path.Combine(workspacePath, ".editorconfig"),
        };
        relevantFiles.AddRange(GetSiblingSourceFiles(workspacePath, editableDocumentPath));

        return string.Join(
            '|',
            relevantFiles
                .Select(Path.GetFullPath)
                .Where(File.Exists)
                .Order(StringComparer.Ordinal)
                .Select(path => $"{path}:{new FileInfo(path).Length}:{File.GetLastWriteTimeUtc(path).Ticks}"));
    }

    private static async Task<IEnumerable<Diagnostic>> GetAnalyzerDiagnosticsAsync(
        Document document,
        Compilation compilation,
        SyntaxTree syntaxTree,
        List<string> setupMessages,
        CancellationToken cancellationToken)
    {
        try
        {
            var analyzers = document.Project.AnalyzerReferences
                .SelectMany(reference => reference.GetAnalyzers(LanguageNames.CSharp))
                .Concat(ExerciseAnalyzers)
                .ToImmutableArray();

            if (analyzers.IsDefaultOrEmpty)
            {
                return [];
            }

            var compilationWithAnalyzers = compilation.WithAnalyzers(
                analyzers,
                document.Project.AnalyzerOptions);
            return (await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken))
                .Where(diagnostic => diagnostic.Location.SourceTree == syntaxTree);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            setupMessages.Add($"Analyzer diagnostics were unavailable: {exception.Message}");
            return [];
        }
    }

    private static async Task AddExpressionBodyRefactoringAsync(
        Document document,
        SyntaxNode root,
        TextSpan span,
        List<ExerciseCodeActionItemResponse> actions,
        CancellationToken cancellationToken)
    {
        var node = root.FindNode(span, getInnermostNodeForTie: true);
        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.Body?.Statements is not [ReturnStatementSyntax returnStatement] || method.ExpressionBody is not null)
        {
            return;
        }

        var expression = returnStatement.Expression;
        if (expression is null)
        {
            return;
        }

        var arrowMethod = method
            .WithBody(null)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithAdditionalAnnotations(Formatter.Annotation);

        var changedRoot = root.ReplaceNode(method, arrowMethod);
        var changedDocument = document.WithSyntaxRoot(changedRoot);
        var formattedDocument = await Formatter.FormatAsync(changedDocument, cancellationToken: cancellationToken);
        var changedText = await formattedDocument.GetTextAsync(cancellationToken);
        actions.Add(ToFullDocumentAction("Convert to expression-bodied member", "refactor.rewrite", changedText.ToString()));
    }

    private static async Task AddThrowCompletionActionsAsync(
        Document document,
        SyntaxNode root,
        TextSpan span,
        List<ExerciseCodeActionItemResponse> actions,
        CancellationToken cancellationToken)
    {
        var node = root.FindNode(span, getInnermostNodeForTie: true);
        var throwStatement = node.FirstAncestorOrSelf<ThrowStatementSyntax>();
        var method = throwStatement?.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (throwStatement is null || method is null)
        {
            return;
        }

        var replacement = method.ReturnType switch
        {
            PredefinedTypeSyntax predefined when predefined.Keyword.IsKind(SyntaxKind.StringKeyword) =>
                SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(string.Empty))),
            GenericNameSyntax { Identifier.Text: "IReadOnlyList", TypeArgumentList.Arguments.Count: 1 } generic =>
                SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression($"Array.Empty<{generic.TypeArgumentList.Arguments[0]}>()")),
            GenericNameSyntax { Identifier.Text: "Dictionary", TypeArgumentList.Arguments.Count: 2 } generic =>
                SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression($"new Dictionary<{generic.TypeArgumentList.Arguments[0]}, {generic.TypeArgumentList.Arguments[1]}>()")),
            PredefinedTypeSyntax predefined when predefined.Keyword.IsKind(SyntaxKind.VoidKeyword) =>
                SyntaxFactory.ReturnStatement(),
            _ => null,
        };

        if (replacement is null)
        {
            return;
        }

        var changedRoot = root.ReplaceNode(throwStatement, replacement.WithAdditionalAnnotations(Formatter.Annotation));
        var changedDocument = document.WithSyntaxRoot(changedRoot);
        var formattedDocument = await Formatter.FormatAsync(changedDocument, cancellationToken: cancellationToken);
        var changedText = await formattedDocument.GetTextAsync(cancellationToken);
        actions.Add(ToFullDocumentAction("Replace NotImplementedException with a compiling default", "quickfix", changedText.ToString()));
    }

    private static async Task AddMissingUsingActionsAsync(
        Document document,
        SyntaxNode root,
        SourceText sourceText,
        List<ExerciseCodeActionItemResponse> actions,
        CancellationToken cancellationToken)
    {
        var compilation = await document.Project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            return;
        }

        var diagnostics = compilation.GetDiagnostics(cancellationToken)
            .Where(diagnostic => diagnostic.Id == "CS0246" && diagnostic.Location.IsInSource)
            .Take(4);

        foreach (var diagnostic in diagnostics)
        {
            var tokenText = sourceText.ToString(diagnostic.Location.SourceSpan);
            var namespaceName = WellKnownNamespaceForType(tokenText);
            if (namespaceName is null)
            {
                continue;
            }

            var compilationUnit = root as CompilationUnitSyntax ?? root.DescendantNodes().OfType<CompilationUnitSyntax>().FirstOrDefault();
            if (compilationUnit is null || compilationUnit.Usings.Any(usingDirective => usingDirective.Name?.ToString() == namespaceName))
            {
                continue;
            }

            var updated = compilationUnit.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName)))
                .WithAdditionalAnnotations(Formatter.Annotation);
            var changedRoot = root.ReplaceNode(compilationUnit, updated);
            var formattedDocument = await Formatter.FormatAsync(document.WithSyntaxRoot(changedRoot), cancellationToken: cancellationToken);
            var changedText = await formattedDocument.GetTextAsync(cancellationToken);
            actions.Add(ToFullDocumentAction($"Add using {namespaceName}", "quickfix", changedText.ToString()));
        }
    }

    private static ExerciseCodeActionItemResponse ToFullDocumentAction(string title, string kind, string content)
    {
        return new ExerciseCodeActionItemResponse(
            title,
            kind,
            [new ExerciseTextEditResponse(1, 1, int.MaxValue, int.MaxValue, content)]);
    }

    private static string? WellKnownNamespaceForType(string typeName)
    {
        return typeName switch
        {
            "StringBuilder" => "System.Text",
            "Regex" => "System.Text.RegularExpressions",
            "ImmutableArray" or "ImmutableList" or "ImmutableDictionary" => "System.Collections.Immutable",
            "ConcurrentDictionary" => "System.Collections.Concurrent",
            "JsonSerializer" => "System.Text.Json",
            _ => null,
        };
    }

    private static SyntaxNode? FindBestSemanticNode(SyntaxNode root, int position)
    {
        var token = root.FindToken(Math.Clamp(position, 0, Math.Max(root.FullSpan.Length - 1, 0)));
        if (token.IsKind(SyntaxKind.None) && position > 0)
        {
            token = root.FindToken(position - 1);
        }

        return token.Parent?
            .AncestorsAndSelf()
            .FirstOrDefault(node =>
                node is IdentifierNameSyntax
                    or GenericNameSyntax
                    or MemberAccessExpressionSyntax
                    or InvocationExpressionSyntax
                    or ObjectCreationExpressionSyntax
                    or ParameterSyntax
                    or VariableDeclaratorSyntax
                    or MethodDeclarationSyntax
                    or TypeDeclarationSyntax);
    }

    private static ISymbol? ResolveSymbol(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is ParameterSyntax parameter)
        {
            return semanticModel.GetDeclaredSymbol(parameter, cancellationToken);
        }

        if (node is VariableDeclaratorSyntax variable)
        {
            return semanticModel.GetDeclaredSymbol(variable, cancellationToken);
        }

        if (node is BaseMethodDeclarationSyntax method)
        {
            return semanticModel.GetDeclaredSymbol(method, cancellationToken);
        }

        if (node is BaseTypeDeclarationSyntax type)
        {
            return semanticModel.GetDeclaredSymbol(type, cancellationToken);
        }

        var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
        return symbolInfo.Symbol
            ?? symbolInfo.CandidateSymbols.FirstOrDefault()
            ?? semanticModel.GetTypeInfo(node, cancellationToken).Type;
    }

    private static IReadOnlyCollection<string> BuildHoverContents(ISymbol symbol)
    {
        var display = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        var fullyQualified = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty, StringComparison.Ordinal);
        var summary = symbol.GetDocumentationCommentSummary();
        var contents = new List<string>
        {
            $"```csharp\n{SymbolKindLabel(symbol)} {display}\n```",
        };

        if (!string.Equals(display, fullyQualified, StringComparison.Ordinal))
        {
            contents.Add(fullyQualified);
        }

        if (!string.IsNullOrWhiteSpace(summary))
        {
            contents.Add(summary);
        }

        return contents;
    }

    private static string SymbolKindLabel(ISymbol symbol)
    {
        return symbol switch
        {
            IMethodSymbol { MethodKind: MethodKind.Constructor } => "constructor",
            IMethodSymbol => "method",
            IPropertySymbol => "property",
            IFieldSymbol => "field",
            IParameterSymbol => "parameter",
            ILocalSymbol => "local",
            INamedTypeSymbol { TypeKind: TypeKind.Class } => "class",
            INamedTypeSymbol { TypeKind: TypeKind.Interface } => "interface",
            INamedTypeSymbol { IsRecord: true } => "record",
            INamedTypeSymbol { TypeKind: TypeKind.Struct } => "struct",
            INamespaceSymbol => "namespace",
            _ => symbol.Kind.ToString().ToLowerInvariant(),
        };
    }

    private static SignatureContext? FindSignatureContext(SyntaxNode root, int position)
    {
        var token = root.FindToken(Math.Clamp(position, 0, Math.Max(root.FullSpan.Length - 1, 0)));
        return token.Parent?
            .AncestorsAndSelf()
            .Select(node => node switch
            {
                InvocationExpressionSyntax invocation when invocation.ArgumentList.Span.Contains(position) =>
                    new SignatureContext(invocation, ActiveParameterIndex(invocation.ArgumentList, position)),
                ObjectCreationExpressionSyntax creation when creation.ArgumentList?.Span.Contains(position) == true =>
                    new SignatureContext(creation, ActiveParameterIndex(creation.ArgumentList, position)),
                _ => null,
            })
            .FirstOrDefault(context => context is not null);
    }

    private static int ActiveParameterIndex(BaseArgumentListSyntax argumentList, int position)
    {
        var active = 0;
        foreach (var separator in argumentList.Arguments.GetSeparators())
        {
            if (separator.SpanStart < position)
            {
                active++;
            }
        }

        return active;
    }

    private static IEnumerable<ISymbol> ResolveCallableSymbols(
        SemanticModel semanticModel,
        SyntaxNode node,
        CancellationToken cancellationToken)
    {
        if (node is ObjectCreationExpressionSyntax creation)
        {
            var type = semanticModel.GetTypeInfo(creation, cancellationToken).Type as INamedTypeSymbol;
            return type?.Constructors.Where(constructor => !constructor.IsImplicitlyDeclared) ?? [];
        }

        var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
        if (symbolInfo.Symbol is not null)
        {
            return [symbolInfo.Symbol];
        }

        return symbolInfo.CandidateSymbols;
    }

    private static bool ShouldIncludeCompletion(CompletionItem item, bool isMemberAccess)
    {
        if (!isMemberAccess)
        {
            return true;
        }

        return !item.Tags.Contains("Keyword") && !item.Tags.Contains("Snippet");
    }

    private static bool MatchesTypedPrefix(CompletionItem item, string typedPrefix)
    {
        return MatchesTypedPrefix(item.DisplayText, typedPrefix)
            || MatchesTypedPrefix(item.FilterText, typedPrefix);
    }

    private static bool MatchesTypedPrefix(string value, string typedPrefix)
    {
        if (string.IsNullOrWhiteSpace(typedPrefix))
        {
            return true;
        }

        return value.StartsWith(typedPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string SymbolCompletionDetail(ISymbol symbol)
    {
        return symbol switch
        {
            IParameterSymbol parameter => $"{parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {parameter.Name}",
            ILocalSymbol local => $"{local.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {local.Name}",
            _ => symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
        };
    }

    private static IReadOnlyList<string> SymbolCompletionTags(ISymbol symbol)
    {
        return symbol switch
        {
            IParameterSymbol => ["Parameter"],
            ILocalSymbol => ["Local"],
            _ => [],
        };
    }

    private static string GetTypedCompletionPrefix(SourceText sourceText, int position)
    {
        var end = Math.Clamp(position, 0, sourceText.Length);
        var start = end;
        while (start > 0)
        {
            var character = sourceText[start - 1];
            if (!char.IsLetterOrDigit(character) && character != '_')
            {
                break;
            }

            start--;
        }

        return sourceText.ToString(TextSpan.FromBounds(start, end));
    }

    private static bool IsMemberAccessCompletion(SourceText sourceText, int position)
    {
        for (var index = Math.Clamp(position - 1, 0, sourceText.Length - 1); index >= 0; index--)
        {
            var character = sourceText[index];
            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            return character == '.';
        }

        return false;
    }

    private static void EnsureEditableFile(ExerciseWorkspaceResponse workspace, string path)
    {
        if (!workspace.Files.Any(file => file.Path == path && file.Editable))
        {
            throw new InvalidOperationException("Only editable exercise files support language service requests.");
        }
    }

    private static bool IsSwiftFile(string path)
    {
        return path.EndsWith(".swift", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPythonFile(string path)
    {
        return path.EndsWith(".py", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<Solution> AddAnalyzerConfigDocumentAsync(
        Solution solution,
        ProjectId projectId,
        string editorConfigPath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(editorConfigPath))
        {
            return solution;
        }

        var sourceText = SourceText.From(await File.ReadAllTextAsync(editorConfigPath, cancellationToken));
        return solution.AddAnalyzerConfigDocument(
            DocumentId.CreateNewId(projectId, ".editorconfig"),
            ".editorconfig",
            sourceText,
            filePath: editorConfigPath);
    }

    private static int GetAbsolutePosition(SourceText sourceText, int lineNumber, int column)
    {
        var lineIndex = Math.Clamp(lineNumber - 1, 0, sourceText.Lines.Count - 1);
        var line = sourceText.Lines[lineIndex];
        var zeroBasedColumn = Math.Clamp(column - 1, 0, line.Span.Length);
        return line.Start + zeroBasedColumn;
    }

    private static TextSpan ToTextSpan(SourceText sourceText, int startLineNumber, int startColumn, int endLineNumber, int endColumn)
    {
        var start = GetAbsolutePosition(sourceText, startLineNumber, startColumn);
        var end = GetAbsolutePosition(sourceText, endLineNumber, endColumn);
        return TextSpan.FromBounds(Math.Min(start, end), Math.Max(start, end));
    }

    private static ExerciseRangeResponse ToRange(SourceText sourceText, TextSpan span)
    {
        var start = sourceText.Lines.GetLinePosition(span.Start);
        var end = sourceText.Lines.GetLinePosition(span.End);
        return new ExerciseRangeResponse(
            start.Line + 1,
            start.Character + 1,
            end.Line + 1,
            Math.Max(end.Character + 1, start.Character + 2));
    }

    private static MefHostServices CreateRoslynHostServices()
    {
        var assemblies = MefHostServices.DefaultAssemblies
            .Concat([
                typeof(CompletionService).Assembly,
                typeof(CodeAction).Assembly,
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
            ])
            .Distinct();

        return MefHostServices.Create(assemblies);
    }

    private static IReadOnlyCollection<MetadataReference> CreateTrustedPlatformReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            return [];
        }

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }

    private static ExerciseDiagnosticResponse? ToResponse(Diagnostic diagnostic)
    {
        if (!diagnostic.Location.IsInSource)
        {
            return null;
        }

        var span = diagnostic.Location.GetLineSpan();
        return new ExerciseDiagnosticResponse(
            diagnostic.Id,
            diagnostic.GetMessage(),
            diagnostic.Severity.ToString(),
            span.StartLinePosition.Line + 1,
            span.StartLinePosition.Character + 1,
            span.EndLinePosition.Line + 1,
            span.EndLinePosition.Character + 1);
    }

    private sealed record CachedRoslynProject(
        AdhocWorkspace Workspace,
        Solution BaseSolution,
        DocumentId EditableDocumentId,
        string Stamp);

    private sealed record RoslynExerciseContext(
        string WorkspacePath,
        Document Document,
        SourceText SourceText,
        List<string> SetupMessages)
        : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private sealed record SignatureContext(SyntaxNode Node, int ActiveParameter);

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "MicrosoftCodeAnalysisCorrectness",
        "RS1036:Specify analyzer banned API enforcement setting",
        Justification = "This analyzer is embedded exercise feedback, not a compiler extension package.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "MicrosoftCodeAnalysisCorrectness",
        "RS1038:Compiler extensions should not be implemented in assemblies with workspace references",
        Justification = "This analyzer is embedded exercise feedback, not a compiler extension package.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "MicrosoftCodeAnalysisCorrectness",
        "RS1041:Compiler extensions should target netstandard2.0",
        Justification = "This analyzer is embedded exercise feedback, not a compiler extension package.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "MicrosoftCodeAnalysisReleaseTracking",
        "RS2008:Enable analyzer release tracking",
        Justification = "This analyzer is embedded exercise feedback, not a distributed analyzer package.")]
    private sealed class UnusedLocalVariableAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new(
            "PTP0001",
            "Local variable is assigned but never read",
            "Local variable '{0}' is assigned but never read",
            "Prodigee.StaticAnalysis",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarator, SyntaxKind.VariableDeclarator);
        }

        private static void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
        {
            var declarator = (VariableDeclaratorSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(declarator, context.CancellationToken);
            if (symbol is null)
            {
                return;
            }

            var root = declarator.SyntaxTree.GetRoot(context.CancellationToken);
            var isRead = root.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Any(identifier =>
                    identifier.SpanStart > declarator.Span.End
                    && SymbolEqualityComparer.Default.Equals(
                        context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol,
                        symbol));

            if (isRead)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, declarator.Identifier.GetLocation(), declarator.Identifier.Text));
        }
    }
}

internal static partial class ExerciseSymbolDocumentationExtensions
{
    public static string GetDocumentationCommentSummary(this ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: CancellationToken.None);
        if (string.IsNullOrWhiteSpace(xml))
        {
            return string.Empty;
        }

        try
        {
            var document = XDocument.Parse(xml);
            var summary = document.Root?.Element("summary")?.Value ?? string.Empty;
            return WhitespaceRegex().Replace(summary, " ").Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}

public sealed record ExerciseLanguageRequest(
    string ProfileId,
    string Path,
    string Content);

public sealed record ExercisePositionRequest(
    string ProfileId,
    string Path,
    string Content,
    int LineNumber,
    int Column);

public sealed record ExerciseCompletionRequest(
    string ProfileId,
    string Path,
    string Content,
    int LineNumber,
    int Column);

public sealed record ExerciseCodeActionRequest(
    string ProfileId,
    string Path,
    string Content,
    int StartLineNumber,
    int StartColumn,
    int EndLineNumber,
    int EndColumn);

public sealed record ExerciseDiagnosticsResponse(
    IReadOnlyCollection<ExerciseDiagnosticResponse> Diagnostics,
    IReadOnlyCollection<string> SetupMessages);

public sealed record ExerciseDiagnosticResponse(
    string Id,
    string Message,
    string Severity,
    int StartLineNumber,
    int StartColumn,
    int EndLineNumber,
    int EndColumn);

public sealed record ExerciseCompletionsResponse(
    IReadOnlyCollection<ExerciseCompletionItemResponse> Items,
    IReadOnlyCollection<string> SetupMessages);

public sealed record ExerciseCompletionItemResponse(
    string Label,
    string InsertText,
    string FilterText,
    string SortText,
    string Detail,
    IReadOnlyList<string> Tags);

public sealed record ExerciseHoverResponse(
    IReadOnlyCollection<string> Contents,
    ExerciseRangeResponse? Range,
    IReadOnlyCollection<string> SetupMessages);

public sealed record ExerciseSignatureHelpResponse(
    IReadOnlyCollection<ExerciseSignatureItemResponse> Signatures,
    int ActiveSignature,
    int ActiveParameter,
    IReadOnlyCollection<string> SetupMessages);

public sealed record ExerciseSignatureItemResponse(
    string Label,
    string Documentation,
    IReadOnlyCollection<ExerciseSignatureParameterResponse> Parameters);

public sealed record ExerciseSignatureParameterResponse(
    string Label,
    string Name,
    string Documentation);

public sealed record ExerciseCodeActionsResponse(
    IReadOnlyCollection<ExerciseCodeActionItemResponse> Actions,
    IReadOnlyCollection<string> SetupMessages);

public sealed record ExerciseCodeActionItemResponse(
    string Title,
    string Kind,
    IReadOnlyCollection<ExerciseTextEditResponse> Edits);

public sealed record ExerciseTextEditResponse(
    int StartLineNumber,
    int StartColumn,
    int EndLineNumber,
    int EndColumn,
    string Text);

public sealed record ExerciseRangeResponse(
    int StartLineNumber,
    int StartColumn,
    int EndLineNumber,
    int EndColumn);

public sealed record ExerciseFormatResponse(
    string Content,
    IReadOnlyCollection<string> SetupMessages);
