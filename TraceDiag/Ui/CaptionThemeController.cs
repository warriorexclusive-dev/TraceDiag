using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TraceDiag.Models;

namespace TraceDiag.Ui;

/// <summary>Keeps the native title bar in sync with the selected theme.</summary>
internal static class CaptionThemeController
{
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;

    internal static void Apply(Form form, AppTheme theme)
    {
        if (!form.IsHandleCreated) return;
        var palette = ThemeApplier.Palette(theme);
        Set(form, DwmwaCaptionColor, palette.Surface);
        Set(form, DwmwaTextColor, palette.Text);
    }

    private static void Set(Form form, int attribute, Color color)
    {
        var value = ColorTranslator.ToWin32(color);
        try { DwmSetWindowAttribute(form.Handle, attribute, ref value, sizeof(int)); }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr handle, int attribute, ref int value, int size);
}
