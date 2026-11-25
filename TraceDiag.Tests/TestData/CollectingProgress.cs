namespace TraceDiag.Tests.TestData;

internal sealed class CollectingProgress<T> : IProgress<T>
{
    internal List<T> Values { get; } = new();

    public void Report(T value) => Values.Add(value);
}
