using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Fills missing host names with RIPE registration owners.</summary>
internal static class RipeOwnerController
{
    private const int MaxParallel = 6;

    internal static async Task ApplyAsync(DiagnosticContext context, IReadOnlyList<HopInfo> hops,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        if (!context.RipeLookupEnabled || context.RipeOwner is null) return;
        var targets = hops.Where(h => NeedsOwner(h)).ToArray();
        using var gate = new SemaphoreSlim(MaxParallel);
        var tasks = targets.Select(h => ApplyOneAsync(context, h, gate, cancellationToken));
        await Task.WhenAll(tasks);
        DiagnosticReporter.ReportStats(stats, hops);
    }

    private static bool NeedsOwner(HopInfo hop) =>
        NetworkAddressPolicy.IsProbeTarget(hop.Address) && string.IsNullOrWhiteSpace(hop.HostName);

    private static async Task ApplyOneAsync(DiagnosticContext context, HopInfo hop,
        SemaphoreSlim gate, CancellationToken token)
    {
        await gate.WaitAsync(token);
        try
        {
            var owner = await context.RipeOwner!.ResolveOwnerAsync(hop.Address, token);
            if (!string.IsNullOrWhiteSpace(owner)) hop.HostName = owner;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { ReportFailure(context, hop, ex); }
        finally { gate.Release(); }
    }

    private static void ReportFailure(DiagnosticContext context, HopInfo hop, Exception ex)
    {
        if (context.Logger is not null)
            context.Logger.WarnFmt(Strings.Diag.RipeLookupFailedFormat, hop.Address, ex.Message);
    }
}
