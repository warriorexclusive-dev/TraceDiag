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
        var dynamicPacketSize = CreateDynamicPacketSizeCheckBox();
        var dontFragment = CreateDontFragmentCheckBox();
        var monitoringMode = CreateMonitoringModeCheckBox();
        var ripeLookup = CreateRipeLookupCheckBox();
        var darkThemeCheckBox = CreateDarkThemeCheckBox(darkTheme);
        var ipFamily = CreateIpFamilyInput();
        var language = CreateLanguageInput();
        var run = CreateRunButton();
        var (stats, output, status, splitter) = CreateFooter(fontSize);
        var input = CreateInput(host, packet, attempts, lossDelay, font, dynamicPacketSize,
            dontFragment, monitoringMode, ripeLookup, darkThemeCheckBox, ipFamily, language, run);
        return new MainControls(LayoutFactory.Create(input, splitter, status), splitter, host, packet,
            attempts, lossDelay, font, dynamicPacketSize, dontFragment, monitoringMode,
            ripeLookup, darkThemeCheckBox, ipFamily, language, stats, output, run, status);
    }

    private static TableLayoutPanel CreateInput(TextBox host, NumericUpDown packet, NumericUpDown attempts,
        NumericUpDown lossDelay, NumericUpDown font, CheckBox dynamicPacketSize, CheckBox dontFragment,
        CheckBox monitoringMode, CheckBox ripeLookup, CheckBox darkTheme, ComboBox ipFamily, ComboBox language, Button run) =>
        InputPanelFactory.Create(host, packet, attempts, lossDelay, font, dynamicPacketSize,
            dontFragment, monitoringMode, ripeLookup, darkTheme, ipFamily, language, run);

    private static (DataGridView Stats, TextBox Output, Label Status, SplitContainer Splitter) CreateFooter(float fontSize)
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

    private static NumericUpDown CreatePacketInput() => new()
    {
        Dock = DockStyle.None,
        Width = 140,
        Minimum = UiDefaults.MinPacketSize,
        Maximum = UiDefaults.MaxPacketSize,
        Value = UiDefaults.DefaultMaxPacketSize
    };

    private static NumericUpDown CreateAttemptsInput() => new()
    {
        Dock = DockStyle.None,
        Width = 140,
        Minimum = 1,
        Maximum = 1000,
        Value = UiDefaults.DefaultAttempts
    };

    private static NumericUpDown CreateLossDelayInput() => new()
    {
        Width = 80,
        Minimum = 0,
        Maximum = UiDefaults.MaxLossDelayMs,
        Increment = 100,
        Value = UiDefaults.DefaultLossDelayMs,
        Anchor = AnchorStyles.Right
    };

    private static NumericUpDown CreateFontInput(float fontSize) => new()
    {
        Minimum = 8,
        Maximum = 24,
        DecimalPlaces = 1,
        Increment = 0.5m,
        Value = UiDefaults.ClampFontSize(fontSize),
        Width = 80,
        Anchor = AnchorStyles.Right
    };

    private static CheckBox CreateDynamicPacketSizeCheckBox() =>
        CreateCheck(Strings.Ui.DynamicPacketSizeLabel, true, new Padding(8, 6, 0, 0));

    private static CheckBox CreateDontFragmentCheckBox() =>
        CreateCheck(Strings.Ui.DontFragmentLabel, false, Padding.Empty);

    private static CheckBox CreateMonitoringModeCheckBox() =>
        CreateCheck(Strings.Ui.MonitoringModeLabel, false, Padding.Empty);

    private static CheckBox CreateRipeLookupCheckBox() =>
        CreateCheck(Strings.Ui.RipeLookupLabel, true, Padding.Empty);

    private static CheckBox CreateDarkThemeCheckBox(bool darkTheme) =>
        CreateCheck(Strings.Ui.DarkThemeLabel, darkTheme, new Padding(8, 6, 0, 0));

    private static CheckBox CreateCheck(string canonical, bool checkedState, Padding margin)
    {
        var box = new CheckBox
        {
            AutoSize = true,
            Checked = checkedState,
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            Dock = DockStyle.None,
            Margin = margin
        };
        UiText.Set(box, canonical);
        return box;
    }

    private static ComboBox CreateIpFamilyInput()
    {
        var combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 120,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        combo.Items.AddRange(["Auto", "IPv4", "IPv6"]);
        combo.SelectedIndex = 0;
        return combo;
    }

    private static ComboBox CreateLanguageInput()
    {
        var combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 120,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        combo.Items.AddRange(Strings.Ui.LanguageNames.Select(Strings.T).ToArray());
        combo.SelectedIndex = (int)Translator.Current;
        return combo;
    }

    private static Button CreateRunButton()
    {
        var run = new Button
        {
            Size = new Size(240, 64),
            Font = new Font(Strings.Ui.GridFontFamily, 16f, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Margin = new Padding(0, 6, 0, 0)
        };
        UiText.Set(run, Strings.Ui.RunButton);
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
        var status = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 6, 0, 0)
        };
        UiText.Set(status, Strings.Ui.Ready);
        return status;
    }
}
