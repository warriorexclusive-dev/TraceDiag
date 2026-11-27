namespace TraceDiag.Ui;

/// <summary>Small main toolbar: host name and three SVG action icons.</summary>
internal static class ToolbarFactory
{
    internal static TableLayoutPanel Create(TextBox host, PictureBox play, PictureBox stop,
        PictureBox settings)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 5,
            RowCount = 1,
            Padding = new Padding(8, 4, 8, 4)
        };
        AddLabel(layout);
        layout.Controls.Add(host, 1, 0);
        layout.Controls.Add(play, 2, 0);
        layout.Controls.Add(stop, 3, 0);
        layout.Controls.Add(settings, 4, 0);
        return layout;
    }

    private static void AddLabel(TableLayoutPanel layout)
    {
        var label = new Label { AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left,
            Margin = new Padding(3, 7, 3, 3) };
        UiText.Set(label, Strings.Ui.RemoteHostLabel);
        layout.Controls.Add(label, 0, 0);
    }
}
