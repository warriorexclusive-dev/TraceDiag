using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class DiagnosticReporter
{
    internal static void Report(DiagnosticContext context, IProgress<string> progress, string message,
        bool isError = false, bool isWarn = false, Exception? ex = null) =>
        ReportCore(context, progress, Strings.T(message), message, [], isError, isWarn, ex);

    internal static void ReportFormat(DiagnosticContext context, IProgress<string> progress,
        string canonical, params object?[] args) =>
        Structured(context, progress, canonical, args, false, false, null);

    internal static void ReportFormatWarn(DiagnosticContext context, IProgress<string> progress,
        string canonical, params object?[] args) =>
        Structured(context, progress, canonical, args, false, true, null);

    internal static void ReportFormatError(DiagnosticContext context, IProgress<string> progress,
        string canonical, Exception? ex, params object?[] args) =>
        Structured(context, progress, canonical, args, true, false, ex);

    internal static void Separator(DiagnosticContext context, IProgress<string> progress) =>
        Report(context, progress, new string(Strings.Diag.SeparatorChar, Strings.Diag.SeparatorLength));

    internal static void ReportHeader(DiagnosticContext context, IProgress<string> progress,
        DiagnosticInput input, IReadOnlyList<int> sizes)
    {
        ReportFormat(context, progress, Strings.Diag.TargetFormat, input.RemoteHostLabel, input.RemoteAddress);
        ReportFormat(context, progress, Strings.Diag.MaxPacketSizeFormat, input.MaxPacketSize);
        ReportFormat(context, progress, Strings.Diag.AttemptsFormat, input.Attempts);
        ReportFormat(context, progress, Strings.Diag.LossDelayFormat, input.LossDelayMs);
        Report(context, progress, input.DynamicPacketSize ? Strings.Diag.DynamicEnabled : Strings.Diag.DynamicDisabled);
        if (input.DontFragment) Report(context, progress, Strings.Diag.DontFragmentEnabled);
        var sizesFormat = input.DynamicPacketSize ? Strings.Diag.PacketSizesFormat : Strings.Diag.PacketSizeFixedFormat;
        ReportFormat(context, progress, sizesFormat, string.Join(Strings.Diag.PacketSizeListSeparator, sizes));
        Separator(context, progress);
    }

    internal static void ReportStats(IProgress<IReadOnlyList<HopStatSnapshot>> stats, IReadOnlyList<HopInfo> hops)
    {
        var snapshot = hops.Select(Snapshot).ToArray();
        stats.Report(snapshot);
    }

    private static void Structured(DiagnosticContext context, IProgress<string> progress, string canonical,
        object?[] args, bool isError, bool isWarn, Exception? ex) =>
        ReportCore(context, progress, TextFormatter.FmtDisplay(Strings.T(canonical), args),
            canonical, args, isError, isWarn, ex);

    private static void ReportCore(DiagnosticContext context, IProgress<string> progress, string display,
        string canonical, object?[] args, bool isError, bool isWarn, Exception? ex)
    {
        var stamped = TextFormatter.Fmt(Strings.App.TimestampedMessageFormat, TextFormatter.Now(), display);
        progress.Report(stamped);
        Log(context, canonical, args, isError, isWarn, ex);
    }

    private static void Log(DiagnosticContext context, string canonical, object?[] args,
        bool isError, bool isWarn, Exception? ex)
    {
        var logger = context.Logger;
        if (logger is null) return;
        if (isError) logger.ErrorFmt(canonical, ex, args);
        else if (isWarn) logger.WarnFmt(canonical, args);
        else logger.InfoFmt(canonical, args);
    }

    internal static HopStatSnapshot Snapshot(HopInfo hop) => new(
        hop.Ttl,
        hop.Address,
        hop.HostName,
        hop.Sent,
        hop.Received,
        hop.AvgPingMs,
        hop.MinPingMs,
        hop.MaxPingMs,
        hop.LastPingMs,
        hop.Status,
        hop.Diagnosis,
        hop.JitterMs,
        hop.StdDevMs);
}
