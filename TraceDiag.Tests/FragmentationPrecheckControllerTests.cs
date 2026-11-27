using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class FragmentationPrecheckControllerTests
{
    [Fact]
    public async Task RunAsync_SendsEverySizeOnceWithDontFragment()
    {
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        var hops = new[] { TestFactories.Hop(1, 1), TestFactories.Hop(2, 2) };
        var progress = TestFactories.TextProgress();
        await CreateController().RunAsync(TestFactories.Context(probe), hops, progress, CancellationToken.None);
        Assert.Equal(10, probe.Calls.Count);
        Assert.All(probe.Calls, call => Assert.True(call.DontFragment));
        Assert.Equal([64, 128, 256, 1024, 1400, 64, 128, 256, 1024, 1400],
            probe.Calls.Select(call => call.BufferSize));
        Assert.All(hops, hop => Assert.Equal(0, hop.Sent));
        Assert.Contains(progress.Values, value => value.Contains("df-precheck hop  1"));
    }

    [Fact]
    public async Task RunAsync_ReportsFailuresAsWarnings()
    {
        var probe = new FakeProbeClient(call => TestFactories.Timeout(call));
        var logger = new FakeDiagnosticLogger();
        var progress = TestFactories.TextProgress();
        var hops = new[] { TestFactories.Hop(1, 1) };
        await CreateController().RunAsync(TestFactories.Context(probe, logger), hops,
            progress, CancellationToken.None);
        Assert.Equal(5, logger.Warnings.Count);
        Assert.Contains(progress.Values, value => value.Contains("нет ответа"));
    }

    private static FragmentationPrecheckController CreateController() =>
        new(new ProbeJobFactory(), new ProbeJobRunner());
}
