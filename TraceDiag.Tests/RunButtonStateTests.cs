using System.Windows.Forms;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class RunButtonStateTests
{
    [Fact]
    public void Begin_ShowsStopAndKeepsButtonEnabled()
    {
        using var button = CreateButton(Strings.Ui.RunButton);
        RunButtonState.Begin(button);
        Assert.Equal(Strings.Ui.StopButton, button.Text);
        Assert.True(button.Enabled);
    }

    [Fact]
    public void Stopping_DisablesButtonAndShowsStopping()
    {
        using var button = CreateButton(Strings.Ui.StopButton);
        RunButtonState.Stopping(button);
        Assert.Equal(Strings.Ui.Stopping, button.Text);
        Assert.False(button.Enabled);
    }

    [Fact]
    public void End_RestoresRunButton()
    {
        using var button = CreateButton(Strings.Ui.Stopping);
        RunButtonState.End(button);
        Assert.Equal(Strings.Ui.RunButton, button.Text);
        Assert.True(button.Enabled);
    }

    private static Button CreateButton(string text) => new() { Text = text };
}
