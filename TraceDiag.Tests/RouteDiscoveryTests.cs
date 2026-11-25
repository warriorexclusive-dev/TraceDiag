using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class RouteDiscoveryTests
{
    [Fact]
    public void MaxHops_EqualsIpTtlLimit()
    {
        Assert.Equal(255, DiagnosticConstants.MaxHops);
    }

    [Fact]
    public async Task DiscoverAsync_StopsWhenTargetReached()
    {
        var probe = new FakeProbeClient(call => call.Ttl == 1
            ? TestFactories.ExpiredFrom(call, TestFactories.Address(1))
            : TestFactories.Success(call));
        var progress = TestFactories.TextProgress();
        var stats = TestFactories.StatsProgress();
        var hops = await RouteDiscovery.DiscoverAsync(
            TestFactories.Context(probe), TestFactories.Address(2), 64, progress, stats, CancellationToken.None);
        Assert.Equal(2, hops.Count);
        Assert.Equal([1, 2], probe.Calls.Select(c => c.Ttl).Distinct());
        Assert.Equal(2, stats.Values.Count);
        Assert.Contains(progress.Values, value => value.Contains("Tracing route"));
    }

    [Fact]
    public async Task DiscoverAsync_WithNoResponseAtFirstTtl_ReturnsEmptyHops()
    {
        var probe = new FakeProbeClient(TestFactories.Timeout);
        var progress = TestFactories.TextProgress();
        var hops = await RouteDiscovery.DiscoverAsync(
            TestFactories.Context(probe), TestFactories.Address(2), 64, progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Empty(hops);
        Assert.Contains(progress.Values, value => value.Contains("no response"));
    }

    [Fact]
    public async Task DiscoverAsync_WithoutTarget_WarnsAboutHopLimit()
    {
        var probe = new FakeProbeClient(call => TestFactories.ExpiredFrom(call, TestFactories.Address(9)));
        var logger = new FakeDiagnosticLogger();
        var progress = TestFactories.TextProgress();
        var hops = await RouteDiscovery.DiscoverAsync(
            TestFactories.Context(probe, logger), TestFactories.Address(2), 64, progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(DiagnosticConstants.MaxHops, hops.Count);
        Assert.Contains(progress.Values, value => value.Contains("hop limit"));
    }
}
