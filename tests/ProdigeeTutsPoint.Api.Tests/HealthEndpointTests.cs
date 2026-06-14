using System.Net;
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
}
