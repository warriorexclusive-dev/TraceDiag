using TraceDiag.Ui;

namespace TraceDiag.Models;

/// <summary>Serializable snapshot of the application window and input controls.</summary>
internal sealed class UiState
{
    public string RemoteHost { get; set; } = string.Empty;
    public int MaxPacketSize { get; set; } = UiDefaults.DefaultMaxPacketSize;
    public int Attempts { get; set; } = UiDefaults.DefaultAttempts;
    public int LossDelayMs { get; set; } = UiDefaults.DefaultLossDelayMs;
    public float OutputFontSize { get; set; } = UiDefaults.DefaultOutputFontSize;
    public bool DynamicPacketSize { get; set; } = true;
    public bool DontFragment { get; set; }
    public bool MonitoringMode { get; set; }
    public bool DarkTheme { get; set; } = true;
    public int StatsHeightPercent { get; set; } = UiDefaults.DefaultStatsPercent;
    public string WindowState { get; set; } = UiDefaults.NormalWindowState;
    public UiWindowBounds Bounds { get; set; } = new();
    public Dictionary<string, int> ColumnWidths { get; set; } = new();
}

internal sealed class UiWindowBounds
{
    public int X { get; set; }
    public int Y { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }
}
