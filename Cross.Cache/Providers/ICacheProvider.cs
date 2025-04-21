namespace Cross.Cache.Providers;

public interface ICacheProvider
{
    CacheOptions CacheOptions { get; }

    int GetCacheCount();

    void ClearCache();

    Task RemoveKeyCache(string key);

    bool CheckCacheFull(int maxCacheSize);

    Task<string> GetCacheAsync(string key);

    Task<T?> GetValueAsync<T>(string key);

    Task<byte[]?> GetCacheInBytesAsync(string key);

    Task SetCacheAsync(string key, string value);

    Task SetCacheAsync(string key, string value, TimeSpan expiry);

    Task SetCacheAsync(string key, byte[] value);

    Task SetCacheAsync(string key, byte[] value, TimeSpan expiry);

    string BuildCacheKey(string typeName, string key);

    Task RemoveCachesByPatternAsync(string pattern, IDatabase? database = null);

    Task<bool> KeyExistsAsync(string key);

    Task<IDatabase> GetDatabase(int dbIndex = -1);
}
