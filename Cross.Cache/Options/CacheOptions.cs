namespace Cross.Cache.Options;

public class CacheOptions
{
    public required string UseCache { get; set; }

    public int MaxCachedSearchPageSize { get; set; }

    public int MaxCacheSize { get; set; }

    public int CacheWarmPopularCount { get; set; }

    public int CacheWarmLatestCount { get; set; }

    public CacheInRedisOptions? CacheInRedis { get; set; }
}
