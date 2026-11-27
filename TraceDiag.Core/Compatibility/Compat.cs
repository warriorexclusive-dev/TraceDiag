using System.Diagnostics;
using System.Threading.Channels;

namespace TraceDiag;

/// <summary>Small helpers for APIs missing on netstandard2.0 and .NET Framework 4.8.</summary>
internal static class Compat
{
    internal static T Clamp<T>(T value, T min, T max) where T : IComparable<T> =>
        value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;

    internal static long MonotonicMilliseconds() =>
        (long)(Stopwatch.GetTimestamp() * (1000.0 / Stopwatch.Frequency));

    internal static void ThrowIfNull(object? value, string parameterName)
    {
        if (value is null) throw new ArgumentNullException(parameterName);
    }

    internal static async IAsyncEnumerable<T> ReadAllAsync<T>(ChannelReader<T> reader)
    {
        while (await reader.WaitToReadAsync())
            while (reader.TryRead(out var item)) yield return item;
    }

    internal static async Task<T> WithCancellation<T>(Task<T> task, CancellationToken token)
    {
        if (!token.CanBeCanceled) return await task.ConfigureAwait(false);
        var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (token.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), cancelled))
        {
            if (task != await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false))
                throw new OperationCanceledException(token);
        }
        return await task.ConfigureAwait(false);
    }
}
