using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class ProbeExecutorTests
{
    [Fact]
    public async Task SendAsync_ReturnsClientResult()
    {
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        var context = TestFactories.Context(probe);
        var request = CreateRequest();
        var result = await ProbeExecutor.SendAsync(context, request, TestFactories.TextProgress(), CancellationToken.None);
        Assert.Equal(ProbeOutcome.Success, result.Outcome);
        Assert.Single(probe.Calls);
    }

    [Fact]
    public async Task SendAsync_WhenClientThrows_ReturnsFailed()
    {
        var probe = new FakeProbeClient(_ => throw new InvalidOperationException("boom"));
        var logger = new FakeDiagnosticLogger();
        var context = TestFactories.Context(probe, logger);
        var result = await ProbeExecutor.SendAsync(context, CreateRequest(), TestFactories.TextProgress(), CancellationToken.None);
        Assert.Equal(ProbeOutcome.Failed, result.Outcome);
        Assert.NotEmpty(logger.Errors);
    }

    [Fact]
    public async Task SendAsync_WhenCancelled_PropagatesCancellation()
    {
        var probe = new FakeProbeClient(_ => throw new OperationCanceledException());
        var context = TestFactories.Context(probe);
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            ProbeExecutor.SendAsync(context, CreateRequest(), TestFactories.TextProgress(), CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenClientHangs_ReturnsTimedOut()
    {
        var context = new DiagnosticContext(new HangingProbeClient(), new FakeDnsResolver(_ => null), null);
        var request = new ProbeRequest(TestFactories.Address(1), 64, 64, 1, "hang");
        var result = await ProbeExecutor.SendAsync(context, request, TestFactories.TextProgress(), CancellationToken.None);
        Assert.Equal(ProbeOutcome.TimedOut, result.Outcome);
    }

    private static ProbeRequest CreateRequest() =>
        new(TestFactories.Address(1), 64, 128, 100, "test");

    private sealed class HangingProbeClient : IIcmpProbeClient
    {
        public Task<ProbeResult> SendAsync(IPAddress address, int ttl, int bufferSize, int timeoutMs, CancellationToken cancellationToken, bool dontFragment = false) =>
            new TaskCompletionSource<ProbeResult>().Task;
    }
}
