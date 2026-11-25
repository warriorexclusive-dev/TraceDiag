namespace TraceDiag.Ui;

internal static class UiDefaults
{
    internal const float MinFontSize = 8f;
    internal const float MaxFontSize = 24f;

    internal static decimal ClampFontSize(float value) =>
        (decimal)Math.Clamp(value, MinFontSize, MaxFontSize);
}
