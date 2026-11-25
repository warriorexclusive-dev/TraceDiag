namespace TraceDiag.Ui;

internal sealed class FullScreenState
{
    internal FormWindowState PreviousState { get; set; } = FormWindowState.Normal;
    internal FormBorderStyle PreviousBorder { get; set; } = FormBorderStyle.Sizable;
    internal Rectangle PreviousBounds { get; set; }
}

internal static class FullScreenController
{
    internal static bool HandleKey(Form form, KeyEventArgs e, FullScreenState state)
    {
        if (e.KeyCode == Keys.F11) { Toggle(form, state); return true; }
        if (e.KeyCode == Keys.Escape && form.FormBorderStyle == FormBorderStyle.None)
        {
            Exit(form, state);
            return true;
        }
        return false;
    }

    private static void Toggle(Form form, FullScreenState state)
    {
        if (form.FormBorderStyle == FormBorderStyle.None) Exit(form, state);
        else Enter(form, state);
    }

    private static void Enter(Form form, FullScreenState state)
    {
        state.PreviousState = form.WindowState;
        state.PreviousBorder = form.FormBorderStyle;
        state.PreviousBounds = form.Bounds;
        form.FormBorderStyle = FormBorderStyle.None;
        form.WindowState = FormWindowState.Normal;
        form.Bounds = Screen.FromControl(form).Bounds;
    }

    private static void Exit(Form form, FullScreenState state)
    {
        form.FormBorderStyle = state.PreviousBorder;
        form.WindowState = state.PreviousState;
        if (state.PreviousState != FormWindowState.Normal) return;
        if (state.PreviousBounds.Width <= 0 || state.PreviousBounds.Height <= 0) return;
        form.Bounds = state.PreviousBounds;
    }
}
