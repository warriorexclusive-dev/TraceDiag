namespace TraceDiag.Ui;

internal static class UiDefaults
{
    internal const float MinFontSize = 8f;
    internal const float MaxFontSize = 24f;
    internal const int DefaultAttempts = 100;

    internal static decimal ClampFontSize(float value) =>
        (decimal)Math.Clamp(value, MinFontSize, MaxFontSize);
}
