namespace Cross.Cache.Services.CachedServiceAsync;

public interface ICachedServiceAsync
{
    Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null);
    bool TryGetValue<T>(string key, out T value);
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    CachedSummaryStats GetStats();
}