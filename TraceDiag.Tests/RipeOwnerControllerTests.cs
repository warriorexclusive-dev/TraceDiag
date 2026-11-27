using System.Net.Sockets;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class RipeOwnerControllerTests
{
    [Fact]
    public async Task ApplyAsync_FillsMissingHostNameWithOwner()
    {
        var ripe = new FakeRipeOwnerService(_ => "Example Network");
        var context = CreateContext(ripe, true);
        var hops = new[] { TestFactories.Hop(1, 1) };
        await RipeOwnerController.ApplyAsync(context, hops, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal("Example Network", hops[0].HostName);
        Assert.Single(ripe.Calls);
    }

    [Fact]
    public async Task ApplyAsync_SkipsHopsWithHostName()
    {
        var ripe = new FakeRipeOwnerService(_ => "Example Network");
        var context = CreateContext(ripe, true);
        var hops = new[] { TestFactories.Hop(1, 1, "router") };
        await RipeOwnerController.ApplyAsync(context, hops, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Empty(ripe.Calls);
    }

    [Fact]
    public async Task ApplyAsync_WhenDisabled_DoesNotQuery()
    {
        var ripe = new FakeRipeOwnerService(_ => "Example Network");
        var context = CreateContext(ripe, false);
        var hops = new[] { TestFactories.Hop(1, 1) };
        await RipeOwnerController.ApplyAsync(context, hops, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Empty(ripe.Calls);
    }

    [Fact]
    public async Task ApplyAsync_OnFailure_KeepsHostNameEmpty()
    {
        var ripe = new FakeRipeOwnerService(_ => throw new SocketException());
        var context = CreateContext(ripe, true);
        var hops = new[] { TestFactories.Hop(1, 1) };
        await RipeOwnerController.ApplyAsync(context, hops, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Null(hops[0].HostName);
    }

    private static DiagnosticContext CreateContext(FakeRipeOwnerService ripe, bool enabled) =>
        new(new FakeProbeClient(call => TestFactories.Success(call)),
            new FakeDnsResolver(_ => null), new FakeDiagnosticLogger(), enabled, ripe);
}
