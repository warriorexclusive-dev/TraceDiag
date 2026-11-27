using System.Globalization;
using System.Text;

namespace TraceDiag.Services;

internal static class LogLineBuilder
{
    internal static string Build(string level, string message, Exception? exception) =>
        Build(Timestamp(), level, message, exception);

    internal static string[] BuildPair(string level, string fileMessage, string consoleMessage,
        Exception? exception)
    {
        var timestamp = Timestamp();
        return
        [
            Build(timestamp, level, fileMessage, exception),
            Build(timestamp, level, consoleMessage, exception)
        ];
    }

    private static string Build(string timestamp, string level, string message, Exception? exception)
    {
        var sb = new StringBuilder();
        sb.AppendFormat(CultureInfo.InvariantCulture, Strings.App.LogLinePrefixFormat, timestamp, level);
        sb.Append(message);
        AppendException(sb, exception);
        return sb.ToString();
    }

    private static string Timestamp() =>
        DateTime.Now.ToString(Strings.App.LogTimestampFormat, CultureInfo.InvariantCulture);

    private static void AppendException(StringBuilder sb, Exception? exception)
    {
        if (exception is null) return;
        sb.AppendLine();
        sb.Append(exception);
    }
}
