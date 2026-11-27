using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Services.Monitoring;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class MonitoringServiceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunAsync_PrechecksAfterDiscoveryThenUsesSelectedTrigger(bool dontFragment)
    {
        using var source = new CancellationTokenSource();
        var probe = new FakeProbeClient(Reply);
        var progress = new CollectingProgress<string>();
        var service = new MonitoringService(probe, new FakeDnsResolver(_ => "host"), new FakeDiagnosticLogger());
        var input = new MonitoringInput(TestFactories.Address(2), "target", 2048, 0, dontFragment,
            RoutePrecheck: false);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(
            input, new CancelOnMonitorProgress(progress, source),
            new CollectingProgress<HopStatSnapshot>(), source.Token));
        Assert.All(PrecheckCalls(probe.Calls), call => Assert.True(call.DontFragment));
        Assert.All(DiscoveryCalls(probe.Calls), call => Assert.Equal(dontFragment, call.DontFragment));
        Assert.Contains(MonitorCalls(probe.Calls), call => call.DontFragment == dontFragment
            && call.BufferSize == (dontFragment ? 1024 : 2048));
        Assert.True(probe.Calls.FindIndex(IsDirect) > probe.Calls.FindLastIndex(IsDiscovery));
        Assert.Contains(progress.Values, value => value.Contains("DF precheck before the main test"));
    }

    private static ProbeResult Reply(ProbeCall call) => call.Ttl == 1
        ? TestFactories.ExpiredFrom(call, TestFactories.Address(1))
        : TestFactories.Success(call);

    private static bool IsDirect(ProbeCall call) => call.Ttl == DiagnosticConstants.DirectProbeTtl;

    private static bool IsDiscovery(ProbeCall call) => !IsDirect(call);

    private static IReadOnlyList<ProbeCall> DirectCalls(List<ProbeCall> calls) =>
        calls.Where(IsDirect).ToList();

    private static IReadOnlyList<ProbeCall> PrecheckCalls(List<ProbeCall> calls) =>
        DirectCalls(calls).Take(2 * ProbeLimits.PrecheckPacketSizes.Count).ToList();

    private static IReadOnlyList<ProbeCall> MonitorCalls(List<ProbeCall> calls) =>
        DirectCalls(calls).Skip(2 * ProbeLimits.PrecheckPacketSizes.Count).ToList();

    private static IReadOnlyList<ProbeCall> DiscoveryCalls(List<ProbeCall> calls) =>
        calls.Where(IsDiscovery).ToList();

    private sealed class CancelOnMonitorProgress(
        IProgress<string> target, CancellationTokenSource source) : IProgress<string>
    {
        public void Report(string value)
        {
            target.Report(value);
            if (value.IndexOf("monitor hop", StringComparison.Ordinal) >= 0) source.Cancel();
        }
    }
}
