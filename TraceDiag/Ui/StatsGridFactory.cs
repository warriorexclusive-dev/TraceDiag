namespace TraceDiag.Ui;

internal static class StatsGridFactory
{
    internal static DataGridView Create()
    {
        var grid = CreateGrid();
        AddColumns(grid);
        SetWeights(grid);
        return grid;
    }

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        RowHeadersVisible = false,
        BackgroundColor = Color.White,
        Font = new Font(Strings.Ui.GridFontFamily, 9f)
    };

    private static void AddColumns(DataGridView grid)
    {
        grid.Columns.Add(Strings.Ui.ColumnKeyHostName, Strings.Ui.ColumnHostName);
        grid.Columns.Add(Strings.Ui.ColumnKeyIp, Strings.Ui.ColumnIp);
        grid.Columns.Add(Strings.Ui.ColumnKeyAvgPing, Strings.Ui.ColumnAvgPing);
        grid.Columns.Add(Strings.Ui.ColumnKeyMinPing, Strings.Ui.ColumnMinPing);
        grid.Columns.Add(Strings.Ui.ColumnKeyMaxPing, Strings.Ui.ColumnMaxPing);
        grid.Columns.Add(Strings.Ui.ColumnKeyLastPing, Strings.Ui.ColumnLastPing);
        grid.Columns.Add(Strings.Ui.ColumnKeySent, Strings.Ui.ColumnSent);
        grid.Columns.Add(Strings.Ui.ColumnKeyReceived, Strings.Ui.ColumnReceived);
        grid.Columns.Add(Strings.Ui.ColumnKeyStatus, Strings.Ui.ColumnStatus);
    }

    private static void SetWeights(DataGridView grid)
    {
        grid.Columns[Strings.Ui.ColumnKeyHostName]!.FillWeight = 150;
        grid.Columns[Strings.Ui.ColumnKeyIp]!.FillWeight = 95;
        grid.Columns[Strings.Ui.ColumnKeyAvgPing]!.FillWeight = 85;
        grid.Columns[Strings.Ui.ColumnKeyMinPing]!.FillWeight = 80;
        grid.Columns[Strings.Ui.ColumnKeyMaxPing]!.FillWeight = 80;
        grid.Columns[Strings.Ui.ColumnKeyLastPing]!.FillWeight = 90;
        grid.Columns[Strings.Ui.ColumnKeySent]!.FillWeight = 70;
        grid.Columns[Strings.Ui.ColumnKeyReceived]!.FillWeight = 70;
        grid.Columns[Strings.Ui.ColumnKeyStatus]!.FillWeight = 140;
    }
}
