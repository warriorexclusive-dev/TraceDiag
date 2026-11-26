using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class RunSummaryTests
{
    [Fact]
    public void Report_WhenTargetReplies_SetsRespondsStatus()
    {
        var target = TestFactories.Hop(2, 2);
        target.ApplyPing(true, 5);
        var hops = new List<HopInfo> { TestFactories.Hop(1, 1), target };
        var progress = TestFactories.TextProgress();
        RunSummary.Report(TestFactories.Context(AlwaysSuccess()), TestFactories.Address(2), hops, progress);
        Assert.Equal(Strings.Status.Responds, target.Status);
        Assert.Contains(progress.Values, value => value.Contains("remote host responds"));
    }

    [Fact]
    public void Report_WhenTargetSilent_SetsNoAnswerStatus()
    {
        var target = TestFactories.Hop(2, 2);
        var hops = new List<HopInfo> { TestFactories.Hop(1, 1), target };
        RunSummary.Report(TestFactories.Context(AlwaysSuccess()), TestFactories.Address(2), hops, TestFactories.TextProgress());
        Assert.Equal(Strings.Status.NoAnswer, target.Status);
    }

    [Fact]
    public void Report_WithoutLoss_ShowsNoLossMessage()
    {
        var target = TestFactories.Hop(1, 1);
        target.ApplyPing(true, 5);
        var progress = TestFactories.TextProgress();
        RunSummary.Report(TestFactories.Context(AlwaysSuccess()), TestFactories.Address(1), [target], progress);
        Assert.Contains(progress.Values, value => value.Contains("no packet loss"));
    }

    [Fact]
    public void Report_WithLoss_KeepsCalculatedLossStatus()
    {
        var current = HopWithLoss(1);
        var target = TestFactories.Hop(2, 2);
        target.ApplyPing(true, 7);
        RunSummary.Report(TestFactories.Context(AlwaysSuccess()), TestFactories.Address(2), [current, target], TestFactories.TextProgress());
        Assert.Equal(TextFormatter.Fmt(Strings.Status.PacketLossFormat, 50.0), current.Status);
        Assert.Equal(Strings.Diagnosis.None, current.Diagnosis);
    }

    [Fact]
    public void Report_DoesNotSendVerificationPings()
    {
        var probe = AlwaysSuccess();
        var hops = new List<HopInfo> { TestFactories.Hop(1, 1), HopWithLoss(2) };
        RunSummary.Report(TestFactories.Context(probe), TestFactories.Address(2), hops, TestFactories.TextProgress());
        Assert.Empty(probe.Calls);
    }

    private static HopInfo HopWithLoss(int address)
    {
        var hop = TestFactories.Hop(1, address);
        hop.ApplyPing(true, 6);
        hop.ApplyPing(false, 0);
        return hop;
    }

    private static FakeProbeClient AlwaysSuccess() => new(call => TestFactories.Success(call));
}
