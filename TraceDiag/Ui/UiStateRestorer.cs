using System.Windows.Forms;
using TraceDiag.Models;

namespace TraceDiag.Ui;

internal static class UiStateRestorer
{
    internal static void Apply(Form form, MainControls controls, UiState state, AppConfig config)
    {
        ApplyInputs(controls, state);
        ApplyConfig(config, controls);
        ApplyColumns(controls.Stats, state.ColumnWidths);
        SplitterController.Apply(controls.Splitter, state.StatsHeightPercent);
        ApplyWindow(form, state);
    }

    private static void ApplyInputs(MainControls controls, UiState state)
    {
        var policy = new PacketSizePolicyController(controls, () => true);
        controls.DontFragment.Checked = state.DontFragment;
        policy.ApplyDontFragment();
        controls.RemoteHost.Text = state.RemoteHost ?? string.Empty;
        controls.MaxPacketSize.Value = Clamp(state.MaxPacketSize, controls.MaxPacketSize);
        controls.Attempts.Value = Clamp(state.Attempts, controls.Attempts);
        controls.LossDelay.Value = Clamp(state.LossDelayMs, controls.LossDelay);
        controls.FontSize.Value = Clamp((decimal)state.OutputFontSize, controls.FontSize);
        controls.DynamicPacketSize.Checked = state.DynamicPacketSize && !state.DontFragment;
        controls.MonitoringMode.Checked = state.MonitoringMode;
        controls.DarkTheme.Checked = state.DarkTheme;
        policy.ApplyDontFragment();
    }

    private static decimal Clamp(decimal value, NumericUpDown input) =>
        Compat.Clamp(value, input.Minimum, input.Maximum);

    private static void ApplyConfig(AppConfig config, MainControls controls)
    {
        config.OutputFontSize = (float)controls.FontSize.Value;
        config.Theme = controls.DarkTheme.Checked ? AppTheme.Dark : AppTheme.Light;
    }

    private static void ApplyColumns(DataGridView grid, Dictionary<string, int>? widths)
    {
        if (widths is null) return;
        foreach (var pair in widths)
        {
            var column = grid.Columns[pair.Key];
            if (column is not null) column.Width = Compat.Clamp(pair.Value, UiDefaults.MinColumnWidth, UiDefaults.MaxColumnWidth);
        }
    }

    private static void ApplyWindow(Form form, UiState state)
    {
        form.StartPosition = FormStartPosition.Manual;
        if (state.WindowState == nameof(FormWindowState.Maximized))
        {
            form.WindowState = FormWindowState.Maximized;
            return;
        }
        form.WindowState = FormWindowState.Normal;
        form.Bounds = ToVisibleRectangle(state.Bounds);
    }

    private static Rectangle ToVisibleRectangle(UiWindowBounds? bounds)
    {
        var source = bounds ?? new UiWindowBounds();
        var area = SystemInformation.VirtualScreen;
        var width = Compat.Clamp(source.X2 - source.X, UiDefaults.MinWindowWidth, Math.Max(UiDefaults.MinWindowWidth, area.Width));
        var height = Compat.Clamp(source.Y2 - source.Y, UiDefaults.MinWindowHeight, Math.Max(UiDefaults.MinWindowHeight, area.Height));
        return new Rectangle(
            Compat.Clamp(source.X, area.Left, Math.Max(area.Left, area.Right - width)),
            Compat.Clamp(source.Y, area.Top, Math.Max(area.Top, area.Bottom - height)),
            width,
            height);
    }
}
