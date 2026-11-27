using TraceDiag.Localization;
using TraceDiag.Ui;

namespace TraceDiag.Models;

public sealed class AppConfig
{
    public float OutputFontSize { get; set; } = UiDefaults.DefaultOutputFontSize;
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public Language Language { get; set; } = Language.En;
    public int IconSizePx { get; set; } = UiDefaults.DefaultIconSizePx;
    public string AltDnsFilePath { get; set; } = string.Empty;
}
