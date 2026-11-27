using System.Drawing;
using System.Reflection;
using Svg;

namespace TraceDiag.Ui;

/// <summary>Renders the bundled SVG icons at the requested pixel size and theme color.</summary>
internal static class SvgIconLoader
{
    private const string SourceColor = "#e3e3e3";
    private const string ResourcePrefix = "TraceDiag.Resources.";

    internal static Bitmap Load(string name, int size, Color color)
    {
        var svg = Read(name).Replace(SourceColor, ColorTranslator.ToHtml(color));
        var document = SvgDocument.FromSvg<SvgDocument>(svg);
        document.Width = new SvgUnit(size);
        document.Height = new SvgUnit(size);
        return document.Draw();
    }

    private static string Read(string name)
    {
        var assembly = typeof(SvgIconLoader).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourcePrefix + name + ".svg");
        if (stream is null) return string.Empty;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
