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

    private static HopStatSnapshot CreateSnapshot() => new(
        1, TestFactories.Address(1), null, 4, 3, 8, 5, 10, 9,
        Strings.Status.Normal, Strings.Diagnosis.PacketSizeLimit);
}
