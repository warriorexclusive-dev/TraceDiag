using System.Globalization;
using TraceDiag.Models;

namespace TraceDiag.Ui;

internal static class StatsGridRows
{
    internal static void AddRow(DataGridView grid, HopStatSnapshot stat)
    {
        var rowIndex = grid.Rows.Add(CreateCells(stat));
        ApplyHighlight(grid.Rows[rowIndex], stat);
    }

    private static object[] CreateCells(HopStatSnapshot stat)
    {
        return IdentityCells(stat)
            .Concat(PingCells(stat))
            .Concat(CounterCells(stat))
            .Append(stat.Status)
            .ToArray();
    }

    private static object[] IdentityCells(HopStatSnapshot stat)
    {
        return
        [
            string.IsNullOrWhiteSpace(stat.HostName) ? Strings.Ui.NoDns : stat.HostName,
            stat.Address.ToString()
        ];
    }

    private static object[] PingCells(HopStatSnapshot stat)
    {
        return
        [
            stat.Received == 0
                ? Strings.Ui.Dash
                : stat.AvgPingMs.ToString(Strings.Ui.AvgPingFormat, CultureInfo.InvariantCulture),
            FormatPing(stat.MinPingMs, stat.Sent),
            FormatPing(stat.MaxPingMs, stat.Sent),
            FormatPing(stat.LastPingMs, stat.Sent)
        ];
    }

    private static object[] CounterCells(HopStatSnapshot stat)
    {
        return
        [
            stat.Sent.ToString(CultureInfo.InvariantCulture),
            stat.Received.ToString(CultureInfo.InvariantCulture)
        ];
    }

    private static void ApplyHighlight(DataGridViewRow row, HopStatSnapshot stat)
    {
        if (stat.Sent > 0 && (stat.Lost > 0 || !stat.LastPingMs.HasValue))
            row.DefaultCellStyle.ForeColor = Color.Firebrick;
    }

    private static string FormatPing(long? value, int sent)
    {
        return value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : (sent == 0 ? Strings.Ui.Dash : Strings.Ui.NoAnswer);
    }
}
