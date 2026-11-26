namespace TraceDiag.Ui;

/// <summary>CheckBox drawn with palette colors instead of system black.</summary>
internal sealed class ThemedCheckBox : CheckBox
{
    private const int BoxSize = 14;
    private ThemePalette _palette = ThemePalette.Dark;

    internal ThemedCheckBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.UseTextForAccessibility, true);
        FlatStyle = FlatStyle.Flat;
        AutoSize = true;
    }

    internal void SetPalette(ThemePalette palette)
    {
        _palette = palette;
        Invalidate();
    }

    internal ThemePalette GetPalette() => _palette;

    protected override void OnPaint(PaintEventArgs e)
    {
        var box = CreateBox();
        DrawBox(e.Graphics, box);
        if (Checked) DrawCheck(e.Graphics, box);
        DrawText(e.Graphics);
    }

    private Rectangle CreateBox() => new(1, (ClientSize.Height - BoxSize) / 2, BoxSize, BoxSize);

    private void DrawBox(Graphics graphics, Rectangle box)
    {
        using var brush = new SolidBrush(Checked && Enabled ? _palette.Accent : _palette.Input);
        using var pen = new Pen(Enabled ? _palette.GridLine : _palette.DisabledText);
        graphics.FillRectangle(brush, box);
        graphics.DrawRectangle(pen, box);
    }

    private void DrawCheck(Graphics graphics, Rectangle box)
    {
        using var pen = new Pen(Enabled ? _palette.AccentText : _palette.DisabledText, 2f);
        graphics.DrawLines(pen, CheckPoints(box));
    }

    private static Point[] CheckPoints(Rectangle box) =>
    [
        new(box.Left + 3, box.Top + box.Height / 2),
        new(box.Left + 6, box.Bottom - 3),
        new(box.Right - 2, box.Top + 2)
    ];

    private void DrawText(Graphics graphics)
    {
        var color = Enabled ? ForeColor : _palette.DisabledText;
        var area = new Rectangle(BoxSize + 5, 0, Math.Max(0, ClientSize.Width - BoxSize - 5), ClientSize.Height);
        TextRenderer.DrawText(graphics, Text, Font, area, color,
            TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
}
