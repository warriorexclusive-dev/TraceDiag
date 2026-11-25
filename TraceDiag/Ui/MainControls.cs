namespace TraceDiag.Ui;

internal sealed record MainControls(
    TableLayoutPanel Root,
    TextBox RemoteHost,
    NumericUpDown MaxPacketSize,
    NumericUpDown Attempts,
    NumericUpDown FontSize,
    CheckBox DynamicPacketSize,
    CheckBox DarkTheme,
    DataGridView Stats,
    TextBox Output,
    Button Run,
    Label Status);
