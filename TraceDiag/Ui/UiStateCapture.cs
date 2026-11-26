using TraceDiag.Models;

namespace TraceDiag.Ui;

internal static class UiStateCapture
{
    internal static UiState Create(Form form, MainControls controls, FullScreenState fullScreen)
    {
        var state = CaptureInputs(controls);
        var window = CaptureWindow(form, fullScreen);
        state.WindowState = window.State;
        state.Bounds = window.Bounds;
        state.ColumnWidths = CaptureColumns(controls.Stats);
        return state;
    }

    private static UiState CaptureInputs(MainControls controls) => new()
    {
        RemoteHost = controls.RemoteHost.Text.Trim(),
        MaxPacketSize = (int)controls.MaxPacketSize.Value,
        Attempts = (int)controls.Attempts.Value,
        LossDelayMs = (int)controls.LossDelay.Value,
        OutputFontSize = (float)controls.FontSize.Value,
        DynamicPacketSize = controls.DynamicPacketSize.Checked,
        DarkTheme = controls.DarkTheme.Checked
    };

    private static (string State, UiWindowBounds Bounds) CaptureWindow(Form form, FullScreenState fullScreen)
    {
        if (form.FormBorderStyle == FormBorderStyle.None)
            return (Normalize(fullScreen.PreviousState).ToString(), ToBounds(fullScreen.PreviousBounds));
        var state = Normalize(form.WindowState);
        var rect = state == FormWindowState.Normal ? form.Bounds : form.RestoreBounds;
        if (rect.Width <= 0 || rect.Height <= 0) rect = form.Bounds;
        return (state.ToString(), ToBounds(rect));
    }

    private static FormWindowState Normalize(FormWindowState state) =>
        state == FormWindowState.Minimized ? FormWindowState.Normal : state;

    private static UiWindowBounds ToBounds(Rectangle rect) => new()
    {
        X = rect.X,
        Y = rect.Y,
        X2 = rect.Right,
        Y2 = rect.Bottom
    };

    private static Dictionary<string, int> CaptureColumns(DataGridView grid) =>
        grid.Columns.Cast<DataGridViewColumn>().ToDictionary(
            column => column.Name,
            column => Math.Clamp(column.Width, UiDefaults.MinColumnWidth, UiDefaults.MaxColumnWidth));
}
