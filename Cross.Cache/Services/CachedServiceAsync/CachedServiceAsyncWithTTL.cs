namespace Cross.Cache.Services.CachedServiceAsync;

/// <summary>
/// - Cache with TTL
/// - Asynchronous support
/// - Metrics in general and by keys
/// - Integration with OpenTelemetry
/// - Export to Prometheus via /metrics
/// </summary>
public class CachedServiceAsyncWithTTL : ICachedServiceAsync
{
    private class CacheEntry
    {
        public Lazy<Task<object>> Value { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
    }

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly CachedSummaryStats _stats = new();

    // OpenTelemetry metrics
    private static readonly Meter _meter = new("RequestCacheMetrics");
    private static readonly Counter<long> _hitCounter = _meter.CreateCounter<long>("cache_hits");
    private static readonly Counter<long> _missCounter = _meter.CreateCounter<long>("cache_misses");
    private static readonly Counter<long> _expiredCounter = _meter.CreateCounter<long>("cache_expired");

    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
    {
        Interlocked.Increment(ref _stats.TotalAccesses);
        var now = DateTimeOffset.UtcNow;

        if (_cache.TryGetValue(key, out var existingEntry))
        {
            if (existingEntry.ExpiresAt >= now)
            {
                IncrementKeyStat(key, s => s.Hits++);
                _hitCounter.Add(1, new KeyValuePair<string, object?>("key", key));
                return (T)(await existingEntry.Value.Value);
            }
            else
            {
                IncrementKeyStat(key, s => s.Expired++);
                _expiredCounter.Add(1, new KeyValuePair<string, object?>("key", key));
                _cache.TryRemove(key, out _);
            }
        }

        IncrementKeyStat(key, s => s.Misses++);
        _missCounter.Add(1, new KeyValuePair<string, object?>("key", key));

        var lazy = new Lazy<Task<object>>(async () => (object)(await factory()));
        var newEntry = new CacheEntry
        {
            Value = lazy,
            ExpiresAt = now.Add(ttl ?? TimeSpan.FromMinutes(5))
        };

        var finalEntry = _cache.GetOrAdd(key, newEntry);
        return (T)(await finalEntry.Value.Value);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        Interlocked.Increment(ref _stats.TotalAccesses);
        value = default!;
        var now = DateTimeOffset.UtcNow;

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt >= now && entry.Value is { IsValueCreated: true, Value.IsCompletedSuccessfully: true })
            {
                IncrementKeyStat(key, s => s.Hits++);
                _hitCounter.Add(1, new KeyValuePair<string, object?>("key", key));
                value = (T)entry.Value.Value.Result;
                return true;
            }

            IncrementKeyStat(key, s => s.Expired++);
            _expiredCounter.Add(1, new KeyValuePair<string, object?>("key", key));
            _cache.TryRemove(key, out _);
        }

        IncrementKeyStat(key, s => s.Misses++);
        _missCounter.Add(1, new KeyValuePair<string, object?>("key", key));
        return false;
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(60));
        var lazy = new Lazy<Task<object>>(() => Task.FromResult((object)value!));

        _cache[key] = new CacheEntry
        {
            Value = lazy,
            ExpiresAt = expiresAt
        };
    }

    public CachedSummaryStats GetStats() => _stats;

    private void IncrementKeyStat(string key, Action<CachedKeyStats> update)
    {
        var stats = _stats.PerKey.GetOrAdd(key, _ => new CachedKeyStats());
        update(stats);
    }
}
