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
    CheckBox DarkTheme,
    DataGridView Stats,
    TextBox Output,
    Button Run,
    Label Status);
