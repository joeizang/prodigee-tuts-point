using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.AI;
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
            && provider.Preset == "LocalOllama"
            && provider.Model == "gemma4:31b-mlx");
        Assert.Contains(providers, provider =>
            provider.Id == "local-ollama-qwen"
            && provider.Preset == "LocalOllama"
            && provider.Model == "qwen3.6:35b-mlx");
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

            var service = new AiReviewService(db, reader, new FakeAiChatClientFactory(
                """
                {"score":82,"maxScore":100,"summary":"Solid advisory review.","strengths":["Good pure function boundary"],"risks":["Some edge cases need more proof"],"nextSteps":["Add more edge-case evidence"]}
                """));

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
    public async Task ProviderTestEnablesProviderWhenGreetingReturnsText()
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

            var service = new AiReviewService(db, reader, new FakeAiChatClientFactory("Hi, I am ready."));

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
    public async Task ProviderTestAcceptsTextContentWhenResponseTextIsEmpty()
    {
        await using var factory = new WebApplicationFactory<Program>();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reader = scope.ServiceProvider.GetRequiredService<ContentFileReader>();
        var provider = await db.AiReviewProviderSettings.FindAsync(["local-ollama-qwen"], TestContext.Current.CancellationToken);
        Assert.NotNull(provider);
        var originalEndpoint = provider.Endpoint;
        var originalModel = provider.Model;
        var originalEnabled = provider.IsEnabled;
        try
        {
            provider.Endpoint = "http://127.0.0.1:11434/v1";
            provider.Model = "qwen3.6:35b-mlx";
            provider.IsEnabled = false;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var response = new ChatResponse(new ChatMessage(
                ChatRole.Assistant,
                new List<AIContent> { new TextContent("OK") }));
            var service = new AiReviewService(db, reader, new FakeAiChatClientFactory(response));

            var result = await service.TestProviderAsync("local-ollama-qwen", TestContext.Current.CancellationToken);

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
    public async Task ProviderTestFailsClosedWhenGreetingReturnsEmptyText()
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
            provider.IsEnabled = true;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new AiReviewService(db, reader, new FakeAiChatClientFactory(string.Empty));

            var result = await service.TestProviderAsync("local-ollama", TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(provider.IsEnabled);
            Assert.Contains("empty response", result.Message, StringComparison.OrdinalIgnoreCase);
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
    public async Task ProviderTestReturnsFailureWhenProviderTimesOut()
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
            provider.IsEnabled = true;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new AiReviewService(db, reader, new FakeAiChatClientFactory(
                responseText: null,
                exception: new TaskCanceledException("Simulated provider timeout.")));

            var result = await service.TestProviderAsync("local-ollama", TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(provider.IsEnabled);
            Assert.Contains("timed out", result.Message, StringComparison.OrdinalIgnoreCase);
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

            var service = new AiReviewService(db, reader, new FakeAiChatClientFactory("unused"));

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

    private sealed class FakeAiChatClientFactory : IAiChatClientFactory
    {
        private readonly ChatResponse? response;
        private readonly string? responseText;
        private readonly Exception? exception;

        public FakeAiChatClientFactory(string? responseText, Exception? exception = null)
        {
            this.responseText = responseText;
            this.exception = exception;
        }

        public FakeAiChatClientFactory(ChatResponse response)
        {
            this.response = response;
        }

        public AiChatClientLease Create(ProdigeeTutsPoint.Domain.Learning.AiReviewProviderSetting provider, string? secret)
        {
            return new AiChatClientLease(new FakeAiChatClient(response, responseText, exception));
        }
    }

    private sealed class FakeAiChatClient(ChatResponse? response, string? responseText, Exception? exception) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (exception is not null)
            {
                return Task.FromException<ChatResponse>(exception);
            }

            return Task.FromResult(response ?? new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText ?? string.Empty)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return null;
        }

        public void Dispose()
        {
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
