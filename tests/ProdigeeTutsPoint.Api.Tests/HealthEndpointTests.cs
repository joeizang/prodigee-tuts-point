using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProdigeeTutsPoint.Api.Tests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task HealthEndpointReturnsOk()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SetupDiagnosticsReturnActionableChecks()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var diagnostics = await client.GetFromJsonAsync<SetupDiagnosticsTestResponse>(
            "/api/setup/diagnostics",
            TestContext.Current.CancellationToken);

        Assert.NotNull(diagnostics);
        Assert.Contains(diagnostics.Checks, check => check.Id == "dotnet-sdk" && !string.IsNullOrWhiteSpace(check.Message));
        Assert.Contains(diagnostics.Checks, check => check.Id == "content" && check.Status == "ok");
        Assert.Contains(diagnostics.Checks, check => check.Id == "ollama" && !string.IsNullOrWhiteSpace(check.Message));
        Assert.All(diagnostics.Checks, check => Assert.True(check.Status is "ok" or "warning" or "error"));
    }

    [Fact]
    public async Task BuiltFrontendCanBeServedByAspNetHost()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<div id=\"root\"></div>", body, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record SetupDiagnosticsTestResponse(
        bool IsReady,
        IReadOnlyCollection<SetupCheckTestResponse> Checks);

    private sealed record SetupCheckTestResponse(
        string Id,
        string Status,
        string Message);
}
