namespace TraceDiag.Ui;

/// <summary>Hosts the stats grid above the log with a draggable horizontal splitter.</summary>
internal static class SplitViewFactory
{
    internal static SplitContainer Create(Control top, Control bottom)
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = UiDefaults.SplitterWidth,
            Panel1MinSize = UiDefaults.MinPaneHeight,
            Panel2MinSize = UiDefaults.MinPaneHeight
        };
        split.Panel1.Controls.Add(top);
        split.Panel2.Controls.Add(bottom);
        return split;
    }
}
