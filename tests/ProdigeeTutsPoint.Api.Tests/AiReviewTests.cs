using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProdigeeTutsPoint.Api.Features.AiReview;
using ProdigeeTutsPoint.Infrastructure.Content;
using ProdigeeTutsPoint.Infrastructure.Persistence;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class AiReviewTests
{
    [Fact]
    public async Task ProvidersEndpointReturnsHostedOpenAiAndLocalOllamaPresetsWithoutSecrets()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var providers = await client.GetFromJsonAsync<IReadOnlyCollection<AiProviderTestResponse>>(
            "/api/ai/providers",
            TestContext.Current.CancellationToken);

        Assert.NotNull(providers);
        Assert.Contains(providers, provider =>
            provider.Id == "hosted-openai"
            && provider.Preset == "HostedOpenAI"
            && !string.IsNullOrWhiteSpace(provider.SecretName)
            && !provider.SecretName.StartsWith("sk-", StringComparison.Ordinal));
        Assert.Contains(providers, provider =>
            provider.Id == "local-ollama"
            && provider.Preset == "LocalOllama");
    }

    [Fact]
    public async Task HostedProviderTestFailsClosedWhenSecretIsMissing()
    {
        await using var factory = new WebApplicationFactory<Program>();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var provider = await db.AiReviewProviderSettings.FindAsync(["hosted-openai"], TestContext.Current.CancellationToken);
            Assert.NotNull(provider);
            provider.SecretName = $"PRODIGEE_TEST_MISSING_SECRET_{Guid.NewGuid():N}";
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var client = factory.CreateClient();

        try
        {
            var response = await client.PostAsJsonAsync(
                "/api/ai/providers/hosted-openai/test",
                new { },
                TestContext.Current.CancellationToken);
            var result = await response.Content.ReadFromJsonAsync<AiProviderCapabilityTestResponse>(
                TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Missing provider secret", result.Message);
        }
        finally
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var provider = await db.AiReviewProviderSettings.FindAsync(["hosted-openai"], TestContext.Current.CancellationToken);
            Assert.NotNull(provider);
            provider.SecretName = "OPENAI_API_KEY";
            provider.IsEnabled = false;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task AiReviewServiceStoresStructuredAdvisoryReview()
    {
        await using var factory = new WebApplicationFactory<Program>();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reader = scope.ServiceProvider.GetRequiredService<ContentFileReader>();
        var provider = await db.AiReviewProviderSettings.FindAsync(["local-ollama"], TestContext.Current.CancellationToken);
        Assert.NotNull(provider);
        var originalEndpoint = provider.Endpoint;
        var originalModel = provider.Model;
        var originalEnabled = provider.IsEnabled;
        try
        {
            provider.IsEnabled = true;
            provider.Endpoint = "http://127.0.0.1:11434/v1";
            provider.Model = "fake-review-model";
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            using var http = new HttpClient(new FakeOpenAiHandler())
            {
                BaseAddress = new Uri("http://127.0.0.1:11434"),
            };
            var service = new AiReviewService(db, reader, http);

            var review = await service.RunReviewAsync(
                new AiReviewRequest(
                    "ai-review-test-profile",
                    "wordfreq-csharp",
                    "pure-word-counting-core",
                    "local-ollama"),
                TestContext.Current.CancellationToken);

            Assert.NotNull(review);
            Assert.Equal("Advisory", review.Policy);
            Assert.Equal("ai-review-v1", review.PromptVersion);
            Assert.StartsWith("tracks/csharp/projects/wordfreq/milestones/pure-word-counting-core.md:", review.RubricVersion, StringComparison.Ordinal);
            Assert.Equal("fake-review-model", review.Model);
            Assert.Contains("Good pure function boundary", review.Strengths);
            Assert.Contains("Add more edge-case evidence", review.NextSteps);
        }
        finally
        {
            provider.Endpoint = originalEndpoint;
            provider.Model = originalModel;
            provider.IsEnabled = originalEnabled;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task ProviderTestRequiresChatAndJsonObjectCapabilityBeforeEnabling()
    {
        await using var factory = new WebApplicationFactory<Program>();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reader = scope.ServiceProvider.GetRequiredService<ContentFileReader>();
        var provider = await db.AiReviewProviderSettings.FindAsync(["local-ollama"], TestContext.Current.CancellationToken);
        Assert.NotNull(provider);
        var originalEndpoint = provider.Endpoint;
        var originalModel = provider.Model;
        var originalEnabled = provider.IsEnabled;
        try
        {
            provider.Endpoint = "http://127.0.0.1:11434/v1";
            provider.Model = "fake-review-model";
            provider.IsEnabled = false;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            using var http = new HttpClient(new FakeOpenAiHandler());
            var service = new AiReviewService(db, reader, http);

            var result = await service.TestProviderAsync("local-ollama", TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(provider.IsEnabled);
        }
        finally
        {
            provider.Endpoint = originalEndpoint;
            provider.Model = originalModel;
            provider.IsEnabled = originalEnabled;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task AiReviewFailsClosedWhenMilestoneRubricIsMissing()
    {
        await using var factory = new WebApplicationFactory<Program>();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reader = scope.ServiceProvider.GetRequiredService<ContentFileReader>();
        var provider = await db.AiReviewProviderSettings.FindAsync(["local-ollama"], TestContext.Current.CancellationToken);
        var milestone = await db.ProjectMilestones.FindAsync(["pure-word-counting-core"], TestContext.Current.CancellationToken);
        Assert.NotNull(provider);
        Assert.NotNull(milestone);
        var originalEndpoint = provider.Endpoint;
        var originalEnabled = provider.IsEnabled;
        var originalMarkdownPath = milestone.MarkdownPath;
        try
        {
            provider.IsEnabled = true;
            provider.Endpoint = "http://127.0.0.1:11434/v1";
            milestone.MarkdownPath = $"missing-rubric-{Guid.NewGuid():n}.md";
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            using var http = new HttpClient(new FakeOpenAiHandler());
            var service = new AiReviewService(db, reader, http);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RunReviewAsync(
                    new AiReviewRequest(
                        "missing-rubric-review-test-profile",
                        "wordfreq-csharp",
                        "pure-word-counting-core",
                        "local-ollama"),
                    TestContext.Current.CancellationToken));

            Assert.Contains("rubric", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            provider.Endpoint = originalEndpoint;
            provider.IsEnabled = originalEnabled;
            milestone.MarkdownPath = originalMarkdownPath;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    private sealed class FakeOpenAiHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            if (requestBody.Contains("Reply with exactly OK", StringComparison.Ordinal))
            {
                return JsonChatResponse("OK");
            }

            if (requestBody.Contains("\"ok\":true", StringComparison.Ordinal))
            {
                return JsonChatResponse("""{"ok":true}""");
            }

            const string reviewJson = """
            {"score":82,"maxScore":100,"summary":"Solid advisory review.","strengths":["Good pure function boundary"],"risks":["Some edge cases need more proof"],"nextSteps":["Add more edge-case evidence"]}
            """;
            return JsonChatResponse(reviewJson);
        }

        private static HttpResponseMessage JsonChatResponse(string content)
        {
            var responseJson = $$"""
            {
              "choices": [
                {
                  "message": {
                    "content": {{System.Text.Json.JsonSerializer.Serialize(content)}}
                  }
                }
              ]
            }
            """;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed record AiProviderTestResponse(
        string Id,
        string DisplayName,
        string Preset,
        string Endpoint,
        string Model,
        string? SecretName,
        bool IsEnabled);

    private sealed record AiProviderCapabilityTestResponse(
        string ProviderId,
        bool Success,
        string Message);
}
