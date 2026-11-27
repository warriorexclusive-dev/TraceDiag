namespace TraceDiag.Ui;

internal static class InputPanelFactory
{
    private const int RightColumnWidth = 250;

    internal static TableLayoutPanel Create(TextBox host, NumericUpDown packet, NumericUpDown attempts,
        NumericUpDown lossDelay, NumericUpDown font, CheckBox dynamicPacketSize, CheckBox dontFragment,
        CheckBox monitoringMode, CheckBox darkTheme, ComboBox ipFamily, ComboBox language, Button run)
    {
        var layout = CreateLayout();
        layout.Controls.Add(CreateLeftFrame(host, packet, attempts, dynamicPacketSize, dontFragment, monitoringMode, darkTheme), 0, 0);
        layout.Controls.Add(CreateRightFrame(lossDelay, font, ipFamily, language, run), 1, 0);
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
        CheckBox dynamicPacketSize, CheckBox dontFragment, CheckBox monitoringMode, CheckBox darkTheme)
    {
        var frame = CreateFramePanel(4);
        frame.Padding = new Padding(0, 0, 12, 0);
        frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.RemoteHostLabel, host, 276, false), 0, 0);
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.PacketSizeLabelFormat, packet, 140, false,
            UiDefaults.MinPacketSize, UiDefaults.MaxPacketSize), 0, 1);
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.AttemptsLabel, attempts, 140, false), 0, 2);
        frame.Controls.Add(CreateCheckBoxRow(dynamicPacketSize, dontFragment, monitoringMode, darkTheme), 0, 3);
        return frame;
    }

    private static TableLayoutPanel CreateRightFrame(NumericUpDown lossDelay, NumericUpDown font,
        ComboBox ipFamily, ComboBox language, Button run)
    {
        var frame = CreateFramePanel(5);
        frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.FontSizeLabel, font, 80, true), 0, 0);
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.LossDelayLabel, lossDelay, 80, true), 0, 1);
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.IpFamilyLabel, ipFamily, 120, true), 0, 2);
        frame.Controls.Add(CreateLabelledRow(Strings.Ui.LanguageLabel, language, 120, true), 0, 3);
        frame.Controls.Add(CreateAnchored(run), 0, 4);
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
        var row = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 4, 0, 0)
        };
        foreach (var box in boxes) row.Controls.Add(PrepareCheckBox(box));
        return row;
    }

    private static CheckBox PrepareCheckBox(CheckBox box)
    {
        box.Dock = DockStyle.None;
        box.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        box.Margin = new Padding(0, 0, 16, 0);
        return box;
    }

    private static TableLayoutPanel CreateLabelledRow(string canonical, Control input, int inputWidth,
        bool alignRight, params object?[]? args)
    {
        var row = CreateRowPanel(inputWidth, alignRight);
        row.Controls.Add(CreateLabel(canonical, alignRight, args), 0, 0);
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

    private static Label CreateLabel(string canonical, bool alignRight, object?[]? args)
    {
        var label = new Label
        {
            AutoSize = true,
            Anchor = alignRight ? AnchorStyles.Top | AnchorStyles.Right : AnchorStyles.Top | AnchorStyles.Left,
            TextAlign = alignRight ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 0, 6, 0),
            Margin = Padding.Empty
        };
        UiText.Set(label, canonical, args ?? []);
        return label;
    }

    private static Control CreateAnchored(Control control)
    {
        control.Dock = DockStyle.None;
        control.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        control.Margin = new Padding(0, 4, 0, 0);
        return control;
    }
}
