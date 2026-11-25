using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class LossAnalyzerTests
{
    [Fact]
    public async Task AnalyzeAsync_WhenTargetReplies_SetsRespondsStatus()
    {
        var target = TestFactories.Hop(2, 2);
        target.ApplyPing(true, 5);
        var hops = new List<HopInfo> { TestFactories.Hop(1, 1), target };
        var progress = TestFactories.TextProgress();
        await AnalyzeAsync(hops, 2, progress, AlwaysSuccess());
        Assert.Equal(Strings.Status.Responds, target.Status);
        Assert.Contains(progress.Values, value => value.Contains("remote host responds"));
    }

    [Fact]
    public async Task AnalyzeAsync_WhenTargetSilent_SetsNoAnswerStatus()
    {
        var target = TestFactories.Hop(2, 2);
        var hops = new List<HopInfo> { TestFactories.Hop(1, 1), target };
        await AnalyzeAsync(hops, 2, TestFactories.TextProgress(), AlwaysSuccess());
        Assert.Equal(Strings.Status.NoAnswer, target.Status);
    }

    [Fact]
    public async Task AnalyzeAsync_WithFirstHopLoss_SetsFirstHopStatus()
    {
        var first = TestFactories.Hop(1, 1);
        first.ApplyPing(true, 5);
        first.ApplyPing(false, 0);
        var target = TestFactories.Hop(2, 2);
        target.ApplyPing(true, 6);
        var hops = new List<HopInfo> { first, target };
        await AnalyzeAsync(hops, 2, TestFactories.TextProgress(), AlwaysSuccess());
        Assert.Equal(Strings.Status.FirstHopLoss, first.Status);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenOnlyCurrentHopLoses_SetsDamagedStatus()
    {
        var previous = TestFactories.Hop(1, 1);
        previous.ApplyPing(true, 5);
        var current = HopWithLoss(2);
        var target = TestFactories.Hop(3, 3);
        target.ApplyPing(true, 7);
        var probe = new FakeProbeClient(call => call.Address.Equals(TestFactories.Address(2))
            ? TestFactories.Timeout(call)
            : TestFactories.Success(call));
        await AnalyzeAsync([previous, current, target], 3, TestFactories.TextProgress(), probe);
        Assert.Equal(Strings.Status.DamagedLoss, current.Status);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenLossNotReproduced_SetsTransientStatus()
    {
        var previous = TestFactories.Hop(1, 1);
        previous.ApplyPing(true, 5);
        var current = HopWithLoss(2);
        var target = TestFactories.Hop(3, 3);
        target.ApplyPing(true, 7);
        await AnalyzeAsync([previous, current, target], 3, TestFactories.TextProgress(), AlwaysSuccess());
        Assert.Equal(Strings.Status.LossNotConfirmed, current.Status);
    }

    private static HopInfo HopWithLoss(int address)
    {
        var hop = TestFactories.Hop(address - 1, address);
        hop.ApplyPing(true, 6);
        hop.ApplyPing(false, 0);
        return hop;
    }

    private static FakeProbeClient AlwaysSuccess() => new(call => TestFactories.Success(call));

    private static Task AnalyzeAsync(List<HopInfo> hops, int target, CollectingProgress<string> progress, FakeProbeClient probe) =>
        LossAnalyzer.AnalyzeAsync(
            TestFactories.Context(probe), TestFactories.Address(target), hops, 64, 2,
            progress, TestFactories.StatsProgress(), CancellationToken.None);
}
