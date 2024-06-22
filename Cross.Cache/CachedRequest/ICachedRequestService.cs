namespace Cross.Cache.CachedRequest;

public interface ICachedRequestService<out TCachedValue>
    where TCachedValue : class
{
    TCachedValue? GetAll(bool force = false);
}
