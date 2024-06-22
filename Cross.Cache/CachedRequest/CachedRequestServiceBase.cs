namespace Cross.Cache.CachedRequest;

public abstract class CachedRequestServiceBase<TCachedValue> : ICachedRequestService<TCachedValue>
    where TCachedValue : class
{
    private TCachedValue _cachedValue;

    public TCachedValue GetAll(bool force)
    {
        var cache = force ? null : _cachedValue;
        if (cache == null)
        {
            cache = Load().GetAwaiter().GetResult();
            if (cache != null)
            {
                Setup(cache);
            }
        }

        return cache ?? GetDefaultValue();
    }

    protected virtual TCachedValue GetDefaultValue() => default;

    protected virtual void Setup(TCachedValue cache) => _cachedValue = cache;

    protected abstract Task<TCachedValue> Load();
}
