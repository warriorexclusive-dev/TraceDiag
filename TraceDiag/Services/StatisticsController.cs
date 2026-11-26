using System.Threading.Channels;
using TraceDiag.Models;

namespace TraceDiag.Services;

/// <summary>Bounded statistic history processed on a dedicated worker.</summary>
internal sealed class StatisticsController : IDisposable
{
    internal const int DefaultMaxItems = 1_000_000;
    private readonly Channel<StatsCommand> _queue = Channel.CreateUnbounded<StatsCommand>();
    private readonly List<HopStatSnapshot> _items = new();
    private readonly List<HopStatSnapshot> _history = new();
    private readonly Action<IReadOnlyList<HopStatSnapshot>> _publish;
    private readonly Task _worker;
    private int _pending;

    internal StatisticsController(int maxItems = DefaultMaxItems, Action<IReadOnlyList<HopStatSnapshot>>? publish = null)
    {
        MaxItems = Math.Max(1, maxItems);
        _publish = publish ?? (_ => { });
        _worker = Task.Run(ConsumeAsync);
    }

    internal int MaxItems { get; }
    internal int Count => _history.Count;
    internal int RowCount => _items.Count;

    internal void Append(HopStatSnapshot stat) => Enqueue(new StatsCommand.Append([stat]));

    internal void Append(IReadOnlyList<HopStatSnapshot> stats) => Enqueue(new StatsCommand.Append(stats));

    internal void Upsert(HopStatSnapshot stat) => Enqueue(new StatsCommand.Upsert(stat));

    internal void Replace(IReadOnlyList<HopStatSnapshot> stats) => Enqueue(new StatsCommand.Replace(stats));

    internal void Clear() => Enqueue(new StatsCommand.Clear());

    internal void Flush()
    {
        var deadline = Environment.TickCount64 + 5000;
        while (Volatile.Read(ref _pending) > 0 && Environment.TickCount64 < deadline)
            Thread.Sleep(1);
    }

    public void Dispose()
    {
        _queue.Writer.TryComplete();
        _worker.Wait(TimeSpan.FromSeconds(5));
    }

    private void Enqueue(StatsCommand command)
    {
        Interlocked.Increment(ref _pending);
        if (!_queue.Writer.TryWrite(command)) Interlocked.Decrement(ref _pending);
    }

    private async Task ConsumeAsync()
    {
        await foreach (var command in _queue.Reader.ReadAllAsync())
        {
            try { Apply(command); _publish(_items.ToArray()); }
            catch { /* Statistics must never break diagnostics flow. */ }
            finally { Interlocked.Decrement(ref _pending); }
        }
    }

    private void Apply(StatsCommand command)
    {
        ApplyRows(command);
        ApplyHistory(command);
        Trim(_items);
        Trim(_history);
    }

    private void ApplyRows(StatsCommand command)
    {
        switch (command)
        {
            case StatsCommand.Clear: _items.Clear(); break;
            case StatsCommand.Replace replace: ReplaceItems(replace.Items); break;
            case StatsCommand.Append append: _items.AddRange(append.Items); break;
            case StatsCommand.Upsert upsert: UpsertItem(upsert.Item); break;
        }
    }

    private void ApplyHistory(StatsCommand command)
    {
        switch (command)
        {
            case StatsCommand.Clear: _history.Clear(); break;
            case StatsCommand.Replace replace: _history.AddRange(replace.Items); break;
            case StatsCommand.Append append: _history.AddRange(append.Items); break;
            case StatsCommand.Upsert upsert: _history.Add(upsert.Item); break;
        }
    }

    private void Trim(List<HopStatSnapshot> items)
    {
        if (items.Count > MaxItems) items.RemoveRange(0, items.Count - MaxItems);
    }

    private void ReplaceItems(IReadOnlyList<HopStatSnapshot> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }

    private void UpsertItem(HopStatSnapshot stat)
    {
        var index = _items.FindIndex(item => SameHop(item, stat));
        if (index < 0) _items.Add(stat);
        else _items[index] = stat;
    }

    private static bool SameHop(HopStatSnapshot left, HopStatSnapshot right) =>
        left.Ttl == right.Ttl && left.Address.Equals(right.Address);
}
