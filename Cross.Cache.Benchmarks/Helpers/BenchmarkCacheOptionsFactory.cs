namespace Cross.Cache.Benchmarks.Helpers;

public static class BenchmarkCacheOptionsFactory
{
    public static IOptions<CacheOptions> CreateInMemory()
        => Microsoft.Extensions.Options.Options.Create(new CacheOptions
        {
            UseCache = "InMemory",
            MaxCachedSearchPageSize = 100,
            MaxCacheSize = 10_000,
            CacheWarmPopularCount = 0,
            CacheWarmLatestCount = 0,
            CacheInRedis = null
        });

    public static IOptions<CacheOptions> CreateRedis(string connectionString)
        => Microsoft.Extensions.Options.Options.Create(new CacheOptions
        {
            UseCache = "InRedis",
            MaxCachedSearchPageSize = 100,
            MaxCacheSize = 10_000,
            CacheWarmPopularCount = 0,
            CacheWarmLatestCount = 0,
            CacheInRedis = new CacheInRedisOptions
            {
                ConnectionString = connectionString,
                AbsoluteExpirationRelativeToNow = 24,
                SlidingExpiration = 60
            }
        });

    public static IOptions<CacheOptions> Create()
        => CreateInMemory();
}
