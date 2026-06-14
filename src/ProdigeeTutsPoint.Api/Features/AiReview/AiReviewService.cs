using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProdigeeTutsPoint.Domain.Learning;
using ProdigeeTutsPoint.Infrastructure.Content;
using ProdigeeTutsPoint.Infrastructure.Persistence;

namespace ProdigeeTutsPoint.Api.Features.AiReview;

public sealed class AiReviewService(
    AppDbContext db,
    ContentFileReader contentFiles,
    HttpClient httpClient)
{
    private const string PromptVersion = "ai-review-v1";

    public async Task<IReadOnlyCollection<AiProviderResponse>> GetProvidersAsync(CancellationToken cancellationToken)
    {
        return await db.AiReviewProviderSettings
            .OrderBy(provider => provider.DisplayName)
            .Select(provider => new AiProviderResponse(
                provider.Id,
                provider.DisplayName,
                provider.Preset,
                provider.Endpoint,
                provider.Model,
                provider.SecretName,
                provider.IsEnabled))
            .ToListAsync(cancellationToken);
    }

    public async Task<AiProviderTestResponse?> TestProviderAsync(
        string providerId,
        CancellationToken cancellationToken)
    {
        var provider = await db.AiReviewProviderSettings.FirstOrDefaultAsync(provider => provider.Id == providerId, cancellationToken);
        if (provider is null)
        {
            return null;
        }

        var secret = AiSecretLoader.GetSecret(provider.SecretName);
        if (!string.IsNullOrWhiteSpace(provider.SecretName) && string.IsNullOrWhiteSpace(secret))
        {
            provider.IsEnabled = false;
            provider.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return new AiProviderTestResponse(provider.Id, false, "Missing provider secret.");
        }

        try
        {
            EnsureAllowedEndpoint(provider.Endpoint);
            using var pingRequest = BuildChatRequest(
                provider,
                secret,
                "Reply with exactly OK.",
                responseFormatJson: false);
            using var pingResponse = await httpClient.SendAsync(pingRequest, cancellationToken);
            var pingBody = await pingResponse.Content.ReadAsStringAsync(cancellationToken);

            using var jsonRequest = BuildChatRequest(
                provider,
                secret,
                "Return JSON with exactly this shape: {\"ok\":true}",
                responseFormatJson: true);
            using var jsonResponse = await httpClient.SendAsync(jsonRequest, cancellationToken);
            var jsonBody = await jsonResponse.Content.ReadAsStringAsync(cancellationToken);

            provider.IsEnabled = pingResponse.IsSuccessStatusCode
                && jsonResponse.IsSuccessStatusCode
                && ChatContentContains(pingBody, "OK")
                && ChatContentParsesAsJsonObject(jsonBody);
            provider.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return new AiProviderTestResponse(
                provider.Id,
                provider.IsEnabled,
                provider.IsEnabled
                    ? "Provider responded successfully."
                    : $"Provider test failed: chat {(int)pingResponse.StatusCode} {pingResponse.ReasonPhrase}; JSON {(int)jsonResponse.StatusCode} {jsonResponse.ReasonPhrase}");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            provider.IsEnabled = false;
            provider.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return new AiProviderTestResponse(provider.Id, false, $"Provider test failed: {exception.Message}");
        }
    }

    public async Task<AiReviewResponse?> RunReviewAsync(
        AiReviewRequest request,
        CancellationToken cancellationToken)
    {
        var provider = await db.AiReviewProviderSettings.FirstOrDefaultAsync(provider => provider.Id == request.ProviderId, cancellationToken);
        if (provider is null)
        {
            return null;
        }

        if (!provider.IsEnabled)
        {
            throw new InvalidOperationException("Provider must pass the test action before review can run.");
        }

        var milestone = await db.ProjectMilestones
            .Where(milestone => milestone.ProjectId == request.ProjectId && milestone.Id == request.MilestoneId)
            .Select(milestone => new
            {
                milestone.Id,
                milestone.ProjectId,
                milestone.Title,
                milestone.Summary,
                milestone.MarkdownPath,
                Lessons = milestone.Lessons
                    .OrderBy(link => link.Order)
                    .Select(link => link.Lesson!.Title)
                    .ToList(),
                Exercises = milestone.Exercises
                    .OrderBy(link => link.Order)
                    .Select(link => link.Exercise!.Title)
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (milestone is null)
        {
            return null;
        }

        var secret = AiSecretLoader.GetSecret(provider.SecretName);
        if (!string.IsNullOrWhiteSpace(provider.SecretName) && string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Provider secret is missing.");
        }

        var rubric = await contentFiles.ReadMarkdownAsync(milestone.MarkdownPath, cancellationToken);
        if (string.IsNullOrWhiteSpace(rubric))
        {
            throw new InvalidOperationException("Milestone rubric markdown is missing or empty.");
        }

        EnsureAllowedEndpoint(provider.Endpoint);
        var attempts = await db.ExerciseRunHistory
            .Where(history => history.ProfileId == request.ProfileId)
            .Select(history => new
            {
                history.ExerciseId,
                history.Status,
                history.Summary,
                history.StaticAnalysisErrorCount,
                history.StaticAnalysisWarningCount,
                history.CreatedAt,
            })
            .ToListAsync(cancellationToken);
        attempts = attempts
            .OrderByDescending(history => history.CreatedAt)
            .Take(20)
            .ToList();
        var rubricVersion = RubricVersionFor(milestone.MarkdownPath, rubric);
        var prompt = BuildReviewPrompt(milestone.Title, milestone.Summary, rubric, milestone.Lessons, milestone.Exercises, attempts);

        using var aiRequest = BuildChatRequest(provider, secret, prompt, responseFormatJson: true);
        using var response = await httpClient.SendAsync(aiRequest, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"AI review failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var parsed = ParseReview(raw);
        var now = DateTimeOffset.UtcNow;
        var review = new AiReviewResult
        {
            Id = Guid.NewGuid().ToString("n"),
            ProfileId = request.ProfileId,
            ProjectId = request.ProjectId,
            MilestoneId = request.MilestoneId,
            ProviderId = provider.Id,
            ProviderPreset = provider.Preset,
            Model = provider.Model,
            PromptVersion = PromptVersion,
            RubricVersion = rubricVersion,
            Policy = "Advisory",
            Status = "Completed",
            Score = parsed.Score,
            MaxScore = parsed.MaxScore,
            Summary = parsed.Summary,
            StrengthsJson = JsonSerializer.Serialize(parsed.Strengths),
            RisksJson = JsonSerializer.Serialize(parsed.Risks),
            NextStepsJson = JsonSerializer.Serialize(parsed.NextSteps),
            RawOutput = raw.Length <= 24_000 ? raw : raw[..24_000],
            CreatedAt = now,
        };
        db.AiReviewResults.Add(review);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(review);
    }

    public async Task<IReadOnlyCollection<AiReviewResponse>> GetReviewsAsync(
        string profileId,
        string projectId,
        string milestoneId,
        CancellationToken cancellationToken)
    {
        var reviews = await db.AiReviewResults
            .Where(review => review.ProfileId == profileId && review.ProjectId == projectId && review.MilestoneId == milestoneId)
            .ToListAsync(cancellationToken);
        return reviews
            .OrderByDescending(review => review.CreatedAt)
            .Take(10)
            .Select(ToResponse)
            .ToArray();
    }

    private static HttpRequestMessage BuildChatRequest(
        AiReviewProviderSetting provider,
        string? secret,
        string prompt,
        bool responseFormatJson)
    {
        var endpoint = provider.Endpoint.TrimEnd('/');
        var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/chat/completions");
        if (!string.IsNullOrWhiteSpace(secret))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
        }

        object payload = responseFormatJson
            ? new
            {
                model = provider.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a strict senior software engineering reviewer. Return concise, practical feedback." },
                    new { role = "user", content = prompt },
                },
                temperature = 0.2,
                max_tokens = 900,
                response_format = new { type = "json_object" },
            }
            : new
            {
                model = provider.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a strict senior software engineering reviewer. Return concise, practical feedback." },
                    new { role = "user", content = prompt },
                },
                temperature = 0.2,
                max_tokens = 16,
            };
        request.Content = JsonContent.Create(payload);
        return request;
    }

    private static void EnsureAllowedEndpoint(string endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException("AI provider endpoint must be an absolute HTTP(S) URL.");
        }

        var host = uri.Host.ToLowerInvariant();
        var isAllowed = host is "api.openai.com" or "localhost" or "127.0.0.1" or "::1";
        if (!isAllowed)
        {
            throw new InvalidOperationException("AI provider endpoint must use Hosted OpenAI or a local Ollama-compatible host.");
        }
    }

    private static string RubricVersionFor(string markdownPath, string rubric)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rubric))).ToLowerInvariant();
        return $"{markdownPath}:{hash[..12]}";
    }

    private static bool ChatContentContains(string rawResponse, string expected)
    {
        return TryReadChatContent(rawResponse, out var content)
            && content.Trim().Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ChatContentParsesAsJsonObject(string rawResponse)
    {
        if (!TryReadChatContent(rawResponse, out var content))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryReadChatContent(string rawResponse, out string content)
    {
        content = string.Empty;
        try
        {
            using var envelope = JsonDocument.Parse(rawResponse);
            content = envelope.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(content);
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return false;
        }
    }

    private static string BuildReviewPrompt(
        string title,
        string summary,
        string rubric,
        IReadOnlyCollection<string> lessons,
        IReadOnlyCollection<string> exercises,
        object attempts)
    {
        return $$"""
        Review this milestone as advisory feedback. Do not invent hidden tests or claim a pass/fail gate.

        Return JSON with this shape:
        {"score":0,"maxScore":100,"summary":"","strengths":[],"risks":[],"nextSteps":[]}

        Milestone: {{title}}
        Summary: {{summary}}

        Rubric:
        {{rubric}}

        Required lessons:
        {{string.Join("\n", lessons.Select(lesson => "- " + lesson))}}

        Required exercises:
        {{string.Join("\n", exercises.Select(exercise => "- " + exercise))}}

        Recent visible attempt evidence:
        {{JsonSerializer.Serialize(attempts)}}
        """;
    }

    private static ParsedAiReview ParseReview(string rawResponse)
    {
        try
        {
            using var envelope = JsonDocument.Parse(rawResponse);
            var content = envelope.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            return new ParsedAiReview(
                root.TryGetProperty("score", out var score) ? score.GetInt32() : 0,
                root.TryGetProperty("maxScore", out var maxScore) ? maxScore.GetInt32() : 100,
                root.TryGetProperty("summary", out var summary) ? summary.GetString() ?? "Review completed." : "Review completed.",
                ReadStringArray(root, "strengths"),
                ReadStringArray(root, "risks"),
                ReadStringArray(root, "nextSteps"));
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return new ParsedAiReview(0, 100, "AI review returned unstructured output.", [], [], ["Review the raw output manually."]);
        }
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var element) || element.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return element.EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Cast<string>()
            .ToArray();
    }

    private static AiReviewResponse ToResponse(AiReviewResult review)
    {
        return new AiReviewResponse(
            review.Id,
            review.ProviderId,
            review.ProviderPreset,
            review.Model,
            review.PromptVersion,
            review.RubricVersion,
            review.Policy,
            review.Status,
            review.Score,
            review.MaxScore,
            review.Summary,
            JsonSerializer.Deserialize<IReadOnlyCollection<string>>(review.StrengthsJson) ?? [],
            JsonSerializer.Deserialize<IReadOnlyCollection<string>>(review.RisksJson) ?? [],
            JsonSerializer.Deserialize<IReadOnlyCollection<string>>(review.NextStepsJson) ?? [],
            review.CreatedAt);
    }

    private sealed record ParsedAiReview(
        int Score,
        int MaxScore,
        string Summary,
        IReadOnlyCollection<string> Strengths,
        IReadOnlyCollection<string> Risks,
        IReadOnlyCollection<string> NextSteps);
}

public static class AiSecretLoader
{
    public static string? GetSecret(string? secretName)
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            return null;
        }

        var environmentValue = Environment.GetEnvironmentVariable(secretName);
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".prodigee-tuts-point",
            "secrets.json");
        if (!File.Exists(path))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.TryGetProperty(secretName, out var value) ? value.GetString() : null;
    }
}

public sealed record AiProviderResponse(
    string Id,
    string DisplayName,
    string Preset,
    string Endpoint,
    string Model,
    string? SecretName,
    bool IsEnabled);

public sealed record AiProviderTestResponse(
    string ProviderId,
    bool Success,
    string Message);

public sealed record AiReviewRequest(
    string ProfileId,
    string ProjectId,
    string MilestoneId,
    string ProviderId);

public sealed record AiReviewResponse(
    string Id,
    string ProviderId,
    string ProviderPreset,
    string Model,
    string PromptVersion,
    string RubricVersion,
    string Policy,
    string Status,
    int Score,
    int MaxScore,
    string Summary,
    IReadOnlyCollection<string> Strengths,
    IReadOnlyCollection<string> Risks,
    IReadOnlyCollection<string> NextSteps,
    DateTimeOffset CreatedAt);
