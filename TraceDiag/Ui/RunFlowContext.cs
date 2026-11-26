using System.Net;
using TraceDiag.Models;
using TraceDiag.Services;

namespace TraceDiag.Ui;

internal sealed class RunFlowContext(
    CancellationToken token,
    FileDiagnosticLogger logger,
    Label status,
    Func<RunInput?> readInput,
    Action beginRun,
    Action<string> appendLine,
    Action<IReadOnlyList<HopStatSnapshot>> applyStats,
    Func<string, CancellationToken, Task<IPAddress?>> resolveHost,
    Func<DiagnosticInput, IProgress<string>, IProgress<IReadOnlyList<HopStatSnapshot>>, CancellationToken, Task> executeService,
    Action endRun)
{
    internal RunFlowContext(MainForm form, CancellationToken token)
        : this(
            token, form.Logger, form.StatusLabel, form.ReadRunInput, form.BeginRun,
            form.AppendOutputLine, form.ApplyStats, HostResolver.ResolveAsync,
            form.ExecuteServiceAsync, form.EndRun)
    {
    }

    internal CancellationToken Token { get; } = token;
    internal FileDiagnosticLogger Logger { get; } = logger;
    internal Label Status { get; } = status;
    internal Func<RunInput?> ReadInput { get; } = readInput;
    internal Action BeginRun { get; } = beginRun;
    internal Action<string> AppendLine { get; } = appendLine;
    internal Action<IReadOnlyList<HopStatSnapshot>> ApplyStats { get; } = applyStats;
    internal Func<string, CancellationToken, Task<IPAddress?>> ResolveHost { get; } = resolveHost;
    internal Func<DiagnosticInput, IProgress<string>, IProgress<IReadOnlyList<HopStatSnapshot>>, CancellationToken, Task> ExecuteService { get; } = executeService;
    internal Action EndRun { get; } = endRun;
}
