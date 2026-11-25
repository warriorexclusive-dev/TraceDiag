using TraceDiag.Models;

namespace TraceDiag.Ui;

internal static class ThemeApplier
{
    internal static void Apply(Form form, AppTheme theme) => Apply(form, PaletteFor(theme));

    internal static void Apply(Form form, ThemePalette palette)
    {
        form.BackColor = palette.Window;
        form.ForeColor = palette.Text;
        ApplyControl(form, palette);
    }

    private static ThemePalette PaletteFor(AppTheme theme) =>
        theme == AppTheme.Dark ? ThemePalette.Dark : ThemePalette.Light;

    private static void ApplyControl(Control control, ThemePalette palette)
    {
        ApplyOwnColors(control, palette);
        foreach (Control child in control.Controls) ApplyControl(child, palette);
    }

    private static void ApplyOwnColors(Control control, ThemePalette palette)
    {
        switch (control)
        {
            case TextBox text: ApplyTextBox(text, palette); break;
            case NumericUpDown numeric: ApplyNumeric(numeric, palette); break;
            case DataGridView grid: ApplyGrid(grid, palette); break;
            case CheckBox check: ApplyCheckBox(check, palette); break;
            case Button button: ApplyButton(button, palette); break;
            case Label label: ApplyLabel(label, palette); break;
            case Panel panel: ApplyPanel(panel, palette); break;
        }
    }

    private static void ApplyTextBox(TextBox text, ThemePalette palette)
    {
        text.BackColor = palette.Input;
        text.ForeColor = palette.Text;
        text.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void ApplyNumeric(NumericUpDown numeric, ThemePalette palette)
    {
        numeric.BackColor = palette.Input;
        numeric.ForeColor = palette.Text;
        numeric.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void ApplyGrid(DataGridView grid, ThemePalette palette)
    {
        grid.EnableHeadersVisualStyles = false;
        grid.BackgroundColor = palette.Grid;
        grid.GridColor = palette.GridLine;
        grid.ForeColor = palette.Text;
        ApplyGridStyles(grid, palette);
    }

    private static void ApplyGridStyles(DataGridView grid, ThemePalette palette)
    {
        ApplyCellStyle(grid.DefaultCellStyle, palette.Grid, palette.Text);
        ApplyCellStyle(grid.AlternatingRowsDefaultCellStyle, palette.Grid, palette.Text);
        ApplyCellStyle(grid.ColumnHeadersDefaultCellStyle, palette.Header, palette.HeaderText);
        ApplyCellStyle(grid.RowHeadersDefaultCellStyle, palette.Header, palette.HeaderText);
        grid.DefaultCellStyle.SelectionBackColor = palette.Accent;
        grid.DefaultCellStyle.SelectionForeColor = palette.AccentText;
    }

    private static void ApplyCellStyle(DataGridViewCellStyle style, Color back, Color fore)
    {
        style.BackColor = back;
        style.ForeColor = fore;
    }

    private static void ApplyCheckBox(CheckBox check, ThemePalette palette)
    {
        check.BackColor = palette.Surface;
        check.ForeColor = palette.Text;
    }

    private static void ApplyButton(Button button, ThemePalette palette)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = palette.Accent;
        button.ForeColor = palette.AccentText;
        button.FlatAppearance.BorderColor = palette.GridLine;
    }

    private static void ApplyLabel(Label label, ThemePalette palette)
    {
        label.BackColor = Color.Transparent;
        label.ForeColor = palette.Text;
    }

    private static void ApplyPanel(Panel panel, ThemePalette palette)
    {
        panel.BackColor = palette.Surface;
        panel.ForeColor = palette.Text;
    }
}
