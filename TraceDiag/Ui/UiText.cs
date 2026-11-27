namespace TraceDiag.Ui;

/// <summary>Canonical text and optional format arguments attached to a control.</summary>
internal sealed record UiTextKey(string Canonical, object?[]? Args);

/// <summary>Keeps canonical keys on controls so language changes can retranslate them.</summary>
internal static class UiText
{
    internal static void Set(Control control, string canonical, params object?[]? args)
    {
        control.Tag = new UiTextKey(canonical, args);
        Refresh(control);
    }

    internal static void Refresh(Control control)
    {
        if (control.Tag is not UiTextKey key) return;
        control.Text = key.Args is null
            ? Strings.T(key.Canonical)
            : TextFormatter.Fmt(Strings.T(key.Canonical), key.Args);
    }

    internal static void Set(DataGridViewColumn column, string canonical)
    {
        column.Tag = canonical;
        column.HeaderText = Strings.T(canonical);
    }

    internal static void Refresh(DataGridViewColumn column)
    {
        if (column.Tag is string key) column.HeaderText = Strings.T(key);
    }
}
