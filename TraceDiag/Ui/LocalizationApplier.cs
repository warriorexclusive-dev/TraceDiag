namespace TraceDiag.Ui;

/// <summary>Applies the selected language to every tagged control and grid header.</summary>
internal static class LocalizationApplier
{
    internal static void Apply(Form form, MainControls controls, Form? settings = null)
    {
        form.Text = Strings.T(Strings.App.Title);
        ApplyControl(form);
        if (settings is not null) ApplyControl(settings);
        UiText.Refresh(controls.Run);
        UiText.RefreshLanguage(controls.Language);
        foreach (DataGridViewColumn column in controls.Stats.Columns) UiText.Refresh(column);
        controls.Stats.Invalidate();
    }

    private static void ApplyControl(Control control)
    {
        UiText.Refresh(control);
        foreach (Control child in control.Controls) ApplyControl(child);
    }
}
