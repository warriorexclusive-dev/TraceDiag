namespace TraceDiag.Ui;

internal static class UiInvoker
{
    internal static void OnUiThread(Control control, Action action)
    {
        if (control.IsDisposed) return;
        if (control.InvokeRequired) { control.BeginInvoke(action); return; }
        action();
    }
}
