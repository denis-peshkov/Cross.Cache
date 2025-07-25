namespace Cross.Cache.Services.CachedServiceAsync;

public class CachedKeyStats
{
    public int Hits { get; internal set; }
    public int Misses { get; internal set; }
    public int Expired { get; internal set; }
}