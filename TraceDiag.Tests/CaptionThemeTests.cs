using System.Windows.Forms;
using TraceDiag.Models;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class CaptionThemeTests
{
    [Fact]
    public void Apply_BeforeHandleCreation_IsSafe()
    {
        using var form = new Form();
        CaptionThemeController.Apply(form, AppTheme.Dark);
        Assert.False(form.IsHandleCreated);
    }

    [Fact]
    public void Apply_WithHandle_SupportsBothThemes()
    {
        using var form = new Form();
        _ = form.Handle;
        CaptionThemeController.Apply(form, AppTheme.Dark);
        CaptionThemeController.Apply(form, AppTheme.Light);
        Assert.True(form.IsHandleCreated);
    }
}
