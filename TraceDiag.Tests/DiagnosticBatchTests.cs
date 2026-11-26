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
        Assert.Equal(1, hop.Sent);
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
        Assert.Contains(progress.Values, value => value.Contains("пропуск"));
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
    }
}
