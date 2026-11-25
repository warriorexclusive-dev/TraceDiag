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
        Assert.Equal([64, 128, 256], probe.Calls.Where(IsProbing).Select(c => c.BufferSize).Distinct());
        Assert.Contains(progress.Values, value => value.Contains("Packet sizes (x2): 64, 128, 256"));
        Assert.NotEmpty(stats.Values);
    }

    [Fact]
    public async Task RunAsync_WithFixedSize_UsesOnlyMaxPacketSize()
    {
        var probe = new FakeProbeClient(Reply);
        var progress = TestFactories.TextProgress();
        await CreateService(probe).RunAsync(CreateInput(false), progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.All(probe.Calls.Where(IsProbing), call => Assert.Equal(256, call.BufferSize));
        Assert.Contains(progress.Values, value => value.Contains("Packet size (fixed): 256 bytes"));
    }

    [Fact]
    public async Task RunAsync_WithoutRouteHops_DoesNotProbeNodes()
    {
        var probe = new FakeProbeClient(TestFactories.Timeout);
        var progress = TestFactories.TextProgress();
        await CreateService(probe).RunAsync(CreateInput(true), progress, TestFactories.StatsProgress(), CancellationToken.None);
        Assert.Equal(DiagnosticConstants.MaxHops * 2, probe.Calls.Count);
        Assert.Contains(progress.Values, value => value.Contains("no route hops discovered"));
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
            new DiagnosticInput(TestFactories.Address(2), "target", 256, 4, false),
            TestFactories.TextProgress(), stats, CancellationToken.None);
        var target = stats.Values.Last().Single(s => s.Address.Equals(TestFactories.Address(2)));
        Assert.Equal(4, target.Sent);
        Assert.Equal(3, target.Received);
    }

    private static TraceDiagnosticsService CreateService(FakeProbeClient probe) =>
        new(probe, new FakeDnsResolver(address => "host-" + address), new FakeDiagnosticLogger());

    private static DiagnosticInput CreateInput(bool dynamic) =>
        new(TestFactories.Address(2), "target", 256, 1, dynamic);

    private static bool IsProbing(ProbeCall call) => call.Ttl == 64;

    private static FakeProbeClient CreateLossOnceProbe()
    {
        var lost = false;
        return new FakeProbeClient(call =>
        {
            if (call.Ttl != DiagnosticConstants.DirectProbeTtl || lost ||
                !call.Address.Equals(TestFactories.Address(2)))
                return TestFactories.Success(call, 9);
            lost = true;
            return TestFactories.Timeout(call);
        });
    }

    private static ProbeResult Reply(ProbeCall call) => call.Ttl == 1
        ? TestFactories.ExpiredFrom(call, TestFactories.Address(1))
        : TestFactories.Success(call, 9);
}
