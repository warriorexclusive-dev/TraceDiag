using System.Net;
using System.Windows.Forms;
using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Tests.TestData;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class MonitorFlowTests
{
    [Fact]
    public async Task StartAsync_UsesMonitoringContractWithSinglePacketSize()
    {
        using var directory = new TempDirectory();
        using var status = new Label();
        using var logger = new FileDiagnosticLogger(directory.File("monitor.log"));
        MonitoringInput? captured = null;
        var context = CreateContext(logger, status, (input, _, _, _) => { captured = input; return Task.CompletedTask; });
        await MonitorFlow.StartAsync(context);
        Assert.Equal(512, captured?.PacketSize);
        Assert.Equal(250, captured?.LossDelayMs);
        Assert.Equal(Strings.Ui.MonitoringCompleted, status.Text);
    }

    [Fact]
    public async Task StartAsync_PassesDontFragmentToMonitoringContract()
    {
        using var directory = new TempDirectory();
        using var status = new Label();
        using var logger = new FileDiagnosticLogger(directory.File("monitor-df.log"));
        MonitoringInput? captured = null;
        var context = CreateContext(logger, status, true, (input, _, _, _) => { captured = input; return Task.CompletedTask; });
        await MonitorFlow.StartAsync(context);
        Assert.True(captured?.DontFragment);
    }

    [Fact]
    public async Task StartAsync_WhenStopped_SetsCancelledStatus()
    {
        using var directory = new TempDirectory();
        using var status = new Label();
        using var logger = new FileDiagnosticLogger(directory.File("stop-monitor.log"));
        var context = CreateContext(logger, status, CancelExecute);
        await MonitorFlow.StartAsync(context);
        Assert.Equal(Strings.Ui.Cancelled, status.Text);
    }

    private static MonitorFlowContext CreateContext(FileDiagnosticLogger logger, Label status,
        Func<MonitoringInput, IProgress<string>, IProgress<HopStatSnapshot>, CancellationToken, Task> execute) =>
        CreateContext(logger, status, false, execute);

    private static MonitorFlowContext CreateContext(FileDiagnosticLogger logger, Label status, bool dontFragment,
        Func<MonitoringInput, IProgress<string>, IProgress<HopStatSnapshot>, CancellationToken, Task> execute) =>
        new(CancellationToken.None, logger, status,
            () => new MonitorInput("10.0.0.1", 512, 250, dontFragment),
            () => { }, _ => { }, _ => { }, Resolve, execute, () => { });

    private static Task<IPAddress?> Resolve(string host, IpFamily family, CancellationToken token) =>
        Task.FromResult<IPAddress?>(IPAddress.Parse(host));

    private static Task CancelExecute(MonitoringInput input, IProgress<string> text,
        IProgress<HopStatSnapshot> stats, CancellationToken token) => throw new OperationCanceledException();
}
