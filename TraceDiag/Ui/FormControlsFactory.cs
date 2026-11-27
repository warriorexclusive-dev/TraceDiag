using TraceDiag.Localization;

namespace TraceDiag.Ui;

internal static class FormControlsFactory
{
    internal static MainControls Create(float fontSize, bool darkTheme = true)
    {
        var host = CreateHostTextBox();
        var packet = CreatePacketInput();
        var attempts = CreateAttemptsInput();
        var lossDelay = CreateLossDelayInput();
        var font = CreateFontInput(fontSize);
        var probeTimeout = CreateProbeTimeoutInput();
        var ipFamily = CreateIpFamilyInput();
        var language = CreateLanguageInput();
        var adapter = CreateAdapterInput();
        var iconSize = CreateIconSizeInput();
        var dnsPath = CreateDnsPathInput();
        var editDns = CreateButton(Strings.Ui.AltDnsEditorButton);
        var adapterNote = CreateLabel(Strings.Ui.AdapterUnavailable);
        var play = CreateIcon();
        var stop = CreateIcon();
        stop.Visible = false;
        var settings = CreateIcon();
        var run = CreateRunButton();
        var (stats, output, status, splitter) = CreateFooter(fontSize);
        var toolbar = ToolbarFactory.Create(host, play, stop, settings);
        return new MainControls(LayoutFactory.Create(toolbar, splitter, status),
            splitter, host, packet, attempts, lossDelay, font, probeTimeout,
            CreateDynamicPacketSizeCheckBox(), CreateDontFragmentCheckBox(),
            CreateMonitoringModeCheckBox(), CreateRipeLookupCheckBox(),
            CreateRoutePrecheckCheckBox(), CreateDeepRoutesCheckBox(),
            CreateAltDnsCheckBox(), CreateAsnLookupCheckBox(),
            CreateDarkThemeCheckBox(darkTheme), ipFamily, language, adapter, iconSize,
            dnsPath, editDns, adapterNote, play, stop, settings, run, stats, output, status);
    }

    private static (DataGridView Stats, TextBox Output, Label Status, SplitContainer Splitter) CreateFooter(
        float fontSize)
    {
        var stats = StatsGridFactory.Create();
        var output = CreateOutputTextBox(fontSize);
        return (stats, output, CreateStatusLabel(), SplitViewFactory.Create(stats, output));
    }

    private static TextBox CreateHostTextBox() => new()
    {
        Dock = DockStyle.None,
        Width = 276,
        Anchor = AnchorStyles.Top | AnchorStyles.Left
    };

    private static NumericUpDown CreatePacketInput() => CreateNumeric(UiDefaults.MinPacketSize,
        UiDefaults.MaxPacketSize, UiDefaults.DefaultMaxPacketSize, 140);

    private static NumericUpDown CreateAttemptsInput() => CreateNumeric(1, 1000,
        UiDefaults.DefaultAttempts, 140);

    private static NumericUpDown CreateLossDelayInput() => CreateNumeric(0, UiDefaults.MaxLossDelayMs,
        UiDefaults.DefaultLossDelayMs, 80);

    private static NumericUpDown CreateFontInput(float fontSize) => new()
    {
        Minimum = 8,
        Maximum = 24,
        DecimalPlaces = 1,
        Increment = 0.5m,
        Value = UiDefaults.ClampFontSize(fontSize),
        Width = 80
    };

    private static NumericUpDown CreateProbeTimeoutInput() => CreateNumeric(100, 3000, 3000, 100);

    private static NumericUpDown CreateNumeric(int min, int max, int value, int width) => new()
    {
        Minimum = min,
        Maximum = max,
        Value = value,
        Width = width
    };

    private static CheckBox CreateDynamicPacketSizeCheckBox() =>
        CreateCheck(Strings.Ui.DynamicPacketSizeLabel, true);

    private static CheckBox CreateDontFragmentCheckBox() =>
        CreateCheck(Strings.Ui.DontFragmentLabel, false);

    private static CheckBox CreateMonitoringModeCheckBox() =>
        CreateCheck(Strings.Ui.MonitoringModeLabel, false);

    private static CheckBox CreateRipeLookupCheckBox() => CreateCheck(Strings.Ui.RipeLookupLabel, true);

    private static CheckBox CreateRoutePrecheckCheckBox() => CreateCheck(Strings.Ui.RoutePrecheckLabel, true);

    private static CheckBox CreateDeepRoutesCheckBox() => CreateCheck(Strings.Ui.DeepRoutesLabel, false);

    private static CheckBox CreateAltDnsCheckBox() => CreateCheck(Strings.Ui.AltDnsLabel, true);

    private static CheckBox CreateAsnLookupCheckBox() => CreateCheck(Strings.Ui.AsnLookupLabel, true);

    private static CheckBox CreateDarkThemeCheckBox(bool darkTheme) =>
        CreateCheck(Strings.Ui.DarkThemeLabel, darkTheme);

    private static CheckBox CreateCheck(string canonical, bool checkedState)
    {
        var box = new CheckBox { AutoSize = true, Checked = checkedState };
        UiText.Set(box, canonical);
        return box;
    }

    private static ComboBox CreateIpFamilyInput() => CreateCombo(["Auto", "IPv4", "IPv6"]);

    private static ComboBox CreateLanguageInput() =>
        CreateCombo(Strings.Ui.LanguageNames.Select(Strings.T).ToArray());

    private static ComboBox CreateAdapterInput()
    {
        var combo = CreateCombo([]);
        AdapterList.Fill(combo);
        return combo;
    }

    private static ComboBox CreateIconSizeInput() =>
        CreateCombo(UiDefaults.IconSizes.Select(size => size.ToString()).ToArray(),
            UiDefaults.DefaultIconSizePx.ToString());

    private static ComboBox CreateCombo(string[] items, string? selected = null)
    {
        var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        combo.Items.AddRange(items);
        if (selected is not null && combo.Items.Contains(selected)) combo.SelectedItem = selected;
        else if (items.Length > 0) combo.SelectedIndex = 0;
        return combo;
    }

    private static TextBox CreateDnsPathInput() => new() { Width = 320, Text = string.Empty };

    private static Button CreateButton(string canonical)
    {
        var button = new Button { AutoSize = true, MinimumSize = new Size(120, 32) };
        UiText.Set(button, canonical);
        return button;
    }

    private static PictureBox CreateIcon() => new()
    {
        SizeMode = PictureBoxSizeMode.Zoom,
        Size = new Size(UiDefaults.DefaultIconSizePx, UiDefaults.DefaultIconSizePx),
        Cursor = Cursors.Hand
    };

    private static Label CreateLabel(string canonical)
    {
        var label = new Label { AutoSize = true, ForeColor = Color.FromArgb(255, 120, 120) };
        UiText.Set(label, canonical);
        return label;
    }

    private static Button CreateRunButton()
    {
        var run = CreateButton(Strings.Ui.RunButton);
        run.Visible = false;
        return run;
    }

    private static TextBox CreateOutputTextBox(float fontSize) => new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = false,
        Font = new Font(Strings.Ui.LogFontFamily, fontSize),
        BackColor = Color.White
    };

    private static Label CreateStatusLabel()
    {
        var status = new Label { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
        UiText.Set(status, Strings.Ui.Ready);
        return status;
    }
}
