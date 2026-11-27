using System.Globalization;
using System.Linq;

namespace TraceDiag;

internal static class TextFormatter
{
    internal static string Now() =>
        DateTime.Now.ToString(Strings.App.ConsoleTimestampFormat, CultureInfo.InvariantCulture);

    internal static string Fmt(string format, params object?[] args) =>
        string.Format(CultureInfo.InvariantCulture, format, args);

    internal static string FmtDisplay(string format, params object?[] args) =>
        string.Format(CultureInfo.InvariantCulture, format, args.Select(Display).ToArray());

    private static object? Display(object? value) =>
        value is LocalizedArg arg ? arg.Localized : value;
}
