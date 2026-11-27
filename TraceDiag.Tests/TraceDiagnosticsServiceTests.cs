using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class TraceDiagnosticsServiceTests
{
    [Fact]
    public async Task RunAsync_WithDynamicSizes_UsesAllPacketSizes()
    {
        var probe = new FakeProbeClient(Reply);
        var progress = TestFactories.TextProgress();
        var stats = TestFactories.StatsProgress();
        await CreateService(probe).RunAsync(CreateInput(true), progress, stats, CancellationToken.None);
        Assert.Equal([64, 128, 256], ProbingCalls(probe.Calls).Select(c => c.BufferSize).Distinct());
        Assert.Contains(progress.Values, value => value.Contains("Packet sizes (x2): 64, 128, 256"));
        Assert.NotEmpty(stats.Values);
    }

    [Fact]
    public async Task RunAsync_WithFixedSize_UsesOnlyMaxPacketSize()
    {
        var probe = new FakeProbeClient(Reply);
        var progress = TestFactories.TextProgress();
        await CreateService(probe).RunAsync(CreateInput(false), progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.All(ProbingCalls(probe.Calls), call => Assert.Equal(256, call.BufferSize));
        Assert.Contains(progress.Values, value => value.Contains("Packet size (fixed): 256 bytes"));
    }

    [Fact]
    public async Task RunAsync_ReportsConfiguredLossDelay()
    {
        var progress = TestFactories.TextProgress();
        var input = new DiagnosticInput(TestFactories.Address(2), "target", 256, 1, 20, false);
        await CreateService(new FakeProbeClient(Reply)).RunAsync(input, progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Contains(progress.Values, value => value.Contains("ICMP loss delay: 20 ms"));
    }

    [Fact]
    public async Task RunAsync_WithNoDiscoveryReplies_MarksAllHopsSkipped()
    {
        var probe = new FakeProbeClient(TestFactories.Timeout);
        var progress = TestFactories.TextProgress();
        var stats = TestFactories.StatsProgress();
        await CreateService(probe).RunAsync(CreateInput(true), progress, stats, CancellationToken.None);
        Assert.Equal(DiagnosticConstants.MaxHops * DiagnosticConstants.DiscoveryRoundLimit, probe.Calls.Count);
        Assert.Contains(progress.Values, value => value.Contains("no response"));
        Assert.Contains(progress.Values, value => value.Contains("round limit"));
        Assert.All(stats.Values.Last(), hop => Assert.Equal(Strings.Status.NoAnswer, hop.Status));
    }

    [Fact]
    public async Task RunAsync_WithNullInput_ThrowsArgumentNullException()
    {
        var service = CreateService(new FakeProbeClient(Reply));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.RunAsync(null!, TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None));
    }

    [Fact]
    public async Task RunAsync_VerificationDoesNotInflateTableCounters()
    {
        var stats = TestFactories.StatsProgress();
        await CreateService(CreateLossOnceProbe()).RunAsync(
            new DiagnosticInput(TestFactories.Address(2), "target", 256, 4, 0, false),
            TestFactories.TextProgress(), stats, CancellationToken.None);
        var target = stats.Values.Last().Single(s => s.Address.Equals(TestFactories.Address(2)));
        Assert.Equal(4, target.Sent);
        Assert.Equal(3, target.Received);
    }

    [Fact]
    public async Task RunAsync_WithDontFragment_CapsPacketSizeAndUsesDfForChecks()
    {
        var probe = new FakeProbeClient(Reply);
        var progress = TestFactories.TextProgress();
        var input = new DiagnosticInput(TestFactories.Address(2), "target", 2048, 1, 0, true, true);
        await CreateService(probe).RunAsync(input, progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal([1024], ProbingCalls(probe.Calls).Select(call => call.BufferSize).Distinct());
        Assert.All(DiscoveryCalls(probe.Calls).Concat(ProbingCalls(probe.Calls)), call => Assert.True(call.DontFragment));
        Assert.Contains(progress.Values, value => value.Contains("Packet size (fixed): 1024 bytes"));
        Assert.Contains(progress.Values, value => value.Contains("Don't Fragment (DF): enabled"));
    }

    [Fact]
    public async Task RunAsync_WithoutDontFragment_StillPrechecksWithDf()
    {
        var probe = new FakeProbeClient(Reply);
        await CreateService(probe).RunAsync(CreateInput(false), TestFactories.TextProgress(),
            TestFactories.StatsProgress(), CancellationToken.None);
        Assert.All(PrecheckCalls(probe.Calls), call => Assert.True(call.DontFragment));
        Assert.All(DiscoveryCalls(probe.Calls).Concat(ProbingCalls(probe.Calls)), call => Assert.False(call.DontFragment));
    }

    private static TraceDiagnosticsService CreateService(FakeProbeClient probe) =>
        new(probe, new FakeDnsResolver(address => "host-" + address), new FakeDiagnosticLogger());

    private static DiagnosticInput CreateInput(bool dynamic) =>
        new(TestFactories.Address(2), "target", 256, 1, 0, dynamic);

    private static int PrecheckCount => 2 * ProbeLimits.PrecheckPacketSizes.Count;

    private static IReadOnlyList<ProbeCall> DirectCalls(List<ProbeCall> calls) =>
        calls.Where(call => call.Ttl == DiagnosticConstants.DirectProbeTtl).ToList();

    private static IReadOnlyList<ProbeCall> PrecheckCalls(List<ProbeCall> calls) =>
        DirectCalls(calls).Take(PrecheckCount).ToList();

    private static IReadOnlyList<ProbeCall> ProbingCalls(List<ProbeCall> calls) =>
        DirectCalls(calls).Skip(PrecheckCount).ToList();

    private static IReadOnlyList<ProbeCall> DiscoveryCalls(List<ProbeCall> calls) =>
        calls.Where(call => call.Ttl != DiagnosticConstants.DirectProbeTtl).ToList();

    private static FakeProbeClient CreateLossOnceProbe()
    {
        var targetCalls = 0;
        return new FakeProbeClient(call =>
        {
            if (call.Ttl != DiagnosticConstants.DirectProbeTtl ||
                !call.Address.Equals(TestFactories.Address(2)))
                return TestFactories.Success(call, 9);
            targetCalls++;
            return targetCalls == ProbeLimits.PrecheckPacketSizes.Count + 1
                ? TestFactories.Timeout(call)
                : TestFactories.Success(call, 9);
        });
    }

    private static ProbeResult Reply(ProbeCall call) => call.Ttl == 1
        ? TestFactories.ExpiredFrom(call, TestFactories.Address(1))
        : TestFactories.Success(call, 9);
}
