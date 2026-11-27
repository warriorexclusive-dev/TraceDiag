using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

internal static class MonitoringReporter
{
    internal static void Header(DiagnosticContext context, IProgress<string> progress, MonitoringInput input)
    {
        DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.TargetFormat,
            input.RemoteHostLabel, input.RemoteAddress);
        DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.PacketSizeFixedFormat, input.PacketSize);
        DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.LossDelayFormat, input.LossDelayMs);
        if (input.DontFragment) DiagnosticReporter.Report(context, progress, Strings.Diag.DontFragmentEnabled);
        DiagnosticReporter.Separator(context, progress);
    }
}
