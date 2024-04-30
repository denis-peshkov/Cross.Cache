namespace Cross.Cache.Services;

public class CacheInRedisService : ICacheService
{
    private readonly ILogger<CacheInRedisService> _logger;

    public int ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW() => 1440;

    private const int SLIDING_EXPIRATION = 1440;

    public CacheOptions CacheOptions { get; }

    private static IConnectionMultiplexerPool _connectionPool;

    private static int[] _connectionsErrorCount;

    public CacheInRedisService(IOptions<CacheOptions> cacheOptions, ILogger<CacheInRedisService> logger)
    {
        _logger = logger;
        CacheOptions = cacheOptions.Value;

        var poolSize = 30;
        _connectionPool = ConnectionMultiplexerPoolFactory.Create(
            poolSize: poolSize,
            configuration: CacheOptions.CacheInRedisOptions.ConnectionStringRedis,
            connectionSelectionStrategy: ConnectionSelectionStrategy.RoundRobin);
        _connectionsErrorCount = new int[poolSize];
    }

    public void ClearCache()
    {
        // The method is deliberately left empty
        // method left to implement ICacheService
    }

    private async Task<TResult> QueryRedisAsync<TResult>(Func<IDatabase, Task<TResult>> op, int dbIndex = -1)
    {
        var connection = await _connectionPool.GetAsync();
        //_logger.LogTrace($"Connection '{connection.ConnectionIndex}' established at {connection.ConnectionTimeUtc}");

        try
        {
            return await op(connection.Connection.GetDatabase(dbIndex));
        }
        catch (RedisConnectionException)
        {
            _connectionsErrorCount[connection.ConnectionIndex]++;
            if (_connectionsErrorCount[connection.ConnectionIndex] > 3)
            {
                throw;
            }
            // Decide when to reconnect based on your own custom logic
            _logger.LogInformation($"Re-establishing connection on index '{connection.ConnectionIndex}'");
            await connection.ReconnectAsync();
            return await op(connection.Connection.GetDatabase(dbIndex));
        }
    }

    public int GetCacheCount()
    {
        var count = 0;

        var multiplexer = ConnectionMultiplexer.Connect(CacheOptions.CacheInRedisOptions.ConnectionStringRedis);

        foreach (var endPoint in multiplexer.GetEndPoints())
        {
            var server = multiplexer.GetServer(endPoint);

            for (var databaseIndex = 0; databaseIndex < server.DatabaseCount; databaseIndex++)
            {
                var keys = server.Keys(pattern: "*", database: databaseIndex).ToList();

                count += keys.Count;
            }
        }

        return count;
    }

    public async Task RemoveCachesByPatternAsync(string pattern, IDatabase? database = null)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            throw new InvalidOperationException("Value cannot be null or empty.");
        }
        database ??= await QueryRedisAsync(db => Task.FromResult(db));
        var keyList = await GetKeysByPatternAsync(pattern, database);

        var deleteTasks = keyList.Select(key => database.KeyDeleteAsync(key));

        await Task.WhenAll(deleteTasks);
    }

    public async Task<string> GetCacheAsync(string key)
    {
        var database = await QueryRedisAsync(db => Task.FromResult(db));
        var redisValue = await database.StringGetAsync(key);
        var res = redisValue.HasValue
            ? redisValue.ToString()
            : null;

        return res;
    }

    public async Task<byte[]> GetCacheInBytesAsync(string key)
    {
        var database = await QueryRedisAsync(db => Task.FromResult(db));
        var redisValue = await database.StringGetAsync(key);
        var res = redisValue.HasValue
            ? (byte[])redisValue
            : null;

        return res;
    }

    public async Task SetCacheAsync(string key, string value, bool defaultOptions)
    {
        await QueryRedisAsync(async db => await db.StringSetAsync(
            key,
            value,
            TimeSpan.FromMinutes(ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW())
        ));
    }

    public async Task SetCacheAsync(string key, byte[] value)
    {
        await QueryRedisAsync(async db => await db.StringSetAsync(
            key,
            value,
            TimeSpan.FromMinutes(ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW())
        ));
    }

    public async Task SetCacheAsync(string key, string value, DistributedCacheEntryOptions options)
    {
        await QueryRedisAsync(async db => await db.StringSetAsync(
            key,
            value,
            TimeSpan.FromMinutes(options.AbsoluteExpiration?.Minute ?? ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW())
            ));
    }

    public bool CheckCacheFull(int maxCacheSize)
        => false;

    public string BuildCacheKey(string typeName, string key)
    {
        var keyCache = new StringBuilder();
        keyCache.Append(typeName);
        keyCache.Append('/');
        keyCache.Append(key);

        return keyCache.ToString();
    }

    public async Task RemoveKeyCache(string key)
    {
        await QueryRedisAsync(async db => await db.KeyDeleteAsync(key));
    }

    private async Task<IReadOnlyCollection<string>> GetKeysByPatternAsync(string pattern, IDatabase database)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            throw new InvalidOperationException("Value cannot be null or empty.");
        }

        var result = new List<string>();
        long cursor = 0;
        do
        {
            var batchResult = await database.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", pattern);
            var innerResult = (RedisResult[])batchResult;
            cursor = long.Parse((string)innerResult[0]);
            var redisKeys = (RedisKey[])innerResult[1];
            result.AddRange(redisKeys.Select(key => key.ToString()));
        } while (cursor != 0);

        return result;
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await QueryRedisAsync(db => db.KeyExistsAsync(key));
    }

    public async Task<IDatabase> GetDatabase(int dbIndex = -1)
    {
        return await QueryRedisAsync(db => Task.FromResult(db), dbIndex);
    }
}
