namespace TraceDiag.Services.Diagnostics;

/// <summary>Assigns stable, run-local numbers to observed route keys.</summary>
internal sealed class RouteRegistry
{
    private readonly object _gate = new();
    private readonly Dictionary<string, int> _uids = new(StringComparer.Ordinal);
    private int _next;

    internal int GetOrCreate(string key)
    {
        lock (_gate)
        {
            if (_uids.TryGetValue(key, out var existing)) return existing;
            var uid = ++_next;
            _uids[key] = uid;
            return uid;
        }
    }
}
