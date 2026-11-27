using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class DeepModeTests
{
    [Fact]
    public void RouteProgress_Use_StampsRouteMetadataOnStats()
    {
        var target = Target(3);
        var route = new RouteNode { Uid = 7, Key = "key", Target = target };
        var collected = new CollectingProgress<IReadOnlyList<HopStatSnapshot>>();
        var decorator = new RouteStatsDecorator(collected);
        var progress = new RouteProgress(new CollectingProgress<string>(), decorator);
        progress.Use(route);
        progress.Report([Stat(1, 2)]);
        var stat = Assert.Single(collected.Values.Last());
        Assert.Equal(7, stat.RouteUid);
        Assert.Equal(target.Address.ToString(), stat.RouteTarget);
        Assert.Equal(target.Resolver, stat.RouteResolver);
    }

    [Fact]
    public void RouteStatsDecorator_KeepsHopsFromDifferentRoutes()
    {
        var decorator = new RouteStatsDecorator(new CollectingProgress<IReadOnlyList<HopStatSnapshot>>());
        decorator.Use(new RouteNode { Uid = 1, Key = "key", Target = Target(1) });
        decorator.Report([Stat(1, 9)]);
        decorator.Use(new RouteNode { Uid = 2, Key = "key", Target = Target(2) });
        decorator.Report([Stat(1, 9)]);
        Assert.Equal([1, 2], decorator.All().Select(stat => stat.RouteUid).OrderBy(uid => uid));
    }

    [Fact]
    public async Task DetectAsync_DeepMode_ReportsEveryFlow()
    {
        var text = new CollectingProgress<string>();
        var settings = RoutePrecheckSettings.Create(true, true, 3000) with { MaxHops = 2 };
        await new AlternateRouteDetector().DetectAsync(
            TestFactories.Context(new FakeProbeClient(TestFactories.Timeout)), "example.com",
            [Target(2)], settings, new RouteRegistry(), text, CancellationToken.None);
        Assert.Contains(text.Values, line => line.Contains(Prefix(Strings.Diag.RoutePrecheckStartedFormat)));
        Assert.Contains(text.Values, line => line.Contains(Prefix(Strings.Diag.RoutePrecheckTargetFormat)));
        Assert.Contains(text.Values, line => line.Contains(Prefix(Strings.Diag.RoutePrecheckFlowFormat)));
        Assert.Contains(text.Values, line => line.Contains(Prefix(Strings.Diag.RoutePrecheckFinishedFormat)));
    }

    [Fact]
    public async Task DetectAsync_InternalDeadline_ReturnsFallbackInsteadOfCancellation()
    {
        var text = new CollectingProgress<string>();
        var settings = RoutePrecheckSettings.Create(true, true, 3000) with
        {
            DeadlineMs = 20, MaxHops = 1, Concurrency = 1
        };
        var tree = await new AlternateRouteDetector().DetectAsync(
            Context(new DelayingProbeClient()), "example.com",
            [Target(2)], settings, new RouteRegistry(), text, CancellationToken.None);
        Assert.True(tree.Partial);
        Assert.True(tree.Fallback);
        Assert.Contains(text.Values, line => line.Contains(Prefix(Strings.Diag.RoutePrecheckDeadlineFormat)));
    }

    private static DiagnosticContext Context(IIcmpProbeClient probe) =>
        new(probe, new FakeDnsResolver(address => address.ToString()), null);

    private static string Prefix(string canonical) => Strings.T(canonical).Split('{')[0];

    private static DnsAnswer Target(int last) =>
        new(TestFactories.Address(last), "resolver", false);

    private static HopStatSnapshot Stat(int ttl, int address) =>
        new(ttl, TestFactories.Address(address), null, 1, 1, 1, 1, 1, 1, "ok", "");

    private sealed class DelayingProbeClient : IIcmpProbeClient
    {
        public async Task<ProbeResult> SendAsync(IPAddress address, int ttl, int bufferSize,
            int timeoutMs, CancellationToken cancellationToken, bool dontFragment = false)
        {
            await Task.Delay(100, cancellationToken);
            return new ProbeResult(ProbeOutcome.Success, address, 1, bufferSize, ttl);
        }
    }
}
