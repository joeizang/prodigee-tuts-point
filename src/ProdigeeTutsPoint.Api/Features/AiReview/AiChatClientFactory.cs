using System.ClientModel;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using ProdigeeTutsPoint.Domain.Learning;

namespace ProdigeeTutsPoint.Api.Features.AiReview;

public interface IAiChatClientFactory
{
    AiChatClientLease Create(AiReviewProviderSetting provider, string? secret);
}

public sealed class AiChatClientFactory : IAiChatClientFactory
{
    public AiChatClientLease Create(AiReviewProviderSetting provider, string? secret)
    {
        return provider.Preset switch
        {
            "LocalOllama" => CreateOllamaClient(provider),
            "HostedOpenAI" => CreateOpenAiClient(provider, secret),
            _ => throw new InvalidOperationException($"Unsupported AI provider preset: {provider.Preset}."),
        };
    }

    private static AiChatClientLease CreateOllamaClient(AiReviewProviderSetting provider)
    {
        var endpoint = NormalizeOllamaEndpoint(provider.Endpoint);
        var httpClient = new HttpClient
        {
            BaseAddress = endpoint,
            Timeout = TimeSpan.FromMinutes(4),
        };
        var client = new OllamaApiClient(httpClient, provider.Model);
        return new AiChatClientLease(client);
    }

    private static AiChatClientLease CreateOpenAiClient(AiReviewProviderSetting provider, string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Hosted OpenAI provider secret is missing.");
        }

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(provider.Endpoint),
        };
        var openAiClient = new OpenAIClient(new ApiKeyCredential(secret), options);
        var chatClient = openAiClient.GetChatClient(provider.Model).AsIChatClient();
        return new AiChatClientLease(chatClient);
    }

    private static Uri NormalizeOllamaEndpoint(string endpoint)
    {
        var uri = new Uri(endpoint.TrimEnd('/'));
        if (uri.AbsolutePath.Equals("/v1", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new UriBuilder(uri)
            {
                Path = string.Empty,
                Query = string.Empty,
            };
            return builder.Uri;
        }

        return uri;
    }
}

public sealed class AiChatClientLease(IChatClient client) : IDisposable
{
    public IChatClient Client { get; } = client;

    public void Dispose()
    {
        if (Client is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
