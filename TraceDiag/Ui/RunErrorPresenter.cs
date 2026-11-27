using System.Net.Sockets;
using TraceDiag.Services;

namespace TraceDiag.Ui;

internal static class RunErrorPresenter
{
    internal static void Present(Exception ex, Label status, Action<string> append, FileDiagnosticLogger logger)
    {
        if (ex is OperationCanceledException) { PresentCancelled(status, append, logger); return; }
        if (ex is SocketException socket) { PresentSocket(socket, status, append, logger); return; }
        PresentGeneral(ex, status, append, logger);
    }

    private static void PresentCancelled(Label status, Action<string> append, FileDiagnosticLogger logger)
    {
        logger.Warn(Strings.Log.RunCancelled);
        append(TextFormatter.Fmt(
            Strings.App.TimestampedMessageFormat,
            TextFormatter.Now(),
            Strings.Log.ConsoleOperationCancelled));
        UiText.Set(status, Strings.Ui.Cancelled);
    }

    private static void PresentSocket(SocketException ex, Label status, Action<string> append, FileDiagnosticLogger logger)
    {
        logger.Error(Strings.Log.DnsNetworkError, ex);
        append(TextFormatter.Fmt(Strings.Log.ConsoleDnsNetworkErrorFormat, TextFormatter.Now(), ex.Message));
        UiText.Set(status, Strings.Ui.ResolveError);
    }

    private static void PresentGeneral(Exception ex, Label status, Action<string> append, FileDiagnosticLogger logger)
    {
        logger.Error(Strings.Log.UnhandledError, ex);
        append(TextFormatter.Fmt(
            Strings.Log.ConsoleErrorFormat,
            TextFormatter.Now(),
            ex.GetType().Name,
            ex.Message));
        UiText.Set(status, Strings.Ui.Error);
    }
}
