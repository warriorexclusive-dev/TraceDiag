namespace TraceDiag.Ui;

internal static class RunButtonState
{
    internal static void Begin(Button button) => Apply(button, Strings.Ui.StopButton, true);

    internal static void Stopping(Button button) => Apply(button, Strings.Ui.Stopping, false);

    internal static void End(Button button) => Apply(button, Strings.Ui.RunButton, true);

    private static void Apply(Button button, string text, bool enabled)
    {
        UiText.Set(button, text);
        button.Enabled = enabled;
    }
}
