using TraceDiag.Services.Abstractions;

namespace TraceDiag;

/// <summary>Logs canonical templates so each host can translate or keep English.</summary>
internal static class StructuredLogger
{
    internal static void InfoFmt(this IDiagnosticLogger? logger, string canonical, params object?[] args)
    {
        if (logger is null) return;
        if (logger is IStructuredDiagnosticLogger structured) structured.InfoStructured(canonical, args);
        else logger.Info(TextFormatter.Fmt(canonical, args));
    }

    internal static void WarnFmt(this IDiagnosticLogger? logger, string canonical, params object?[] args)
    {
        if (logger is null) return;
        if (logger is IStructuredDiagnosticLogger structured) structured.WarnStructured(canonical, args);
        else logger.Warn(TextFormatter.Fmt(canonical, args));
    }

    internal static void ErrorFmt(this IDiagnosticLogger? logger, string canonical, Exception? exception,
        params object?[] args)
    {
        if (logger is null) return;
        if (logger is IStructuredDiagnosticLogger structured) structured.ErrorStructured(canonical, args, exception);
        else logger.Error(TextFormatter.Fmt(canonical, args), exception);
    }
}
