using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Ui;

internal static class RunFlow
{
    internal static async Task StartAsync(RunFlowContext context)
    {
        try
        {
            if (!TryBegin(context, out var input)) return;
            await RunWithCancellationAsync(context, input!);
        }
        finally { context.EndRun(); }
    }

    private static bool TryBegin(RunFlowContext context, out RunInput? input)
    {
        input = context.ReadInput();
        if (input is not null) context.BeginRun();
        return input is not null;
    }

    private static async Task RunWithCancellationAsync(RunFlowContext context, RunInput input)
    {
        try { await ExecuteAsync(context, input, context.Token); }
        catch (Exception ex) { RunErrorPresenter.Present(ex, context.Status, context.AppendLine, context.Logger); }
    }

    private static async Task ExecuteAsync(RunFlowContext context, RunInput input, CancellationToken token)
    {
        LogRequest(context, input);
        var address = await context.ResolveHost(input.Host, token);
        if (address is null) { ReportUnresolved(context, input); return; }
        await RunServiceAsync(context, input, address, token);
    }

    private static void LogRequest(RunFlowContext context, RunInput input)
    {
        context.Logger.Info(TextFormatter.Fmt(
            Strings.Log.RunRequestedFormat,
            input.Host,
            input.MaxPacketSize,
            input.Attempts,
            input.DynamicPacketSize));
    }

    private static void ReportUnresolved(RunFlowContext context, RunInput input)
    {
        var message = TextFormatter.Fmt(Strings.Log.FailedToResolveFormat, input.Host);
        context.Logger.Warn(message);
        context.AppendLine(TextFormatter.Fmt(Strings.Log.ConsoleFailedResolveFormat, TextFormatter.Now(), message));
        context.Status.Text = Strings.Ui.ResolveError;
    }

    private static async Task RunServiceAsync(RunFlowContext context, RunInput input, IPAddress address, CancellationToken token)
    {
        context.Status.Text = Strings.Ui.Running;
        var text = new Progress<string>(context.AppendLine);
        var stats = new Progress<IReadOnlyList<HopStatSnapshot>>(context.ApplyStats);
        await context.ExecuteService(CreateDiagnosticInput(input, address), text, stats, token);
        context.Status.Text = Strings.Ui.Completed;
        context.Logger.Info(Strings.Log.RunCompleted);
    }

    private static DiagnosticInput CreateDiagnosticInput(RunInput input, IPAddress address)
    {
        var hostLabel = input.Host.Equals(address.ToString(), StringComparison.OrdinalIgnoreCase)
            ? input.Host
            : TextFormatter.Fmt(Strings.Ui.HostLabelFormat, input.Host, address);
        return new DiagnosticInput(address, hostLabel, input.MaxPacketSize, input.Attempts, input.DynamicPacketSize);
    }
}
