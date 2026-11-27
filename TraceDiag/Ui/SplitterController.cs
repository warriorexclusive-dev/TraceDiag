namespace TraceDiag.Ui;

/// <summary>Converts the splitter position to and from a stable percentage.</summary>
internal static class SplitterController
{
    internal static int Percent(SplitContainer split)
    {
        var usable = split.Height - split.SplitterWidth;
        return usable <= 0 ? UiDefaults.DefaultStatsPercent
            : ClampPercent((split.SplitterDistance * 100 + usable / 2) / usable);
    }

    internal static void Apply(SplitContainer split, int percent)
    {
        var usable = split.Height - split.SplitterWidth;
        if (usable <= 0) return;
        var distance = usable * ClampPercent(percent) / 100;
        var max = Math.Max(split.Panel1MinSize, usable - split.Panel2MinSize);
        split.SplitterDistance = Compat.Clamp(distance, split.Panel1MinSize, max);
    }

    private static int ClampPercent(int percent) =>
        Compat.Clamp(percent, UiDefaults.MinStatsPercent, UiDefaults.MaxStatsPercent);
}
