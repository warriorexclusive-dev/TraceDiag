using System.Windows.Forms;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class ThemeApplierTests
{
    [Fact]
    public void ApplyCheckBox_UsesExplicitFlatPalette()
    {
        using var form = new Form();
        var check = new CheckBox();
        form.Controls.Add(check);
        var palette = ThemePalette.Dark;
        ThemeApplier.Apply(form, palette);
        Assert.Equal(FlatStyle.Flat, check.FlatStyle);
        Assert.Equal(palette.Surface, check.BackColor);
        Assert.Equal(palette.Text, check.ForeColor);
        Assert.Equal(palette.GridLine, check.FlatAppearance.BorderColor);
        Assert.Equal(palette.Accent, check.FlatAppearance.CheckedBackColor);
    }
}
