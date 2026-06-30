namespace Cross.Cache.Benchmarks.Helpers;

public static class BenchmarkCacheProviderFactory
{
    private const string RedisConnectionEnvironmentVariable = "CACHE_BENCHMARK_REDIS_CONNECTION";
    private const string DefaultRedisConnectionString = "localhost:5379";

    public static IEnumerable<BenchmarkCacheProviderKind> GetAvailableProviderKinds()
    {
        yield return BenchmarkCacheProviderKind.InMemory;

        if (IsRedisAvailable())
            yield return BenchmarkCacheProviderKind.Redis;
    }

    public static ICacheProvider Create(BenchmarkCacheProviderKind kind)
        => kind switch
        {
            BenchmarkCacheProviderKind.InMemory => new CacheInMemoryProvider(BenchmarkCacheOptionsFactory.CreateInMemory()),
            BenchmarkCacheProviderKind.Redis => new CacheInRedisProvider(
                BenchmarkCacheOptionsFactory.CreateRedis(GetRedisConnectionString()),
                NullLogger<CacheInRedisProvider>.Instance),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public static bool IsRedisAvailable()
    {
        var connectionString = GetRedisConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 2_000;
            options.SyncTimeout = 2_000;
            options.AbortOnConnectFail = true;

            using var multiplexer = ConnectionMultiplexer.Connect(options);
            return multiplexer.IsConnected;
        }
        catch
        {
            return false;
        }
    }

    public static string GetRedisConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable(RedisConnectionEnvironmentVariable)
                               ?? DefaultRedisConnectionString;

        return NormalizeRedisConnectionString(connectionString);
    }

    private static string NormalizeRedisConnectionString(string connectionString)
    {
        if (connectionString.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return connectionString["http://".Length..];

        if (connectionString.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return connectionString["https://".Length..];

        return connectionString;
    }
}
