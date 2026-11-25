using TraceDiag.Models;
using TraceDiag.Services;
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
        Assert.DoesNotContain(probe.Calls, IsProbing);
        Assert.Contains(progress.Values, value => value.Contains("no route hops discovered"));
    }

    [Fact]
    public async Task RunAsync_WithNullInput_ThrowsArgumentNullException()
    {
        var service = CreateService(new FakeProbeClient(Reply));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.RunAsync(null!, TestFactories.TextProgress(), TestFactories.StatsProgress(), CancellationToken.None));
    }

    private static TraceDiagnosticsService CreateService(FakeProbeClient probe) =>
        new(probe, new FakeDnsResolver(address => "host-" + address), new FakeDiagnosticLogger());

    private static DiagnosticInput CreateInput(bool dynamic) =>
        new(TestFactories.Address(2), "target", 256, 1, dynamic);

    private static bool IsProbing(ProbeCall call) => call.Ttl == 64;

    private static ProbeResult Reply(ProbeCall call) => call.Ttl == 1
        ? TestFactories.ExpiredFrom(call, TestFactories.Address(1))
        : TestFactories.Success(call, 9);
}
