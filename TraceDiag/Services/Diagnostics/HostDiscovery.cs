using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

internal static class HostDiscovery
{
    internal static async Task<HostDiscoveryResult> DiscoverAsync(DiagnosticContext context, IPAddress target,
        DiscoverySettings settings, IProbeJobFactory factory, IProbeJobRunner runner,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken)
    {
        DiagnosticReporter.Report(context, progress, Strings.Diag.TracingRoute);
        var trigger = ProbeTriggers.Create(settings.DontFragment);
        var state = new DiscoveryState();
        await RunRoundsAsync(context, state, target, settings, trigger, factory, runner, progress, stats, cancellationToken);
        ReportLimit(context, state, settings, progress);
        return new HostDiscoveryResult(state.Hops, state.Skipped, state.Reached);
    }

    private static async Task RunRoundsAsync(DiagnosticContext context, DiscoveryState state,
        IPAddress target, DiscoverySettings settings, IProbeTrigger trigger,
        IProbeJobFactory factory, IProbeJobRunner runner, IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var round = 0;
        while (!state.Reached && !IsLimitReached(state, settings, round))
        {
            cancellationToken.ThrowIfCancellationRequested();
            round++;
            await SweepAsync(context, state, target, settings, trigger, factory, runner, round, progress, stats, cancellationToken);
            if (state.Reached || IsLimitReached(state, settings, round)) break;
        }
    }

    private static async Task SweepAsync(DiagnosticContext context, DiscoveryState state,
        IPAddress target, DiscoverySettings settings, IProbeTrigger trigger,
        IProbeJobFactory factory, IProbeJobRunner runner, int round,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken)
    {
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.DiscoveryRoundFormat, round));
        for (var ttl = 1; ttl <= DiagnosticConstants.MaxHops && !state.Reached; ttl++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!state.ShouldProbe(ttl)) continue;
            await ProbeTtlAsync(context, state, target, settings, trigger, factory, runner, ttl, round, progress, stats, cancellationToken);
        }
    }

    private static bool IsLimitReached(DiscoveryState state, DiscoverySettings settings, int round) =>
        !state.Reached && settings.RoundLimit > 0 && round >= settings.RoundLimit;

    private static async Task ProbeTtlAsync(DiagnosticContext context, DiscoveryState state, IPAddress target,
        DiscoverySettings settings, IProbeTrigger trigger, IProbeJobFactory factory, IProbeJobRunner runner,
        int ttl, int round, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken)
    {
        var job = CreateJob(target, ttl, round, settings, trigger, factory);
        var result = await runner.RunAsync(context, job, progress, cancellationToken);
        if (!IsHopReply(result.Outcome))
        {
            ReportNoResponse(context, state, ttl, progress, stats);
            await ProbeDelay.ApplyAsync(true, settings.LossDelayMs, cancellationToken);
            return;
        }
        ApplyResolved(context, state, target, result, ttl, progress, stats);
    }

    private static ProbeJob CreateJob(IPAddress target, int ttl, int round, DiscoverySettings settings,
        IProbeTrigger trigger, IProbeJobFactory factory) =>
        factory.Create(trigger, target, ttl, settings.PacketSize, settings.TimeoutMs,
            TextFormatter.Fmt(Strings.Diag.TraceRoundContextFormat, ttl, round));

    private static bool IsHopReply(ProbeOutcome outcome) =>
        outcome is ProbeOutcome.Success or ProbeOutcome.TtlExpired;

    private static void ReportNoResponse(DiagnosticContext context, DiscoveryState state, int ttl,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        var added = state.MarkNoResponse(ttl);
        DiagnosticReporter.ReportStats(stats, state.Hops);
        if (added) DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.NoResponseHopFormat, ttl));
    }

    private static void ApplyResolved(DiagnosticContext context, DiscoveryState state, IPAddress target,
        ProbeResult result, int ttl, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        var hop = CreateHop(ttl, result.Address!, target);
        if (state.MarkResolved(ttl, hop)) ReportDiscovered(context, target, hop, progress);
        DiagnosticReporter.ReportStats(stats, state.Hops);
        if (IsReached(result, hop, target)) state.MarkReached();
    }

    private static HopInfo CreateHop(int ttl, IPAddress address, IPAddress target) => new()
    {
        Ttl = ttl,
        Address = address,
        Status = address.Equals(target) ? Strings.Status.Target : Strings.Status.Pending
    };

    private static bool IsReached(ProbeResult result, HopInfo hop, IPAddress target) =>
        result.Outcome == ProbeOutcome.Success || hop.Address.Equals(target);

    private static void ReportDiscovered(DiagnosticContext context, IPAddress target, HopInfo hop, IProgress<string> progress)
    {
        var marker = hop.Address.Equals(target) ? Strings.Diag.TargetMarker : string.Empty;
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.HopDiscoveredFormat, hop.Ttl, hop.Address, marker));
    }

    private static void ReportLimit(DiagnosticContext context, DiscoveryState state, DiscoverySettings settings, IProgress<string> progress)
    {
        if (state.Reached || settings.RoundLimit <= 0) return;
        DiagnosticReporter.Report(context, progress, Strings.Diag.TraceRoundLimit, isWarn: true);
    }
}
