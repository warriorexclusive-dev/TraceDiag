using System.Net.Sockets;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class HopNameResolverTests
{
    [Fact]
    public async Task ResolveAsync_AssignsNames()
    {
        var dns = new FakeDnsResolver(address => address.Equals(TestFactories.Address(1)) ? "router" : null);
        var context = new DiagnosticContext(new FakeProbeClient(call => TestFactories.Success(call)), dns, null);
        var hops = new[] { TestFactories.Hop(1, 1), TestFactories.Hop(2, 2) };
        var progress = TestFactories.TextProgress();
        await HopNameResolver.ResolveAsync(context, hops.ToList(), progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal("router", hops[0].HostName);
        Assert.Null(hops[1].HostName);
        Assert.Equal(2, dns.Calls.Count);
    }

    [Fact]
    public async Task ResolveAsync_OnDnsFailure_KeepsNameEmptyAndLogsError()
    {
        var dns = new FakeDnsResolver(_ => throw new SocketException());
        var logger = new FakeDiagnosticLogger();
        var context = new DiagnosticContext(new FakeProbeClient(call => TestFactories.Success(call)), dns, logger);
        var hops = new[] { TestFactories.Hop(1, 1) };
        await HopNameResolver.ResolveAsync(context, hops.ToList(), TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Null(hops[0].HostName);
        Assert.Single(logger.Errors);
    }
}
