namespace Cross.Cache.Services;

public interface ICacheService
{
    int ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW();

    CacheOptions CacheOptions { get; }

    int GetCacheCount();

    void ClearCache();

    Task RemoveKeyCache(string key);

    bool CheckCacheFull(int maxCacheSize);

    Task<string> GetCacheAsync(string key);

    Task<byte[]> GetCacheInBytesAsync(string key);

    Task SetCacheAsync(string key, string value, bool defaultOptions);

    Task SetCacheAsync(string key, string value, DistributedCacheEntryOptions options);

    Task SetCacheAsync(string key, byte[] value);

    string BuildCacheKey(string typeName, string key);

    Task RemoveCachesByPatternAsync(string pattern, IDatabase? database = null);

    Task<bool> KeyExistsAsync(string key);

    Task<IDatabase> GetDatabase(int dbIndex = -1);
}
