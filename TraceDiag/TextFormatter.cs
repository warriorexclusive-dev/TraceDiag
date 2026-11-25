using System.Globalization;

namespace TraceDiag;

internal static class TextFormatter
{
    internal static string Now() =>
        DateTime.Now.ToString(Strings.App.ConsoleTimestampFormat, CultureInfo.InvariantCulture);

    internal static string Fmt(string format, params object?[] args) =>
        string.Format(CultureInfo.InvariantCulture, format, args);
}
