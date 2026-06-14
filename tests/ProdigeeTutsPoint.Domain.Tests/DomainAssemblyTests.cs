using ProdigeeTutsPoint.Domain;

namespace ProdigeeTutsPoint.Domain.Tests;

public sealed class DomainAssemblyTests
{
    [Fact]
    public void DomainAssemblyMarkerIsAvailable()
    {
        Assert.Equal("ProdigeeTutsPoint.Domain", typeof(DomainAssemblyMarker).Namespace);
    }
}
