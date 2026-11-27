using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class AlternateRouteDetectorTests
{
    [Fact]
    public async Task DetectAsync_KeepsOnlyUniquePathsAndFreezesNumbers()
    {
        var probe = new FakeProbeClient(call =>
        {
            var first = call.BufferSize == 64 ? TestFactories.Address(10) : TestFactories.Address(11);
            if (call.Ttl == 1) return TestFactories.ExpiredFrom(call, first);
            return TestFactories.Success(call);
        });
        var context = TestFactories.Context(probe);
        var registry = new RouteRegistry();
        var detector = new AlternateRouteDetector();
        var settings = RoutePrecheckSettings.Create(true, false, 3000);

        var first = await detector.DetectAsync(context, "example.com", [Target(2), Target(3)],
            settings, registry, CancellationToken.None);
        var second = await detector.DetectAsync(context, "example.com", [Target(2), Target(3)],
            settings, registry, CancellationToken.None);

        Assert.False(first.Fallback);
        Assert.True(first.Frozen);
        Assert.Equal(2, first.Hosts.Count);
        Assert.All(first.Hosts, host => Assert.Equal(2, host.Routes.Count));
        Assert.Equal(first.AllRoutes().Select(item => item.Route.Uid),
            second.AllRoutes().Select(item => item.Route.Uid));
        Assert.Equal(8, first.AllRoutes().Sum(item => item.Route.Samples.Count));
        Assert.Equal([0, 1, 2, 3], first.Hosts[0].Routes
            .SelectMany(route => route.Flows).OrderBy(flow => flow));
    }

    [Fact]
    public async Task DetectAsync_MarksMissingTtlWithAsterisk()
    {
        var probe = new FakeProbeClient(TestFactories.Timeout);
        var detector = new AlternateRouteDetector();
        var settings = RoutePrecheckSettings.Create(true, false, 3000);

        var tree = await detector.DetectAsync(TestFactories.Context(probe), "example.com",
            [Target(2)], settings, new RouteRegistry(), CancellationToken.None);

        Assert.True(tree.Partial);
        Assert.Contains(Strings.Diag.RouteMiss, tree.Hosts[0].Routes[0].Key);
    }

    [Fact]
    public void CreateFallback_UsesOriginalAlgorithmWithoutPrecheck()
    {
        var tree = new AlternateRouteDetector().CreateFallback("example.com",
            [Target(2), Target(3)], new RouteRegistry());

        Assert.True(tree.Fallback);
        Assert.True(tree.Partial);
        Assert.Equal(2, tree.RouteCount);
        Assert.All(tree.AllRoutes(), item => Assert.Single(item.Route.Flows));
    }

    private static DnsAnswer Target(int last) =>
        new(TestFactories.Address(last), "resolver", false);
}
