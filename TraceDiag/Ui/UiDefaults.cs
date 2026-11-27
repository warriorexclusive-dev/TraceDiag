using TraceDiag.Models;

namespace TraceDiag.Ui;

internal static class UiDefaults
{
    internal const float MinFontSize = 8f;
    internal const float MaxFontSize = 24f;
    internal const float DefaultOutputFontSize = 9.5f;
    internal const int MinPacketSize = ProbeLimits.MinPacketSize;
    internal const int MaxPacketSize = ProbeLimits.MaxPacketSize;
    internal const int DefaultMaxPacketSize = ProbeLimits.DontFragmentPacketSize;
    internal const int DefaultAttempts = 100;
    internal const int DefaultLossDelayMs = 1000;
    internal const int MaxLossDelayMs = 60000;
    internal const int MinColumnWidth = 40;
    internal const int MaxColumnWidth = 1000;
    internal const int MinWindowWidth = 800;
    internal const int MinWindowHeight = 600;
    internal const string NormalWindowState = "Normal";

    internal static decimal ClampFontSize(float value) =>
        (decimal)Math.Clamp(value, MinFontSize, MaxFontSize);
}
