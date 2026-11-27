using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Ui;

internal static class MonitorFlow
{
    internal static async Task StartAsync(MonitorFlowContext context)
    {
        try
        {
            var input = context.ReadInput();
            if (input is null) return;
            context.BeginMonitor();
            await RunAsync(context, input);
        }
        finally { context.EndRun(); }
    }

    private static async Task RunAsync(MonitorFlowContext context, MonitorInput input)
    {
        try
        {
            LogRequest(context, input);
            var address = await context.ResolveHost(input.Host, input.Family, context.Token);
            if (address is null) { ReportUnresolved(context, input); return; }
            await ExecuteAsync(context, input, address);
        }
        catch (Exception ex) { RunErrorPresenter.Present(ex, context.Status, context.AppendLine, context.Logger); }
    }

    private static async Task ExecuteAsync(MonitorFlowContext context, MonitorInput input, IPAddress address)
    {
        UiText.Set(context.Status, Strings.Ui.Monitoring);
        var text = new Progress<string>(context.AppendLine);
        var stats = new Progress<HopStatSnapshot>(context.AppendStat);
        await context.ExecuteMonitor(CreateInput(input, address), text, stats, context.Token);
        UiText.Set(context.Status, Strings.Ui.MonitoringCompleted);
        context.Logger.Info(Strings.Log.MonitorCompleted);
    }

    private static void LogRequest(MonitorFlowContext context, MonitorInput input) =>
        context.Logger.Info(TextFormatter.Fmt(Strings.Log.MonitorRequestedFormat, input.Host,
            input.PacketSize, input.LossDelayMs, input.DontFragment));

    private static void ReportUnresolved(MonitorFlowContext context, MonitorInput input)
    {
        var message = TextFormatter.Fmt(Strings.Log.FailedToResolveFormat, input.Host);
        context.Logger.Warn(message);
        context.AppendLine(TextFormatter.Fmt(Strings.Log.ConsoleFailedResolveFormat, TextFormatter.Now(), message));
        UiText.Set(context.Status, Strings.Ui.ResolveError);
    }

    private static MonitoringInput CreateInput(MonitorInput input, IPAddress address)
    {
        var label = input.Host.Equals(address.ToString(), StringComparison.OrdinalIgnoreCase)
            ? input.Host
            : TextFormatter.Fmt(Strings.Ui.HostLabelFormat, input.Host, address);
        return new MonitoringInput(address, label, input.PacketSize, input.LossDelayMs,
            input.DontFragment, input.RipeLookup, input.RoutePrecheck, input.DeepRoutes,
            AlternativeDns: input.AlternativeDns);
    }
}
