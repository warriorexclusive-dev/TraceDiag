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
            TestFactories.Context(probe), hop, [hop], 64, 4,
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
            TestFactories.Context(probe), hop, [hop], 128, 2,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(0, sample.Lost);
        Assert.Equal(0, sample.Percent);
    }

    [Fact]
    public async Task MeasureAsync_DoesNotChangeMainHopCounters()
    {
        var hop = TestFactories.Hop(1, 1);
        for (var i = 0; i < 100; i++) hop.ApplyPing(true, 5);
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        var sample = await LossMeasurement.MeasureAsync(
            TestFactories.Context(probe), hop, [hop], 64, 10,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(100, hop.Sent);
        Assert.Equal(100, hop.Received);
        Assert.Equal(10, sample.Sent);
    }
}
