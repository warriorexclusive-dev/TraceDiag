using System.Threading.Channels;

namespace TraceDiag.Services.Logging;

/// <summary>Writes log lines on a dedicated worker instead of the probing threads.</summary>
internal sealed class AsyncLogWriter : IDisposable
{
    private readonly Channel<string> _queue = Channel.CreateUnbounded<string>();
    private readonly LogRotationController _rotation;
    private readonly Task _worker;
    private int _pending;

    internal AsyncLogWriter(string logFilePath)
    {
        LogFileWriter.EnsureDirectory(logFilePath);
        _rotation = new LogRotationController(logFilePath);
        _worker = Task.Run(ConsumeAsync);
    }

    internal string CurrentPath => _rotation.CurrentPath;

    internal void Write(string line)
    {
        Interlocked.Increment(ref _pending);
        if (!_queue.Writer.TryWrite(line)) Interlocked.Decrement(ref _pending);
    }

    internal void Flush()
    {
        var deadline = Compat.MonotonicMilliseconds() + 5000;
        while (Volatile.Read(ref _pending) > 0 && Compat.MonotonicMilliseconds() < deadline)
            Thread.Sleep(1);
    }

    public void Dispose()
    {
        _queue.Writer.TryComplete();
        _worker.Wait(TimeSpan.FromSeconds(5));
    }

    private async Task ConsumeAsync()
    {
        await foreach (var line in Compat.ReadAllAsync(_queue.Reader))
        {
            try { _rotation.Append(line); }
            catch { /* Logging must never break diagnostics flow. */ }
            finally { Interlocked.Decrement(ref _pending); }
        }
    }
}
