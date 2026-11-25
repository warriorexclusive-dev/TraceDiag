namespace TraceDiag.Ui;

internal static class LayoutFactory
{
    internal static TableLayoutPanel Create(Control input, DataGridView stats, TextBox output, Label status)
    {
        var layout = CreateLayoutPanel();
        AddRows(layout);
        AddControls(layout, input, stats, output, status);
        return layout;
    }

    private static TableLayoutPanel CreateLayoutPanel() => new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 4,
        Padding = new Padding(12)
    };

    private static void AddControls(TableLayoutPanel layout, Control input, DataGridView stats, TextBox output, Label status)
    {
        layout.Controls.Add(input, 0, 0);
        layout.Controls.Add(stats, 0, 1);
        layout.Controls.Add(output, 0, 2);
        layout.Controls.Add(status, 0, 3);
    }

    private static void AddRows(TableLayoutPanel layout)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }
}
