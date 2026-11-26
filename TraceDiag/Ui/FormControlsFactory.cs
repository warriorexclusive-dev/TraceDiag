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
        var darkThemeCheckBox = CreateDarkThemeCheckBox(darkTheme);
        var run = CreateRunButton();
        var (stats, output, status) = CreateFooter(fontSize);
        var input = CreateInput(host, packet, attempts, lossDelay, font, dynamicPacketSize, darkThemeCheckBox, run);
        return new MainControls(LayoutFactory.Create(input, stats, output, status), host, packet, attempts, lossDelay, font, dynamicPacketSize, darkThemeCheckBox, stats, output, run, status);
    }

    private static TableLayoutPanel CreateInput(TextBox host, NumericUpDown packet, NumericUpDown attempts, NumericUpDown lossDelay, NumericUpDown font, CheckBox dynamicPacketSize, CheckBox darkTheme, Button run) =>
        InputPanelFactory.Create(host, packet, attempts, lossDelay, font, dynamicPacketSize, darkTheme, run);

    private static (DataGridView Stats, TextBox Output, Label Status) CreateFooter(float fontSize) =>
        (StatsGridFactory.Create(), CreateOutputTextBox(fontSize), CreateStatusLabel());

    private static TextBox CreateHostTextBox() => new()
    {
        Dock = DockStyle.Fill
    };

    private static NumericUpDown CreatePacketInput() => new()
    {
        Dock = DockStyle.Left,
        Width = 140,
        Minimum = 64,
        Maximum = 65534,
        Value = 1024
    };

    private static NumericUpDown CreateAttemptsInput() => new()
    {
        Dock = DockStyle.Left,
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

    private static CheckBox CreateDynamicPacketSizeCheckBox() => new()
    {
        Text = Strings.Ui.DynamicPacketSizeLabel,
        AutoSize = true,
        Checked = true,
        Anchor = AnchorStyles.Top | AnchorStyles.Right,
        Dock = DockStyle.None,
        Margin = new Padding(8, 6, 0, 0)
    };

    private static CheckBox CreateDarkThemeCheckBox(bool darkTheme) => new()
    {
        Text = Strings.Ui.DarkThemeLabel,
        AutoSize = true,
        Checked = darkTheme,
        Anchor = AnchorStyles.Top | AnchorStyles.Right,
        Dock = DockStyle.None,
        Margin = new Padding(8, 6, 0, 0)
    };

    private static Button CreateRunButton() => new()
    {
        Text = Strings.Ui.RunButton,
        Size = new Size(240, 64),
        Font = new Font(Strings.Ui.GridFontFamily, 16f, FontStyle.Bold),
        Anchor = AnchorStyles.Top | AnchorStyles.Right,
        Margin = new Padding(0, 6, 0, 0)
    };

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

    private static Label CreateStatusLabel() => new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        Text = Strings.Ui.Ready,
        Padding = new Padding(0, 6, 0, 0)
    };
}
