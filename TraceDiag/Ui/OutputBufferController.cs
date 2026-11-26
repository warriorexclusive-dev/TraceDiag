namespace TraceDiag.Ui;

/// <summary>Bounded console output buffer, independent of the run mode.</summary>
internal sealed class OutputBufferController
{
    internal const int DefaultMaxLines = 10_000;
    private readonly Queue<string> _lines = new();

    internal OutputBufferController(int maxLines = DefaultMaxLines) =>
        MaxLines = Math.Max(1, maxLines);

    internal int MaxLines { get; }
    internal int Count => _lines.Count;

    internal void Clear() => _lines.Clear();

    internal void Append(TextBox output, string line)
    {
        _lines.Enqueue(line);
        if (_lines.Count > MaxLines)
        {
            _lines.Dequeue();
            Render(output);
            return;
        }
        output.AppendText(line + Environment.NewLine);
    }

    internal void Render(TextBox output)
    {
        output.Text = string.Join(Environment.NewLine, _lines) + Environment.NewLine;
        output.SelectionStart = output.TextLength;
        output.ScrollToCaret();
    }
}
