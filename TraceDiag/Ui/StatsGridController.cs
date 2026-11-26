using TraceDiag.Models;

namespace TraceDiag.Ui;

/// <summary>Renders a bounded snapshot history in virtual mode.</summary>
internal sealed class StatsGridController
{
    private readonly DataGridView _grid;
    private IReadOnlyList<HopStatSnapshot> _items = Array.Empty<HopStatSnapshot>();
    private ThemePalette _palette = ThemePalette.Dark;

    internal StatsGridController(DataGridView grid)
    {
        _grid = grid;
        _grid.VirtualMode = true;
        _grid.CellValueNeeded += CellValueNeeded;
        _grid.CellFormatting += CellFormatting;
    }

    internal void SetPalette(ThemePalette palette)
    {
        _palette = palette;
        _grid.Invalidate();
    }

    internal void Render(IReadOnlyList<HopStatSnapshot> items)
    {
        _items = items;
        _grid.RowCount = items.Count;
        _grid.Invalidate();
    }

    private void CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
    {
        if (!TryGet(e.RowIndex, e.ColumnIndex, out var stat, out var column)) return;
        e.Value = StatsGridRows.Value(stat, column);
    }

    private void CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (!TryGet(e.RowIndex, e.ColumnIndex, out var stat, out var column)) return;
        StatsGridRows.ApplyCellStyle(e.CellStyle, stat, column, _palette);
    }

    private bool TryGet(int row, int column, out HopStatSnapshot stat, out string name)
    {
        stat = null!;
        name = string.Empty;
        if (row < 0 || row >= _items.Count) return false;
        if (column < 0 || column >= _grid.Columns.Count) return false;
        stat = _items[row];
        name = _grid.Columns[column].Name;
        return true;
    }
}
