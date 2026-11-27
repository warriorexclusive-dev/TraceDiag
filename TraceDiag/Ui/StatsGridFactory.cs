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
        AddColumn(grid, Strings.Ui.ColumnKeyHostName, Strings.Ui.ColumnHostName);
        AddColumn(grid, Strings.Ui.ColumnKeyIp, Strings.Ui.ColumnIp);
        AddColumn(grid, Strings.Ui.ColumnKeyAvgPing, Strings.Ui.ColumnAvgPing);
        AddColumn(grid, Strings.Ui.ColumnKeyJitter, Strings.Ui.ColumnJitter);
        AddColumn(grid, Strings.Ui.ColumnKeyStdDev, Strings.Ui.ColumnStdDev);
        AddColumn(grid, Strings.Ui.ColumnKeyMinPing, Strings.Ui.ColumnMinPing);
        AddColumn(grid, Strings.Ui.ColumnKeyMaxPing, Strings.Ui.ColumnMaxPing);
        AddColumn(grid, Strings.Ui.ColumnKeyLastPing, Strings.Ui.ColumnLastPing);
        AddColumn(grid, Strings.Ui.ColumnKeySent, Strings.Ui.ColumnSent);
        AddColumn(grid, Strings.Ui.ColumnKeyReceived, Strings.Ui.ColumnReceived);
        AddColumn(grid, Strings.Ui.ColumnKeyStatus, Strings.Ui.ColumnStatus);
        AddColumn(grid, Strings.Ui.ColumnKeyDiagnosis, Strings.Ui.ColumnDiagnosis);
    }

    private static void AddColumn(DataGridView grid, string name, string canonical)
    {
        var column = new DataGridViewTextBoxColumn { Name = name };
        UiText.Set(column, canonical);
        grid.Columns.Add(column);
    }

    private static void SetWeights(DataGridView grid)
    {
        grid.Columns[Strings.Ui.ColumnKeyHostName]!.FillWeight = 150;
        grid.Columns[Strings.Ui.ColumnKeyIp]!.FillWeight = 95;
        grid.Columns[Strings.Ui.ColumnKeyAvgPing]!.FillWeight = 85;
        grid.Columns[Strings.Ui.ColumnKeyJitter]!.FillWeight = 75;
        grid.Columns[Strings.Ui.ColumnKeyStdDev]!.FillWeight = 75;
        grid.Columns[Strings.Ui.ColumnKeyMinPing]!.FillWeight = 80;
        grid.Columns[Strings.Ui.ColumnKeyMaxPing]!.FillWeight = 80;
        grid.Columns[Strings.Ui.ColumnKeyLastPing]!.FillWeight = 90;
        grid.Columns[Strings.Ui.ColumnKeySent]!.FillWeight = 70;
        grid.Columns[Strings.Ui.ColumnKeyReceived]!.FillWeight = 70;
        grid.Columns[Strings.Ui.ColumnKeyStatus]!.FillWeight = 140;
        grid.Columns[Strings.Ui.ColumnKeyDiagnosis]!.FillWeight = 150;
    }
}
