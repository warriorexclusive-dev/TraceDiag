using System.Net;
using System.Windows.Forms;
using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Tests.TestData;
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

    [Fact]
    public async Task StartAsync_PassesReplyDelayToDiagnosticInput()
    {
        using var directory = new TempDirectory();
        using var status = new Label();
        DiagnosticInput? captured = null;
        var input = new RunInput("10.0.0.1", 64, 1, 250, false);
        var context = new RunFlowContext(
            CancellationToken.None, new FileDiagnosticLogger(directory.File("run.log")), status,
            () => input, () => { }, _ => { }, _ => { }, ResolveLocalhost,
            (diagnostic, _, _, _) => { captured = diagnostic; return Task.CompletedTask; }, () => { });
        await RunFlow.StartAsync(context);
        Assert.Equal(250, captured?.DelayMs);
    }

    private static Task<IPAddress?> ResolveNothing(string host, CancellationToken token) =>
        Task.FromResult<IPAddress?>(null);

    private static Task<IPAddress?> ResolveLocalhost(string host, CancellationToken token) =>
        Task.FromResult<IPAddress?>(IPAddress.Parse(host));

    private static Task ExecuteNothing(
        DiagnosticInput input,
        IProgress<string> text,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken token) => Task.CompletedTask;
}
