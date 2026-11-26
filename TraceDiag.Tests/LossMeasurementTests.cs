using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class LossMeasurementTests
{
    [Fact]
    public async Task MeasureAsync_CalculatesLossSample()
    {
        var hop = TestFactories.Hop(1, 1);
        var index = 0;
        var probe = new FakeProbeClient(call => ++index % 2 == 0
            ? TestFactories.Timeout(call)
            : TestFactories.Success(call, 6));
        var sample = await LossMeasurement.MeasureAsync(
            TestFactories.Context(probe), hop, [hop], 64, 4, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(4, sample.Sent);
        Assert.Equal(2, sample.Lost);
        Assert.Equal(50, sample.Percent);
    }

    [Fact]
    public async Task MeasureAsync_WithNoLoss_ReturnsZeroPercent()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        var sample = await LossMeasurement.MeasureAsync(
            TestFactories.Context(probe), hop, [hop], 128, 2, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(0, sample.Lost);
        Assert.Equal(0, sample.Percent);
    }

    [Fact]
    public async Task MeasureAsync_AfterLoss_UsesDelayForVerificationHop()
    {
        var hop = TestFactories.Hop(1, 1);
        var index = 0;
        var probe = new FakeProbeClient(call => ++index == 1 ? TestFactories.Timeout(call) : TestFactories.Success(call));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await LossMeasurement.MeasureAsync(
            TestFactories.Context(probe), hop, [hop], 64, 2, 60,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.True(stopwatch.ElapsedMilliseconds >= 45);
    }

    [Fact]
    public void VerificationHop_CopiesLossDelayFlag()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(false, 0);
        Assert.True(VerificationHop.Create(hop).LossDelayEnabled);
    }

    [Fact]
    public async Task MeasureAsync_DoesNotChangeMainHopCounters()
    {
        var hop = TestFactories.Hop(1, 1);
        for (var i = 0; i < 100; i++) hop.ApplyPing(true, 5);
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        var sample = await LossMeasurement.MeasureAsync(
            TestFactories.Context(probe), hop, [hop], 64, 10, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(100, hop.Sent);
        Assert.Equal(100, hop.Received);
        Assert.Equal(10, sample.Sent);
    }
}
