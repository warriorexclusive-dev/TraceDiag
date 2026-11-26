using System.Windows.Forms;
using TraceDiag.Models;
using TraceDiag.Ui;

namespace TraceDiag.Services;

internal static class UiStateValidator
{
    internal static bool IsValid(UiState state) =>
        IsValidInputs(state) &&
        IsValidWindow(state.WindowState, state.Bounds) &&
        IsValidColumns(state.ColumnWidths);

    private static bool IsValidInputs(UiState state) =>
        state.MaxPacketSize >= 64 && state.MaxPacketSize <= 65534 &&
        state.Attempts >= 1 && state.Attempts <= 1000 &&
        state.LossDelayMs >= 0 && state.LossDelayMs <= UiDefaults.MaxLossDelayMs &&
        state.OutputFontSize >= UiDefaults.MinFontSize && state.OutputFontSize <= UiDefaults.MaxFontSize;

    private static bool IsValidWindow(string windowState, UiWindowBounds? bounds) =>
        windowState is nameof(FormWindowState.Normal) or nameof(FormWindowState.Maximized) &&
        bounds is not null &&
        bounds.X2 - bounds.X >= UiDefaults.MinWindowWidth &&
        bounds.Y2 - bounds.Y >= UiDefaults.MinWindowHeight;

    private static bool IsValidColumns(Dictionary<string, int>? widths) =>
        widths is not null &&
        widths.All(pair => !string.IsNullOrWhiteSpace(pair.Key) &&
                           pair.Value >= UiDefaults.MinColumnWidth &&
                           pair.Value <= UiDefaults.MaxColumnWidth);
}
