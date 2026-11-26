using System.Net;
using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class RunFlowTests
{
    [Fact]
    public async Task StartAsync_WhenValidationFails_StillEndsRun()
    {
        var ended = false;
        var context = new RunFlowContext(
            CancellationToken.None, null!, null!, () => null, () => { },
            _ => { }, _ => { }, ResolveNothing, ExecuteNothing, () => ended = true);
        await RunFlow.StartAsync(context);
        Assert.True(ended);
    }

    private static Task<IPAddress?> ResolveNothing(string host, CancellationToken token) =>
        Task.FromResult<IPAddress?>(null);

    private static Task ExecuteNothing(
        DiagnosticInput input,
        IProgress<string> text,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken token) => Task.CompletedTask;
}
