using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class DiagnosticBatchTests
{
    [Fact]
    public async Task ProbeAllAsync_UsesEveryConfiguredPacketSize()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64, 128, 256], [hop], new HashSet<int>(), 1, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal([64, 128, 256], probe.Calls.Select(c => c.BufferSize).Distinct());
        Assert.Equal(3, hop.Sent);
        Assert.Equal(Strings.Diagnosis.None, hop.Diagnosis);
    }

    [Fact]
    public async Task ProbeAllAsync_KeepsCountersAcrossPacketSizes()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(call => TestFactories.Success(call, 8));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64, 128], [hop], new HashSet<int>(), 2, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(4, hop.Sent);
        Assert.Equal(4, hop.Received);
        Assert.Equal(8, hop.AvgPingMs);
        Assert.Equal(4, hop.RttSamples.Count);
    }

    [Fact]
    public async Task ProbeAllAsync_FinishesAllSizesForHopBeforeNextHop()
    {
        var first = TestFactories.Hop(1, 1);
        var second = TestFactories.Hop(2, 2);
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64, 128], [first, second], new HashSet<int>(), 1, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(
            [64, 128, 64, 128],
            probe.Calls.Select(call => call.BufferSize));
        Assert.Equal(
            [first.Address, first.Address, second.Address, second.Address],
            probe.Calls.Select(call => call.Address));
    }

    [Fact]
    public async Task ProbeAllAsync_AfterTotalLoss_SkipsHopForLaterSizes()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(call => call.BufferSize == 128
            ? TestFactories.Timeout(call)
            : TestFactories.Success(call));
        var progress = TestFactories.TextProgress();
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64, 128, 256], [hop], new HashSet<int>(), 10, 0,
            progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.DoesNotContain(probe.Calls, call => call.BufferSize == 256);
        Assert.Equal(Strings.Status.NoAnswer, hop.Status);
        Assert.Equal(Strings.Diagnosis.PacketSizeLimit, hop.Diagnosis);
        Assert.Contains(progress.Values, value => value.Contains("skipped"));
    }

    [Fact]
    public async Task ProbeAllAsync_WithPartialLossMarksAntiFlood()
    {
        var hop = TestFactories.Hop(1, 1);
        var index = 0;
        var probe = new FakeProbeClient(call => ++index == 1 ? TestFactories.Timeout(call) : TestFactories.Success(call));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64], [hop], new HashSet<int>(), 2, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(Strings.Diagnosis.AntiFlood, hop.Diagnosis);
    }

    [Fact]
    public async Task ProbeAllAsync_WithAlternatingReplySources_MarksEcmpBalancing()
    {
        var hop = TestFactories.Hop(1, 1);
        var index = 0;
        var probe = new FakeProbeClient(call => ++index % 2 == 0
            ? TestFactories.Success(call)
            : TestFactories.SuccessFrom(call, TestFactories.Address(2)));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64], [hop], new HashSet<int>(), 4, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(Strings.Diagnosis.EcmpBalancing, hop.Diagnosis);
    }

    [Fact]
    public async Task ProbeAllAsync_SkipsDiscoveryPlaceholder()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.Status = Strings.Status.NoAnswer;
        hop.IsSkipped = true;
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64], [hop], new HashSet<int>(), 1, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Empty(probe.Calls);
        Assert.Equal(0, hop.Sent);
        Assert.Equal(Strings.Status.NoAnswer, hop.Status);
        Assert.Equal(Strings.Diagnosis.IcmpDisabled, hop.Diagnosis);
    }

    [Fact]
    public async Task ProbeAllAsync_WithDontFragmentAndLargeLoss_MarksMtuBlackHole()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(call => call.BufferSize == 128
            ? TestFactories.Timeout(call)
            : TestFactories.Success(call));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64, 128], [hop], new HashSet<int>(), 1, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None,
            dontFragment: true);
        Assert.Contains(Strings.Diagnosis.MtuBlackHole, hop.DiagnosisReasons);
    }

    [Fact]
    public async Task ProbeAllAsync_WithGapBeforeNextHop_MarksIcmpBlocked()
    {
        var skipped = TestFactories.Hop(1, 1);
        skipped.IsSkipped = true;
        skipped.Diagnosis = Strings.Diagnosis.IcmpDisabled;
        var responding = TestFactories.Hop(2, 2);
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64], [skipped, responding], new HashSet<int> { 1 }, 1, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(Strings.Diagnosis.IcmpBlocked, skipped.Diagnosis);
    }

    [Fact]
    public async Task ProbeAllAsync_WithDifferentRespondersPerSize_MarksRouteAsymmetry()
    {
        var hop = TestFactories.Hop(1, 1);
        var probe = new FakeProbeClient(call => call.BufferSize == 128
            ? TestFactories.SuccessFrom(call, TestFactories.Address(2))
            : TestFactories.Success(call));
        await DiagnosticBatch.ProbeAllAsync(
            TestFactories.Context(probe), [64, 128], [hop], new HashSet<int>(), 1, 0,
            TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Contains(Strings.Diagnosis.RouteAsymmetry, hop.DiagnosisReasons);
    }
}
