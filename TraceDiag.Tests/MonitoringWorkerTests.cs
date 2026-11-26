using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Services.Monitoring;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class MonitoringWorkerTests
{
    [Fact]
    public async Task MonitorHopWorker_UsesFixedSizeAndKeepsProbingAfterSizeLimit()
    {
        using var source = new CancellationTokenSource();
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        var hop = TestFactories.Hop(1, 1);
        hop.Diagnosis = Strings.Diagnosis.PacketSizeLimit;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => MonitorHopWorker.RunAsync(
            TestFactories.Context(probe), hop, 512,
            TestFactories.TextProgress(), new CancelOnStat(source), source.Token));
        Assert.Single(probe.Calls);
        Assert.Equal(512, probe.Calls[0].BufferSize);
        Assert.Equal(DiagnosticConstants.DirectProbeTtl, probe.Calls[0].Ttl);
    }

    [Fact]
    public void MonitorEligibility_ExcludesOnlyIcmpDisabledNodes()
    {
        var hop = TestFactories.Hop(1, 1);
        Assert.True(MonitorEligibility.IsEligible(hop));
        hop.Diagnosis = Strings.Diagnosis.IcmpDisabled;
        Assert.False(MonitorEligibility.IsEligible(hop));
    }

    private sealed class CancelOnStat(CancellationTokenSource source) : IProgress<HopStatSnapshot>
    {
        public void Report(HopStatSnapshot value) => source.Cancel();
    }
}
