using System.Windows.Forms;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class OutputBufferControllerTests
{
    [Fact]
    public void Append_DropsFirstLineWhenOverLimit()
    {
        using var output = new TextBox();
        var buffer = new OutputBufferController(3);
        foreach (var line in new[] { "one", "two", "three", "four" }) buffer.Append(output, line);
        Assert.Equal(3, buffer.Count);
        Assert.DoesNotContain("one", output.Text);
        Assert.Contains("four", output.Text);
    }

    [Fact]
    public void Clear_RemovesBufferedLines()
    {
        using var output = new TextBox();
        var buffer = new OutputBufferController(3);
        buffer.Append(output, "line");
        buffer.Clear();
        Assert.Equal(0, buffer.Count);
    }
}
