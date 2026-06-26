namespace Cross.Cache.Providers;

public class CacheInMemoryProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, string> _cacheInMemory;

    public CacheOptions CacheOptions { get; }

    public CacheInMemoryProvider(IOptions<CacheOptions> cacheOptions)
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

    public async Task<T?> GetValueAsync<T>(string key)
    {
        var value = await GetCacheAsync(key);
        if (string.IsNullOrEmpty(value))
            return default;

        return JsonSerializer.Deserialize<T>(value);
    }

    public int GetCacheCount()
        => _cacheInMemory.Count;

    public bool CheckCacheFull(int maxCacheSize)
        => GetCacheCount() >= maxCacheSize;

    public string BuildCacheKey(string typeName, string key)
    {
        // Build cache key
        var keyCache = new StringBuilder();
        keyCache.Append(typeName);
        keyCache.Append('/');
        keyCache.Append(key);

        return keyCache.ToString();
    }

    public Task RemoveCachesByPatternAsync(string pattern)
    {
        var keys = _cacheInMemory.Keys.Where(k => k.Contains(pattern));
        foreach (var key in keys)
        {
            _cacheInMemory.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task RemoveKeyCache(string key)
    {
        _cacheInMemory.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task SetCacheAsync(string key, string value)
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

    public Task SetCacheAsync(string key, string value, TimeSpan expiry)
    {
        // Since this is in-memory implementation, we'll set the value
        // and create a background task to remove it after expiry
        if (expiry > TimeSpan.Zero)
        {
            _ = Task.Delay(expiry).ContinueWith(_ => _cacheInMemory.TryRemove(key, out var _));
        }

        return SetCacheAsync(key, value);
    }

    public Task SetCacheAsync(string key, byte[] value, TimeSpan expiry)
    {
        var base64Value = Convert.ToBase64String(value);
        return SetCacheAsync(key, base64Value, expiry);
    }

    public Task SetCacheAsync(string key, byte[] value)
        => SetCacheAsync(key, value, TimeSpan.Zero);

    public async Task<byte[]?> GetCacheInBytesAsync(string key)
    {
        var value = await GetCacheAsync(key);
        if (string.IsNullOrEmpty(value))
            return null;

        return Convert.FromBase64String(value);
    }

    public Task<bool> KeyExistsAsync(string key)
    {
        return Task.FromResult(_cacheInMemory.ContainsKey(key));
    }

    public Task RemoveCachesByPatternAsync(string pattern, IDatabase? database = null)
    {
        if (database is not null)
        {
            throw new NotSupportedException("In-memory cache provider does not support multiple databases");
        }

        return RemoveCachesByPatternAsync(pattern);
    }

    public Task<IDatabase> GetDatabase(int dbIndex = -1)
    {
        throw new NotSupportedException("In-memory cache provider does not support multiple databases");
    }
}
