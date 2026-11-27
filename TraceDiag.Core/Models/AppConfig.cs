using TraceDiag.Ui;

namespace TraceDiag.Models;

public sealed class AppConfig
{
    public float OutputFontSize { get; set; } = UiDefaults.DefaultOutputFontSize;
    public AppTheme Theme { get; set; } = AppTheme.Dark;
}
