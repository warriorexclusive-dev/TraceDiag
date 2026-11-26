namespace TraceDiag.Ui;

internal static class InputPanelFactory
{
    private const int RightColumnWidth = 250;

    internal static TableLayoutPanel Create(TextBox host, NumericUpDown packet, NumericUpDown attempts,
        NumericUpDown lossDelay, NumericUpDown font, CheckBox dynamicPacketSize, CheckBox monitoringMode,
        CheckBox darkTheme, Button run)
    {
        var layout = CreateLayout();
        layout.Controls.Add(CreateLeftFrame(host, packet, attempts, dynamicPacketSize, monitoringMode, darkTheme), 0, 0);
        layout.Controls.Add(CreateRightFrame(lossDelay, font, run), 1, 0);
        return layout;
    }

    private static TableLayoutPanel CreateLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, RightColumnWidth));
        return layout;
    }

    private static TableLayoutPanel CreateLeftFrame(TextBox host, NumericUpDown packet, NumericUpDown attempts,
        CheckBox dynamicPacketSize, CheckBox monitoringMode, CheckBox darkTheme)
    {
        var frame = CreateFramePanel(4);
        frame.Padding = new Padding(0, 0, 12, 0);
        frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.RemoteHostLabel, host, 276, false), 0, 0);
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.PacketSizeLabel, packet, 140, false), 0, 1);
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.AttemptsLabel, attempts, 140, false), 0, 2);
        frame.Controls.Add(CreateLeftAnchored(CreateCheckBoxRow(dynamicPacketSize, monitoringMode, darkTheme)), 0, 3);
        return frame;
    }

    private static TableLayoutPanel CreateRightFrame(NumericUpDown lossDelay, NumericUpDown font, Button run)
    {
        var frame = CreateFramePanel(3);
        frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.FontSizeLabel, font, 80, true), 0, 0);
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.LossDelayLabel, lossDelay, 80, true), 0, 1);
        frame.Controls.Add(CreateAnchored(run), 0, 2);
        return frame;
    }

    private static TableLayoutPanel CreateFramePanel(int rows)
    {
        var frame = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = rows,
            Padding = new Padding(8, 0, 0, 0),
            Margin = Padding.Empty
        };
        for (var i = 0; i < rows; i++) frame.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        return frame;
    }

    private static FlowLayoutPanel CreateCheckBoxRow(params CheckBox[] boxes)
    {
        var row = CreateFlowRow();
        foreach (var box in boxes) row.Controls.Add(PrepareCheckBox(box));
        return row;
    }

    private static FlowLayoutPanel CreateFlowRow() => new()
    {
        AutoSize = true,
        Dock = DockStyle.None,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = false
    };

    private static CheckBox PrepareCheckBox(CheckBox box)
    {
        box.Dock = DockStyle.None;
        box.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        box.Margin = new Padding(0, 0, 16, 0);
        return box;
    }

    private static TableLayoutPanel CreateLabelledRow(string text, Control input, int inputWidth, bool alignRight)
    {
        var row = CreateRowPanel(inputWidth, alignRight);
        row.Controls.Add(CreateLabel(text, alignRight), 0, 0);
        input.Dock = DockStyle.None;
        input.Anchor = alignRight
            ? AnchorStyles.Top | AnchorStyles.Right
            : AnchorStyles.Top | AnchorStyles.Left;
        row.Controls.Add(input, 1, 0);
        return row;
    }

    private static TableLayoutPanel CreateRowPanel(int inputWidth, bool alignRight)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.None,
            ColumnCount = 2,
            RowCount = 1,
            Anchor = alignRight ? AnchorStyles.Top | AnchorStyles.Right : AnchorStyles.Top | AnchorStyles.Left,
            Margin = new Padding(0, 0, 0, 4)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, inputWidth));
        return row;
    }

    private static Label CreateLabel(string text, bool alignRight) => new()
    {
        Text = text,
        AutoSize = true,
        Anchor = alignRight ? AnchorStyles.Top | AnchorStyles.Right : AnchorStyles.Top | AnchorStyles.Left,
        TextAlign = alignRight ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
        Padding = new Padding(0, 0, 6, 0),
        Margin = Padding.Empty
    };

    private static Control CreateAnchored(Control control)
    {
        control.Dock = DockStyle.None;
        control.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        control.Margin = new Padding(0, 4, 0, 0);
        return control;
    }

    private static Control CreateLeftAnchored(Control control)
    {
        control.Dock = DockStyle.None;
        control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        control.Margin = new Padding(0, 4, 0, 0);
        return control;
    }
}
