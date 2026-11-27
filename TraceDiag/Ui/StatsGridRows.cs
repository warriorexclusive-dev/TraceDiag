using System.Globalization;
using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

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
        Strings.Ui.ColumnKeyRouteUid => stat.RouteUid <= 0 ? Strings.Ui.Dash : stat.RouteUid.ToString(),
        Strings.Ui.ColumnKeyRouteTarget => string.IsNullOrWhiteSpace(stat.RouteTarget) ? Strings.Ui.Dash : stat.RouteTarget,
        Strings.Ui.ColumnKeyIp => stat.Address.ToString(),
        Strings.Ui.ColumnKeyAsn => string.IsNullOrWhiteSpace(stat.Asn) ? Strings.Ui.Dash : stat.Asn,
        Strings.Ui.ColumnKeyAvgPing => Average(stat),
        Strings.Ui.ColumnKeyJitter => Metric(stat, stat.JitterMs),
        Strings.Ui.ColumnKeyStdDev => Metric(stat, stat.StdDevMs),
        Strings.Ui.ColumnKeyMinPing => FormatPing(stat.MinPingMs, stat.Sent),
        Strings.Ui.ColumnKeyMaxPing => FormatPing(stat.MaxPingMs, stat.Sent),
        Strings.Ui.ColumnKeyLastPing => FormatPing(stat.LastPingMs, stat.Sent),
        Strings.Ui.ColumnKeySent => Counter(stat.Sent),
        Strings.Ui.ColumnKeyReceived => Counter(stat.Received),
        Strings.Ui.ColumnKeyStatus => Strings.T(stat.Status),
        Strings.Ui.ColumnKeyDiagnosis => DiagnosisText.Localize(stat.Diagnosis),
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
        Strings.Ui.ColumnKeyRouteUid,
        Strings.Ui.ColumnKeyRouteTarget,
        Strings.Ui.ColumnKeyIp,
        Strings.Ui.ColumnKeyAsn,
        Strings.Ui.ColumnKeyAvgPing,
        Strings.Ui.ColumnKeyJitter,
        Strings.Ui.ColumnKeyStdDev,
        Strings.Ui.ColumnKeyMinPing,
        Strings.Ui.ColumnKeyMaxPing,
        Strings.Ui.ColumnKeyLastPing,
        Strings.Ui.ColumnKeySent,
        Strings.Ui.ColumnKeyReceived,
        Strings.Ui.ColumnKeyStatus,
        Strings.Ui.ColumnKeyDiagnosis
    ];

    private static string HostName(HopStatSnapshot stat) =>
        string.IsNullOrWhiteSpace(stat.HostName) ? Strings.T(Strings.Ui.NoDns) : stat.HostName!;

    private static string Average(HopStatSnapshot stat) => Metric(stat, stat.AvgPingMs);

    private static string Metric(HopStatSnapshot stat, double value) =>
        stat.Received == 0
            ? Strings.Ui.Dash
            : value.ToString(Strings.Ui.AvgPingFormat, CultureInfo.InvariantCulture);

    private static string Counter(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static void ApplyHighlight(DataGridViewRow row, HopStatSnapshot stat, ThemePalette palette)
    {
        ApplyCellStyle(row.DefaultCellStyle, stat, string.Empty, palette);
        row.Cells[Strings.Ui.ColumnKeyDiagnosis].Style.ForeColor =
            TextColor(stat, Strings.Ui.ColumnKeyDiagnosis, palette);
    }

    private static bool IsInactive(HopStatSnapshot stat) =>
        DiagnosisText.Contains(stat.Diagnosis, Strings.Diagnosis.IcmpDisabled) ||
        DiagnosisText.Contains(stat.Diagnosis, Strings.Diagnosis.IcmpBlocked);

    private static bool HasLossHighlight(HopStatSnapshot stat) =>
        stat.Sent > 0 && (stat.Lost > 0 || !stat.LastPingMs.HasValue);

    private static bool IsDiagnosis(string columnName, HopStatSnapshot stat) =>
        columnName == Strings.Ui.ColumnKeyDiagnosis && stat.Diagnosis != Strings.Diagnosis.None;

    private static string FormatPing(long? value, int sent) =>
        value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : (sent == 0 ? Strings.Ui.Dash : Strings.T(Strings.Ui.NoAnswer));
}
