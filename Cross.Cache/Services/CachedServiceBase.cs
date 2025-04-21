namespace Cross.Cache.Services;

public abstract class CachedServiceBase<TCachedValue> : ICachedService<TCachedValue>
    where TCachedValue : class
{
    private readonly object _lock = new();
    private TCachedValue? _cachedValue;

    public TCachedValue GetAll(bool force = false)
    {
        if (!force && _cachedValue != null)
            return _cachedValue;

        lock (_lock)
        {
            if (!force && _cachedValue != null)
                return _cachedValue;

            var loaded = Load().GetAwaiter().GetResult();
            _cachedValue = loaded ?? GetDefaultValue();
            return _cachedValue;
        }
    }

    public void Invalidate()
    {
        lock (_lock)
        {
            _cachedValue = null;
        }
    }

    protected virtual TCachedValue GetDefaultValue() => default!;

    protected abstract Task<TCachedValue> Load();
}
