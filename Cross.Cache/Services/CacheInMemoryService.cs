namespace Cross.Cache.Services;

public class CacheInMemoryService : ICacheService
{
    private readonly ConcurrentDictionary<string, string> _cacheInMemory;

    public CacheOptions CacheOptions { get; }

    public CacheInMemoryService(IOptions<CacheOptions> cacheOptions)
    {
        _cacheInMemory = new ConcurrentDictionary<string, string>();
        CacheOptions = cacheOptions.Value;
    }

    public void ClearCache()
        => _cacheInMemory.Clear();

    public Task<string> GetCacheAsync(string key)
        => _cacheInMemory.TryGetValue(key, out var result)
            ? Task.FromResult(result)
            : Task.FromResult(string.Empty);

    public Task SetCacheAsync(string key, string value, bool defaultOptions)
    {
        if (_cacheInMemory.ContainsKey(key))
        {
            _cacheInMemory[key] = value;
        }
        else
        {
            _cacheInMemory.TryAdd(key, value);
        }

        return Task.CompletedTask;
    }

    public int GetCacheCount()
        => _cacheInMemory.Count;

    public bool CheckCacheFull(int maxCacheSize)
        => GetCacheCount() >= maxCacheSize;

    public string BuildCacheKey(string typeName, string key)
    {
        //Создаем ключ для cache
        var keyCache = new StringBuilder();
        keyCache.Append(typeName);
        keyCache.Append('/');
        keyCache.Append(key);

        return keyCache.ToString();
    }

    public Task RemoveCachesByPatternAsync(string pattern) => throw new NotImplementedException();

    /// The method is deliberately left empty
    /// method left to implement ICacheService
    public Task RemoveKeyCache(string key)
        => Task.CompletedTask;

    /// The method is deliberately left empty
    /// method left to implement ICacheService
    public Task SetCacheAsync(string key, string value, DistributedCacheEntryOptions options)
        => Task.CompletedTask;

    /// The method is deliberately left empty
    /// method left to implement ICacheService
    public Task SetCacheAsync(string key, byte[] value)
        => Task.CompletedTask;

    /// The method is deliberately left empty
    /// method left to implement ICacheService
    public Task<byte[]?> GetCacheInBytesAsync(string key)
        => Task.FromResult(Array.Empty<byte>());

    /// The method is deliberately left empty
    /// method left to implement ICacheService
    public Task<bool> KeyExistsAsync(string key)
    {
        throw new NotImplementedException();
    }

    public int ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW()
    {
        throw new NotImplementedException();
    }

    public Task RemoveCachesByPatternAsync(string pattern, IDatabase? database = null)
    {
        throw new NotImplementedException();
    }

    public Task<IDatabase> GetDatabase(int dbIndex = -1)
    {
        throw new NotImplementedException();
    }
}
