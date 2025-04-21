namespace Cross.Cache.Services;

public interface ICachedService<out TCachedValue>
    where TCachedValue : class
{
    TCachedValue GetAll(bool force = false);
    void Invalidate();
}
