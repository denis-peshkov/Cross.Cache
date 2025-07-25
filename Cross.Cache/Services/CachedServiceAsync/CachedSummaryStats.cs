namespace Cross.Cache.Services.CachedServiceAsync;

public class CachedSummaryStats
{
    public long TotalAccesses;
    public ConcurrentDictionary<string, CachedKeyStats> PerKey { get; } = new();
}