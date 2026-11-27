namespace TraceDiag.Console;

/// <summary>Progress adapter that reports on the calling thread to preserve output order.</summary>
internal sealed class SyncProgress<T>(Action<T> action) : IProgress<T>
{
    public void Report(T value) => action(value);
}
