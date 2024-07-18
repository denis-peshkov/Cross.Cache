namespace Cross.Cache.Services;

public class CacheInRedisService : ICacheService
{
    private readonly ILogger<CacheInRedisService> _logger;

    private const int EXPIRATION_IN_MINUTES = 1440;

    public CacheOptions CacheOptions { get; }

    private readonly IConnectionMultiplexerPool _connectionPool;

    private readonly int[] _connectionsErrorCount;

    public CacheInRedisService(IOptions<CacheOptions> cacheOptions, ILogger<CacheInRedisService> logger)
    {
        CacheOptions = cacheOptions.Value;
        _logger = logger;

        if (string.IsNullOrEmpty(CacheOptions?.CacheInRedis?.ConnectionString))
        {
            throw new InvalidOperationException($"Property 'CacheOptions.CacheInRedis.ConnectionString' cannot be null or empty.");
        }

        var poolSize = 30;
        _connectionPool = ConnectionMultiplexerPoolFactory.Create(
            poolSize: poolSize,
            configuration: CacheOptions.CacheInRedis!.ConnectionString,
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
        if (string.IsNullOrEmpty(CacheOptions?.CacheInRedis?.ConnectionString))
        {
            throw new InvalidOperationException("Property 'CacheOptions.CacheInRedis.ConnectionString' cannot be null or empty.");
        }

        var multiplexer = ConnectionMultiplexer.Connect(CacheOptions.CacheInRedis.ConnectionString);

        var count = 0;

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
            throw new InvalidOperationException($"Value {nameof(pattern)} cannot be null or empty.");
        }

        database ??= await QueryRedisAsync(Task.FromResult);
        var keyList = await GetKeysByPatternAsync(pattern, database);

        var deleteTasks = keyList.Select(key => database.KeyDeleteAsync(key));

        await Task.WhenAll(deleteTasks);
    }

    public async Task<string> GetCacheAsync(string key)
    {
        var database = await QueryRedisAsync(Task.FromResult);
        var redisValue = await database.StringGetAsync(key);
        var res = redisValue.HasValue
            ? redisValue.ToString()
            : string.Empty;

        return res;
    }
    
    public async Task<T?> GetValueAsync<T>(string key)
    {
        var database = await QueryRedisAsync(Task.FromResult);
        var redisValue = await database.StringGetAsync(key);
        var redisString = redisValue.HasValue
            ? redisValue.ToString()
            : null;
        
        if (!string.IsNullOrEmpty(redisString))
        {
            var type = typeof(T);
            var result = Convert(type, redisString);
            if (result != null)
            {
                return (T?)result;
            }

            return default;
        }

        return default;
    }
    
    private static object? Convert(Type type, string value)
    {
        // Локальная фукнция для проверки является ли JSON объектом строка value
        bool IsValidJson(string jsonString)
        {
            try
            {
                if (jsonString.StartsWith('{') && jsonString.EndsWith('}'))
                {
                    var jsonParsed = JsonValue.Parse(jsonString);
                    if (jsonParsed != null)
                    {
                        return true;
                    }
                }
                else if (jsonString.StartsWith('[') && jsonString.EndsWith(']'))
                {
                    var jsonParsed = JsonValue.Parse(jsonString);
                    if (jsonParsed != null)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
        
        if (type == typeof(object))
        {
            return value;
        }

        bool isJsonObject = IsValidJson(value);
        bool isCollection = type.Name != nameof(String) && type.GetInterface(nameof(IEnumerable)) != null;
        
        // Создание конвертера для конвертации не JSON объектов
        TypeConverter converter = TypeDescriptor.GetConverter(type);
        object? result = null;

        try
        {
            // Особая логика проверки если вычитываем коллекцию, если коллекция пустая - вернем null
            if (isCollection)
            {
                // Конверсия для других типов коллекций
                if (isJsonObject)
                {
                    result = JsonSerializer.Deserialize(value, type);
                }
                else
                {
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        result = converter.ConvertFromInvariantString(value);
                    }
                }

                if (result is ICollection { Count: > 0 } collectionResult)
                {
                    return collectionResult;
                }

                return null;
            }

            // Конверсия в одиночные объекты или системные типы данных
            if (isJsonObject)
            {
                result = JsonSerializer.Deserialize(value, type);
            }
            else
            {
                if (converter.CanConvertFrom(typeof(string)))
                {
                    result = converter.ConvertFromInvariantString(value);
                }
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]?> GetCacheInBytesAsync(string key)
    {
        var database = await QueryRedisAsync(Task.FromResult);
        var redisValue = await database.StringGetAsync(key);
        var res = redisValue.HasValue
            ? (byte[]?)redisValue
            : null;

        return res;
    }

    public async Task SetCacheAsync(string key, string value)
    {
        await SetCacheAsync(key, value, TimeSpan.FromMinutes(EXPIRATION_IN_MINUTES));
    }

    public async Task SetCacheAsync(string key, string value, TimeSpan expiry)
    {
        await QueryRedisAsync(async db => await db.StringSetAsync(
            key,
            value,
            expiry
        ));
    }

    public Task SetCacheAsync(string key, byte[] value)
    {
        return SetCacheAsync(key, value, TimeSpan.FromMinutes(EXPIRATION_IN_MINUTES));
    }

    public async Task SetCacheAsync(string key, byte[] value, TimeSpan expiry)
    {
        await QueryRedisAsync(async db => await db.StringSetAsync(
            key,
            value,
            expiry
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
            throw new InvalidOperationException($"Value {nameof(pattern)} cannot be null or empty.");
        }

        var result = new List<string>();
        long cursor = 0;
        do
        {
            var batchResult = await database.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", pattern);
            var innerResult = (RedisResult[]?)batchResult;
            cursor = long.Parse(((string)innerResult![0])!);
            var redisKeys = (RedisKey[]?)innerResult[1];
            if (redisKeys != null)
            {
                result.AddRange(redisKeys.Select(key => key.ToString()));
            }
        } while (cursor != 0);

        return result;
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await QueryRedisAsync(db => db.KeyExistsAsync(key));
    }

    public async Task<IDatabase> GetDatabase(int dbIndex = -1)
    {
        return await QueryRedisAsync(Task.FromResult, dbIndex);
    }
}
