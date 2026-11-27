using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class AsnResolverControllerTests
{
    [Fact]
    public async Task ResolveAsync_RunsAsnLookupsInParallel()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var dns = new GatedDnsResolver(cts.Token);
        var context = new DiagnosticContext(new FakeProbeClient(call => TestFactories.Success(call)), dns, null);
        var hops = Enumerable.Range(1, 16).Select(i => TestFactories.Hop(i, i)).ToArray();
        await HopNameResolver.ResolveAsync(context, hops, TestFactories.TextProgress(), TestFactories.StatsProgress(), cts.Token);
        Assert.Equal(16, dns.Started);
        Assert.All(hops, hop => Assert.Equal("ASN", hop.Asn));
    }
}
