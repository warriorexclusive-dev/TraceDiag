using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

internal static class MonitoringReporter
{
    internal static void Header(DiagnosticContext context, IProgress<string> progress, MonitoringInput input)
    {
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.TargetFormat, input.RemoteHostLabel, input.RemoteAddress));
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.PacketSizeFixedFormat, input.PacketSize));
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.LossDelayFormat, input.LossDelayMs));
        DiagnosticReporter.Separator(context, progress);
    }
}
