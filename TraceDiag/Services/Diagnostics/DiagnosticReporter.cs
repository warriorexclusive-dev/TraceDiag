using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class DiagnosticReporter
{
    internal static void Report(DiagnosticContext context, IProgress<string> progress, string message, bool isError = false, bool isWarn = false, Exception? ex = null)
    {
        var stamped = TextFormatter.Fmt(Strings.App.TimestampedMessageFormat, TextFormatter.Now(), message);
        progress.Report(stamped);
        Log(context, message, isError, isWarn, ex);
    }

    internal static void Separator(DiagnosticContext context, IProgress<string> progress) =>
        Report(context, progress, new string(Strings.Diag.SeparatorChar, Strings.Diag.SeparatorLength));

    internal static void ReportHeader(DiagnosticContext context, IProgress<string> progress, DiagnosticInput input, IReadOnlyList<int> sizes)
    {
        Report(context, progress, TextFormatter.Fmt(Strings.Diag.TargetFormat, input.RemoteHostLabel, input.RemoteAddress));
        Report(context, progress, TextFormatter.Fmt(Strings.Diag.MaxPacketSizeFormat, input.MaxPacketSize));
        Report(context, progress, TextFormatter.Fmt(Strings.Diag.AttemptsFormat, input.Attempts));
        Report(context, progress, TextFormatter.Fmt(Strings.Diag.ReplyDelayFormat, input.DelayMs));
        Report(context, progress, input.DynamicPacketSize ? Strings.Diag.DynamicEnabled : Strings.Diag.DynamicDisabled);
        var sizesFormat = input.DynamicPacketSize ? Strings.Diag.PacketSizesFormat : Strings.Diag.PacketSizeFixedFormat;
        Report(context, progress, TextFormatter.Fmt(sizesFormat, string.Join(Strings.Diag.PacketSizeListSeparator, sizes)));
        Separator(context, progress);
    }

    internal static void ReportStats(IProgress<IReadOnlyList<HopStatSnapshot>> stats, IReadOnlyList<HopInfo> hops)
    {
        var snapshot = hops.Select(ToSnapshot).ToArray();
        stats.Report(snapshot);
    }

    private static void Log(DiagnosticContext context, string message, bool isError, bool isWarn, Exception? ex)
    {
        if (isError) context.Logger?.Error(message, ex);
        else if (isWarn) context.Logger?.Warn(message);
        else context.Logger?.Info(message);
    }

    private static HopStatSnapshot ToSnapshot(HopInfo hop) => new(
        hop.Ttl,
        hop.Address,
        hop.HostName,
        hop.Sent,
        hop.Received,
        hop.AvgPingMs,
        hop.MinPingMs,
        hop.MaxPingMs,
        hop.LastPingMs,
        hop.Status);
}
