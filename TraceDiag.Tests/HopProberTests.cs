using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class HopProberTests
{
    [Fact]
    public async Task ProbeAsync_WithReplies_UpdatesExactStats()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(call => TestFactories.Success(call, 10));
        var progress = TestFactories.TextProgress();
        var stopped = await HopProber.ProbeAsync(
            TestFactories.Context(probe), hop, [hop], new ProbeSettings(64, 3, 0),
            progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.False(stopped);
        Assert.Equal(3, probe.Calls.Count);
        Assert.Equal(3, hop.Received);
        Assert.Equal(10, hop.AvgPingMs);
        Assert.Equal(Strings.Status.Normal, hop.Status);
    }

    [Fact]
    public async Task ProbeAsync_WithReplies_DoesNotApplyLossDelay()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await HopProber.ProbeAsync(
            TestFactories.Context(probe), hop, [hop], new ProbeSettings(64, 2, 500),
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.True(stopwatch.ElapsedMilliseconds < 200);
    }

    [Fact]
    public async Task ProbeAsync_AfterLoss_DelaysLaterAttemptsForSameHop()
    {
        var hop = TestFactories.Hop(1, 1);
        var index = 0;
        var probe = new FakeProbeClient(call => ++index == 1 ? TestFactories.Timeout(call) : TestFactories.Success(call));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await HopProber.ProbeAsync(
            TestFactories.Context(probe), hop, [hop], new ProbeSettings(64, 2, 60),
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.True(stopwatch.ElapsedMilliseconds >= 45);
        Assert.True(hop.LossDelayEnabled);
    }

    [Fact]
    public async Task ProbeAsync_WithAllLoss_StopsAfterTenAttempts()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(TestFactories.Timeout);
        var logger = new FakeDiagnosticLogger();
        var stopped = await HopProber.ProbeAsync(
            TestFactories.Context(probe, logger), hop, [hop], new ProbeSettings(128, 20, 0),
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.True(stopped);
        Assert.Equal(DiagnosticConstants.MaxLostPingsBeforeSkip, probe.Calls.Count);
        Assert.Equal(0, hop.Received);
        Assert.Equal(Strings.Status.NoAnswer, hop.Status);
        Assert.NotEmpty(logger.Warnings);
    }

    [Fact]
    public async Task ProbeAsync_WithPartialLoss_SetsLossStatus()
    {
        var hop = TestFactories.Hop(1, 1);
        var index = 0;
        var probe = new FakeProbeClient(call => ++index % 2 == 0 ? TestFactories.Timeout(call) : TestFactories.Success(call, 5));
        await HopProber.ProbeAsync(
            TestFactories.Context(probe), hop, [hop], new ProbeSettings(64, 4, 0),
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(4, hop.Sent);
        Assert.Equal(2, hop.Received);
        Assert.Equal(TextFormatter.Fmt(Strings.Status.PacketLossFormat, 50), hop.Status);
    }
}
