using System.Net;
using TraceDiag.Models;
using TraceDiag.Services;

namespace TraceDiag.Ui;

internal sealed class MonitorFlowContext(
    CancellationToken token,
    FileDiagnosticLogger logger,
    Label status,
    Func<MonitorInput?> readInput,
    Action beginMonitor,
    Action<string> appendLine,
    Action<HopStatSnapshot> appendStat,
    Func<string, IpFamily, CancellationToken, Task<IPAddress?>> resolveHost,
    Func<MonitoringInput, IProgress<string>, IProgress<HopStatSnapshot>, CancellationToken, Task> executeMonitor,
    Action endRun)
{
    internal MonitorFlowContext(MainForm form, CancellationToken token)
        : this(
            token, form.Logger, form.StatusLabel, form.ReadMonitorInput, form.BeginMonitor,
            form.AppendOutputLine, form.ApplyMonitorStat, HostResolver.ResolveAsync,
            form.ExecuteMonitorAsync, form.EndRun)
    {
    }

    internal CancellationToken Token { get; } = token;
    internal FileDiagnosticLogger Logger { get; } = logger;
    internal Label Status { get; } = status;
    internal Func<MonitorInput?> ReadInput { get; } = readInput;
    internal Action BeginMonitor { get; } = beginMonitor;
    internal Action<string> AppendLine { get; } = appendLine;
    internal Action<HopStatSnapshot> AppendStat { get; } = appendStat;
    internal Func<string, IpFamily, CancellationToken, Task<IPAddress?>> ResolveHost { get; } = resolveHost;
    internal Func<MonitoringInput, IProgress<string>, IProgress<HopStatSnapshot>, CancellationToken, Task> ExecuteMonitor { get; } = executeMonitor;
    internal Action EndRun { get; } = endRun;
}
