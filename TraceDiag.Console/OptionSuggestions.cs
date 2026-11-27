namespace TraceDiag.Console;

/// <summary>Finds the closest canonical option and renders a compact option list.</summary>
internal static class OptionSuggestions
{
    internal static string? Closest(string option)
    {
        var normalized = Normalize(option);
        if (normalized.Length == 0) return null;
        return ConsoleOptions.Names
            .Select(name => (Name: name, Distance: Distance(normalized, Normalize(name))))
            .Where(item => item.Distance <= Threshold(normalized.Length))
            .OrderBy(item => item.Distance)
            .ThenBy(item => item.Name, StringComparer.Ordinal)
            .Select(item => item.Name)
            .FirstOrDefault();
    }

    internal static void Write(TextWriter output, string? suggestion) =>
        output.WriteLine(List(suggestion));

    internal static string List(string? suggestion)
    {
        var lines = new List<string>();
        if (suggestion is not null)
            lines.Add(TextFormatter.Fmt(Strings.T(Strings.Console.DidYouMeanFormat), suggestion));
        lines.Add(Strings.T(Strings.Console.OptionsTitle));
        lines.AddRange(ConsoleOptions.Usage.Select(usage =>
            TextFormatter.Fmt(Strings.Console.OptionLineFormat, Strings.T(usage))));
        return string.Join(Environment.NewLine, lines);
    }

    private static string Normalize(string value) =>
        value.TrimStart('-').ToLowerInvariant();

    private static int Threshold(int length) => length <= 3 ? 1 : 2;

    private static int Distance(string left, string right)
    {
        var previous = Enumerable.Range(0, right.Length + 1).ToArray();
        for (var i = 1; i <= left.Length; i++)
        {
            var current = new int[right.Length + 1];
            current[0] = i;
            for (var j = 1; j <= right.Length; j++)
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1));
            previous = current;
        }
        return previous[right.Length];
    }
}
