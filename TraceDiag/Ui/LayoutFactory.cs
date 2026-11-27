namespace TraceDiag.Ui;

internal static class LayoutFactory
{
    internal static TableLayoutPanel Create(Control input, SplitContainer splitter, Label status)
    {
        var layout = CreateLayoutPanel();
        AddRows(layout);
        layout.Controls.Add(input, 0, 0);
        layout.Controls.Add(splitter, 0, 1);
        layout.Controls.Add(status, 0, 2);
        return layout;
    }

    private static TableLayoutPanel CreateLayoutPanel() => new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 3,
        Padding = new Padding(12)
    };

    private static void AddRows(TableLayoutPanel layout)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }
}
