namespace TraceDiag.Ui;

internal static class InputPanelFactory
{
    internal static TableLayoutPanel Create(TextBox host, NumericUpDown packet, NumericUpDown attempts, NumericUpDown font, CheckBox dynamicPacketSize, Button run)
    {
        var layout = CreateLayout();
        layout.Controls.Add(CreateLeftFrame(host, packet, attempts), 0, 0);
        layout.Controls.Add(CreateRightFrame(font, dynamicPacketSize, run), 1, 0);
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
        return layout;
    }

    private static TableLayoutPanel CreateLeftFrame(TextBox host, NumericUpDown packet, NumericUpDown attempts)
    {
        var frame = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(0, 0, 12, 0)
        };
        ConfigureLeftFrame(frame);
        AddLeftFields(frame, host, packet, attempts);
        return frame;
    }

    private static void ConfigureLeftFrame(TableLayoutPanel frame)
    {
        frame.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
    }

    private static void AddLeftFields(TableLayoutPanel frame, TextBox host, NumericUpDown packet, NumericUpDown attempts)
    {
        frame.Controls.Add(CreateFieldLabel(Strings.Ui.RemoteHostLabel), 0, 0);
        frame.Controls.Add(host, 1, 0);
        frame.Controls.Add(CreateFieldLabel(Strings.Ui.PacketSizeLabel), 0, 1);
        frame.Controls.Add(packet, 1, 1);
        frame.Controls.Add(CreateFieldLabel(Strings.Ui.AttemptsLabel), 0, 2);
        frame.Controls.Add(attempts, 1, 2);
    }

    private static TableLayoutPanel CreateRightFrame(NumericUpDown font, CheckBox dynamicPacketSize, Button run)
    {
        var frame = CreateFramePanel();
        ConfigureRightFrame(frame);
        frame.Controls.Add(CreateFontPanel(font), 0, 0);
        frame.Controls.Add(dynamicPacketSize, 0, 1);
        frame.Controls.Add(run, 0, 2);
        return frame;
    }

    private static TableLayoutPanel CreateFramePanel() => new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        ColumnCount = 1,
        RowCount = 3,
        Padding = new Padding(8, 0, 0, 0)
    };

    private static void ConfigureRightFrame(TableLayoutPanel frame)
    {
        frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        frame.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        frame.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        frame.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }

    private static TableLayoutPanel CreateFontPanel(NumericUpDown font)
    {
        var panel = CreateFontPanelLayout();
        ConfigureFontPanel(panel);
        panel.Controls.Add(CreateFontLabel(), 0, 0);
        panel.Controls.Add(font, 1, 0);
        return panel;
    }

    private static TableLayoutPanel CreateFontPanelLayout() => new()
    {
        AutoSize = true,
        ColumnCount = 2,
        RowCount = 1,
        Anchor = AnchorStyles.Top | AnchorStyles.Right,
        Dock = DockStyle.None,
        Margin = new Padding(8, 0, 0, 0)
    };

    private static void ConfigureFontPanel(TableLayoutPanel panel)
    {
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
    }

    private static Label CreateFontLabel()
    {
        return new Label
        {
            Text = Strings.Ui.FontSizeLabel,
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 6, 6, 6)
        };
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 6, 0, 6)
        };
    }
}
