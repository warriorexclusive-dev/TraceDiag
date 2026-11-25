using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class HostDiscoveryTests
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
        var result = await DiscoverAsync(probe, TestFactories.Discovery(roundLimit: 2), progress);
        Assert.Equal(2, result.Hops.Count);
        Assert.True(result.TargetReached);
        Assert.Equal([1, 2], probe.Calls.Select(call => call.Ttl).Distinct());
        Assert.Contains(progress.Values, value => value.Contains("Tracing route"));
    }

    [Fact]
    public async Task DiscoverAsync_WithNoResponse_AddsSkippedPlaceholders()
    {
        var probe = new FakeProbeClient(TestFactories.Timeout);
        var result = await DiscoverAsync(probe, TestFactories.Discovery(roundLimit: 1));
        Assert.Equal(DiagnosticConstants.MaxHops, result.Hops.Count);
        Assert.Equal(DiagnosticConstants.MaxHops, result.SkippedTtls.Count);
        Assert.All(result.Hops, hop => Assert.Equal(Strings.Status.NoAnswer, hop.Status));
        Assert.All(result.Hops, hop => Assert.True(hop.IsSkipped));
        Assert.All(probe.Calls, call => Assert.True(call.DontFragment));
    }

    [Fact]
    public async Task DiscoverAsync_RetriesUnresolvedTtlAcrossRounds()
    {
        var ttl2Calls = 0;
        var probe = new FakeProbeClient(call =>
        {
            if (call.Ttl != 2) return TestFactories.ExpiredFrom(call, TestFactories.Address(call.Ttl));
            return ++ttl2Calls == 1 ? TestFactories.Timeout(call) : TestFactories.Success(call);
        });
        var result = await DiscoverAsync(probe, TestFactories.Discovery(roundLimit: 2));
        Assert.Equal(2, ttl2Calls);
        Assert.True(result.TargetReached);
        Assert.DoesNotContain(2, result.SkippedTtls);
        Assert.False(result.Hops.Single(hop => hop.Ttl == 2).IsSkipped);
    }

    [Fact]
    public async Task DiscoverAsync_UsesDiscoveryDefaults()
    {
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        await DiscoverAsync(probe, DiscoverySettings.CreateDefault());
        var call = Assert.Single(probe.Calls);
        Assert.Equal(64, call.BufferSize);
        Assert.Equal(5000, call.TimeoutMs);
        Assert.True(call.DontFragment);
    }

    [Fact]
    public async Task DiscoverAsync_WithoutTarget_WarnsAboutRoundLimit()
    {
        var probe = new FakeProbeClient(TestFactories.Timeout);
        var progress = TestFactories.TextProgress();
        var result = await DiscoverAsync(probe, TestFactories.Discovery(roundLimit: 1), progress);
        Assert.False(result.TargetReached);
        Assert.Contains(progress.Values, value => value.Contains("round limit"));
    }

    private static Task<HostDiscoveryResult> DiscoverAsync(FakeProbeClient probe, DiscoverySettings settings, CollectingProgress<string>? progress = null) =>
        HostDiscovery.DiscoverAsync(TestFactories.Context(probe), TestFactories.Address(2), settings,
            progress ?? TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
}
