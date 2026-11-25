using System.Net;
using TraceDiag.Models;
using TraceDiag.Services;

namespace TraceDiag.Ui;

internal sealed class RunFlowContext
{
    internal RunFlowContext(MainForm form, CancellationToken token)
    {
        Token = token;
        Logger = form.Logger;
        Status = form.StatusLabel;
        ReadInput = form.ReadRunInput;
        BeginRun = form.BeginRun;
        AppendLine = form.AppendOutputLine;
        ApplyStats = form.ApplyStats;
        ResolveHost = HostResolver.ResolveAsync;
        ExecuteService = form.ExecuteServiceAsync;
        EndRun = form.EndRun;
    }

    internal CancellationToken Token { get; }
    internal FileDiagnosticLogger Logger { get; }
    internal Label Status { get; }
    internal Func<RunInput?> ReadInput { get; }
    internal Action BeginRun { get; }
    internal Action<string> AppendLine { get; }
    internal Action<IReadOnlyList<HopStatSnapshot>> ApplyStats { get; }
    internal Func<string, CancellationToken, Task<IPAddress?>> ResolveHost { get; }
    internal Func<DiagnosticInput, IProgress<string>, IProgress<IReadOnlyList<HopStatSnapshot>>, CancellationToken, Task> ExecuteService { get; }
    internal Action EndRun { get; }
}
