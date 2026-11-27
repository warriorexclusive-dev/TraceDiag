using System.Net;
using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Services.Monitoring;

namespace TraceDiag.Console;

/// <summary>Maps CLI options onto the same services used by the WinForms host.</summary>
internal sealed class ConsoleRunner(Invocation options, OutputWriter writer, ConsoleLogger logger)
{
    internal async Task<int> RunAsync(CancellationToken token)
    {
        try
        {
            var address = await HostResolver.ResolveAsync(options.Host, token);
            if (address is null) return ReportUnresolved();
            return options.Command == "monitor"
                ? await MonitorAsync(address, token)
                : await FiniteAsync(address, token);
        }
        catch (OperationCanceledException) { logger.Info(Strings.Console.Cancelled); return 130; }
        catch (Exception ex) { logger.Error(Strings.Diag.RunFailed, ex); return 1; }
        finally
        {
            logger.Flush();
            writer.WriteSummary();
        }
    }

    private async Task<int> FiniteAsync(IPAddress address, CancellationToken token)
    {
        LogRequest();
        var input = new DiagnosticInput(address, Label(address), options.PacketSize, options.Attempts,
            options.LossDelayMs, options.Dynamic, options.DontFragment);
        var service = new TraceDiagnosticsService(new IcmpProbeClient(), new DnsResolverService(), logger);
        var stats = new SyncProgress<IReadOnlyList<HopStatSnapshot>>(writer.Stats);
        await service.RunAsync(input, writer, stats, token);
        logger.Info(Strings.Log.RunCompleted);
        return 0;
    }

    private async Task<int> MonitorAsync(IPAddress address, CancellationToken token)
    {
        logger.Info(TextFormatter.Fmt(Strings.Log.MonitorRequestedFormat, options.Host,
            options.PacketSize, options.LossDelayMs, options.DontFragment));
        var input = new MonitoringInput(address, Label(address), options.PacketSize,
            options.LossDelayMs, options.DontFragment);
        var service = new MonitoringService(new IcmpProbeClient(), new DnsResolverService(), logger);
        var stats = new SyncProgress<HopStatSnapshot>(writer.Stats);
        await service.RunAsync(input, writer, stats, token);
        logger.Info(Strings.Log.MonitorCompleted);
        return 0;
    }

    private void LogRequest() =>
        logger.Info(TextFormatter.Fmt(Strings.Log.RunRequestedFormat, options.Host,
            options.PacketSize, options.Attempts, options.Dynamic, options.LossDelayMs, options.DontFragment));

    private string Label(IPAddress address) =>
        options.Host.Equals(address.ToString(), StringComparison.OrdinalIgnoreCase)
            ? options.Host
            : TextFormatter.Fmt(Strings.Ui.HostLabelFormat, options.Host, address);

    private int ReportUnresolved()
    {
        var message = TextFormatter.Fmt(Strings.Log.FailedToResolveFormat, options.Host);
        logger.Warn(message);
        return 1;
    }
}
