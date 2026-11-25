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
            TestFactories.Context(probe), [64, 128, 256], [hop], 1,
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
            TestFactories.Context(probe), [64, 128, 256], [hop], 10,
            progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.DoesNotContain(probe.Calls, call => call.BufferSize == 256);
        Assert.Equal(Strings.Status.NoAnswer, hop.Status);
        Assert.Contains(progress.Values, value => value.Contains("пропуск"));
    }
}
