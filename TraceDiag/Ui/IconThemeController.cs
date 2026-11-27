using System.Drawing;

namespace TraceDiag.Ui;

/// <summary>Keeps SVG action icons in sync with theme and configured pixel size.</summary>
internal static class IconThemeController
{
    internal static void Apply(MainControls controls, ThemePalette palette, int size)
    {
        Apply(controls.Play, "play", palette, size);
        Apply(controls.Stop, "stop", palette, size);
        Apply(controls.Settings, "settings", palette, size);
    }

    internal static void Running(MainControls controls, bool running)
    {
        controls.Play.Visible = !running;
        controls.Stop.Visible = running;
    }

    private static void Apply(PictureBox box, string name, ThemePalette palette, int size)
    {
        var previous = box.Image;
        box.Size = new Size(size, size);
        box.Image = SvgIconLoader.Load(name, size, palette.Text);
        previous?.Dispose();
    }
}
