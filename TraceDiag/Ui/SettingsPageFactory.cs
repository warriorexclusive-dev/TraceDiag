using System.Drawing;

namespace TraceDiag.Ui;

/// <summary>Builds the five settings pages from the shared control instances.</summary>
internal static class SettingsPageFactory
{
    internal static TabControl Create(MainControls controls)
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(Page(Strings.Ui.ConnectionGroup,
            (Strings.Ui.IpFamilyLabel, controls.IpFamily),
            (Strings.Ui.AdapterLabel, controls.Adapter)));
        tabs.TabPages.Add(Page(Strings.Ui.SourcesGroup,
            (Strings.Ui.RipeLookupLabel, controls.RipeLookup),
            (Strings.Ui.AsnLookupLabel, controls.AsnLookup),
            (Strings.Ui.AltDnsLabel, controls.AlternativeDns),
            (Strings.Ui.AltDnsPathLabel, controls.DnsFilePath),
            (string.Empty, Hint(Strings.Ui.DnsPathHint)),
            (string.Empty, Hint(Strings.Ui.PrivateSourcesNote)),
            (string.Empty, controls.EditDns)));
        tabs.TabPages.Add(Page(Strings.Ui.ModesGroup,
            (Strings.Ui.MonitoringModeLabel, controls.MonitoringMode),
            (Strings.Ui.RoutePrecheckLabel, controls.RoutePrecheck),
            (Strings.Ui.DeepRoutesLabel, controls.DeepRoutes),
            (Strings.Ui.DynamicPacketSizeLabel, controls.DynamicPacketSize)));
        tabs.TabPages.Add(Page(Strings.Ui.InterfaceGroup,
            (Strings.Ui.LanguageLabel, controls.Language),
            (Strings.Ui.DarkThemeLabel, controls.DarkTheme),
            (Strings.Ui.FontSizeLabel, controls.FontSize),
            (Strings.Ui.IconSizeLabel, controls.IconSize)));
        tabs.TabPages.Add(Page(Strings.Ui.CheckOptionsGroup,
            (Strings.Ui.PacketSizeLabelFormat, controls.MaxPacketSize),
            (Strings.Ui.AttemptsLabel, controls.Attempts),
            (Strings.Ui.LossDelayLabel, controls.LossDelay),
            (Strings.Ui.DontFragmentLabel, controls.DontFragment),
            (Strings.Ui.ProbeTimeoutLabel, controls.ProbeTimeout)));
        return tabs;
    }

    private static TabPage Page(string title, params (string Label, Control Input)[] rows)
    {
        var tab = new TabPage
        {
            AutoScroll = true,
            Padding = new Padding(12),
            Text = Strings.T(title),
            Tag = new UiTextKey(title, null)
        };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var index = 0; index < rows.Length; index++) AddRow(layout, rows[index], index);
        tab.Controls.Add(layout);
        return tab;
    }

    private static void AddRow(TableLayoutPanel layout, (string Label, Control Input) row, int index)
    {
        layout.RowCount = index + 1;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var hasLabel = !string.IsNullOrEmpty(row.Label);
        if (hasLabel) layout.Controls.Add(Label(row.Label), 0, index);
        layout.Controls.Add(row.Input, hasLabel ? 1 : 0, index);
    }

    private static Label Hint(string canonical)
    {
        var hint = new Label { AutoSize = true, ForeColor = Color.Gray };
        UiText.Set(hint, canonical);
        return hint;
    }

    private static Label Label(string canonical)
    {
        var label = new Label { AutoSize = true, Padding = new Padding(0, 8, 12, 0) };
        UiText.Set(label, canonical,
            canonical == Strings.Ui.PacketSizeLabelFormat
                ? [UiDefaults.MinPacketSize, UiDefaults.MaxPacketSize] : null);
        return label;
    }
}
