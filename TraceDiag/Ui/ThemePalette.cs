using System.Drawing;

namespace TraceDiag.Ui;

internal sealed record ThemePalette(
    Color Window,
    Color Surface,
    Color Input,
    Color Text,
    Color Grid,
    Color GridLine,
    Color Header,
    Color HeaderText,
    Color Accent,
    Color AccentText)
{
    internal static ThemePalette Dark { get; } = new(
        Color.FromArgb(32, 32, 32),
        Color.FromArgb(45, 45, 48),
        Color.FromArgb(25, 25, 25),
        Color.FromArgb(240, 240, 240),
        Color.FromArgb(25, 25, 25),
        Color.FromArgb(70, 70, 70),
        Color.FromArgb(60, 60, 65),
        Color.FromArgb(235, 235, 235),
        Color.FromArgb(0, 120, 215),
        Color.White);

    internal static ThemePalette Light { get; } = new(
        SystemColors.Control,
        SystemColors.Control,
        Color.White,
        SystemColors.ControlText,
        Color.White,
        Color.FromArgb(200, 200, 200),
        SystemColors.Control,
        SystemColors.ControlText,
        SystemColors.Control,
        SystemColors.ControlText);
}
