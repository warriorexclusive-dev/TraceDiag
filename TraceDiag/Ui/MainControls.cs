namespace TraceDiag.Ui;

internal sealed record MainControls(
    TableLayoutPanel Root,
    SplitContainer Splitter,
    TextBox RemoteHost,
    NumericUpDown MaxPacketSize,
    NumericUpDown Attempts,
    NumericUpDown LossDelay,
    NumericUpDown FontSize,
    CheckBox DynamicPacketSize,
    CheckBox DontFragment,
    CheckBox MonitoringMode,
    CheckBox RipeLookup,
    CheckBox RoutePrecheck,
    CheckBox DeepRoutes,
    CheckBox AlternativeDns,
    CheckBox DarkTheme,
    ComboBox IpFamily,
    ComboBox Language,
    DataGridView Stats,
    TextBox Output,
    Button Run,
    Label Status);
