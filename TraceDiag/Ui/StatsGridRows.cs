using System.Globalization;
using TraceDiag.Models;

namespace TraceDiag.Ui;

internal static class StatsGridRows
{
    internal static void AddRow(DataGridView grid, HopStatSnapshot stat, ThemePalette? palette = null)
    {
        var rowIndex = grid.Rows.Add(CreateCells(stat));
        ApplyHighlight(grid.Rows[rowIndex], stat, palette ?? ThemePalette.Dark);
    }

    internal static object Value(HopStatSnapshot stat, string columnName) => columnName switch
    {
        Strings.Ui.ColumnKeyHostName => HostName(stat),
        Strings.Ui.ColumnKeyIp => stat.Address.ToString(),
        Strings.Ui.ColumnKeyAvgPing => Average(stat),
        Strings.Ui.ColumnKeyMinPing => FormatPing(stat.MinPingMs, stat.Sent),
        Strings.Ui.ColumnKeyMaxPing => FormatPing(stat.MaxPingMs, stat.Sent),
        Strings.Ui.ColumnKeyLastPing => FormatPing(stat.LastPingMs, stat.Sent),
        Strings.Ui.ColumnKeySent => Counter(stat.Sent),
        Strings.Ui.ColumnKeyReceived => Counter(stat.Received),
        Strings.Ui.ColumnKeyStatus => stat.Status,
        Strings.Ui.ColumnKeyDiagnosis => stat.Diagnosis,
        _ => string.Empty
    };

    internal static void ApplyCellStyle(DataGridViewCellStyle style, HopStatSnapshot stat,
        string columnName, ThemePalette palette) => style.ForeColor = TextColor(stat, columnName, palette);

    private static Color TextColor(HopStatSnapshot stat, string columnName, ThemePalette palette)
    {
        if (IsInactive(stat)) return palette.DisabledText;
        if (HasLossHighlight(stat) || IsDiagnosis(columnName, stat)) return palette.WarningText;
        return palette.Text;
    }

    private static object[] CreateCells(HopStatSnapshot stat) =>
        Columns.Select(name => Value(stat, name)).ToArray();

    private static string[] Columns =>
    [
        Strings.Ui.ColumnKeyHostName,
        Strings.Ui.ColumnKeyIp,
        Strings.Ui.ColumnKeyAvgPing,
        Strings.Ui.ColumnKeyMinPing,
        Strings.Ui.ColumnKeyMaxPing,
        Strings.Ui.ColumnKeyLastPing,
        Strings.Ui.ColumnKeySent,
        Strings.Ui.ColumnKeyReceived,
        Strings.Ui.ColumnKeyStatus,
        Strings.Ui.ColumnKeyDiagnosis
    ];

    private static string HostName(HopStatSnapshot stat) =>
        string.IsNullOrWhiteSpace(stat.HostName) ? Strings.Ui.NoDns : stat.HostName!;

    private static string Average(HopStatSnapshot stat) =>
        stat.Received == 0
            ? Strings.Ui.Dash
            : stat.AvgPingMs.ToString(Strings.Ui.AvgPingFormat, CultureInfo.InvariantCulture);

    private static string Counter(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static void ApplyHighlight(DataGridViewRow row, HopStatSnapshot stat, ThemePalette palette)
    {
        ApplyCellStyle(row.DefaultCellStyle, stat, string.Empty, palette);
        row.Cells[Strings.Ui.ColumnKeyDiagnosis].Style.ForeColor =
            TextColor(stat, Strings.Ui.ColumnKeyDiagnosis, palette);
    }

    private static bool IsInactive(HopStatSnapshot stat) =>
        stat.Diagnosis == Strings.Diagnosis.IcmpDisabled;

    private static bool HasLossHighlight(HopStatSnapshot stat) =>
        stat.Sent > 0 && (stat.Lost > 0 || !stat.LastPingMs.HasValue);

    private static bool IsDiagnosis(string columnName, HopStatSnapshot stat) =>
        columnName == Strings.Ui.ColumnKeyDiagnosis && stat.Diagnosis != Strings.Diagnosis.None;

    private static string FormatPing(long? value, int sent) =>
        value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : (sent == 0 ? Strings.Ui.Dash : Strings.Ui.NoAnswer);
}
