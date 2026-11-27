using System.Windows.Forms;
using TraceDiag.Models;
using TraceDiag.Tests.TestData;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class StatsGridTests
{
    [Fact]
    public void Create_AddsDiagnosisColumn()
    {
        using var grid = StatsGridFactory.Create();
        Assert.True(grid.Columns.Contains(Strings.Ui.ColumnKeyDiagnosis));
    }

    [Fact]
    public void AddRow_WritesDiagnosisCell()
    {
        using var grid = StatsGridFactory.Create();
        StatsGridRows.AddRow(grid, CreateSnapshot());
        Assert.Equal(Strings.Diagnosis.PacketSizeLimit, grid.Rows[0].Cells[Strings.Ui.ColumnKeyDiagnosis].Value);
    }

    [Fact]
    public void AddRow_WritesEveryDiagnosis()
    {
        using var grid = StatsGridFactory.Create();
        var combined = string.Join(Strings.Diagnosis.Separator,
            Strings.Diagnosis.PacketSizeLimit, Strings.Diagnosis.AntiFlood,
            Strings.Diagnosis.EcmpBalancing);
        StatsGridRows.AddRow(grid, CreateSnapshot(combined));
        Assert.Equal(combined, grid.Rows[0].Cells[Strings.Ui.ColumnKeyDiagnosis].Value);
    }

    [Fact]
    public void ApplyCellStyle_UsesPaletteTextForActiveHop()
    {
        var style = new DataGridViewCellStyle();
        var stat = CreateSnapshot(Strings.Diagnosis.None) with { Sent = 1, Received = 1, LastPingMs = 1 };
        StatsGridRows.ApplyCellStyle(style, stat, Strings.Ui.ColumnKeyIp, ThemePalette.Dark);
        Assert.Equal(ThemePalette.Dark.Text, style.ForeColor);
    }

    [Fact]
    public void ApplyCellStyle_UsesGrayForIcmpDisabledHop()
    {
        var style = new DataGridViewCellStyle();
        StatsGridRows.ApplyCellStyle(style, CreateSnapshot(Strings.Diagnosis.IcmpDisabled),
            Strings.Ui.ColumnKeyIp, ThemePalette.Dark);
        Assert.Equal(ThemePalette.Dark.DisabledText, style.ForeColor);
    }

    [Fact]
    public void ApplyCellStyle_UsesGrayForLightThemeIcmpDisabledHop()
    {
        var style = new DataGridViewCellStyle();
        StatsGridRows.ApplyCellStyle(style, CreateSnapshot(Strings.Diagnosis.IcmpDisabled),
            Strings.Ui.ColumnKeyIp, ThemePalette.Light);
        Assert.Equal(ThemePalette.Light.DisabledText, style.ForeColor);
    }

    private static HopStatSnapshot CreateSnapshot(string diagnosis = Strings.Diagnosis.PacketSizeLimit) => new(
        1, TestFactories.Address(1), null, 4, 3, 8, 5, 10, 9,
        Strings.Status.Normal, diagnosis);
}
