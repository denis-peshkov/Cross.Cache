namespace Cross.Cache.Options;

public class CacheInRedisOptions
{
    public required string ConnectionString { get; set; }

    // Hour
    public int AbsoluteExpirationRelativeToNow { get; set; }

    // Minutes
    public int SlidingExpiration { get; set; }
}
